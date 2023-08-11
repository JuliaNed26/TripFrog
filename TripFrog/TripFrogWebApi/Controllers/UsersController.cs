using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripFrogWebApi.DTO;

namespace TripFrogWebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public UsersController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IServiceResponse<List<IUserDto>>>> GetUsers()
    {
        return Ok(await _unitOfWork.Users.GetUsers());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<IServiceResponse<IUserDto>>> GetUser(Guid id)
    {
        var response = await _unitOfWork.Users.GetUserById(id);
        return response.Successful ? Ok(response) : NotFound(response);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<IServiceResponse<IUserDto>>> RegisterUser(RegisterUserDto registerUser)
    {
        if (!this.IsModelValid(registerUser, out var modelsState))
        {
            return BadRequest(modelsState);
        }
        var response = await _unitOfWork.Users.RegisterUser(registerUser);
        return response.Successful ? Ok(response) : BadRequest(response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<IServiceResponse<Tokens>>> LoginUser(LoginUserDto loginUser)
    {
        var response = await _unitOfWork.LoginUser(loginUser);
        if (!response.Successful)
        {
            return BadRequest(response);
        }
        SaveJwtTokenToCookie(response.Data!.JwtToken);

        return Ok(response);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<IServiceResponse<Tokens>>> RefreshTokens(Tokens tokens)
    {
        var response = await _unitOfWork.RefreshJwtToken(tokens);
        if (response.Successful)
        {
            SaveJwtTokenToCookie(response.Data!.JwtToken);
            return Ok(response);
        }
        return BadRequest(response);
    }

    [HttpPost("logout/{userId}")]
    public async Task<ActionResult> LogoutUser(Guid userId)
    {
        await _unitOfWork.LogoutUser(userId);
        HttpContext.Response.Cookies.Delete(CookieKeys.KeyForSavingJwtInCookie);
        return Ok();
    }

    [HttpPut]
    public async Task<ActionResult<IServiceResponse<IUserDto>>> ChangeUserInfo(ChangedUserInfoDto user)
    {
        if (!this.IsModelValid(user, out var modelsState))
        {
            return BadRequest(modelsState);
        }
        var response = await _unitOfWork.Users.ChangeUserInfo(user);
        return response.Successful ? Ok(response) : NotFound(response);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<IServiceResponse<IUserDto>>> DeleteUser(Guid id)
    {
        var response = await _unitOfWork.Users.DeleteUserAsync(id);
        return response.Successful ? Ok(response) : NotFound(response);
    }

    private void SaveJwtTokenToCookie(string jwtToken)
    {
        HttpContext.Response.Cookies.Append(CookieKeys.KeyForSavingJwtInCookie, jwtToken,
            new CookieOptions
            {
                MaxAge = TimeSpan.FromMinutes(20)
            });
    }
}

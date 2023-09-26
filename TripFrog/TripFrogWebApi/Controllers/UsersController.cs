using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripFrogWebApi.DTO;
using TripFrogWebApi.Repositories;
using TripFrogWebApi.Services;

namespace TripFrogWebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailSender _emailSender;

    public UsersController(IUnitOfWork unitOfWork, IEmailSender emailSender)
    {
        _unitOfWork = unitOfWork;
        _emailSender = emailSender;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IResponse<List<IUserDto>>>> GetUsers()
    {
        return Ok(await _unitOfWork.UserRepository.GetUsersAsync());
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<IResponse<IUserDto>>> RegisterUser(RegisterUserDto registerUser)
    {
        if (!this.IsModelValid(registerUser, out var modelsState))
        {
            return BadRequest(modelsState);
        }
        var response = await _unitOfWork.UserRepository.RegisterUserAsync(registerUser);
        if (!response.Successful)
        {
            return BadRequest(response);
        }

        await _emailSender.SendRegistrationConfirmationEmailAsync(registerUser.Email, $"{registerUser.FirstName}, happy to see you registered!");
        return Ok(response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<IResponse<ILoginInfoDto>>> LoginUser(LoginUserCredentialsDto loginUserCredentials)
    {
        var response = await _unitOfWork.LoginUser(loginUserCredentials);
        if (!response.Successful)
        {
            return BadRequest(response);
        }
        
        return Ok(response);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<IResponse<ILoginInfoDto>>> RefreshJwtToken(TokensDto tokens)
    {
        var response = await _unitOfWork.RefreshTokenRepository.RegenerateJwtTokenWithRefreshTokenAsync(tokens);
        if (!response.Successful)
        {
            return BadRequest(response);
        }
        
        return Ok(response);
    }

    [HttpPost("logout/{userId}")]
    public async Task<ActionResult> LogoutUser(Guid userId)
    {
        await _unitOfWork.LogoutUser(userId);
        HttpContext.Response.Cookies.Delete(AppConstants.KeyForSavingJwtInCookie);
        return Ok();
    }

    [HttpPut]
    public async Task<ActionResult<IResponse<IUserDto>>> ChangeUserInfo(ChangedUserInfoDto user)
    {
        if (!this.IsModelValid(user, out var modelsState))
        {
            return BadRequest(modelsState);
        }
        var response = await _unitOfWork.UserRepository.ChangeUserInfoAsync(user);
        return response.Successful ? Ok(response) : NotFound(response);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<IResponse<IUserDto>>> DeleteUser(Guid id)
    {
        var response = await _unitOfWork.UserRepository.DeleteUserAsync(id);
        return response.Successful ? Ok(response) : NotFound(response);
    }
}

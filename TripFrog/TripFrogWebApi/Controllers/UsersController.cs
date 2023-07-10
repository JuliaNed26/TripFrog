using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripFrogWebApi.ActionsClasses;
using TripFrogWebApi.DTO;

namespace TripFrogWebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }
    
    [HttpGet]
    public async Task<ActionResult<IServiceResponse<List<IUserDto>>>> GetUsers()
    {
        return Ok(await _userService.GetUsers());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<IServiceResponse<IUserDto>>> GetUser(Guid id)
    {
        var response = await _userService.GetUserById(id);
        return response.Successful ? Ok(response) : NotFound(response);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<IServiceResponse<IUserDto>>> RegisterUser(RegisterUserDto registerUser)
    {
        var response = await _userService.RegisterUser(registerUser);
        return response.Successful ? Ok(response) : BadRequest(response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<IServiceResponse<string>>> LoginUser(LoginUserDto loginUser)
    {
        var response = await _userService.LoginUser(loginUser);
        return response.Successful ? Ok(response) : BadRequest(response);
    }

    [HttpPut]
    public async Task<ActionResult<IServiceResponse<IUserDto>>> ChangeUserInfo(ChangedUserInfoDto user)
    {
        var response = await _userService.ChangeUserInfo(user);
        return response.Successful ? Ok(response) : NotFound(response);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<IServiceResponse<IUserDto>>> DeleteUser(Guid id)
    {
        var response = await _userService.DeleteUser(id);
        return response.Successful ? Ok(response) : NotFound(response);
    }
}

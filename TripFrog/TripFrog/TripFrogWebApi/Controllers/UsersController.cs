using Microsoft.AspNetCore.Mvc;
using TripFrogWebApi.ActionsClasses;
using TripFrogWebApi.DTO;

namespace TripFrogWebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly UserService _userActions;

    public UsersController(UserService userActions)
    {
        _userActions = userActions;
    }
    
    [HttpGet]
    public async Task<ActionResult<IServiceResponse<List<IUserDto>>>> GetUsers()
    {
        return Ok(await _userActions.GetUsers());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<IServiceResponse<IUserDto>>> GetUser(Guid id)
    {
        var response = await _userActions.GetUserById(id);
        return response.Successful ? Ok(response) : NotFound(response);
    }

    [HttpPost]
    public async Task<ActionResult<IServiceResponse<IUserDto>>> PostUser(RegisterUserDto registerUser)
    {
        var response = await _userActions.RegisterUser(registerUser);
        return response.Successful ? Ok(response) : BadRequest(response);
    }

    [HttpPut]
    public async Task<ActionResult<IServiceResponse<IUserDto>>> ChangeUserInfo(ChangedUserInfoDto user)
    {
        var response = await _userActions.ChangeUserInfo(user);
        return response.Successful ? Ok(response) : NotFound(response);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<IServiceResponse<IUserDto>>> DeleteUser(Guid id)
    {
        var response = await _userActions.DeleteUser(id);
        return response.Successful ? Ok(response) : NotFound(response);
    }
}

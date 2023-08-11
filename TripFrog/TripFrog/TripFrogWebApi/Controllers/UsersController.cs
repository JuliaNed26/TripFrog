using Microsoft.AspNetCore.Mvc;
using TripFrogWebApi.ActionsClasses;
using TripFrogWebApi.Dtos;

namespace TripFrogWebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly UserActions _userActions;

    public UsersController(UserActions userActions)
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
    public async Task<ActionResult<IServiceResponse<IRegisterUserDto>>> PostUser(RegisterUserDto registerUser)
    {
        var response = await _userActions.RegisterUser(registerUser);
        return response.Successful ? Ok(response) : BadRequest(response);
    }

    /*[HttpPut]
    public async Task<ActionResult<IServiceResponse<IUserDto>>> ChangeUserInfo(UserDto user)
    {
        var response = await _userActions.ChangeUserInfo(user);
        return response.Successful ? Ok(response) : NotFound(response);
    }*/

    [HttpDelete("{id}")]
    public async Task<ActionResult<IServiceResponse<IUserDto>>> DeleteUser(Guid id)
    {
        var response = await _userActions.DeleteUser(id);
        return response.Successful ? Ok(response) : NotFound(response);
    }
}

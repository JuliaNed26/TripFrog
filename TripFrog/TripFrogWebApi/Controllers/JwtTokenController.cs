using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripFrogWebApi.DTO;
using TripFrogWebApi.Services;

namespace TripFrogWebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class JwtTokenController : ControllerBase
{
    private readonly IJwtTokenService _jwtTokenService;

    public JwtTokenController(IJwtTokenService jwtTokenService)
    {
        _jwtTokenService = jwtTokenService;
    }

    [HttpGet("/retrieveUser/{jwtToken}")]
    public ActionResult<IResponse<IUserDto>> GetLoggedUserByToken(string jwtToken)
    {
        var loggedUserResponse = new Response<IUserDto>();
        if (!_jwtTokenService.TryGetUserInfoFromToken(jwtToken, out var loggedUser))
        {
            loggedUserResponse.Successful = false;
            loggedUserResponse.Message = "Can not retrieve user's data from jwtToken. Maybe jwtToken is invalid.";
            return BadRequest(loggedUserResponse);
        }

        loggedUserResponse.Successful = true;
        loggedUserResponse.Data = loggedUser;
        return Ok(loggedUserResponse);
    }
}

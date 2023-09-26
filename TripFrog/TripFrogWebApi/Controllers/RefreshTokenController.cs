using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripFrogWebApi.DTO;
using TripFrogWebApi.Repositories;

namespace TripFrogWebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RefreshTokenController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<IResponse<string>>> GetRefreshTokenForUser(Guid userId)
    {
        var response = await _unitOfWork.RefreshTokenRepository.GetRefreshTokenForUser(userId);
        return response.Successful ? Ok(response) : NotFound(response);
    }

    [HttpPost("refreshUserToken")]
    public async Task<ActionResult<IResponse<ITokensDto>>> RefreshJwtTokenWithRefreshToken(TokensDto tokens)
    {
        var response = await _unitOfWork.RefreshTokenRepository.RegenerateJwtTokenWithRefreshTokenAsync(tokens);
        return response.Successful ? Ok(response) : BadRequest(response);
    }
}

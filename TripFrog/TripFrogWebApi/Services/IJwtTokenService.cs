using System.Security.Claims;
using TripFrogWebApi.DTO;

namespace TripFrogWebApi.TokensCreator;
public interface IJwtTokenService
{
    string GenerateJwtToken(IUserDto user);
    public bool TryGetClaimsFromToken(string token, out ClaimsPrincipal? principal);
}

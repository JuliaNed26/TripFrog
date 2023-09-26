using System.Security.Claims;
using TripFrogWebApi.DTO;

namespace TripFrogWebApi.Services;
public interface IJwtTokenService
{
    public IJwtTokenDto GenerateJwtToken(IUserDto user);
    DateTime GetTokenExpirationDate(ClaimsPrincipal tokenClaimsPrincipal); 
    bool TryGetUserInfoFromToken(string token, out IUserDto loggedUser);
    bool TryGetClaimsFromToken(string token, out ClaimsPrincipal claimsPrincipalFromToken);
}

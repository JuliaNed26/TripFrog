using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TripFrogModels;
using TripFrogWebApi.DTO;

namespace TripFrogWebApi.Services;
public class JwtTokenService : IJwtTokenService
{
    private const string EncryptAlgorithm = SecurityAlgorithms.HmacSha256Signature;
    private readonly TokenValidationParameters _validationParameters;
    private readonly string _tokenKey;

    public JwtTokenService(string tokenKey, TokenValidationParameters tokenValidationParameters)
    {
        if (string.IsNullOrEmpty(tokenKey))
        {
            throw new ArgumentNullException(nameof(tokenKey));
        }
        _tokenKey = tokenKey;
        _validationParameters = tokenValidationParameters;
    }

    public IJwtTokenDto GenerateJwtToken(IUserDto user)
    {
        var userClaims = GenerateClaimsList(user);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenKey));
        var credentials = new SigningCredentials(key, EncryptAlgorithm);
        var expirationDate = DateTime.UtcNow.AddMinutes(AppConstants.MinutesJwtTokenAlive);
        var token = new JwtSecurityToken(
            claims: userClaims,
            expires: expirationDate,
            signingCredentials: credentials);

        var createdToken = new JwtSecurityTokenHandler().WriteToken(token);
        return new JwtTokenDto
        {
            Token = createdToken,
            ExpirationDate = expirationDate
        };
    }

    public DateTime GetTokenExpirationDate(ClaimsPrincipal tokenClaimsPrincipal)
    {
        var jwtTokenExpirationUnix = long.Parse(tokenClaimsPrincipal.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
        var jwtExpirationDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(jwtTokenExpirationUnix);
        return jwtExpirationDate;
    }

    public bool TryGetUserInfoFromToken(string token, out IUserDto loggedUser)
    {
        if (!TryGetClaimsFromToken(token, out var claimsPrincipal))
        {
            loggedUser = new UserDto();
            return false;
        }

        loggedUser = new UserDto
        {
            Id = Guid.Parse(claimsPrincipal.Claims.Single(x => x.Type == "Id").Value),
            FirstName = claimsPrincipal.Claims.Single(x => x.Type == "FirstName").Value,
            LastName = claimsPrincipal.Claims.Single(x => x.Type == "LastName").Value,
            Email = claimsPrincipal.Claims.Single(x => x.Type == "Email").Value,
            Phone = claimsPrincipal.Claims.Single(x => x.Type == "Phone").Value,
            PictureUrl = claimsPrincipal.Claims.Single(x => x.Type == "AccountPhoto").Value,
            Role = claimsPrincipal.Claims.Single(x => x.Type == "Role").Value == "Traveler"
                   ? Role.Traveler
                   : Role.Landlord
        };
        return true;
    }

    public bool TryGetClaimsFromToken(string token, out ClaimsPrincipal claimsPrincipalFromToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        claimsPrincipalFromToken = new ClaimsPrincipal();

        try
        {
            var claimsPrincipal = tokenHandler.ValidateToken(token, _validationParameters, out var validatedToken);
            if (validatedToken is JwtSecurityToken jwtSecurityToken 
                && jwtSecurityToken.Header.Alg.Equals(EncryptAlgorithm, StringComparison.InvariantCultureIgnoreCase))
            {
                claimsPrincipalFromToken = claimsPrincipal;
            }
        }
        catch
        {
            return false;
        }

        return true;
    }

    private static IEnumerable<Claim> GenerateClaimsList(IUserDto user)
    {
        return new List<Claim>()
        {
            new ("Id", user.Id.ToString()),
            new ("FirstName",user.FirstName),
            new ("LastName",user.LastName ?? string.Empty),
            new ("Email",user.Email),
            new ("Phone",user.Phone ?? string.Empty),
            new ("AccountPhoto",user.PictureUrl ?? string.Empty),
            new ("Role",user.Role.ToString()),
        };
    }
}

using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TripFrogWebApi.DTO;

namespace TripFrogWebApi.TokensCreator;
public sealed class JwtTokenService: IJwtTokenService
{
    private readonly string _tokenKey;
    private readonly TokenValidationParameters _validationParameters;
    private readonly string _encryptAlgorithm = SecurityAlgorithms.HmacSha256Signature;

    public JwtTokenService(string tokenKey, TokenValidationParameters tokenValidationParameters)
    {
        if (string.IsNullOrEmpty(tokenKey))
        {
            throw new ArgumentNullException(nameof(tokenKey));
        }
        _tokenKey = tokenKey;
        _validationParameters = tokenValidationParameters;
    }

    public string GenerateJwtToken(IUserDto user)
    {
        var userClaims = GenerateClaimsList(user);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenKey));
        var credentials = new SigningCredentials(key, _encryptAlgorithm);
        var token = new JwtSecurityToken(
            claims: userClaims,
            expires: DateTime.UtcNow.AddMinutes(20),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool TryGetClaimsFromToken(string token, out ClaimsPrincipal principal)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        principal = new ClaimsPrincipal();

        try
        {
            var testPrincipal = tokenHandler.ValidateToken(token, _validationParameters, out var validatedToken);
            if (validatedToken is JwtSecurityToken jwtSecurityToken 
                && jwtSecurityToken.Header.Alg.Equals(_encryptAlgorithm, StringComparison.InvariantCultureIgnoreCase))
            {
                principal = testPrincipal;
                return true;
            }
        }
        catch
        {
            return false;
        }
        return false;
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
            new ("PictureUrl",user.PictureUrl ?? string.Empty),
            new ("Role",user.Role.ToString()),
        };
    }
}

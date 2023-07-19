using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TripFrogWebApi.DTO;

namespace TripFrogWebApi;

public class JWTTokenCreator
{
    private string tokenKey;

    public JWTTokenCreator(string? tokenKey)
    {
        if(string.IsNullOrEmpty(tokenKey))
        {
            throw new ArgumentNullException(nameof(tokenKey));
        }
        this.tokenKey = tokenKey;
    }

    public string CreateJWTToken(IUserDto user)
    {
        List<Claim> userClaims = GenerateClaimsList(user);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        var token = new JwtSecurityToken(
            claims: userClaims, 
            expires:DateTime.Now.AddMinutes(20),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private List<Claim> GenerateClaimsList(IUserDto user)
    {
        return new List<Claim>()
        {
            new Claim("Id", user.Id.ToString()),
            new Claim("FirstName",user.FirstName),
            new Claim("LastName",user.LastName ?? string.Empty),
            new Claim("Email",user.Email),
            new Claim("Phone",user.Phone ?? string.Empty),
            new Claim("PictureUrl",user.PictureUrl ?? string.Empty),
            new Claim("Role",user.Role.ToString()),
        };
    }
}

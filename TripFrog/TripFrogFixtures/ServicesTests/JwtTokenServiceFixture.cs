using Microsoft.IdentityModel.Tokens;
using System.Text;
using TripFrogModels;
using TripFrogWebApi.DTO;
using TripFrogWebApi.Services;

namespace TripFrogFixtures.ServicesTests;

internal class JwtTokenServiceFixture
{
    private static readonly IUserDto TestUser = new UserDto
    {
        Id = Guid.NewGuid(),
        FirstName = "first name",
        LastName = "last name",
        Email = "email@gmail.com",
        Phone = "033449922",
        PictureUrl = string.Empty,
        Role = Role.Traveler
    };

    private readonly JwtTokenService _jwtTokenService;

    public JwtTokenServiceFixture()
    {
        string tokenKey = "my imaginable key";
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
        };

        _jwtTokenService = new JwtTokenService(tokenKey, tokenValidationParameters);
    }

    [Test]
    public void InstantiateJwtTokenService_EmptyTokenKey_ThrowsArgumentException()
    {
        //Arrange
        var validationParameters = new TokenValidationParameters();
        var tokenKey = string.Empty;

        //Assert
        Assert.Throws<ArgumentNullException>(() => { var token = new JwtTokenService(tokenKey, validationParameters); });
    }

    [Test]
    public void GenerateJwtToken_ReturnsNonEmptyString()
    {
        //Act
        var jwtTokenDto = _jwtTokenService.GenerateJwtToken(TestUser);

        //Assert
        Assert.IsFalse(string.IsNullOrEmpty(jwtTokenDto.Token));
    }

    [Test]
    public void GetTokenExpirationDate_NewToken_ReturnsDate20MinutesFromCreation()
    {
        //Arrange
        var jwtTokenDto = _jwtTokenService.GenerateJwtToken(TestUser);
        var expectedExpirationDate = DateTime.UtcNow.AddMinutes(20);
        _jwtTokenService.TryGetClaimsFromToken(jwtTokenDto.Token, out var claims);

        //Act
        var resultDate = _jwtTokenService.GetTokenExpirationDate(claims);

        //Arrange
        Assert.That(resultDate.Year, Is.EqualTo(expectedExpirationDate.Year));
        Assert.That(resultDate.Month, Is.EqualTo(expectedExpirationDate.Month));
        Assert.That(resultDate.Day, Is.EqualTo(expectedExpirationDate.Day));
        Assert.That(resultDate.Hour, Is.EqualTo(expectedExpirationDate.Hour));
        Assert.That(resultDate.Minute, Is.EqualTo(expectedExpirationDate.Minute));
        Assert.That(resultDate.Second, Is.EqualTo(expectedExpirationDate.Second));
    }

    [Test]
    public void TryGetClaimsFromToken_InvalidToken_ReturnsFalse()
    {
        //Arrange
        var invalidToken = "invalid";

        //Act
        var successful = _jwtTokenService.TryGetClaimsFromToken(invalidToken, out _);

        //Assert
        Assert.False(successful);
    }

    [Test]
    public void TryGetClaimsFromToken_ValidToken_ReturnsClaimsPrincipalWithUsersClaims()
    {
        //Arrange
        var jwtTokenDto = _jwtTokenService.GenerateJwtToken(TestUser);

        //Act
        var successful = _jwtTokenService.TryGetClaimsFromToken(jwtTokenDto.Token, out var claims);
        var userIdFromCreatedClaims = claims.Claims.Single(claim => claim.Type == "Id").Value;

        //Assert
        Assert.True(successful);
        Assert.That(userIdFromCreatedClaims, Is.EqualTo(TestUser.Id.ToString()));
    }

    [Test]
    public void TryGetUserInfoFromToken_ValidToken_ReturnsUserForToken()
    {
        //Arrange
        var jwtTokenDto = _jwtTokenService.GenerateJwtToken(TestUser);

        //Act
        var userFromTokenSuccessful = _jwtTokenService.TryGetUserInfoFromToken(jwtTokenDto.Token, out var userFromToken);

        //Assert
        Assert.IsTrue(userFromTokenSuccessful);
        Assert.That(userFromToken.Id, Is.EqualTo(TestUser.Id));
        Assert.That(userFromToken.FirstName, Is.EqualTo(TestUser.FirstName));
        Assert.That(userFromToken.LastName, Is.EqualTo(TestUser.LastName));
        Assert.That(userFromToken.Email, Is.EqualTo(TestUser.Email));
        Assert.That(userFromToken.Phone, Is.EqualTo(TestUser.Phone));
        Assert.That(userFromToken.PictureUrl, Is.EqualTo(TestUser.PictureUrl));
        Assert.That(userFromToken.Role, Is.EqualTo(TestUser.Role));
    }


    [Test]
    public void TryGetUserInfoFromToken_InvalidToken_ReturnsFalseWithDefaultIdUser()
    {
        //Arrange
        var invalidToken = "invalid";

        //Act
        var userFromTokenSuccessful = _jwtTokenService.TryGetUserInfoFromToken(invalidToken, out var userFromToken);

        //Assert
        Assert.IsFalse(userFromTokenSuccessful);
        Assert.That(userFromToken.Id, Is.EqualTo(default(Guid)));
    }
}

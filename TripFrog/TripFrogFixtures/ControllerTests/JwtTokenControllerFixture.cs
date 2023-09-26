using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using TripFrogModels;
using TripFrogWebApi.Controllers;
using TripFrogWebApi.DTO;
using TripFrogWebApi.Services;

namespace TripFrogFixtures.ControllerTests;

internal class JwtTokenControllerFixture
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

    private IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();
    private JwtTokenController _controller;

    public JwtTokenControllerFixture()
    {
        _controller = new JwtTokenController(_jwtTokenService);
    }

    [Test]
    public void GetLoggedUserByToken_ValidToken_ReturnsOkObjectResult()
    {
        //Arrange
        var validToken = "valid token";
        _jwtTokenService.TryGetUserInfoFromToken(validToken, out IUserDto _)
                        .Returns(x =>
                        {
                            x[1] = TestUser;
                            return true;
                        });

        //Act
        var getLoggedUserResponse = _controller.GetLoggedUserByToken(validToken);

        //Assert
        Assert.IsTrue(getLoggedUserResponse.Result is OkObjectResult);
    }

    [Test]
    public void GetLoggedUserByToken_InvalidToken_ReturnsBadRequestObjectResult()
    {
        //Arrange
        var invalidToken = "invalid token";
        _jwtTokenService.TryGetUserInfoFromToken(invalidToken, out IUserDto _)
                        .Returns(false);

        //Act
        var getLoggedUserResponse = _controller.GetLoggedUserByToken(invalidToken);

        //Assert
        Assert.IsTrue(getLoggedUserResponse.Result is BadRequestObjectResult);
    }
}

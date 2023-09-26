using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using TripFrogWebApi.Controllers;
using TripFrogWebApi.DTO;
using TripFrogWebApi.Repositories;

namespace TripFrogFixtures.ControllerTests;

internal class RefreshTokenControllerFixture
{
    private RefreshTokenController _controller;
    private IUnitOfWork _fakeUnitOfWork;

    [OneTimeSetUp]
    public void SetupFields()
    {
        _fakeUnitOfWork = Substitute.For<IUnitOfWork>();
        _controller = new RefreshTokenController(_fakeUnitOfWork)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Test]
    public async Task GetRefreshTokenForUser_UnsuccessfulResponse_ReturnsNotFoundObjectResult()
    {
        //Arrange
        var unsuccessfulResponse = new Response<string>
        {
            Successful = false
        };
        var fakeUserId = Guid.NewGuid();
        _fakeUnitOfWork.RefreshTokenRepository.GetRefreshTokenForUser(Arg.Any<Guid>()).Returns(unsuccessfulResponse);

        //Act
        var response = await _controller.GetRefreshTokenForUser(fakeUserId);

        //Assert
        Assert.IsTrue(response.Result is NotFoundObjectResult);
    }

    [Test]
    public async Task GetRefreshTokenForUser_SuccessfulResponse_ReturnsOkObjectResult()
    {
        //Arrange
        var successfulResponse = new Response<string>
        {
            Successful = true
        };
        var fakeUserId = Guid.NewGuid();
        _fakeUnitOfWork.RefreshTokenRepository.GetRefreshTokenForUser(Arg.Any<Guid>()).Returns(successfulResponse);

        //Act
        var response = await _controller.GetRefreshTokenForUser(fakeUserId);

        //Assert
        Assert.IsTrue(response.Result is OkObjectResult);
    }

    [Test]
    public async Task RefreshJwtTokenWithRefreshToken_SuccessfulResponse_ReturnsOkObjectResult()
    {
        //Arrange
        var successfulResponse = new Response<ITokensDto>
        {
            Successful = true
        };
        var fakeTokens = new TokensDto
        {
            JwtToken = new JwtTokenDto(),
            RefreshToken = string.Empty
        };
        _fakeUnitOfWork.RefreshTokenRepository.RegenerateJwtTokenWithRefreshTokenAsync(Arg.Any<TokensDto>()).Returns(successfulResponse);

        //Act
        var response = await _controller.RefreshJwtTokenWithRefreshToken(fakeTokens);

        //Assert
        Assert.IsTrue(response.Result is OkObjectResult);
    }

    [Test]
    public async Task RefreshJwtTokenWithRefreshToken_UnsuccessfulResponse_ReturnsBadRequestObjectResult()
    {
        //Arrange
        var unsuccessfulResponse = new Response<ITokensDto>
        {
            Successful = false
        };
        var fakeTokens = new TokensDto
        {
            JwtToken = new JwtTokenDto(),
            RefreshToken = string.Empty
        };
        _fakeUnitOfWork.RefreshTokenRepository.RegenerateJwtTokenWithRefreshTokenAsync(Arg.Any<TokensDto>()).Returns(unsuccessfulResponse);

        //Act
        var response = await _controller.RefreshJwtTokenWithRefreshToken(fakeTokens);

        //Assert
        Assert.IsTrue(response.Result is BadRequestObjectResult);
    }
}

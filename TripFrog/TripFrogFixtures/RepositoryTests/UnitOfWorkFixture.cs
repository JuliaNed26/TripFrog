using Microsoft.EntityFrameworkCore;
using NSubstitute;
using TripFrogModels.Models;
using TripFrogWebApi.DTO;
using TripFrogWebApi.Repositories;
using TripFrogWebApi.Services;

namespace TripFrogFixtures.RepositoryTests;

internal sealed class UnitOfWorkFixture
{
    private static readonly LoginUserCredentialsDto[] UsersCredentialsData = new LoginUserCredentialsDto[UsersData.UsersWithoutPassword.Length];

    private readonly DatabaseService _dbService = new();
    private readonly IJwtTokenService _tokenService = Substitute.For<IJwtTokenService>();
    private UnitOfWork _unitOfWork;

    static UnitOfWorkFixture()
    {
        for (int i = 0; i < UsersData.UsersWithoutPassword.Length; i++)
        {
            UsersCredentialsData[i] = new LoginUserCredentialsDto
            {
                Email = UsersData.UsersWithoutPassword[i].Email,
                Password = UsersData.PasswordsForUsers[i]
            };
        }
    }

    [OneTimeSetUp]
    public void SetupFields()
    {
        var mapper = MapperSubstitudeConfigurer.GetConfiguredMapper();
        _tokenService.GenerateJwtToken((Arg.Any<IUserDto>())).Returns(new JwtTokenDto { Token = "token" });
        _unitOfWork = new UnitOfWork(_dbService.Context, mapper, _tokenService);
    }

    [TestCaseSource(nameof(UsersCredentialsData))]
    public async Task LoginUser_CorrectEmailAndPassword_ReturnsUserInfoAndTokens(LoginUserCredentialsDto registeredUserCredentials)
    {
        //Arrange
        _dbService.FillUsersTable();

        //Act
        var response = await _unitOfWork.LoginUser(registeredUserCredentials);

        //Assert
        Assert.IsTrue(response.Successful);
        Assert.That(response.Data.LoggedUser.Email, Is.EqualTo(registeredUserCredentials.Email));
        Assert.IsTrue(!string.IsNullOrEmpty(response.Data.Tokens.JwtToken.Token));
        Assert.IsTrue(!string.IsNullOrEmpty(response.Data.Tokens.RefreshToken));
    }

    [Test]
    public async Task LoginUser_UserDoNotHaveRefreshToken_CreatesAndSavesNewRefreshToken()
    {
        //Arrange
        _dbService.FillUsersTable();

        var registeredUserCredentials = UsersCredentialsData[0];
        var refreshTokensCountBeforeLogin = _dbService.Context.RefreshTokens.Count(token => token.User.Email == registeredUserCredentials.Email);

        //Act
        var response = await _unitOfWork.LoginUser(registeredUserCredentials);
        var refreshTokensCountAfterLogin = _dbService.Context.RefreshTokens.Count(token => token.User.Email == registeredUserCredentials.Email);

        //Assert
        Assert.IsTrue(response.Successful);
        Assert.IsTrue(!string.IsNullOrEmpty(response.Data.Tokens.RefreshToken));
        Assert.That(refreshTokensCountBeforeLogin, Is.EqualTo(0));
        Assert.That(refreshTokensCountAfterLogin, Is.EqualTo(1));
    }

    [Test]
    public async Task LoginUser_LoginTwice_DeletesOldRefreshTokenForUserCreatesNewOneForEachLogin()
    {
        //Arrange
        _dbService.FillUsersTable();
        var registeredUserCredentials = UsersCredentialsData[0];

        //Act
        var firstLoginResponse = await _unitOfWork.LoginUser(registeredUserCredentials);
        var secondLoginResponse = await _unitOfWork.LoginUser(registeredUserCredentials);
        bool oldRefreshTokenExists = await _dbService.Context.RefreshTokens
                                                             .AnyAsync(token => token.Token.ToString() == firstLoginResponse.Data.Tokens.RefreshToken);
        var refreshTokensForUser = _dbService.Context.RefreshTokens
                                                                     .Where(token => token.User.Email == registeredUserCredentials.Email)
                                                                     .ToList();

        //Assert
        Assert.IsFalse(oldRefreshTokenExists);
        Assert.That(secondLoginResponse.Data.Tokens.RefreshToken, Is.EqualTo(refreshTokensForUser[0].Token.ToString()));
        Assert.That(refreshTokensForUser.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task LoginUser_UserWithSuchEmailNotExist_ReturnsUnsuccessfulResponse()
    {
        //Arrange
        _dbService.FillUsersTable();
        LoginUserCredentialsDto loginUserCredentials = new LoginUserCredentialsDto
        {
            Email = "nonexistemail@gmail.com",
            Password = UsersCredentialsData[0].Password
        };

        //Act
        var response = await _unitOfWork.LoginUser(loginUserCredentials);

        //Assert
        Assert.IsFalse(response.Successful);
    }

    [Test]
    public async Task LoginUser_WrongPassword_ReturnsUnsuccessfulResponse()
    {
        //Arrange
        _dbService.FillUsersTable();
        var registeredUserCredentials = UsersCredentialsData[0];
        registeredUserCredentials.Password += "1";

        //Act
        var response = await _unitOfWork.LoginUser(registeredUserCredentials);

        //Assert
        Assert.IsFalse(response.Successful);
    }

    [Test]
    public async Task LogoutUser_RefreshTokenForUserExists_DeletesUserRefreshToken()
    {
        //Arrange
        _dbService.FillUsersTable();
        var registeredUserCredentials = UsersCredentialsData[0];
        var curUser = _dbService.Context.Users.Single(user => user.Email == registeredUserCredentials.Email);
        await SaveRefreshTokenForUser();

        //Act
        await _unitOfWork.LogoutUser(curUser.Id);
        var countOfRefreshTokensForUserAfterLogout = _dbService.Context.RefreshTokens
                                                                           .Count(token => token.UserId == curUser.Id);

        //Assert
        Assert.That(countOfRefreshTokensForUserAfterLogout, Is.EqualTo(0));

        async Task SaveRefreshTokenForUser()
        {
            var refreshToken = new RefreshToken
            {
                Token = Guid.NewGuid(),
                ExpirationDate = DateTime.UtcNow.AddMinutes(1),
                User = curUser,
                UserId = curUser.Id,
            };
            _dbService.Context.RefreshTokens.Add(refreshToken);
            await _dbService.Context.SaveChangesAsync();
        }
    }

    [Test]
    public async Task LogoutUser_RefreshTokenForUserNotExists_DoNothing()
    {
        //Arrange
        _dbService.FillUsersTable();
        var registeredUserCredentials = UsersCredentialsData[0];
        var curUser = _dbService.Context.Users.Single(user => user.Email == registeredUserCredentials.Email);
        var countOfRefreshTokensForUserBeforeLogout = _dbService.Context.RefreshTokens
                                                                            .Count(token => token.UserId == curUser.Id);

        //Act
        await _unitOfWork.LogoutUser(curUser.Id);
        var countOfRefreshTokensForUserAfterLogout = _dbService.Context.RefreshTokens
                                                                           .Count(token => token.UserId == curUser.Id);

        //Assert
        Assert.That(countOfRefreshTokensForUserBeforeLogout, Is.EqualTo(0));
        Assert.That(countOfRefreshTokensForUserAfterLogout, Is.EqualTo(0));
    }

    [TearDown]
    public void ClearUsersTable()
    {
        _dbService.ClearDatabase();
    }
}

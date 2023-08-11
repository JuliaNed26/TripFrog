using System.Security.Claims;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using NSubstitute;
using TripFrogModels.Models;
using TripFrogWebApi.DTO;
using TripFrogWebApi.TokensCreator;
using TripFrogWebApi.UnitOfWork;

namespace TripFrogFixtures.RepositoryTests;

internal sealed class UnitOfWorkFixture
{
    private static readonly LoginUserDto[] UsersCredentialsData =
    {
        new ()
        {
            Email = "email1@gmail.com",
            Password = "crakozyabra0"
        },
        new ()
        {
            Email = "email2@gmail.com",
            Password = "piupes12"
        },
        new ()
        {
            Email = "email3@gmail.com",
            Password = "kitten18"
        }
    };

    private readonly DatabaseService _dbService = new();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly IJwtTokenService _tokenService = Substitute.For<IJwtTokenService>();
    private UnitOfWork _unitOfWork;

    [OneTimeSetUp]
    public void SetupFields()
    {
        _tokenService.GenerateJwtToken((Arg.Any<IUserDto>())).Returns("token");

        _mapper.Map<UserDto>(Arg.Any<User>())
               .ReturnsForAnyArgs(methodParams =>
               {
                   User user = methodParams.Arg<User>();
                   return new UserDto
                   {
                       Id = user.Id,
                       FirstName = user.FirstName,
                       LastName = user.LastName,
                       Email = user.Email,
                       Phone = user.Phone,
                       PictureUrl = user.PictureUrl,
                       Role = user.Role
                   };
               });

        _unitOfWork = new UnitOfWork(_dbService.Context, _mapper, _tokenService);
    }

    [TestCaseSource(nameof(UsersCredentialsData))]
    public async Task LoginUser_CorrectEmailAndPassword_ReturnsJwtAndRefreshTokens(LoginUserDto registeredUserCredentials)
    {
        //arrange
        _dbService.FillUsersTable();

        //act
        var response = await _unitOfWork.LoginUser(registeredUserCredentials);

        //assert
        Assert.IsTrue(response.Successful);
        Assert.That(response.Data, Is.Not.Null);
        Assert.IsTrue(!string.IsNullOrEmpty(response.Data!.JwtToken));
        Assert.IsTrue(!string.IsNullOrEmpty(response.Data!.RefreshToken));
    }

    [Test]
    public async Task LoginUser_UserDoNotHaveRefreshToken_CreatesAndSavesNewRefreshToken()
    {
        //arrange
        _dbService.FillUsersTable();
        var registeredUserCredentials = UsersCredentialsData[0];
        var refreshTokensCountBefore = _dbService.Context.RefreshTokens.Count(token => token.User.Email == registeredUserCredentials.Email);

        //act
        var response = await _unitOfWork.LoginUser(registeredUserCredentials);
        var refreshTokensCountAfter = _dbService.Context.RefreshTokens.Count(token => token.User.Email == registeredUserCredentials.Email);

        //assert
        Assert.IsTrue(response.Successful);
        Assert.That(response.Data, Is.Not.Null);
        Assert.IsTrue(!string.IsNullOrEmpty(response.Data!.RefreshToken));
        Assert.That(refreshTokensCountBefore, Is.EqualTo(0));
        Assert.That(refreshTokensCountAfter, Is.EqualTo(1));
    }

    [Test]
    public async Task LoginUser_LoginTwice_DeletesOldRefreshTokenForUserAndCreatesNewOneEachLogin()
    {
        //arrange
        _dbService.FillUsersTable();
        var registeredUserCredentials = UsersCredentialsData[0];

        //act
        var firstLoginResponse = await _unitOfWork.LoginUser(registeredUserCredentials);
        var secondLoginResponse = await _unitOfWork.LoginUser(registeredUserCredentials);
        var refreshTokensForUserCount = _dbService.Context.RefreshTokens
                                                              .Count(token => token.User.Email == registeredUserCredentials.Email);
        bool oldTokenExists = await _dbService.Context.RefreshTokens
                                                      .AnyAsync(token => token.Token.ToString() == firstLoginResponse.Data!.RefreshToken);

        //assert
        Assert.IsFalse(oldTokenExists);
        Assert.IsTrue(secondLoginResponse.Successful);
        Assert.That(refreshTokensForUserCount, Is.EqualTo(1));
    }

    [Test]
    public async Task LoginUser_UserWithSuchEmailNotExist_ReturnsUnsuccessfulResponse()
    {
        //arrange
        _dbService.FillUsersTable();
        LoginUserDto loginUser = new LoginUserDto
        {
            Email = "nonexistemail@gmail.com",
            Password = "crakozyabra0"
        };

        //act
        var response = await _unitOfWork.LoginUser(loginUser);

        //assert
        Assert.IsFalse(response.Successful);
    }

    [Test]
    public async Task LoginUser_WrongPassword_ReturnsUnsuccessfulResponse()
    {
        //arrange
        _dbService.FillUsersTable();
        var registeredUserCredentials = UsersCredentialsData[0];
        registeredUserCredentials.Password += "1";

        //act
        var response = await _unitOfWork.LoginUser(registeredUserCredentials);

        //assert
        Assert.IsFalse(response.Successful);
    }

    [Test]
    public async Task LogoutUser_RefreshTokenForUserExists_DeletesUserRefreshToken()
    {
        //arrange
        _dbService.FillUsersTable();
        var registeredUserCredentials = UsersCredentialsData[0];
        var curUser = _dbService.Context.Users.Single(user => user.Email == registeredUserCredentials.Email);
        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid(),
            ExpirationDate = DateTime.UtcNow.AddMinutes(1),
            User = curUser,
            UserId = curUser.Id,
        };
        _dbService.Context.RefreshTokens.Add(refreshToken);
        await _dbService.Context.SaveChangesAsync();

        //act
        await _unitOfWork.LogoutUser(curUser.Id);
        var refreshTokensCountForUser = _dbService.Context.RefreshTokens
                                                              .Count(token => token.UserId == curUser.Id);

        //assert
        Assert.That(refreshTokensCountForUser, Is.EqualTo(0));
    }

    [Test]
    public async Task LogoutUser_RefreshTokenForUserNotExists_DoNothing()
    {
        //arrange
        _dbService.FillUsersTable();
        var registeredUserCredentials = UsersCredentialsData[0];
        var curUser = _dbService.Context.Users.Single(user => user.Email == registeredUserCredentials.Email);

        //act
        var refreshTokensCountForUserBefore = _dbService.Context.RefreshTokens
                                                                    .Count(token => token.UserId == curUser.Id);
        await _unitOfWork.LogoutUser(curUser.Id);
        var refreshTokensCountForUserAfter = _dbService.Context.RefreshTokens
                                                              .Count(token => token.UserId == curUser.Id);

        //assert
        Assert.That(refreshTokensCountForUserBefore, Is.EqualTo(0));
        Assert.That(refreshTokensCountForUserAfter, Is.EqualTo(0));
    }

    [Test]
    public async Task RefreshJwtToken_JwtTokenHasNotExpired_ReturnsUnsuccessfulResponse()
    {
        //arrange
        long secondsToFutureDate = (long)DateTime.UtcNow
                                                 .AddMinutes(5)
                                                 .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        var claimsPrincipalWithExp = new ClaimsPrincipal(
            new ClaimsIdentity(
            new[]
            {
                new Claim(JwtRegisteredClaimNames.Exp, secondsToFutureDate.ToString()),

            }));
        _tokenService.TryGetClaimsFromToken(Arg.Any<string>(), out Arg.Any<ClaimsPrincipal?>())
                     .Returns(x =>
                     {
                         x[1] = claimsPrincipalWithExp;
                         return true;
                     });

        //act
        var response = await _unitOfWork.RefreshJwtToken(new Tokens { JwtToken = "", RefreshToken = "" });

        //assert
        Assert.IsFalse(response.Successful);
        Assert.That(response.Message, Is.EqualTo("Jwt token has not expired yet"));
    }

    [Test]
    public async Task RefreshJwtToken_RefreshTokenDoesNotExist_ReturnsUnsuccessfulResponse()
    {
        //arrange
        _dbService.FillUsersTable();
        var claimsPrincipalWithExpAndUserId = new ClaimsPrincipal(
            new ClaimsIdentity(
            new[]
            {
                new Claim(JwtRegisteredClaimNames.Exp, "0"),
                new Claim("Id", (await _dbService.Context.Users.FirstAsync()).Id.ToString())
            }));
        _tokenService.TryGetClaimsFromToken(Arg.Any<string>(), out Arg.Any<ClaimsPrincipal?>())
                     .Returns(x =>
                     {
                         x[1] = claimsPrincipalWithExpAndUserId;
                         return true;
                     });

        //act
        var response = await _unitOfWork.RefreshJwtToken(new Tokens { JwtToken = "", RefreshToken = "someToken" });

        //assert
        Assert.IsFalse(response.Successful);
        Assert.That(response.Message, Is.EqualTo("Refresh token does not exist"));
    }

    [Test]
    public async Task RefreshJwtToken_RefreshTokenExpired_ReturnsUnsuccessfulResponse()
    {
        //arrange 
        _dbService.FillUsersTable();
        var registeredUserCredentials = UsersCredentialsData[0];
        var curUser = _dbService.Context.Users.Single(user => user.Email == registeredUserCredentials.Email);
        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid(),
            ExpirationDate = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1)),
            User = curUser,
            UserId = curUser.Id,
        };
        _dbService.Context.RefreshTokens.Add(refreshToken);
        await _dbService.Context.SaveChangesAsync();

        var claimsPrincipalWithExpAndUserId = new ClaimsPrincipal(
            new ClaimsIdentity(
            new[]
            {
                new Claim(JwtRegisteredClaimNames.Exp, "0"),
                new Claim("Id", curUser.Id.ToString())
            }));
        _tokenService.TryGetClaimsFromToken(Arg.Any<string>(), out Arg.Any<ClaimsPrincipal?>())
                     .Returns(x =>
                     {
                         x[1] = claimsPrincipalWithExpAndUserId;
                         return true;
                     });

        //act
        var response = await _unitOfWork.RefreshJwtToken(new Tokens
        {
            JwtToken = " ",
            RefreshToken = refreshToken.Token.ToString()
        });

        //assert
        Assert.IsFalse(response.Successful);
        Assert.That(response.Message, Is.EqualTo("Refresh token has expired, log in again"));
    }

    [Test]
    public async Task RefreshJwtToken_RefreshTokenNotExpiredJwtTokenExpired_ReturnsNewJwtTokenAndGivenRefreshToken()
    {
        //arrange 
        _dbService.FillUsersTable();
        var registeredUserCredentials = UsersCredentialsData[0];
        var curUser = _dbService.Context.Users.Single(user => user.Email == registeredUserCredentials.Email);
        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid(),
            ExpirationDate = DateTime.UtcNow.AddMinutes(1),
            User = curUser,
            UserId = curUser.Id,
        };
        _dbService.Context.RefreshTokens.Add(refreshToken);
        await _dbService.Context.SaveChangesAsync();

        var claimsPrincipalWithExpAndUserId = new ClaimsPrincipal(
            new ClaimsIdentity(
            new[]
            {
                new Claim(JwtRegisteredClaimNames.Exp, "0"),
                new Claim("Id", curUser.Id.ToString())
            }));
        _tokenService.TryGetClaimsFromToken(Arg.Any<string>(), out Arg.Any<ClaimsPrincipal?>())
                     .Returns(x =>
                     {
                         x[1] = claimsPrincipalWithExpAndUserId;
                         return true;
                     });
        var oldToken = "oldToken";

        //act
        var response = await _unitOfWork.RefreshJwtToken(new Tokens
        {
            JwtToken = oldToken,
            RefreshToken = refreshToken.Token.ToString()
        });

        //assert
        Assert.IsTrue(response.Successful);
        Assert.That(response.Data!.JwtToken, Is.Not.EquivalentTo(oldToken));
        Assert.That(response.Data!.RefreshToken, Is.EquivalentTo(refreshToken.Token.ToString()));
    }

    [TearDown]
    public void ClearUsersTable()
    {
        _dbService.ClearDatabase();
    }
}

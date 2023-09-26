using NSubstitute;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TripFrogModels.Models;
using TripFrogWebApi.DTO;
using TripFrogWebApi.Services;
using TripFrogWebApi.Repositories;

namespace TripFrogFixtures.RepositoryTests;

internal sealed class RefreshTokenRepositoryFixture
{
    private readonly DatabaseService _dbService = new();
    private readonly IJwtTokenService _tokenService = Substitute.For<IJwtTokenService>();
    private UnitOfWork _unitOfWork;

    [OneTimeSetUp]
    public void SetupFields()
    {
        var mapper = MapperSubstitudeConfigurer.GetConfiguredMapper();
        var fakeToken = new JwtTokenDto();
        _tokenService.GenerateJwtToken(Arg.Any<IUserDto>()).Returns(fakeToken);
        _unitOfWork = new UnitOfWork(_dbService.Context, mapper, _tokenService);
    }

    [Test]
    public async Task GetRefreshTokenForUser_UserWithIdDoesNotExist_ReturnsUnsuccessfulResponse()
    {
        //Arrange
        var notExistUserId = Guid.NewGuid();

        //Act
        var getRefreshTokenResponse = await _unitOfWork.RefreshTokenRepository.GetRefreshTokenForUser(notExistUserId);

        //Assert
        Assert.IsFalse(getRefreshTokenResponse.Successful);
    }

    [Test]
    public async Task GetRefreshTokenForUser_RefreshTokenForUserDoesNotExist_ReturnsUnsuccessfulResponse()
    {
        //Arrange
        _dbService.FillUsersTable();
        var existUser = await _dbService.Context.Users.FirstAsync();

        //Act
        var getRefreshTokenResponse = await _unitOfWork.RefreshTokenRepository.GetRefreshTokenForUser(existUser.Id);

        //Assert
        Assert.IsFalse(getRefreshTokenResponse.Successful);
    }

    [Test]
    public async Task GetRefreshTokenForUser_RefreshTokenForUserExists_ReturnsResponseWithRefreshToken()
    {
        //Arrange
        _dbService.FillUsersTable();
        var existUser = await _dbService.Context.Users.FirstAsync();

        var refreshToken = Guid.NewGuid();
        var refreshTokenEntityForUser = new RefreshToken
        {
            Token = refreshToken,
            ExpirationDate = DateTime.UtcNow.AddMinutes(1),
            User = existUser,
            UserId = existUser.Id
        };
        _dbService.Context.RefreshTokens.Add(refreshTokenEntityForUser);
        await _dbService.Context.SaveChangesAsync();

        //Act
        var getRefreshTokenResponse = await _unitOfWork.RefreshTokenRepository.GetRefreshTokenForUser(existUser.Id);

        //Assert
        Assert.IsTrue(getRefreshTokenResponse.Successful);
        Assert.That(getRefreshTokenResponse.Data, Is.EqualTo(refreshToken.ToString()));
    }

    [Test]
    public async Task RegenerateJwtTokenWithRefreshToken_CanNotGetClaimsFromJwtToken_ReturnsUnsuccessfulResponse()
    {
        //Arrange
        var jwtTokenDto = new JwtTokenDto { Token = "token" };
        _tokenService.TryGetClaimsFromToken(jwtTokenDto.Token, out Arg.Any<ClaimsPrincipal>()).Returns(false);

        //Act
        var regenerationResponse = await _unitOfWork.RefreshTokenRepository
                                                    .RegenerateJwtTokenWithRefreshTokenAsync(new TokensDto { JwtToken = jwtTokenDto, RefreshToken = "" });

        //Assert
        Assert.IsFalse(regenerationResponse.Successful);
    }

    [Test]
    public async Task RegenerateJwtTokenWithRefreshToken_JwtTokenHasNotExpired_ReturnsUnsuccessfulResponse()
    {
        //Arrange
        var jwtTokenDto = new JwtTokenDto { Token = "fake token" };
        var claimsPrincipal = new ClaimsPrincipal();
        SetupTryGetClaimsFromTokenReturnClaims(jwtTokenDto.Token, claimsPrincipal);
        SetupGetTokenExpirationDateFromClaimsPrincipalReturnDate(claimsPrincipal, DateTime.UtcNow.AddMinutes(1));

        //Act
        var regenerationResponse = await _unitOfWork.RefreshTokenRepository
                                                    .RegenerateJwtTokenWithRefreshTokenAsync(new TokensDto { JwtToken = jwtTokenDto, RefreshToken = "" });

        //Assert
        Assert.IsFalse(regenerationResponse.Successful);
    }

    [Test]
    public async Task RegenerateJwtTokenWithRefreshToken_UserFromTokenDoesNotExist_ReturnsUnsuccessfulResponse()
    {
        //Arrange
        _dbService.FillUsersTable();

        var unexistentUserId = Guid.NewGuid();
        var claimsPrincipalWithUnexistentUserId = CreateClaimsPrincipalWithUserIdForToken(unexistentUserId);
        SetupTryGetClaimsFromTokenReturnClaims(Arg.Any<string>(), claimsPrincipalWithUnexistentUserId);
        SetupGetTokenExpirationDateFromClaimsPrincipalReturnDate(claimsPrincipalWithUnexistentUserId, DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1)));

        //Act
        var regenerationResponse = await _unitOfWork.RefreshTokenRepository
                                                    .RegenerateJwtTokenWithRefreshTokenAsync(new TokensDto { JwtToken = new JwtTokenDto { Token = "" }, RefreshToken = "" });

        //Assert
        Assert.IsFalse(regenerationResponse.Successful);
    }

    [Test]
    public async Task RegenerateJwtTokenWithRefreshToken_RefreshTokenDoesNotExist_ReturnsUnsuccessfulResponse()
    {
        //Arrange
        _dbService.FillUsersTable();

        var existUserId = (await _dbService.Context.Users.FirstAsync()).Id;
        var claimsPrincipalWithUserId = CreateClaimsPrincipalWithUserIdForToken(existUserId);
        SetupTryGetClaimsFromTokenReturnClaims(Arg.Any<string>(), claimsPrincipalWithUserId);
        SetupGetTokenExpirationDateFromClaimsPrincipalReturnDate(claimsPrincipalWithUserId, DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1))); 
        SetupTryGetUserInfoFromTokenReturnsUserWithId(Arg.Any<string>(), existUserId);

        var nonExistRefreshToken = "nonExistentToken";

        //Act
        var refreshTokenExist = _dbService.Context.RefreshTokens.Any(x => x.Token.ToString() == nonExistRefreshToken);
        var regenerationResponse = await _unitOfWork.RefreshTokenRepository
                                                    .RegenerateJwtTokenWithRefreshTokenAsync(new TokensDto { JwtToken = new JwtTokenDto { Token = "" }, RefreshToken = nonExistRefreshToken });

        //Assert
        Assert.IsFalse(refreshTokenExist);
        Assert.IsFalse(regenerationResponse.Successful);
    }

    [Test]
    public async Task RegenerateJwtTokenWithRefreshToken_RefreshTokenExpired_ReturnsUnsuccessfulResponse()
    {
        //Arrange 
        _dbService.FillUsersTable();
        
        var registeredUser = await _dbService.Context.Users.FirstAsync();
        var expiredRefreshToken = await CreateAndSaveRefreshTokenForUser(registeredUser, DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1)));
        
        var claimsPrincipalWithUserId = CreateClaimsPrincipalWithUserIdForToken(registeredUser.Id);
        SetupTryGetClaimsFromTokenReturnClaims(Arg.Any<string>(), claimsPrincipalWithUserId);
        SetupGetTokenExpirationDateFromClaimsPrincipalReturnDate(claimsPrincipalWithUserId, DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1)));
        SetupTryGetUserInfoFromTokenReturnsUserWithId(Arg.Any<string>(), registeredUser.Id);

        //Act
        var regenerationResponse = await _unitOfWork.RefreshTokenRepository
                                        .RegenerateJwtTokenWithRefreshTokenAsync(new TokensDto
                                        {
                                            JwtToken = new JwtTokenDto { Token = " " }, 
                                            RefreshToken = expiredRefreshToken.Token.ToString()
                                        });

        //Assert
        Assert.IsFalse(regenerationResponse.Successful);
    }

    [Test]
    public async Task RegenerateJwtTokenWithRefreshToken_RefreshTokenNotExpiredJwtTokenExpired_ReturnsNewJwtTokenAndGivenRefreshToken()
    {
        //Arrange 
        _dbService.FillUsersTable();

        var registeredUser = await _dbService.Context.Users.FirstAsync();
        var refreshToken = await CreateAndSaveRefreshTokenForUser(registeredUser, DateTime.UtcNow.AddMinutes(1));

        var oldJwtToken = new JwtTokenDto { Token = "oldToken" };
        var claimsPrincipalWithUserId = CreateClaimsPrincipalWithUserIdForToken(registeredUser.Id);
        SetupTryGetClaimsFromTokenReturnClaims(oldJwtToken.Token, claimsPrincipalWithUserId);
        SetupGetTokenExpirationDateFromClaimsPrincipalReturnDate(claimsPrincipalWithUserId, DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1)));
        SetupTryGetUserInfoFromTokenReturnsUserWithId(oldJwtToken.Token, registeredUser.Id);

        //Act
        var regenerationResponse = await _unitOfWork.RefreshTokenRepository
                                                    .RegenerateJwtTokenWithRefreshTokenAsync(new TokensDto
                                                    {
                                                        JwtToken = oldJwtToken,
                                                        RefreshToken = refreshToken.Token.ToString()
                                                    });

        //Assert
        Assert.IsTrue(regenerationResponse.Successful);
        Assert.That(regenerationResponse.Data.JwtToken.Token, Is.Not.EqualTo(oldJwtToken.Token));
        Assert.That(regenerationResponse.Data.RefreshToken, Is.EquivalentTo(refreshToken.Token.ToString()));
    }

    [TearDown]
    public void ClearUsersTable()
    {
        _dbService.ClearDatabase();
    }

    private ClaimsPrincipal CreateClaimsPrincipalWithUserIdForToken(Guid userId)
    {
        var claimWithUserId = new Claim("Id", userId.ToString());
        var claimsIdentity = new ClaimsIdentity(new[] { claimWithUserId });
        return new ClaimsPrincipal(claimsIdentity);
    }

    private void SetupTryGetClaimsFromTokenReturnClaims(string tokenParameter, ClaimsPrincipal returnClaimsPrincipal)
    {
        _tokenService.TryGetClaimsFromToken(tokenParameter, out Arg.Any<ClaimsPrincipal>())
                     .Returns(x =>
                     {
                         x[1] = returnClaimsPrincipal;
                         return true;
                     });
    }

    private void SetupGetTokenExpirationDateFromClaimsPrincipalReturnDate(ClaimsPrincipal сlaimsPrincipalParameter, DateTime returnExpirationDate)
    {
        _tokenService.GetTokenExpirationDate(сlaimsPrincipalParameter).Returns(returnExpirationDate);
    }

    private void SetupTryGetUserInfoFromTokenReturnsUserWithId(string token, Guid id)
    {
        _tokenService.TryGetUserInfoFromToken(token, out Arg.Any<IUserDto>())
            .Returns(x =>
            {
                x[1] = new UserDto
                {
                    Id = id
                };
                return true;
            });
    }

    private async Task<RefreshToken> CreateAndSaveRefreshTokenForUser(User user, DateTime refreshTokenExpirationDate)
    {
        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid(),
            ExpirationDate = refreshTokenExpirationDate,
            User = user,
            UserId = user.Id
        };
        _dbService.Context.RefreshTokens.Add(refreshToken);
        await _dbService.Context.SaveChangesAsync();
        return refreshToken;
    }


}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using TripFrogModels;
using TripFrogWebApi.Controllers;
using TripFrogWebApi.DTO;
using TripFrogWebApi.Repositories;
using TripFrogWebApi.Services;

namespace TripFrogFixtures.ControllerTests;

internal sealed class UsersControllerFixture
{
    private static readonly RegisterUserDto[] InvalidRegisterData =
    {
        new ()
        {
            FirstName = "Kitty",
            LastName = "Cat",
            Email = "invalidEmail",
            Password = "password1",
            Phone = "066257295",
            Role = Role.Traveler
        },
        new ()
        {
            FirstName = "Kitty",
            LastName = "Cat",
            Email = "email@gmail.com",
            Password = "small1",
            Phone = "066257295",
            Role = Role.Traveler
        },
        new ()
        {
            FirstName = "Kitty",
            LastName = "Cat",
            Email = "email@gmail.com",
            Password = "onlyletters",
            Phone = "066257295",
            Role = Role.Traveler
        },
        new ()
        {
            FirstName = "Kitty",
            LastName = "Cat",
            Email = "email@gmail.com",
            Password = "123456789",
            Phone = "066257295",
            Role = Role.Traveler
        },
        new ()
        {
            FirstName = "Kitty",
            LastName = "Cat",
            Email = "email@gmail.com",
            Password = "toobigpassword1",
            Phone = "066257295",
            Role = Role.Traveler
        },
        new ()
        {
            FirstName = "Kitty",
            LastName = "Cat",
            Email = "email@gmail.com",
            Password = "password1",
            Phone = "incorrect1number",
            Role = Role.Traveler
        }
    };

    private static readonly ChangedUserInfoDto[] InvalidChangeUserData =
    {
        new ()
        {
            Id = Guid.NewGuid(),
            FirstName = UsersData.UsersWithoutPassword[0].FirstName,
            LastName = UsersData.UsersWithoutPassword[0].LastName,
            Password = UsersData.PasswordsForUsers[0],
            Role = UsersData.UsersWithoutPassword[0].Role,
            Email = "invalidEmail"

        },
        new ()
        {
            Id = Guid.NewGuid(),
            FirstName = UsersData.UsersWithoutPassword[0].FirstName,
            LastName = UsersData.UsersWithoutPassword[0].LastName,
            Email = UsersData.UsersWithoutPassword[0].Email,
            Role = UsersData.UsersWithoutPassword[0].Role,
            Password = "small1"
        },
        new ()
        {
            Id = Guid.NewGuid(),
            FirstName = UsersData.UsersWithoutPassword[0].FirstName,
            LastName = UsersData.UsersWithoutPassword[0].LastName,
            Email = UsersData.UsersWithoutPassword[0].Email,
            Role = UsersData.UsersWithoutPassword[0].Role,
            Password = "onlyletters"
        },
        new ()
        {
            Id = Guid.NewGuid(),
            FirstName = UsersData.UsersWithoutPassword[0].FirstName,
            LastName = UsersData.UsersWithoutPassword[0].LastName,
            Email = UsersData.UsersWithoutPassword[0].Email,
            Role = UsersData.UsersWithoutPassword[0].Role,
            Password = "123456789"
        },
        new ()
        {
            Id = Guid.NewGuid(),
            FirstName = UsersData.UsersWithoutPassword[0].FirstName,
            LastName = UsersData.UsersWithoutPassword[0].LastName,
            Email = UsersData.UsersWithoutPassword[0].Email,
            Role = UsersData.UsersWithoutPassword[0].Role,
            Password = "toobigpassword1"
        },
        new ()
        {
            Id = Guid.NewGuid(),
            FirstName = UsersData.UsersWithoutPassword[0].FirstName,
            LastName = UsersData.UsersWithoutPassword[0].LastName,
            Email = UsersData.UsersWithoutPassword[0].Email,
            Role = UsersData.UsersWithoutPassword[0].Role,
            Phone = "incorrect1number"
        }
    };

    private UsersController _controller;
    private IUnitOfWork _fakeUnitOfWork;

    [OneTimeSetUp]
    public void SetupFields()
    {
        _fakeUnitOfWork = Substitute.For<IUnitOfWork>();
        var emailSender = Substitute.For<IEmailSender>();
        emailSender.When(x => x.SendRegistrationConfirmationEmailAsync(Arg.Any<string>(), Arg.Any<string>())).Do(_ => { });
        _controller = new UsersController(_fakeUnitOfWork, emailSender)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Test]
    public async Task GetUsersAsync_ReturnsOkObjectResult()
    {
        //Arrange
        var successfulResponse = new Response<List<IUserDto>>
        {
            Successful = true
        };
        _fakeUnitOfWork.UserRepository.GetUsersAsync().Returns(successfulResponse);

        //Act
        var response = await _controller.GetUsers();

        //Assert
        Assert.IsTrue(response.Result is OkObjectResult);
    }

    [TestCaseSource(nameof(InvalidRegisterData))]
    public async Task RegisterUser_InvalidRegisterUserModel_ReturnsBadRequestResult(RegisterUserDto registerInfo)
    {
        //Act
        var response = await _controller.RegisterUser(registerInfo);

        //Assert
        Assert.IsTrue(response.Result is BadRequestObjectResult);
    }

    [Test]
    public async Task RegisterUser_ValidRegisterUserModel_ReturnsOkObjectResult()
    {
        //Arrange
        RegisterUserDto userToRegister = new()
        {
            FirstName = "Ekler",
            LastName = "Nedavni",
            Email = "email4@gmail.com",
            Password = "password1",
            Role = Role.Landlord
        };
        var successfulResponse = new Response<IUserDto>
        {
            Successful = true
        };
        _fakeUnitOfWork.UserRepository.RegisterUserAsync(userToRegister).Returns(successfulResponse);

        //Act
        var response = await _controller.RegisterUser(userToRegister);

        //Assert
        Assert.IsTrue(response.Result is OkObjectResult);
    }

    [Test]
    public async Task LoginUser_NotSuccessfulRepositoryResponse_ReturnsBadRequestResult()
    {
        //Arrange
        var fakeLoginCredentials = new LoginUserCredentialsDto
        {
            Email = string.Empty,
            Password = string.Empty,
        };
        var unsuccessfulResponse = new Response<ILoginInfoDto>
        {
            Successful = false
        };
        _fakeUnitOfWork.LoginUser(fakeLoginCredentials).Returns(unsuccessfulResponse);

        //Act
        var response = await _controller.LoginUser(fakeLoginCredentials);

        //Assert
        Assert.IsTrue(response.Result is BadRequestObjectResult);
    }

    [Test]
    public async Task LoginUser_SuccessfulRepositoryResponse_ReturnsOkObjectResult()
    {
        //Arrange
        var fakeLoginCredentials = new LoginUserCredentialsDto
        {
            Email = string.Empty,
            Password = string.Empty,
        };
        var successfulResponse = new Response<ILoginInfoDto>
        {
            Successful = true,
            Data = new LoginInfoDto
            {
                LoggedUser = new UserDto(),
                Tokens = new TokensDto
                {
                    JwtToken = new JwtTokenDto(),
                    RefreshToken = ""
                }
            }
        };
        _fakeUnitOfWork.LoginUser(fakeLoginCredentials).Returns(successfulResponse);

        //Act
        var response = await _controller.LoginUser(fakeLoginCredentials);

        //Assert
        Assert.IsTrue(response.Result is OkObjectResult);
    }

    [Test]
    public async Task RefreshTokens_UnsuccessfulRepositoryResponse_ReturnsBadRequestResult()
    {
        //Arrange
        var fakeTokens = new TokensDto
        {
            JwtToken = new JwtTokenDto(),
            RefreshToken = string.Empty
        };
        var unsuccessfulResponse = new Response<ITokensDto>
        {
            Successful = false
        };
        _fakeUnitOfWork.RefreshTokenRepository.RegenerateJwtTokenWithRefreshTokenAsync(fakeTokens).Returns(unsuccessfulResponse);

        //Act
        var response = await _controller.RefreshJwtToken(fakeTokens);

        //Assert
        Assert.IsTrue(response.Result is BadRequestObjectResult);
    }

    [Test]
    public async Task RefreshTokens_SuccessfulRepositoryResponse_ReturnsOkObjectResult()
    {
        //Arrange
        var fakeTokens = new TokensDto
        {
            JwtToken = new JwtTokenDto(),
            RefreshToken = string.Empty
        };
        var successfulResponse = new Response<ITokensDto>
        {
            Successful = true,
            Data = fakeTokens
        };
        _fakeUnitOfWork.RefreshTokenRepository.RegenerateJwtTokenWithRefreshTokenAsync(fakeTokens).Returns(successfulResponse);

        //Act
        var response = await _controller.RefreshJwtToken(fakeTokens);

        //Assert
        Assert.IsTrue(response.Result is OkObjectResult);
    }

    [Test]
    public async Task LogoutUser_ReturnsOkResult()
    {
        //Act 
        var response = await _controller.LogoutUser(Guid.NewGuid());

        //Assert
        Assert.IsTrue(response is OkResult);
    }

    [Test]
    public async Task DeleteUser_UnsuccessfulRepositoryResponse_ReturnsNotFound()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var unsuccessfulResponse = new Response<IUserDto>
        {
            Successful = false
        };
        _fakeUnitOfWork.UserRepository.DeleteUserAsync(userId).Returns(unsuccessfulResponse);

        //Act
        var response = await _controller.DeleteUser(userId);

        //Assert
        Assert.IsTrue(response.Result is NotFoundObjectResult);
    }

    [Test]
    public async Task DeleteUser_SuccessfulRepositoryResponse_ReturnsOkObjectResult()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var successfulResponse = new Response<IUserDto>
        {
            Successful = true
        };
        _fakeUnitOfWork.UserRepository.DeleteUserAsync(userId).Returns(successfulResponse);

        //Act
        var response = await _controller.DeleteUser(userId);

        //Assert
        Assert.IsTrue(response.Result is OkObjectResult);
    }

    [TestCaseSource(nameof(InvalidChangeUserData))]
    public async Task ChangeUserInfo_InvalidChangeUserModel_ReturnsBadRequestResult(ChangedUserInfoDto changeUserInfo)
    {
        //Act
        var response = await _controller.ChangeUserInfo(changeUserInfo);

        //Assert
        Assert.IsTrue(response.Result is BadRequestObjectResult);
    }

    [Test]
    public async Task ChangeUserInfo_UnsuccessfulRepositoryResponse_ReturnsNotFound()
    {
        //Arrange
        var changedUserInfo = new ChangedUserInfoDto { Id = Guid.NewGuid() };
        var unsuccessfulResponse = new Response<IUserDto>
        {
            Successful = false
        };
        _fakeUnitOfWork.UserRepository.ChangeUserInfoAsync(changedUserInfo).Returns(unsuccessfulResponse);

        //Act
        var response = await _controller.ChangeUserInfo(changedUserInfo);

        //Assert
        Assert.IsTrue(response.Result is NotFoundObjectResult);
    }

    [Test]
    public async Task ChangeUserInfo_SuccessfulRepositoryResponse_ReturnsOkObjectResult()
    {
        //Arrange
        var changedUserInfo = new ChangedUserInfoDto { Id = Guid.NewGuid() };
        var successfulResponse = new Response<IUserDto>
        {
            Successful = true
        };
        _fakeUnitOfWork.UserRepository.ChangeUserInfoAsync(changedUserInfo).Returns(successfulResponse);

        //Act
        var response = await _controller.ChangeUserInfo(changedUserInfo);

        //Assert
        Assert.IsTrue(response.Result is OkObjectResult);
    }
}

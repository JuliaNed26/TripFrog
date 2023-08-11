using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using TripFrogModels;
using TripFrogWebApi;
using TripFrogWebApi.Controllers;
using TripFrogWebApi.DTO;
using TripFrogWebApi.Repositories;

namespace TripFrogFixtures.ControllerTests;

internal sealed class UsersControllerFixture
{
    private static RegisterUserDto[] InvalidRegisterData =
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

    private static ChangedUserInfoDto[] InvalidChangeUserData =
    {
        new ()
        {
            Id = Guid.NewGuid(),
            Email = "invalidEmail"
        },
        new ()
        {
            Id = Guid.NewGuid(),
            Password = "small1"
        },
        new ()
        {
            Id = Guid.NewGuid(),
            Password = "onlyletters"
        },
        new ()
        {
            Id = Guid.NewGuid(),
            Password = "123456789"
        },
        new ()
        {
            Id = Guid.NewGuid(),
            Password = "toobigpassword1"
        },
        new ()
        {
            Id = Guid.NewGuid(),
            Phone = "incorrect1number"
        }
    };

    private UsersController _controller;
    private IUnitOfWork _fakeUnitOfWork;

    [OneTimeSetUp]
    public void SetupFields()
    {
        _fakeUnitOfWork = Substitute.For<IUnitOfWork>();
        _fakeUnitOfWork.Users.Returns(Substitute.For<IUserRepository>());
        _controller = new UsersController(_fakeUnitOfWork)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Test]
    public async Task GetUsers_ReturnsOkObjectResult()
    {
        //arrange
        var successfulResponse = new ServiceResponse<List<IUserDto>>
        {
            Successful = true
        };
        _fakeUnitOfWork.Users.GetUsers().Returns(successfulResponse);

        //act
        var response = await _controller.GetUsers();

        //assert
        Assert.IsTrue(response.Result is OkObjectResult);
    }

    [Test]
    public async Task GetUser_SuccessfulRepositoryResponse_ReturnsOkObjectResult()
    {
        //arrange
        var existentUserId = Guid.NewGuid();
        var successfulResponse = new ServiceResponse<IUserDto>
        {
            Successful = true
        };
        _fakeUnitOfWork.Users.GetUserById(existentUserId).Returns(successfulResponse);

        //act
        var response = await _controller.GetUser(existentUserId);

        //assert
        Assert.IsTrue(response.Result is OkObjectResult);
    }

    [Test]
    public async Task GetUser_NotSuccessfulRepositoryResponse_ReturnsNotFoundResult()
    {
        //arrange
        var newUserId = Guid.NewGuid();
        var unsuccessfulResponse = new ServiceResponse<IUserDto>
        {
            Successful = false
        };
        _fakeUnitOfWork.Users.GetUserById(newUserId).Returns(unsuccessfulResponse);

        //act
        var response = await _controller.GetUser(newUserId);

        //assert
        Assert.IsTrue(response.Result is NotFoundObjectResult);
    }

    [TestCaseSource(nameof(InvalidRegisterData))]
    public async Task RegisterUser_InvalidRegisterUserModel_ReturnsBadRequestResult(RegisterUserDto registerInfo)
    {
        //act
        var response = await _controller.RegisterUser(registerInfo);

        //assert
        Assert.IsTrue(response.Result is BadRequestObjectResult);
    }

    [Test]
    public async Task RegisterUser_ValidRegisterUserModel_ReturnsOkObjectResult()
    {
        //arrange
        RegisterUserDto userToRegister = new()
        {
            FirstName = "Ekler",
            LastName = "Nedavni",
            Email = "email4@gmail.com",
            Password = "password1",
            Role = Role.Admin
        };
        var successfulResponse = new ServiceResponse<IUserDto>
        {
            Successful = true
        };
        _fakeUnitOfWork.Users.RegisterUser(userToRegister).Returns(successfulResponse);

        //act
        var response = await _controller.RegisterUser(userToRegister);

        //assert
        Assert.IsTrue(response.Result is OkObjectResult);
    }

    [Test]
    public async Task LoginUser_NotSuccessfulRepositoryResponse_ReturnsBadRequestResult()
    {
        //arrange
        var fakeLoginCredentials = new LoginUserDto
        {
            Email = string.Empty,
            Password = string.Empty,
        };
        var unsuccessfulResponse = new ServiceResponse<Tokens>
        {
            Successful = false
        };
        _fakeUnitOfWork.LoginUser(fakeLoginCredentials).Returns(unsuccessfulResponse);

        //act
        var response = await _controller.LoginUser(fakeLoginCredentials);

        //assert
        Assert.IsTrue(response.Result is BadRequestObjectResult);
    }

    [Test]
    public async Task LoginUser_SuccessfulRRepositoryResponse_ReturnsOkObjectResult()
    {
        //arrange
        var fakeLoginCredentials = new LoginUserDto
        {
            Email = string.Empty,
            Password = string.Empty,
        };
        var successfulResponse = new ServiceResponse<Tokens>
        {
            Successful = true,
            Data = new Tokens()
            {
                JwtToken = string.Empty,
                RefreshToken = string.Empty
            }
        };
        _fakeUnitOfWork.LoginUser(fakeLoginCredentials).Returns(successfulResponse);

        //act
        var response = await _controller.LoginUser(fakeLoginCredentials);

        //assert
        Assert.IsTrue(response.Result is OkObjectResult);
    }

    [Test]
    public async Task RefreshTokens_UnsuccessfulRepositoryResponse_ReturnsBadRequestResult()
    {
        //arrange
        var fakeTokens = new Tokens
        {
            JwtToken = string.Empty,
            RefreshToken = string.Empty
        };
        var unsuccessfulResponse = new ServiceResponse<Tokens>
        {
            Successful = false
        };
        _fakeUnitOfWork.RefreshJwtToken(fakeTokens).Returns(unsuccessfulResponse);

        //act
        var response = await _controller.RefreshTokens(fakeTokens);

        //assert
        Assert.IsTrue(response.Result is BadRequestObjectResult);
    }

    [Test]
    public async Task RefreshTokens_SuccessfulRepositoryResponse_ReturnsOkObjectResult()
    {
        //arrange
        var fakeTokens = new Tokens
        {
            JwtToken = string.Empty,
            RefreshToken = string.Empty
        };
        var successfulResponse = new ServiceResponse<Tokens>
        {
            Successful = true,
            Data = fakeTokens
        };
        _fakeUnitOfWork.RefreshJwtToken(fakeTokens).Returns(successfulResponse);

        //act
        var response = await _controller.RefreshTokens(fakeTokens);

        //assert
        Assert.IsTrue(response.Result is OkObjectResult);
    }

    [Test]
    public async Task LogoutUser_ReturnsOkResult()
    {
        //act 
        var response = await _controller.LogoutUser(Guid.NewGuid());

        //assert
        Assert.IsTrue(response is OkResult);
    }

    [Test]
    public async Task DeleteUser_UnsuccessfulRepositoryResponse_ReturnsNotFound()
    {
        //arrange
        var userId = Guid.NewGuid();
        var unsuccessfulResponse = new ServiceResponse<IUserDto>
        {
            Successful = false
        };
        _fakeUnitOfWork.Users.DeleteUserAsync(userId).Returns(unsuccessfulResponse);

        //act
        var response = await _controller.DeleteUser(userId);

        //assert
        Assert.IsTrue(response.Result is NotFoundObjectResult);
    }

    [Test]
    public async Task DeleteUser_SuccessfulRepositoryResponse_ReturnsOkObjectResult()
    {
        //arrange
        var userId = Guid.NewGuid();
        var successfulResponse = new ServiceResponse<IUserDto>
        {
            Successful = true
        };
        _fakeUnitOfWork.Users.DeleteUserAsync(userId).Returns(successfulResponse);

        //act
        var response = await _controller.DeleteUser(userId);

        //assert
        Assert.IsTrue(response.Result is OkObjectResult);
    }

    [TestCaseSource(nameof(InvalidChangeUserData))]
    public async Task ChangeUserInfo_InvalidChangeUserModel_ReturnsBadRequestResult(ChangedUserInfoDto changeUserInfo)
    {
        //act
        var response = await _controller.ChangeUserInfo(changeUserInfo);

        //assert
        Assert.IsTrue(response.Result is BadRequestObjectResult);
    }

    [Test]
    public async Task ChangeUserInfo_UnsuccessfulRepositoryResponse_ReturnsNotFound()
    {
        //arrange
        var changedUserInfo = new ChangedUserInfoDto { Id = Guid.NewGuid() };
        var unsuccessfulResponse = new ServiceResponse<IUserDto>
        {
            Successful = false
        };
        _fakeUnitOfWork.Users.ChangeUserInfo(changedUserInfo).Returns(unsuccessfulResponse);

        //act
        var response = await _controller.ChangeUserInfo(changedUserInfo);

        //assert
        Assert.IsTrue(response.Result is NotFoundObjectResult);
    }

    [Test]
    public async Task ChangeUserInfo_SuccessfulRepositoryResponse_ReturnsOkObjectResult()
    {
        //arrange
        var changedUserInfo = new ChangedUserInfoDto { Id = Guid.NewGuid() };
        var successfulResponse = new ServiceResponse<IUserDto>
        {
            Successful = true
        };
        _fakeUnitOfWork.Users.ChangeUserInfo(changedUserInfo).Returns(successfulResponse);

        //act
        var response = await _controller.ChangeUserInfo(changedUserInfo);

        //assert
        Assert.IsTrue(response.Result is OkObjectResult);
    }
}

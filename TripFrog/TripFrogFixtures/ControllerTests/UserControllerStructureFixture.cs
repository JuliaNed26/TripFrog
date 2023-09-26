using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using TripFrogWebApi.Controllers;
using TripFrogWebApi.Repositories;
using TripFrogWebApi.Services;

namespace TripFrogFixtures.ControllerTests;

public class UserControllerStructureFixture
{
    private UsersController _controller;

    [OneTimeSetUp]
    public void SetupFields()
    {
        var fakeUnitOfWork = Substitute.For<IUnitOfWork>();
        _controller = new UsersController(fakeUnitOfWork, Substitute.For<EmailSender>(string.Empty))
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Test]
    public void ControllerHasAuthorizedAttribute()
    {
        //arrange
        var controllerType = _controller.GetType();

        //act
        bool hasAuthorizeAttribute = controllerType.GetCustomAttributes<AuthorizeAttribute>().Any();

        //assert
        Assert.IsTrue(hasAuthorizeAttribute);
    }

    [Test]
    public void AllowAnonymousMethodsAreRegisterAndLogin()
    {
        //arrange
        var controllerType = _controller.GetType();
        var allowAnonymousMethods = new[]
        {
            controllerType.GetMethod(nameof(_controller.RegisterUser)),
            controllerType.GetMethod(nameof(_controller.LoginUser))
        }.ToList();

        //act
        var methodsWithAllowAnonymousAttribute = controllerType.GetMethods()
                                                                             .Where(method => method.GetCustomAttributes<AllowAnonymousAttribute>().Any())
                                                                             .ToList();

        //assert
        CollectionAssert.AreEquivalent(allowAnonymousMethods, methodsWithAllowAnonymousAttribute);
    }

}
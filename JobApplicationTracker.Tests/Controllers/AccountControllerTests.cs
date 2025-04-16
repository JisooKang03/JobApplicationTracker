/******************************************************************************
 * Application Name: Job Application Tracker
 * Author: Robert Orazu
 * Instructor: Mahboob Ali
 ******************************************************************************/

using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using JobApplicationTracker.Controllers;
using JobApplicationTracker.Models;
using System.Collections.Generic;

public class AccountControllerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
    private readonly AccountController _controller;

    public AccountControllerTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null, null, null, null, null, null, null, null);

        var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();

        _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
            _userManagerMock.Object, contextAccessor.Object, claimsFactory.Object, null, null, null, null);

        _controller = new AccountController(_userManagerMock.Object, _signInManagerMock.Object);
    }

    [Fact]
    public void Register_Get_ReturnsView()
    {
        var result = _controller.Register();
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Register_Post_SuccessfulRedirectsBasedOnRole()
    {
        var model = new RegisterViewModel
        {
            UserName = "user1",
            Email = "user1@example.com",
            Password = "Test@123",
            Role = "JobSeeker"
        };

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), model.Password))
                        .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), model.Role))
                        .ReturnsAsync(IdentityResult.Success);

        _signInManagerMock.Setup(x => x.SignInAsync(It.IsAny<ApplicationUser>(), false, null))
                          .Returns(Task.CompletedTask);

        var result = await _controller.Register(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Job", redirect.ControllerName);
    }

    [Fact]
    public void Login_Get_ReturnsView()
    {
        var result = _controller.Login();
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Login_Post_ReturnsRedirectToJob_WhenSuccessful()
    {
        var user = new ApplicationUser { UserName = "user1", Email = "user1@example.com" };

        _userManagerMock.Setup(x => x.FindByEmailAsync(user.Email))
                        .ReturnsAsync(user);

        _signInManagerMock.Setup(x => x.PasswordSignInAsync(user.UserName, "Test@123", false, false))
                          .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        _userManagerMock.Setup(x => x.IsInRoleAsync(user, "JobSeeker")).ReturnsAsync(true);

        var result = await _controller.Login(user.Email, "Test@123", false);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Job", redirect.ControllerName);
    }

    [Fact]
    public async Task Login_Post_FailsWithInvalidUser()
    {
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                        .ReturnsAsync((ApplicationUser)null);

        var result = await _controller.Login("notfound@example.com", "invalid", false);
        var view = Assert.IsType<ViewResult>(result);
        Assert.False(view.ViewData.ModelState.IsValid);
    }

    [Fact]
    public async Task Logout_Post_RedirectsToLogin()
    {
        _signInManagerMock.Setup(x => x.SignOutAsync()).Returns(Task.CompletedTask);

        var result = await _controller.Logout();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirect.ActionName);
        Assert.Equal("Account", redirect.ControllerName);
    }

    [Fact]
    public void AccessDenied_ReturnsView()
    {
        var result = _controller.AccessDenied();
        Assert.IsType<ViewResult>(result);
    }
}

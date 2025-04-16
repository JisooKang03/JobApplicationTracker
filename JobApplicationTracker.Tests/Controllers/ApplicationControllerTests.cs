/******************************************************************************
 * Application Name: Job Application Tracker
 * Author: Robert Orazu
 * Instructor: Mahboob Ali
 ******************************************************************************/

using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using JobApplicationTracker.Controllers;
using JobApplicationTracker.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

public class ApplicationsControllerTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly ApplicationsController _controller;

    public ApplicationsControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
       .UseInMemoryDatabase(Guid.NewGuid().ToString())
       .Options;

        _dbContext = new ApplicationDbContext(options);

        var userStore = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(userStore.Object, null, null, null, null, null, null, null, null);

        _controller = new ApplicationsController(_dbContext, _userManagerMock.Object);

        // ✅ Set HttpContext user claims
        var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
        new Claim(ClaimTypes.NameIdentifier, "user123")
    }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = userClaims }
        };

        // ✅ Set TempData
        var tempData = new TempDataDictionary(_controller.ControllerContext.HttpContext, Mock.Of<ITempDataProvider>());
        _controller.TempData = tempData;

        _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user123");

        // ✅ Seed data
        _dbContext.Users.Add(new ApplicationUser { Id = "user123", UserName = "testuser" });
        _dbContext.Jobs.Add(new Job
        {
            Id = 1,
            Title = "Test Job",
            Company = "ABC Corp",
            Description = "Job Desc",
            Location = "Toronto",
            Salary = 70000
        });
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task Apply_FirstTime_AddsApplicationAndRedirects()
    {
        var result = await _controller.Apply(1);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Job", redirect.ControllerName);

        var application = _dbContext.Applications.FirstOrDefault(a => a.UserId == "user123" && a.JobId == 1);
        Assert.NotNull(application);
    }

    [Fact]
    public async Task Apply_AlreadyApplied_ShowsMessageAndRedirects()
    {
        _dbContext.Applications.Add(new Application
        {
            UserId = "user123",
            JobId = 1,
            Status = ApplicationStatus.Pending
        });
        await _dbContext.SaveChangesAsync();

        var result = await _controller.Apply(1);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Job", redirect.ControllerName);
        Assert.Equal("You already applied to this job.", _controller.TempData["Message"]);
    }

    [Fact]
    public async Task MyApplications_ReturnsUserApplicationsView()
    {
        _dbContext.Applications.Add(new Application
        {
            UserId = "user123",
            JobId = 1,
            Status = ApplicationStatus.Pending
        });
        await _dbContext.SaveChangesAsync();

        var result = await _controller.MyApplications();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<Application>>(view.Model);
        Assert.Single(model);
    }

    [Fact]
    public async Task AllApplications_ReturnsAllApplicationsView()
    {
        _dbContext.Applications.Add(new Application
        {
            UserId = "user123",
            JobId = 1,
            Status = ApplicationStatus.Pending
        });
        await _dbContext.SaveChangesAsync();

        var result = await _controller.AllApplications();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<Application>>(view.Model);
        Assert.Single(model);
    }
}

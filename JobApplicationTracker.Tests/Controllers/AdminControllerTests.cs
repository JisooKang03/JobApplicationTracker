/******************************************************************************
 * Application Name: Job Application Tracker
 * Author: Robert Orazu
 * Instructor: Mahboob Ali
 ******************************************************************************/

using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using JobApplicationTracker.Controllers;
using JobApplicationTracker.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

public class AdminControllerTests
{
    private readonly UserManager<ApplicationUser> _userManagerMock;
    private readonly ApplicationDbContext _dbContext;
    private readonly AdminController _controller;

    public AdminControllerTests()
    {
        // ✅ Use a unique in-memory database for each test run to avoid duplicate keys
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        _dbContext = new ApplicationDbContext(options);

        // ✅ Seed required test data
        var seededUser = new ApplicationUser { Id = "test-user", UserName = "testuser", Email = "test@example.com" };
        _dbContext.Users.Add(seededUser);

        _dbContext.Applications.Add(new Application
        {
            UserId = seededUser.Id
            // Add other required properties if necessary
        });

        _dbContext.Jobs.AddRange(
            new Job
            {
                Title = "Job A",
                Description = "Development",
                Location = "Toronto",
                Salary = 75000,
                Company = "Tech Inc."
            },
            new Job
            {
                Title = "Job B",
                Description = "QA Testing",
                Location = "Vancouver",
                Salary = 68000,
                Company = "Testers Co."
            }
        );

        _dbContext.SaveChanges();

        // ✅ Setup mock UserManager
        var store = new Mock<IUserStore<ApplicationUser>>();
        var userManager = new Mock<UserManager<ApplicationUser>>(
            store.Object, null, null, null, null, null, null, null, null);

        _userManagerMock = userManager.Object;

        // ✅ Initialize controller
        _controller = new AdminController(_dbContext, _userManagerMock);
    }

    [Fact]
    public async Task Dashboard_ReturnsCorrectCounts()
    {
        var result = await _controller.Dashboard();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(2, viewResult.ViewData["JobCount"]);
        Assert.Equal(1, viewResult.ViewData["AppCount"]);
        Assert.Equal(1, viewResult.ViewData["UserCount"]);
    }

    [Fact]
    public async Task Users_ReturnsAllUsers()
    {
        var result = await _controller.Users();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<ApplicationUser>>(viewResult.Model);
        Assert.Single(model);
    }

    [Fact]
    public async Task EditUser_Get_ReturnsUserView_WhenFound()
    {
        var user = _dbContext.Users.First();
        var userManagerMock = Mock.Get(_userManagerMock);
        userManagerMock.Setup(u => u.FindByIdAsync(user.Id)).ReturnsAsync(user);

        var result = await _controller.EditUser(user.Id);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(user, viewResult.Model);
    }

    [Fact]
    public async Task EditUser_Get_ReturnsNotFound_WhenUserMissing()
    {
        var userManagerMock = Mock.Get(_userManagerMock);
        userManagerMock.Setup(u => u.FindByIdAsync("missing")).ReturnsAsync((ApplicationUser)null);

        var result = await _controller.EditUser("missing");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteUser_RemovesUser_WhenFound()
    {
        var user = new ApplicationUser { Id = "del-user", UserName = "todelete" };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var userManagerMock = Mock.Get(_userManagerMock);
        userManagerMock.Setup(u => u.FindByIdAsync(user.Id)).ReturnsAsync(user);
        userManagerMock.Setup(u => u.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

        var result = await _controller.DeleteUser(user.Id);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Users", redirect.ActionName);
    }

    [Fact]
    public async Task CreateJob_ValidModel_RedirectsToJobs()
    {
        var job = new Job
        {
            Title = "Junior Developer",
            Description = "Build ASP.NET Core apps.",
            Location = "Remote",
            Salary = 60000m,
            Company = "Dev Solutions Inc.",
            PostedBy = "admin@example.com",
            PostedDate = DateTime.UtcNow
        };

        var result = await _controller.Create(job);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Jobs", redirect.ActionName);
    }
}

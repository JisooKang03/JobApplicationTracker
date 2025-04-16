/******************************************************************************
 * Application Name: Job Application Tracker
 * Author: Robert Orazu
 * Instructor: Mahboob Ali
 ******************************************************************************/

using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobApplicationTracker.Controllers;
using JobApplicationTracker.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Security.Claims;

public class JobControllerTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly JobController _controller;

    public JobControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);

        _dbContext.Jobs.Add(new Job
        {
            Id = 1,
            Title = "Software Engineer",
            Description = "Build things",
            Location = "Remote",
            Salary = 85000,
            Company = "Tech Co",
            PostedBy = "employer@example.com",
            PostedDate = DateTime.UtcNow
        });

        _dbContext.SaveChanges();

        _controller = new JobController(_dbContext);

        var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        _controller.TempData = tempData;
    }

    [Fact]
    public async Task Index_ReturnsViewWithJobs()
    {
        var result = await _controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<Job>>(viewResult.Model);
        Assert.Single(model);
    }

    [Fact]
    public async Task Details_ReturnsView_WhenJobExists()
    {
        var result = await _controller.Details(1);
        var view = Assert.IsType<ViewResult>(result);
        Assert.IsType<Job>(view.Model);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenJobMissing()
    {
        var result = await _controller.Details(999);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Create_Get_ReturnsView()
    {
        var result = _controller.Create();
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Create_Post_ValidModel_SavesAndRedirects()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "admin@example.com")
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        var newJob = new Job
        {
            Title = "Backend Dev",
            Description = "Develop APIs",
            Location = "New York",
            Salary = 90000,
            Company = "API Corp"
        };

        var result = await _controller.Create(newJob);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Jobs", redirect.ActionName);
        Assert.Equal("Admin", redirect.ControllerName);
        Assert.True(_dbContext.Jobs.Any(j => j.Title == "Backend Dev"));
    }

    [Fact]
    public async Task Edit_Get_ReturnsView_WhenJobExists()
    {
        var result = await _controller.Edit(1);
        var view = Assert.IsType<ViewResult>(result);
        Assert.IsType<Job>(view.Model);
    }

    [Fact]
    public async Task Edit_Get_ReturnsNotFound_WhenJobMissing()
    {
        var result = await _controller.Edit(999);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Post_ValidModel_UpdatesAndRedirects()
    {
        var job = _dbContext.Jobs.First();
        job.Title = "Updated Title";

        var result = await _controller.Edit(job);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        var updatedJob = await _dbContext.Jobs.FindAsync(job.Id);
        Assert.Equal("Updated Title", updatedJob.Title);
    }

    [Fact]
    public async Task Delete_RemovesJobAndRedirects()
    {
        var result = await _controller.Delete(1);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        Assert.False(_dbContext.Jobs.Any(j => j.Id == 1));
    }

    [Fact]
    public async Task MyJobs_ReturnsJobsByLoggedInUser()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "employer@example.com")
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        var result = await _controller.MyJobs();
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<Job>>(view.Model);
        Assert.Single(model);
    }
}

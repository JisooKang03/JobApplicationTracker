/******************************************************************************
 * Application Name: Job Application Tracker
 * Author: Robert Orazu
 * Instructor: Mahboob Ali
 ******************************************************************************/

using Xunit;
using JobApplicationTracker.Models;
using System;
using System.Linq;

public class JobModelTests
{
    [Fact]
    public void JobModel_ValidData_PassesValidation()
    {
        var job = new Job
        {
            Title = "Software Engineer",
            Description = "Develop and maintain software.",
            Location = "Toronto",
            Salary = 90000,
            Company = "Tech Inc",
            PostedBy = "employer@example.com",
            PostedDate = DateTime.UtcNow
        };

        var results = ValidationHelper.ValidateModel(job);

        Assert.Empty(results); // ✅ Should pass validation
    }

    [Fact]
    public void JobModel_MissingTitle_FailsValidation()
    {
        var job = new Job
        {
            Description = "Develop backend APIs",
            Location = "Vancouver",
            Salary = 80000,
            Company = "API Corp"
        };

        var results = ValidationHelper.ValidateModel(job);

        Assert.Contains(results, r => r.MemberNames.Contains("Title"));
    }

    [Fact]
    public void JobModel_MissingCompany_FailsValidation()
    {
        var job = new Job
        {
            Title = "Frontend Developer",
            Description = "Build UI",
            Location = "Remote",
            Salary = 75000
        };

        var results = ValidationHelper.ValidateModel(job);

        Assert.Contains(results, r => r.MemberNames.Contains("Company"));
    }

    [Fact]
    public void JobModel_MissingSalary_FailsValidation()
    {
        var job = new Job
        {
            Title = "DevOps Engineer",
            Description = "CI/CD pipeline setup",
            Location = "Calgary",
            Company = "CloudCorp"
        };

        var results = ValidationHelper.ValidateModel(job);

        Assert.Contains(results, r => r.MemberNames.Contains("Salary"));
    }
}

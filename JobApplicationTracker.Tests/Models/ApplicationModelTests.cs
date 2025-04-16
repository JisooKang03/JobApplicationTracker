using JobApplicationTracker.Models;
using System.ComponentModel.DataAnnotations;

public class ApplicationModelTests
{
    public static class ValidationHelper
    {
        public static IList<ValidationResult> ValidateModel(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, context, results, true);
            return results;
        }
    }

    [Fact]
    public void ApplicationModel_ValidData_PassesValidation()
    {
        var application = new Application
        {
            UserId = "user123",
            JobId = 1,
            ResumePath = "resume.pdf",
            Status = ApplicationStatus.Pending
        };

        var results = ValidationHelper.ValidateModel(application);
        Assert.Empty(results);
    }
}

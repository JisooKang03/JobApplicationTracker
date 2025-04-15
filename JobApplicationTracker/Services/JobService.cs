using Microsoft.AspNetCore.Mvc;

namespace JobApplicationTracker.Services
{
    public class JobService : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

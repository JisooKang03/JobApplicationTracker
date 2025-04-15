using JobApplicationTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JobApplicationTracker.Controllers
{
    [Authorize]
    public class ApplicationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ApplicationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(int jobId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var alreadyApplied = await _context.Applications
                .AnyAsync(a => a.JobId == jobId && a.UserId == userId);

            if (alreadyApplied)
            {
                TempData["Message"] = "You already applied to this job.";
                return RedirectToAction("Index", "Job");
            }

            var application = new Application
            {
                JobId = jobId,
                UserId = userId,
                ResumePath = "",
                Status = ApplicationStatus.Pending
            };

            _context.Applications.Add(application);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Application submitted successfully.";
            return RedirectToAction("Index", "Job");
        }

        public async Task<IActionResult> MyApplications()
        {
            var userId = _userManager.GetUserId(User);

            var applications = await _context.Applications
                .Include(a => a.Job)
                .Where(a => a.UserId == userId)
                .ToListAsync();

            return View("~/Views/Applications/MyApplications.cshtml", applications);
        }

        // ✅ Admin-only access to view all applications
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AllApplications()
        {
            var applications = await _context.Applications
                .Include(a => a.Job)
                .Include(a => a.User)
                .ToListAsync();

            return View("~/Views/Applications/AllApplications.cshtml", applications);
        }
    }
}

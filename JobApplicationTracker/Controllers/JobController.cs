using JobApplicationTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace JobApplicationTracker.Controllers
{
    public class JobController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JobController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔍 View all jobs (public)
        public async Task<IActionResult> Index()
        {
            var jobs = await _context.Jobs.ToListAsync();
            return View("~/Views/Job/Index.cshtml", jobs);
        }

        // 🔍 Job details (public)
        public async Task<IActionResult> Details(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null) return NotFound();
            return View("~/Views/Job/Details.cshtml", job);
        }

        // 📝 Show job creation form (Admin & Employer only)
        [Authorize(Roles = "Admin,Employer")]
        public IActionResult Create()
        {
            return View("~/Views/Job/Create.cshtml");
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Job job)
        {
            if (ModelState.IsValid)
            {
                job.PostedBy = User.Identity?.Name ?? "Unknown";
                job.PostedDate = DateTime.Now;

                _context.Jobs.Add(job);
                await _context.SaveChangesAsync();

                TempData["JobCreated"] = "✅ Job posted successfully!";
                return RedirectToAction("Jobs", "Admin");
            }

            return View(job);
        }




        // ✏️ Edit job (Admin & Employer only)
        [Authorize(Roles = "Admin,Employer")]
        public async Task<IActionResult> Edit(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null) return NotFound();
            return View("~/Views/Job/Edit.cshtml", job);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Job job)
        {
            if (ModelState.IsValid)
            {
                _context.Jobs.Update(job);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View("~/Views/Job/Edit.cshtml", job);
        }

        // 🗑️ Delete job (Admin & Employer only)
        [Authorize(Roles = "Admin,Employer")]
        public async Task<IActionResult> Delete(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null) return NotFound();

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // 👔 View jobs posted by logged-in employer
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> MyJobs()
        {
            var username = User.Identity?.Name;
            var jobs = await _context.Jobs
                .Where(j => j.PostedBy == username)
                .ToListAsync();

            return View("~/Views/Job/MyJobs.cshtml", jobs);
        }
    }
}

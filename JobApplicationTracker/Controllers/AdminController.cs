using JobApplicationTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace JobApplicationTracker.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var jobs = await _context.Jobs.ToListAsync();
            var applications = await _context.Applications.ToListAsync();
            var users = await _context.Users.ToListAsync();

            ViewBag.JobCount = jobs.Count;
            ViewBag.AppCount = applications.Count;
            ViewBag.UserCount = users.Count;

            return View("~/Views/Admin/Dashboard.cshtml");
        }

        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            return View("~/Views/Admin/Users.cshtml", users);
        }

        public async Task<IActionResult> Jobs()
        {
            var jobs = await _context.Jobs.ToListAsync();
            return View("~/Views/Admin/Jobs.cshtml", jobs);
        }


        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(ApplicationUser model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
                return NotFound();

            user.UserName = model.UserName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                return RedirectToAction("Users");

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
                return RedirectToAction("Users");

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return RedirectToAction("Users");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Job job)
        {
            if (ModelState.IsValid)
            {
                _context.Jobs.Add(job);
                await _context.SaveChangesAsync();
                return RedirectToAction("Jobs");
            }

            return View(job);
        }
    }
}

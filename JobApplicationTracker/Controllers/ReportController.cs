using Microsoft.AspNetCore.Mvc;
using JobApplicationTracker.Data;
using JobApplicationTracker.Models;
/**
 
Application Name: Job Application Tracker
Author: Rashed Albatayneh
Instructor: [mahboob Ali]**/


using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using Microsoft.Reporting.NETCore;


namespace JobApplicationTracker.Controllers
{
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult ApplicationsByStatus()
        {
            // Step 1: Query applications and project into the report model
            var data = _context.Applications
                .Include(a => a.Job)
                .Include(a => a.User)
                .Select(a => new ApplicationReportModel
                {
                    JobTitle = a.Job.Title,
                    Status = a.Status.ToString(), // ApplicationStatus is enum, so use ToString()
                    ApplicantEmail = a.User.Email,
                    DateApplied = DateTime.Now // ✅ Temporary placeholder
                })
                .ToList();

            // Step 2: Load RDLC report
            var reportPath = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "ApplicationsByStatus_FINAL.rdlc");
            var localReport = new LocalReport { ReportPath = reportPath };

            // Step 3: Add dataset
            localReport.DataSources.Add(new ReportDataSource("AppDataSet", data));

            // Step 4: Render to PDF
            byte[] pdfBytes = localReport.Render("PDF");

            return File(pdfBytes, "application/pdf", "ApplicationsByStatus.pdf");
        }
    }
}

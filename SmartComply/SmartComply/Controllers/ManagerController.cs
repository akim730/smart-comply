using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartComply.data;
using SmartComply.ViewModels;
using SmartComply.Models; // Make sure this namespace is correct for Audit, CorrectivePlan, FoForm, AmCompliance, TyItem, etc.

namespace SmartComply.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ManagerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index() // This action will now fully populate ManagerDashboardViewModel
        {
            // --- Data for Forms Per Compliance Type and Forms Submitted Per Month (Charts) ---
            var foForms = await _context.FoForms
                .Include(f => f.AmCompliance)
                .Include(f => f.Details)
                    .ThenInclude(d => d.TyItem)
                .ToListAsync();

            var formDetails = foForms.SelectMany(f => f.Details).ToList();

            // --- Data for Summary Cards and Tables (Audits and Corrective Plans) ---

            // Fetch all submitted audits (assuming 'Audit' table holds submitted audit records)
            // You might need to adjust the filter based on what defines "submitted" for you.
            // For example, if AuditStatus is "Submitted" or "Completed".
            // I'm assuming for now that `Audit` objects in your `_context.Audits` represent submitted audits.
            var allSubmittedAudits = await _context.Audits
                .Include(a => a.Compliance) // Important for displaying Compliance name in the view
                .ToListAsync();

            // Audits requiring verification
            var auditsRequiringVerification = allSubmittedAudits
                .Where(a => a.VerificationStatus == "Pending") // Filter by "Pending" status
                .ToList();

            // Corrective Plans requiring approval
            var correctivePlansRequiringApproval = await _context.CorrectivePlans
                .Where(cp => cp.Status == "PendingApproval") // Filter by "PendingApproval" status
                .ToListAsync();

            // Initialize the ViewModel
            var model = new ManagerDashboardViewModel
            {
                // Populate properties for the summary cards related to forms (from FoForms)
                TotalForms = foForms.Count,
                TotalCompliance = await _context.AmCompliances.CountAsync(),
                FormsPerCompliance = foForms
                    .GroupBy(f => f.AmCompliance?.ComplianceName ?? "Unknown")
                    .ToDictionary(g => g.Key, g => g.Count()),
                FormsPerInputType = formDetails
                    .GroupBy(d => d.TyItem?.TyName ?? "Unknown")
                    .ToDictionary(g => g.Key, g => g.Count()),
                LatestFormDate = foForms.Max(f => (DateTime?)f.FoDatetime),
                FormsToday = foForms.Count(f => f.FoDatetime.Date == DateTime.Today),
                FormsThisMonth = foForms.Count(f =>
                    f.FoDatetime.Month == DateTime.Today.Month &&
                    f.FoDatetime.Year == DateTime.Today.Year),
                FormsPerMonth = Enumerable.Range(1, 12).ToDictionary(
                    month => new DateTime(1, month, 1).ToString("MMM"), // e.g., "Jan", "Feb"
                    month => foForms.Count(f => f.FoDatetime.Month == month && f.FoDatetime.Year == DateTime.Now.Year)
                ),


                // Populate properties for the new summary cards and tables (from Audits and CorrectivePlans)
                AuditsRequiringVerification = auditsRequiringVerification,
                PendingVerificationCount = auditsRequiringVerification.Count,

                CorrectivePlansRequiringApproval = correctivePlansRequiringApproval,
                PendingCorrectivePlanApprovalCount = correctivePlansRequiringApproval.Count,

                AllSubmittedAudits = allSubmittedAudits, // This will automatically populate TotalSubmittedAudits, ApprovedAudits, RejectedAudits via calculated properties
                // TotalSubmittedAudits, ApprovedAudits, RejectedAudits are calculated properties in the ViewModel itself,
                // so they derive their values from AllSubmittedAudits.
            };

            return View(model);
        }
    }
}
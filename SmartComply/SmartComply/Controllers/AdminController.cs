// SmartComply.Controllers/AdminController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartComply.data;
using SmartComply.Models;
using SmartComply.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System; // Ensure DateTime is accessible

namespace SmartComply.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var complianceOverviewList = await _context.AmCompliances
                .Select(ac => new ComplianceOverviewItem
                {
                    Id = ac.Id,
                    ComplianceName = ac.ComplianceName,
                    ComplianceDescription = ac.ComplianceDescription,
                    CreatedAt = ac.CreatedAt,
                    FormsCount = _context.FoForms.Count(f => f.FoCid == ac.Id)
                })
                .OrderBy(c => c.ComplianceName)
                .ToListAsync();

            var viewModel = new AdminDashboardViewModel
            {
                TotalComplianceTypes = await _context.AmCompliances.CountAsync(),
                TotalForms = await _context.FoForms.CountAsync(),
                TotalArchivedForms = await _context.FoForms.Where(f => f.isArchived).CountAsync(),
                // TotalFoFormDetails = await _context.FoFormDetails.CountAsync(), // REMOVE or comment out this line

                // --- ADDITIONS START ---
                TotalFoldersCreated = await _context.ComplianceFolders.CountAsync(), // Get total count of folders
                AllComplianceFolders = await _context.ComplianceFolders
                                                     .Include(cf => cf.AmCompliance) // Include related compliance type for display
                                                     .OrderBy(cf => cf.Name)
                                                     .ToListAsync(), // Get all folders
                // --- ADDITIONS END ---

                LatestForms = await _context.FoForms
                                            .Include(f => f.AmCompliance)
                                            .OrderByDescending(f => f.FoDatetime)
                                            .Take(5)
                                            .ToListAsync(),

                ComplianceOverview = complianceOverviewList
            };

            return View(viewModel);
        }

        // ... other actions ...
    }
}
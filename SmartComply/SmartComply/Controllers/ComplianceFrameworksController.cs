// SmartComply.Controllers/ComplianceFrameworksController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartComply.Models;
using SmartComply.data;
using Microsoft.AspNetCore.Authorization;
using SmartComply.ViewModels; // Don't forget to add this using statement

namespace SmartComply.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ComplianceFrameworksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ComplianceFrameworksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ComplianceFrameworks
        public async Task<IActionResult> Index()
        {
            var frameworks = await _context.AmCompliances.ToListAsync();
            // Map AmCompliance to ComplianceFrameworkListItemViewModel
            var viewModelList = frameworks.Select(cf => new ComplianceFrameworkListItemViewModel
            {
                Id = cf.Id,
                ComplianceName = cf.ComplianceName,
                ComplianceDescription = cf.ComplianceDescription
            }).ToList();

            return View(viewModelList);
        }

        // GET: ComplianceFrameworks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var complianceFramework = await _context.AmCompliances
                .FirstOrDefaultAsync(m => m.Id == id);
            if (complianceFramework == null)
            {
                return NotFound();
            }

            // Map AmCompliance to ComplianceFrameworkDetailsViewModel
            var viewModel = new ComplianceFrameworkDetailsViewModel
            {
                Id = complianceFramework.Id,
                ComplianceName = complianceFramework.ComplianceName,
                ComplianceDescription = complianceFramework.ComplianceDescription,
                CreatedAt = complianceFramework.CreatedAt,
                UpdatedAt = complianceFramework.UpdatedAt
            };

            return View(viewModel);
        }

        // GET: ComplianceFrameworks/Create
        public IActionResult Create()
        {
            // Return an empty ViewModel for the form
            return View(new ComplianceFrameworkViewModel());
        }

        // POST: ComplianceFrameworks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Bind directly to the ViewModel
        public async Task<IActionResult> Create(ComplianceFrameworkViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Map the ViewModel back to the Domain Model
                var complianceFramework = new AmCompliance
                {
                    ComplianceName = viewModel.ComplianceName,
                    ComplianceDescription = viewModel.ComplianceDescription,
                    // CreatedAt and UpdatedAt are set by the domain model's default or business logic
                    // and should NOT be bound directly from the form for security.
                    // If you need to explicitly set them here, you can do so (e.g., CreatedAt = DateTime.Now)
                };

                _context.Add(complianceFramework);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // If ModelState is not valid, return the ViewModel to the view for error display
            return View(viewModel);
        }

        // GET: ComplianceFrameworks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var complianceFramework = await _context.AmCompliances.FindAsync(id);
            if (complianceFramework == null)
            {
                return NotFound();
            }

            // Map AmCompliance to ComplianceFrameworkViewModel for display in the form
            var viewModel = new ComplianceFrameworkViewModel
            {
                Id = complianceFramework.Id,
                ComplianceName = complianceFramework.ComplianceName,
                ComplianceDescription = complianceFramework.ComplianceDescription
                // CreatedAt/UpdatedAt typically not needed for editing form, but can be added if desired for display
            };

            return View(viewModel);
        }

        // POST: ComplianceFrameworks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Bind directly to the ViewModel
        public async Task<IActionResult> Edit(int id, ComplianceFrameworkViewModel viewModel)
        {
            // Basic ID check (though ViewModel.Id should match route ID)
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Fetch the original entity from the database
                    var complianceFramework = await _context.AmCompliances.FindAsync(id);
                    if (complianceFramework == null)
                    {
                        return NotFound(); // Concurrency or ID not found
                    }

                    // Update properties from the ViewModel to the fetched domain model
                    complianceFramework.ComplianceName = viewModel.ComplianceName;
                    complianceFramework.ComplianceDescription = viewModel.ComplianceDescription;
                    complianceFramework.UpdatedAt = DateTime.Now; // Manually update UpdatedAt

                    _context.Update(complianceFramework); // EF Core will track changes to 'complianceFramework'
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ComplianceFrameworkExists(viewModel.Id.Value)) // Use viewModel.Id.Value since it's confirmed non-null here
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            // If ModelState is not valid, return the ViewModel to the view for error display
            return View(viewModel);
        }

        // GET: ComplianceFrameworks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var complianceFramework = await _context.AmCompliances
                .FirstOrDefaultAsync(m => m.Id == id);
            if (complianceFramework == null)
            {
                return NotFound();
            }

            // Optionally, map to a simple ViewModel for display on the delete confirmation page
            var viewModel = new ComplianceFrameworkDetailsViewModel
            {
                Id = complianceFramework.Id,
                ComplianceName = complianceFramework.ComplianceName,
                ComplianceDescription = complianceFramework.ComplianceDescription,
                CreatedAt = complianceFramework.CreatedAt,
                UpdatedAt = complianceFramework.UpdatedAt
            };

            return View(viewModel);
        }

        // POST: ComplianceFrameworks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var complianceFramework = await _context.AmCompliances.FindAsync(id);
            if (complianceFramework != null)
            {
                _context.AmCompliances.Remove(complianceFramework);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ComplianceFrameworkExists(int id)
        {
            return _context.AmCompliances.Any(e => e.Id == id);
        }
    }
}
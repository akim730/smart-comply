using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using SmartComply.data;
using SmartComply.Models;
using SmartComply.Models.ViewModels; // <--- Add this using directive

namespace SmartComply.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FoFormController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FoFormController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: FoForm
        public async Task<IActionResult> Index()
        {
            var forms = await _context.FoForms
                .Include(f => f.AmCompliance)
                .Include(f => f.Details) // Make sure to include the Details collection
                    .ThenInclude(d => d.TyItem) // Make sure to include TyItem for each detail
                .ToListAsync();

            return View(forms);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var foForm = await _context.FoForms
                .Include(f => f.AmCompliance) // Include Compliance for display
                .Include(f => f.Details)     // Include Sections (FoFormDetail is your "section" model)
                    .ThenInclude(s => s.TyItem) // Then include ItemType for each item in the section
                .FirstOrDefaultAsync(m => m.Id == id);

            if (foForm == null)
            {
                return NotFound();
            }

            // You might map this to a ViewModel if FoForm entity isn't directly used in Details.cshtml,
            // but for simple display, passing the entity is often sufficient.

            return View(foForm);
        }

        // GET: FoForm/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new FoFormMasterViewModel();
            // Start with at least one empty detail row for the user to fill
            viewModel.Details.Add(new FoFormDetailViewModel());
            await PopulateDropdowns(viewModel);
            return View(viewModel);
        }

        // POST: FoForm/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FoFormMasterViewModel viewModel)
        {
            // Always repopulate dropdowns immediately, in case we return to the view with errors.
            await PopulateDropdowns(viewModel);

            // Clean up empty detail rows submitted by the form.
            viewModel.Details.RemoveAll(d => string.IsNullOrWhiteSpace(d.SecName) &&
                                             string.IsNullOrWhiteSpace(d.ItemName) &&
                                             !d.TyItemId.HasValue);

            // If, after cleanup, no detail items remain, add a custom ModelState error.
            if (!viewModel.Details.Any())
            {
                ModelState.AddModelError("Details", "At least one item entry (Section Name, Item Name, and Item Type) is required.");
                viewModel.Details.Add(new FoFormDetailViewModel()); // Add an empty row back
            }

            // --- IMPORTANT DEBUGGING STEP ---
            if (!ModelState.IsValid)
            {
                // CORRECTED LOOP HERE: Iterate over ModelState directly to get KeyValuePair
                foreach (var modelStateEntry in ModelState) // modelStateEntry is now KeyValuePair<string, ModelStateEntry>
                {
                    string key = modelStateEntry.Key; // Access the Key property
                    ModelStateEntry entry = modelStateEntry.Value; // Access the ModelStateEntry object

                    foreach (var error in entry.Errors) // Iterate over the errors of the ModelStateEntry
                    {
                        Console.WriteLine($"ModelState Error (Key: {key}): {error.ErrorMessage}");
                        if (error.Exception != null)
                        {
                            Console.WriteLine($"  Exception: {error.Exception.Message}");
                            if (error.Exception.InnerException != null)
                            {
                                Console.WriteLine($"    Inner Exception: {error.Exception.InnerException.Message}");
                            }
                        }
                    }
                }
                return View(viewModel); // Return the view with validation errors
            }

            try
            {
                // Create the main FoForm header entity
                var foForm = new FoForm
                {
                    FoName = viewModel.FoName,
                    FoCid = viewModel.FoCid.Value, // .Value is safe because Required attribute ensures it has one
                    FoDatetime = DateTime.Now
                };

                // Map and add details from the ViewModel to the FoForm model's collection
                foreach (var detailVm in viewModel.Details)
                {
                    var detail = new FoFormDetail
                    {
                        SecName = detailVm.SecName,
                        ItemName = detailVm.ItemName,
                        TyItemId = detailVm.TyItemId.Value // .Value is safe because Required attribute ensures it has one
                    };
                    foForm.Details.Add(detail); // Add to the collection
                }

                _context.FoForms.Add(foForm); // Add the main FoForm entity (details will be saved automatically)
                await _context.SaveChangesAsync();

                TempData["Success"] = "Form and all detail items created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                // Log the exception details (e.g., using ILogger)
                ModelState.AddModelError("", "Unable to save changes due to a database error. Please try again. " + ex.Message);
                return View(viewModel); // Return the view with the error message
            }
            catch (Exception ex)
            {
                // Catch any other unexpected exceptions
                ModelState.AddModelError("", "An unexpected error occurred. Please try again. " + ex.Message);
                return View(viewModel);
            }
        }

        // GET: FoForm/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            // Load the FoForm (master) and its related details
            var foForm = await _context.FoForms
                .Include(f => f.Details) // Crucial: Eagerly load the detail collection
                    .ThenInclude(d => d.TyItem) // Also include TyItem for each detail if needed for display
                .FirstOrDefaultAsync(f => f.Id == id);

            if (foForm == null) return NotFound();

            // Map data from FoForm model to FoFormMasterViewModel for editing
            var viewModel = new FoFormMasterViewModel
            {
                Id = foForm.Id,
                FoName = foForm.FoName,
                FoCid = foForm.FoCid,
                // Map details from FoForm.Details to a list of FoFormDetailViewModel
                Details = foForm.Details.Select(d => new FoFormDetailViewModel
                {
                    Id = d.Id,        // Preserve existing detail IDs for update/delete tracking
                    SecName = d.SecName,
                    ItemName = d.ItemName,
                    TyItemId = d.TyItemId
                }).ToList()
            };

            // Ensure at least one empty detail row if no details exist (for adding new ones)
            if (!viewModel.Details.Any())
            {
                viewModel.Details.Add(new FoFormDetailViewModel());
            }

            await PopulateDropdowns(viewModel);
            return View(viewModel);
        }

        // POST: FoForm/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FoFormMasterViewModel viewModel)
        {
            // Verify ID matches the viewmodel's ID
            if (id != viewModel.Id) return NotFound();

            await PopulateDropdowns(viewModel); // Repopulate dropdowns immediately

            // Clean up empty detail rows
            viewModel.Details.RemoveAll(d => string.IsNullOrWhiteSpace(d.SecName) &&
                                             string.IsNullOrWhiteSpace(d.ItemName) &&
                                             !d.TyItemId.HasValue);

            // If, after cleanup, no detail items remain, add a custom ModelState error.
            if (!viewModel.Details.Any())
            {
                ModelState.AddModelError("Details", "At least one item entry is required.");
                viewModel.Details.Add(new FoFormDetailViewModel()); // Add an empty row back
            }

            if (!ModelState.IsValid)
            {
                // Log errors
                return View(viewModel);
            }

            // Fetch the existing FoForm entity from the database, including its current details
            var foFormToUpdate = await _context.FoForms
                .Include(f => f.Details) // Crucial: Include existing details to manage them
                .FirstOrDefaultAsync(f => f.Id == id);

            if (foFormToUpdate == null)
            {
                return NotFound(); // Record not found, might have been deleted by another user
            }

            try
            {
                // 1. Update master properties
                foFormToUpdate.FoName = viewModel.FoName;
                foFormToUpdate.FoCid = viewModel.FoCid.Value;
                foFormToUpdate.FoDatetime = DateTime.Now; // Update timestamp on edit if desired

                // 2. Manage the child collection (details)
                // Get current detail IDs from the database
                var existingDetailIds = foFormToUpdate.Details.Select(d => d.Id).ToList();
                // Get submitted detail IDs from the ViewModel
                var submittedDetailIds = viewModel.Details.Where(d => d.Id != 0).Select(d => d.Id).ToList(); // Only existing IDs

                // Remove details that are no longer present in the submitted form
                foreach (var existingDetail in foFormToUpdate.Details.Where(d => !submittedDetailIds.Contains(d.Id)).ToList())
                {
                    _context.FoFormDetails.Remove(existingDetail);
                }

                // Add or update details
                foreach (var detailVm in viewModel.Details)
                {
                    if (detailVm.Id == 0) // This is a new detail item (Id is default int value)
                    {
                        foFormToUpdate.Details.Add(new FoFormDetail
                        {
                            SecName = detailVm.SecName,
                            ItemName = detailVm.ItemName,
                            TyItemId = detailVm.TyItemId.Value,
                            FoFormId = foFormToUpdate.Id // Link to parent
                        });
                    }
                    else // This is an existing detail item (Id is provided from hidden field)
                    {
                        var existingDetail = foFormToUpdate.Details.FirstOrDefault(d => d.Id == detailVm.Id);
                        if (existingDetail != null)
                        {
                            // Update properties of the existing detail
                            existingDetail.SecName = detailVm.SecName;
                            existingDetail.ItemName = detailVm.ItemName;
                            existingDetail.TyItemId = detailVm.TyItemId.Value;
                        }
                        // Else: if existingDetail is null, it means the ID was submitted but not found in DB
                        // (e.g., someone else deleted it). This is a concurrency issue handled by DbUpdateConcurrencyException
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Form and detail items updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FoFormExists(foFormToUpdate.Id))
                    return NotFound();
                else
                    throw; // Re-throw if it's a genuine concurrency issue
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Unable to save changes due to a database error. Please try again. " + ex.Message);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An unexpected error occurred. Please try again. " + ex.Message);
                return View(viewModel);
            }
        }

        // NEW: Archive Action (HTTP POST)
        [HttpPost]
        [ValidateAntiForgeryToken] // Recommended for POST actions
        public async Task<IActionResult> Archive(int id)
        {
            var form = await _context.FoForms.FindAsync(id);
            if (form == null)
            {
                return Json(new { success = false, message = "Form not found." });
            }

            try
            {
                form.isArchived = true; // Set the archived status
                _context.Update(form); // Mark the entity as modified
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Form archived successfully." });
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., using ILogger)
                return Json(new { success = false, message = $"Error archiving form: {ex.Message}" });
            }
        }

        // In FoFormController.cs
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unarchive(int id)
        {
            var form = await _context.FoForms.FindAsync(id);
            if (form == null)
            {
                return Json(new { success = false, message = "Form not found." });
            }

            try
            {
                form.isArchived = false; // Set archived status back to false
                _context.Update(form); // Mark the entity as modified
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Form unarchived successfully." });
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., using ILogger)
                return Json(new { success = false, message = $"Error unarchiving form: {ex.Message}" });
            }
        }


        // IMPORTANT: The 'Delete' action below was the original hard delete.
        // If you only want archiving, you can remove this.
        // If you want both, keep it but make sure the UI calls the correct action.
        // [HttpPost, ActionName("Delete")]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> DeleteConfirmed(int id)
        // {
        //     var foForm = await _context.FoForms.FindAsync(id);
        //     if (foForm != null)
        //     {
        //         _context.FoForms.Remove(foForm);
        //         await _context.SaveChangesAsync();
        //         TempData["Success"] = "Form deleted successfully!";
        //     }
        //     return RedirectToAction(nameof(Index));
        // }

        [HttpGet]
        public async Task<IActionResult> GetFoFormDetailPartial(int index)
        {
            var newDetail = new FoFormDetailViewModel();

            // 1. Fetch ItemTypes directly and add to the controller's ViewData.
            // This data will then be automatically inherited by the partial view.
            ViewData["ItemTypes"] = await _context.TyItems
                                        .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.TyName })
                                        .ToListAsync();

            // 2. Add the 'Index' to the controller's ViewData as well, as the partial expects it.
            ViewData["Index"] = index;

            // 3. Now call PartialView with only two arguments: view name and model.
            // The ViewData (including "ItemTypes" and "Index") will be automatically available.
            return PartialView("_FoFormDetailPartial", newDetail);
        }

        // Add this new action method to your FoFormController.cs
        

        // Helper method to populate SelectListItems for the ViewModel's dropdown properties
        private async Task PopulateDropdowns(FoFormMasterViewModel viewModel)
        {
            viewModel.ComplianceList = await _context.AmCompliances
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.ComplianceName })
                .ToListAsync();

            viewModel.ItemTypes = await _context.TyItems
                .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.TyName })
                .ToListAsync();
        }

        private bool FoFormExists(int id)
        {
            return _context.FoForms.Any(e => e.Id == id);
        }
    }
}
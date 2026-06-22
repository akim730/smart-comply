// SmartComply.Controllers/AdminFilingController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartComply.data;
using SmartComply.Models;
using SmartComply.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace SmartComply.Controllers
{
    [Authorize(Roles = "Admin")] // Only Admins can access this controller
    public class AdminFilingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminFilingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AdminFiling/Index (List all Compliance Folders)
        public async Task<IActionResult> Index()
        {
            var folders = await _context.ComplianceFolders
                                        .Include(cf => cf.AmCompliance)
                                        .OrderBy(cf => cf.Name)
                                        .ToListAsync();
            return View(folders);
        }

        // GET: AdminFiling/CreateFolder
        public async Task<IActionResult> CreateFolder()
        {
            var complianceTypes = await _context.AmCompliances
                                                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.ComplianceName })
                                                .ToListAsync();
            var vm = new ComplianceFolderViewModel
            {
                ComplianceTypes = complianceTypes
            };
            return View(vm);
        }

        // POST: AdminFiling/CreateFolder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFolder(ComplianceFolderViewModel model)
        {
            if (ModelState.IsValid)
            {
                var folder = new ComplianceFolder
                {
                    Name = model.Name,
                    AmComplianceId = model.AmComplianceId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.ComplianceFolders.Add(folder);
                await _context.SaveChangesAsync();
                TempData["Message"] = $"Folder '{folder.Name}' created successfully.";
                return RedirectToAction(nameof(AddItemsToFolder), new { folderId = folder.Id });
            }

            // If validation fails, re-populate dropdown
            model.ComplianceTypes = await _context.AmCompliances
                                                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.ComplianceName })
                                                .ToListAsync();
            return View(model);
        }

        // GET: AdminFiling/FolderDetails/5 (View items in a specific folder)
        public async Task<IActionResult> FolderDetails(int id)
        {
            var folder = await _context.ComplianceFolders
                                        .Include(cf => cf.AmCompliance)
                                        .Include(cf => cf.FolderItems)
                                        .FirstOrDefaultAsync(cf => cf.Id == id);
            if (folder == null) return NotFound();

            ViewBag.ComplianceFolderName = folder.Name;
            ViewBag.ComplianceFolderId = folder.Id;
            return View(folder.FolderItems.OrderBy(fi => fi.Name).ToList());
        }

        [HttpGet]
        public async Task<IActionResult> AddItemsToFolder(int folderId)
        {
            var folder = await _context.ComplianceFolders
                                       .FirstOrDefaultAsync(f => f.Id == folderId);

            if (folder == null)
            {
                TempData["Error"] = "Compliance Folder not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new AddFolderItemsViewModel
            {
                ComplianceFolderId = folder.Id,
                ComplianceFolderName = folder.Name,
                NewItems = new List<FolderItemViewModel>
            {
                new FolderItemViewModel() // Start with one empty item
            }
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItemsToFolder(AddFolderItemsViewModel model)
        {
            // Debugging point: Check ModelState.IsValid here
            // If it's false, check ModelState.Values.Errors to see the specific error.
            // It's highly likely that this will now be true, or it will be false due to the FK,
            // which we are about to fix.

            if (ModelState.IsValid)
            {
                var folder = await _context.ComplianceFolders.FindAsync(model.ComplianceFolderId);
                if (folder == null)
                {
                    TempData["Error"] = "Target Compliance Folder not found for items.";
                    return RedirectToAction(nameof(Index)); // Or a more specific error page
                }

                foreach (var itemViewModel in model.NewItems)
                {
                    // **THIS IS THE CRUCIAL FIX:**
                    // Assign the ComplianceFolderId from the parent model to each itemViewModel
                    // This ensures the foreign key relationship is correctly set before saving.
                    itemViewModel.ComplianceFolderId = model.ComplianceFolderId; // <-- Add or verify this line

                    var folderItem = new FolderItem
                    {
                        Name = itemViewModel.Name,
                        Description = itemViewModel.Description,
                        IsRequired = itemViewModel.IsRequired,
                        // Use the now-corrected ID for the database entity:
                        ComplianceFolderId = itemViewModel.ComplianceFolderId,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _context.FolderItems.Add(folderItem);
                }

                await _context.SaveChangesAsync();
                TempData["Message"] = $"Items successfully added to {folder.Name} folder.";
                return RedirectToAction(nameof(FolderDetails), new { id = model.ComplianceFolderId });
            }

            // If ModelState is not valid, reload the view with the current model
            // You might need to re-populate ComplianceFolderName if it's null, or just handle it in the view.
            // For simplicity, just return the model. The view will display validation errors.
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItem(int itemId, int complianceFolderId)
        {
            var folderItem = await _context.FolderItems.FindAsync(itemId);
            if (folderItem == null)
            {
                TempData["Error"] = "Item not found.";
                return RedirectToAction(nameof(FolderDetails), new { id = complianceFolderId });
            }

            _context.FolderItems.Remove(folderItem);
            await _context.SaveChangesAsync();
            TempData["Message"] = $"Item '{folderItem.Name}' successfully deleted.";
            return RedirectToAction(nameof(FolderDetails), new { id = complianceFolderId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFolder(int id)
        {
            var folder = await _context.ComplianceFolders
                                       .Include(f => f.FolderItems) // Include related items for deletion
                                       .FirstOrDefaultAsync(f => f.Id == id);

            if (folder == null)
            {
                TempData["Error"] = "Compliance Folder not found.";
                return RedirectToAction(nameof(Index));
            }

            // Option 1: Manually delete child items first (if cascade delete is not configured or preferred)
            _context.FolderItems.RemoveRange(folder.FolderItems); // Deletes all associated FolderItems

            // Option 2: If your database has cascade delete configured for the relationship
            //           between ComplianceFolder and FolderItem, you might just need to delete
            //           the folder, and the items will be deleted automatically.
            //           However, explicitly deleting them ensures it, regardless of DB config.

            _context.ComplianceFolders.Remove(folder);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Folder '{folder.Name}' and its contents have been successfully deleted.";
            return RedirectToAction(nameof(Index));
        }

        // Other actions for editing/deleting folders and items could be added here
        // ... (e.g., EditFolder, DeleteFolder, EditItem, DeleteItem)
    }
}
// SmartComply.Controllers/FilingController.cs (MODIFIED for Manager functionality)
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartComply.data;
using SmartComply.Models;
using SmartComply.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering; // For SelectListItem
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace SmartComply.Controllers
{
    [Authorize(Roles = "Manager")] // Or "Admin,Manager" if admins can also upload
    public class FilingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public FilingController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: /Filing/Index (Now lists Compliance Folders for managers to browse)
        public async Task<IActionResult> Index()
        {
            var folders = await _context.ComplianceFolders
                                        .Include(cf => cf.AmCompliance)
                                        .OrderBy(cf => cf.Name)
                                        .ToListAsync();
            return View(folders); // Pass list of ComplianceFolders to the view
        }

        // GET: /Filing/ItemsInFolder/{folderId} (List FolderItems within a selected folder)
        public async Task<IActionResult> ItemsInFolder(int folderId)
        {
            var folder = await _context.ComplianceFolders
                                        .Include(cf => cf.FolderItems)
                                        .FirstOrDefaultAsync(cf => cf.Id == folderId);
            if (folder == null) return NotFound();

            // Populate each FolderItem with info about uploaded documents
            foreach (var item in folder.FolderItems)
            {
                item.FilingDocuments = await _context.FilingDocuments
                                                    .Where(fd => fd.FolderItemId == item.Id)
                                                    .OrderByDescending(fd => fd.UploadedAt)
                                                    .ToListAsync();
            }

            ViewBag.ComplianceFolderName = folder.Name;
            return View(folder.FolderItems.OrderBy(fi => fi.Name).ToList());
        }


        // GET: /Filing/Upload/{folderItemId}
        public async Task<IActionResult> Upload(int folderItemId)
        {
            var folderItem = await _context.FolderItems
                                           .Include(fi => fi.ComplianceFolder)
                                           .FirstOrDefaultAsync(fi => fi.Id == folderItemId);

            if (folderItem == null) return NotFound();

            var vm = new FilingUploadViewModel
            {
                FolderItemId = folderItem.Id,
                ComplianceFolderName = folderItem.ComplianceFolder.Name,
                FolderItemName = folderItem.Name,
                // If you want a dropdown of all items in the folder, populate AvailableFolderItems here.
                // Otherwise, the upload is specific to the passed folderItemId.
            };
            return View(vm);
        }

        // POST: /Filing/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(FilingUploadViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // If validation fails, re-populate any dropdowns
                var folderItem = await _context.FolderItems
                                               .Include(fi => fi.ComplianceFolder)
                                               .FirstOrDefaultAsync(fi => fi.Id == model.FolderItemId);
                if (folderItem != null)
                {
                    model.ComplianceFolderName = folderItem.ComplianceFolder.Name;
                    model.FolderItemName = folderItem.Name;
                }
                return View(model);
            }

            // Ensure the folderItem exists
            var targetFolderItem = await _context.FolderItems
                                     .Include(fi => fi.ComplianceFolder) // <<< Add this line to eager load ComplianceFolder
                                     .FirstOrDefaultAsync(fi => fi.Id == model.FolderItemId);
            if (targetFolderItem == null)
            {
                ModelState.AddModelError("", "Selected document type is invalid.");
                return View(model);
            }

            var fileName = Path.GetFileName(model.File.FileName);
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");

            // Create subfolder based on ComplianceFolder Name and/or FolderItem Name for better organization
            var folderPathSegment = targetFolderItem.ComplianceFolder.Name.Replace(" ", "_"); // Sanitize name for path
            var itemPathSegment = targetFolderItem.Name.Replace(" ", "_");
            var finalUploadPath = Path.Combine(uploadsFolder, folderPathSegment, itemPathSegment);

            Directory.CreateDirectory(finalUploadPath); // Ensure path exists

            var filePath = Path.Combine(finalUploadPath, fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.File.CopyToAsync(fileStream);
            }

            var doc = new FilingDocument
            {
                FolderItemId = model.FolderItemId, // Link to the specific FolderItem
                FileName = fileName,
                // Store relative path from web root
                FilePath = $"~/uploads/{folderPathSegment}/{itemPathSegment}/{fileName}", // Use ~ for web root relative path
                Tags = model.Tags,
                UploadedAt = DateTime.Now
            };

            _context.FilingDocuments.Add(doc);
            await _context.SaveChangesAsync();

            TempData["Message"] = "File uploaded successfully.";
            return RedirectToAction(nameof(ItemsInFolder), new { folderId = targetFolderItem.ComplianceFolderId });
        }


        // GET: /Filing/Download/{id} (Added for completeness to download uploaded files)
        public async Task<IActionResult> Download(int id)
        {
            var doc = await _context.FilingDocuments.FindAsync(id);
            if (doc == null) return NotFound();

            // Resolve the physical path from the stored relative path
            var relativePath = doc.FilePath.Replace("~/", ""); // Remove ~/
            var fullPath = Path.Combine(_env.WebRootPath, relativePath);

            if (!System.IO.File.Exists(fullPath)) return NotFound();

            var mimeType = GetMimeType(doc.FileName); // Helper function to get mime type

            return PhysicalFile(fullPath, mimeType, doc.FileName);
        }

        // Helper to determine mime type
        private string GetMimeType(string fileName)
        {
            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fileName, out var contentType))
            {
                contentType = "application/octet-stream"; // Default for unknown types
            }
            return contentType;
        }


        // GET: /Filing/Delete/17 (Modified to handle new structure and physical file deletion)
        public async Task<IActionResult> Delete(int id)
        {
            var doc = await _context.FilingDocuments.Include(fd => fd.FolderItem).FirstOrDefaultAsync(d => d.Id == id);
            if (doc == null) return NotFound();

            // Resolve the physical path
            var relativePath = doc.FilePath.Replace("~/", "");
            var filePath = Path.Combine(_env.WebRootPath, relativePath);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            int folderIdToRedirect = doc.FolderItem.ComplianceFolderId; // Get folder id for redirect

            _context.FilingDocuments.Remove(doc);
            await _context.SaveChangesAsync();

            TempData["Message"] = "File deleted successfully.";
            return RedirectToAction(nameof(ItemsInFolder), new { folderId = folderIdToRedirect });
        }
    }
}
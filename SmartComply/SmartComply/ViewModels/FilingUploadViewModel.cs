// SmartComply.ViewModels/FilingUploadViewModel.cs (MODIFIED)
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering; // For SelectListItem

namespace SmartComply.ViewModels
{
    public class FilingUploadViewModel
    {
        [Required(ErrorMessage = "Please select a folder item.")]
        [Display(Name = "Select Document Type")]
        public int FolderItemId { get; set; } // New FK to FolderItem

        [Display(Name = "Tags (comma-separated)")]
        public string? Tags { get; set; } // Made nullable as it's optional

        [Required(ErrorMessage = "Please select a file to upload.")]
        [Display(Name = "Upload Document")]
        public IFormFile File { get; set; }

        // For dropdown population (e.g., for selecting the FolderItem)
        public IEnumerable<SelectListItem>? AvailableFolderItems { get; set; }

        // Optional: to display context
        public string? ComplianceFolderName { get; set; }
        public string? FolderItemName { get; set; }
    }
}
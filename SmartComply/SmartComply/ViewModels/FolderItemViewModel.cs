// SmartComply.ViewModels/FolderItemViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace SmartComply.ViewModels
{
    public class FolderItemViewModel
    {
        public int Id { get; set; } // For editing existing items

        [Required(ErrorMessage = "Item Name is required.")]
        [StringLength(100, ErrorMessage = "Item Name cannot exceed 100 characters.")]
        [Display(Name = "Item Name")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        [Display(Name = "Description (e.g., 'Financial Statement for Q1')")]
        public string? Description { get; set; }

        [Display(Name = "Is Required")]
        public bool IsRequired { get; set; }

        public int ComplianceFolderId { get; set; } // Hidden field or from route
        public string? ComplianceFolderName { get; set; } // Display purposes
    }

    public class AddFolderItemsViewModel
    {
        public int ComplianceFolderId { get; set; }
        public string? ComplianceFolderName { get; set; }

        // This will hold multiple items to be added at once
        public List<FolderItemViewModel> NewItems { get; set; } = new List<FolderItemViewModel>();
    }
}
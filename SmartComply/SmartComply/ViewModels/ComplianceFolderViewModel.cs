// SmartComply.ViewModels/ComplianceFolderViewModel.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering; // For SelectListItem

namespace SmartComply.ViewModels
{
    public class ComplianceFolderViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Folder Name is required.")]
        [StringLength(100, ErrorMessage = "Folder Name cannot exceed 100 characters.")]
        [Display(Name = "Folder Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Compliance Type is required.")]
        [Display(Name = "Compliance Type")]
        public int AmComplianceId { get; set; } // For dropdown selection

        // For dropdown population
        public IEnumerable<SelectListItem>? ComplianceTypes { get; set; }
    }
}
// SmartComply.Models/ComplianceFolder.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SmartComply.Models
{
    public class ComplianceFolder
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Folder Name is required.")]
        [StringLength(100, ErrorMessage = "Folder Name cannot exceed 100 characters.")]
        [Display(Name = "Folder Name")]
        public string Name { get; set; }

        [Display(Name = "Compliance Type")]
        public int AmComplianceId { get; set; } // FK to AmCompliance
        public AmCompliance AmCompliance { get; set; } // Navigation property

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation property for items within this folder
        public ICollection<FolderItem> FolderItems { get; set; } = new List<FolderItem>();
    }
}
// SmartComply.Models/FolderItem.cs
using System.ComponentModel.DataAnnotations;

namespace SmartComply.Models
{
    public class FolderItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Item Name is required.")]
        [StringLength(100, ErrorMessage = "Item Name cannot exceed 100 characters.")]
        [Display(Name = "Item Name (e.g., 'Financial Statement', 'Audit Report')")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; } // Optional description for the item

        [Display(Name = "Is Required")]
        public bool IsRequired { get; set; } // e.g., true if this document must be uploaded

        // FK to ComplianceFolder
        public int ComplianceFolderId { get; set; }
        public ComplianceFolder ComplianceFolder { get; set; } // Navigation property

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation property for documents uploaded to this item
        public ICollection<FilingDocument> FilingDocuments { get; set; } = new List<FilingDocument>();
    }
}
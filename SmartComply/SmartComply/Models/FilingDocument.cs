// SmartComply.Models/FilingDocument.cs (MODIFIED)
using System.ComponentModel.DataAnnotations;

namespace SmartComply.Models
{
    public class FilingDocument
    {
        public int Id { get; set; }

        // REMOVED: public string FilingTitle { get; set; }

        public string Tags { get; set; } // Tags might still be useful per document

        [Required(ErrorMessage = "File Name is required.")]
        public string FileName { get; set; }

        [Required(ErrorMessage = "File Path is required.")]
        public string FilePath { get; set; } // Relative path to the uploaded file

        public DateTime UploadedAt { get; set; } = DateTime.Now;

        // NEW: FK to FolderItem
        [Display(Name = "Associated Item")]
        public int? FolderItemId { get; set; }
        public FolderItem? FolderItem { get; set; } // Navigation property
    }
}
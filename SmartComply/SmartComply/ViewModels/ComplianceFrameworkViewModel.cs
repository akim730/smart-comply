// ViewModels/ComplianceFrameworkViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace SmartComply.ViewModels
{
    public class ComplianceFrameworkViewModel
    {
        public int? Id { get; set; } // Nullable for Create, will be populated for Edit

        [Required(ErrorMessage = "Compliance Name is required.")]
        [StringLength(100, ErrorMessage = "Compliance Name cannot exceed 100 characters.")]
        [Display(Name = "Compliance Name")]
        public string ComplianceName { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        [Display(Name = "Description")]
        public string ComplianceDescription { get; set; }

        // We usually don't bind CreatedAt/UpdatedAt directly from the form for security/integrity.
        // These are typically set by the backend.
        // If you need to display them in the Edit view, you can add them, but don't bind them in HttpPost.
        // For simplicity in this ViewModel, we'll omit them from direct binding.
    }
}
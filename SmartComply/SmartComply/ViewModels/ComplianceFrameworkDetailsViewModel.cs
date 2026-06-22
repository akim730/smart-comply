// ViewModels/ComplianceFrameworkDetailsViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace SmartComply.ViewModels
{
    public class ComplianceFrameworkDetailsViewModel
    {
        [Display(Name = "ID")]
        public int Id { get; set; }

        [Display(Name = "Compliance Name")]
        public string ComplianceName { get; set; }

        [Display(Name = "Description")]
        public string ComplianceDescription { get; set; }

        [Display(Name = "Created At")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Last Updated At")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime UpdatedAt { get; set; }
    }
}
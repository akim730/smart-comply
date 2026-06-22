// ViewModels/ComplianceFrameworkListItemViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace SmartComply.ViewModels
{
    public class ComplianceFrameworkListItemViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Compliance Name")]
        public string ComplianceName { get; set; }

        [Display(Name = "Description")]
        public string ComplianceDescription { get; set; }
    }
}
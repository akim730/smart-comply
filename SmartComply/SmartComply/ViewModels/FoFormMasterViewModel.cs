using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering; // For SelectListItem
using System.Collections.Generic; // For List

namespace SmartComply.Models.ViewModels
{
    public class FoFormMasterViewModel
    {
        public int Id { get; set; } // Used for identifying the master FoForm in Edit scenarios

        [Required(ErrorMessage = "Form Title is required.")]
        [StringLength(255, ErrorMessage = "Form Title cannot exceed 255 characters.")]
        [Display(Name = "Form Title")]
        public string FoName { get; set; }

        [Required(ErrorMessage = "Please select a Compliance.")]
        [Display(Name = "Compliance")]
        public int? FoCid { get; set; }

        // NEW: This list will hold all the detail items submitted by the form
        public List<FoFormDetailViewModel> Details { get; set; } = new List<FoFormDetailViewModel>();

        // Properties for dropdowns (shared by both master and detail items if applicable)
        public IEnumerable<SelectListItem> ComplianceList { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> ItemTypes { get; set; } = new List<SelectListItem>(); // For the detail item dropdowns


    }
}
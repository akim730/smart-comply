using System.ComponentModel.DataAnnotations;

namespace SmartComply.Models.ViewModels
{
    public class FoFormDetailViewModel
    {
        public int Id { get; set; } // Will be 0 for new details, non-zero for existing in Edit mode

        [Required(ErrorMessage = "Section Name is required.")]
        [StringLength(255, ErrorMessage = "Section Name cannot exceed 255 characters.")]
        [Display(Name = "Section Name")]
        public string SecName { get; set; }

        [Required(ErrorMessage = "Item Name is required.")]
        [StringLength(255, ErrorMessage = "Item Name cannot exceed 255 characters.")]
        [Display(Name = "Item Name")]
        public string ItemName { get; set; }

        [Required(ErrorMessage = "Please select an Item Type.")]
        [Display(Name = "Item Type")]
        public int? TyItemId { get; set; } // Matches the FK in FoFormDetail


    }
}
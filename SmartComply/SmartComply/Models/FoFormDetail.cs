using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // For ForeignKey attribute

namespace SmartComply.Models
{
    public class FoFormDetail
    {
        public int Id { get; set; } // Primary Key for the detail item

        [Required(ErrorMessage = "Section Name is required.")]
        [StringLength(255, ErrorMessage = "Section Name cannot exceed 255 characters.")]
        public string SecName { get; set; } // Renamed from FoSecName to avoid confusion with FoForm property

        [Required(ErrorMessage = "Item Name is required.")]
        [StringLength(255, ErrorMessage = "Item Name cannot exceed 255 characters.")]
        public string ItemName { get; set; } // Renamed from FoItemName

        // Foreign Key to TyItem (the lookup table for item types)
        [Required(ErrorMessage = "Item Type is required.")]
        [ForeignKey("TyItem")]
        public int TyItemId { get; set; }
        public virtual TyItem TyItem { get; set; } // Navigation property to TyItem

        // Foreign Key to the parent FoForm
        [Required]
        public int FoFormId { get; set; } // The ID of the parent FoForm
        public virtual FoForm FoForm { get; set; } // Navigation property to the parent FoForm

        public string ItemValue { get; set; }

        public FoFormDetail()
        {
            ItemValue = string.Empty; // Initialize to an empty string by default
        }
    }
}
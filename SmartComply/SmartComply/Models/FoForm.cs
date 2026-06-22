using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic; // For ICollection

namespace SmartComply.Models
{
    public class FoForm
    {
        public int Id { get; set; } // Primary Key

        [Required]
        public string FoName { get; set; } // The "Title"

        [Required]
        [ForeignKey("AmCompliance")]
        public int FoCid { get; set; }
        public virtual AmCompliance AmCompliance { get; set; } // Navigation property

        public DateTime FoDatetime { get; set; } = DateTime.Now;

        public bool isArchived { get; set; } = false;

        // REMOVE these properties from FoForm:
        // public string FoSecName { get; set; }
        // public string FoItemName { get; set; }
        // public int? FoTypeName { get; set; } // Or TyItem TyItem

        // NEW: Collection of detail items
        public ICollection<FoFormDetail> Details { get; set; } = new List<FoFormDetail>();

        // --- ADD THIS LINE ---
        public ICollection<Audit> Audits { get; set; } = new List<Audit>(); // Navigation to Audits
        // --- END ADD ---
    }
}
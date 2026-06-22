using System.ComponentModel.DataAnnotations;
using System.Collections.Generic; // For ICollection

namespace SmartComply.Models
{
    public class TyItem
    {
        [Key]
        public int Id { get; set; } // PK
        public string TyName { get; set; }
        public string TyHtmlType { get; set; }
        public string TyValidate { get; set; }

        // --- UPDATED: Navigation property to FoFormItem ---
        public ICollection<FoForm> FoForms { get; set; }  // Navigation
    }
}
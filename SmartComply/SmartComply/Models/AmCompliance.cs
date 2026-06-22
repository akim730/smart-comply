using System.ComponentModel.DataAnnotations;

namespace SmartComply.Models
{
    public class AmCompliance
    {
        public int Id { get; set; }  // PK
        public string ComplianceName { get; set; }
        public string ComplianceDescription { get; set; }

        // Navigation properties
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public ICollection<FoForm> FoForms { get; set; }  // Navigation

        // --- ADD THIS LINE ---
        public ICollection<Audit> Audits { get; set; } = new List<Audit>(); // Navigation to Audits
                                                                            // --- END ADD ---


    }




}

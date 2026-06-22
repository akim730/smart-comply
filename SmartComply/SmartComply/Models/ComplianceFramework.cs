using System;
using System.ComponentModel.DataAnnotations;

namespace SmartComply.Models
{
    public class ComplianceFramework
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartComply.Models
{
    // Main Audit Table
    public class Audit
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public required string AuditTitle { get; set; }
        
        [Required]
        public required string AuditDescription { get; set; }
        
        [Required]
        public int ComplianceId { get; set; } // Foreign key to am_compliance
        
        [Required]
        public int FormId { get; set; } // Foreign key to fo_form
        
        [Required]
        [StringLength(50)]
        public required string AuditStatus { get; set; } // Pending, InProgress, Completed, RequiresCorrection
        
        [Required]
        public DateTime AuditDate { get; set; }
        
        public DateTime? CompletedDate { get; set; }
        
        [Required]
        [StringLength(450)]
        public required string AuditorId { get; set; } // Foreign key to AspNetUsers
          [StringLength(450)]
        public string? AuditeeId { get; set; } // Foreign key to AspNetUsers
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal? AuditScore { get; set; }
        
        public string? AuditNotes { get; set; }
        
        public string? AuditFindings { get; set; }
          [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }

        // Manager Verification Properties
        [StringLength(50)]
        public string? VerificationStatus { get; set; } = "Pending"; // Pending, Approved, Rejected, RequiresRevision
        
        [StringLength(450)]
        public string? VerifiedById { get; set; } // Manager who verified
        
        public DateTime? VerifiedAt { get; set; }
        
        public string? VerificationComments { get; set; }
        
        public bool RequiresManagerApproval { get; set; } = true;
        
        // Navigation properties
        public virtual AmCompliance? Compliance { get; set; }
        public virtual FoForm? Form { get; set; }
        public virtual ICollection<CorrectivePlan> CorrectivePlans { get; set; } = new List<CorrectivePlan>();
        public virtual ICollection<FollowUpAudit> FollowUpAudits { get; set; } = new List<FollowUpAudit>();
    }

    // Corrective Action Plan Table
    public class CorrectivePlan
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int AuditId { get; set; } // Foreign key to Audit
        
        [Required]
        [StringLength(200)]
        public required string IssueIdentified { get; set; }
        
        [Required]
        public required string CorrectiveAction { get; set; }
        
        [Required]
        [StringLength(450)]
        public required string ResponsiblePersonId { get; set; } // Foreign key to AspNetUsers
        
        [Required]
        public DateTime TargetCompletionDate { get; set; }
        
        public DateTime? ActualCompletionDate { get; set; }
        
        [Required]
        [StringLength(50)]
        public required string Status { get; set; } // Pending, InProgress, Completed, Overdue
        
        [Required]
        [StringLength(20)]
        public required string Priority { get; set; } // High, Medium, Low
        
        public string? ProgressNotes { get; set; }
          [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Manager Approval Properties
        [StringLength(50)]
        public string? ApprovalStatus { get; set; } // Pending, Approved, Rejected
        
        [StringLength(450)]
        public string? ApprovedById { get; set; } // Manager who approved
        
        public DateTime? ApprovedAt { get; set; }
        
        public string? ApprovalComments { get; set; }
        
        // Navigation properties
        public virtual Audit? Audit { get; set; }
    }

    // Follow-up Audit Table
    public class FollowUpAudit
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int OriginalAuditId { get; set; } // Foreign key to original Audit
        
        [Required]
        [StringLength(100)]
        public required string FollowUpTitle { get; set; }
        
        public string? FollowUpDescription { get; set; }
        
        [Required]
        public DateTime ScheduledDate { get; set; }
        
        public DateTime? ConductedDate { get; set; }
        
        [Required]
        [StringLength(450)]
        public required string AuditorId { get; set; } // Foreign key to AspNetUsers
        
        [Required]
        [StringLength(50)]
        public required string Status { get; set; } // Scheduled, Completed, Cancelled
        
        public string? Findings { get; set; }
        
        public string? Recommendations { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual Audit? OriginalAudit { get; set; }
    }    // Compliance Table
    

    // Audit Form Response (for storing audit responses)
    public class AuditResponse
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int AuditId { get; set; }
        
        [Required]
        [StringLength(100)]
        public required string QuestionId { get; set; }
        
        [Required]
        public required string QuestionText { get; set; }
        
        public string? ResponseValue { get; set; }
        
        public string? ResponseNotes { get; set; }
        
        public bool? IsCompliant { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual Audit? Audit { get; set; }
    }
}
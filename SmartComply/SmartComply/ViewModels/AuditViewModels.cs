using Microsoft.AspNetCore.Identity;
using SmartComply.Models;
using System.ComponentModel.DataAnnotations;

namespace SmartComply.ViewModels
{
    public class AuditDashboardViewModel
    {
        public List<Audit> Audits { get; set; } = new List<Audit>();
        public int TotalAudits { get; set; }
        public int PendingAudits { get; set; }
        public int InProgressAudits { get; set; }
        public int CompletedAudits { get; set; }
        public int RequiresCorrection { get; set; }
    }    public class CreateAuditViewModel
    {
        public bool IsEdit { get; set; } = false;
        public int? Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Audit Title")]
        public required string AuditTitle { get; set; }

        [Required]
        [Display(Name = "Audit Description")]
        public required string AuditDescription { get; set; }

        [Required]
        [Display(Name = "Compliance")]
        public int ComplianceId { get; set; }

        [Required]
        [Display(Name = "Audit Form")]
        public int FormId { get; set; }

        [Required]
        [Display(Name = "Audit Date")]
        public DateTime AuditDate { get; set; } = DateTime.Today.AddDays(1);

        [Display(Name = "Auditee")]
        public string? AuditeeId { get; set; }

        [Display(Name = "Audit Status")]
        public string AuditStatus { get; set; } = "Pending";

        [Display(Name = "Notes")]
        public string? AuditNotes { get; set; }

        // For dropdowns
        public List<AmCompliance> Compliances { get; set; } = new List<AmCompliance>();
        public List<FoForm> Forms { get; set; } = new List<FoForm>();
        public List<IdentityUser> Users { get; set; } = new List<IdentityUser>();
    }

    public class ConductAuditViewModel
    {
        public Audit Audit { get; set; }
        public IEnumerable<AuditResponse> ExistingResponses { get; set; } = new List<AuditResponse>();

        // Add these properties to hold the manually fetched User objects
        // Replace 'ApplicationUser' with the actual name of your User model (e.g., IdentityUser, AppUser)
        public IdentityUser AuditeeUser { get; set; }
        public IdentityUser AuditorUser { get; set; }
    }

    public class AuditResponseInput
    {
        public string QuestionId { get; set; } = "";
        public string QuestionText { get; set; } = "";
        public string ResponseValue { get; set; } = "";
        public string? ResponseNotes { get; set; }
        public bool? IsCompliant { get; set; }
    }

    public class AuditDetailsViewModel
    {
        public Audit Audit { get; set; } = new Audit 
        { 
            AuditTitle = "",
            AuditDescription = "",
            AuditStatus = "",
            AuditorId = ""
        };
        public List<AuditResponse> Responses { get; set; } = new List<AuditResponse>();
    }

    public class AuditResponseDto
    {
        public string QuestionId { get; set; } = "";
        public string QuestionText { get; set; } = "";
        public string? ResponseValue { get; set; }
        public string? ResponseNotes { get; set; }
        public bool? IsCompliant { get; set; }
    }    public class CorrectivePlanViewModel
    {
        [Required]
        public int AuditId { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Issue Identified")]
        public string IssueIdentified { get; set; } = "";

        [Required]
        [Display(Name = "Corrective Action")]
        public string CorrectiveAction { get; set; } = "";

        [Required]
        [Display(Name = "Responsible Person")]
        public string ResponsiblePersonId { get; set; } = "";

        [Required]
        [Display(Name = "Target Completion Date")]
        public DateTime TargetCompletionDate { get; set; } = DateTime.Today.AddDays(30);

        [Required]
        [Display(Name = "Priority")]
        public string Priority { get; set; } = "Medium";

        [Display(Name = "Progress Notes")]
        public string? ProgressNotes { get; set; }

        // For dropdown
        public List<IdentityUser> Users { get; set; } = new List<IdentityUser>();
    }

    public class FollowUpAuditViewModel
    {
        [Required]
        public int OriginalAuditId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Follow-up Title")]
        public string FollowUpTitle { get; set; } = "";

        [Display(Name = "Description")]
        public string? FollowUpDescription { get; set; }

        [Required]
        [Display(Name = "Scheduled Date")]
        public DateTime ScheduledDate { get; set; } = DateTime.Today.AddDays(7);

        [Required]
        [Display(Name = "Auditor")]        public string AuditorId { get; set; } = "";

        // For dropdown
        public List<IdentityUser> Users { get; set; } = new List<IdentityUser>();
        public Audit? OriginalAudit { get; set; }
    }

    // Manager Dashboard ViewModel
    public class ManagerDashboardViewModel
    {
        public List<Audit> Audits { get; set; } = new List<Audit>();
        public List<Audit> AuditsRequiringVerification { get; set; } = new List<Audit>();
        public List<Audit> AllSubmittedAudits { get; set; } = new List<Audit>();
        public List<CorrectivePlan> CorrectivePlansRequiringApproval { get; set; } = new List<CorrectivePlan>();
        public int PendingVerificationCount { get; set; }
        public int PendingCorrectivePlanApprovalCount { get; set; }
        public int TotalSubmittedAudits => AllSubmittedAudits.Count;
        public int ApprovedAudits => AllSubmittedAudits.Count(a => a.VerificationStatus == "Approved");
        public int RejectedAudits => AllSubmittedAudits.Count(a => a.VerificationStatus == "Rejected");

        public int TotalForms { get; set; }
        public int TotalCompliance { get; set; }

        // FIX: Initialize these dictionary properties
        public Dictionary<string, int> FormsPerCompliance { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> FormsPerInputType { get; set; } = new Dictionary<string, int>(); // Initialize this one too if you use it in other views/logic
        public DateTime? LatestFormDate { get; set; }
        public int FormsToday { get; set; }
        public int FormsThisMonth { get; set; }

        // FIX: Initialize this dictionary property
        public Dictionary<string, int> FormsPerMonth { get; set; } = new Dictionary<string, int>();
        

    }

    public class AuditVerificationViewModel
    {
        public Audit Audit { get; set; } = new Audit 
        { 
            AuditTitle = "",
            AuditDescription = "",
            AuditStatus = "",
            AuditorId = ""
        };
        public List<AuditResponse> AuditResponses { get; set; } = new List<AuditResponse>();
        public string VerificationStatus { get; set; } = "Pending";
        public string? VerificationComments { get; set; }
        
        [Display(Name = "Verification Decision")]
        public string NewVerificationStatus { get; set; } = "Pending";
        
        [Display(Name = "Comments")]
        public string? NewVerificationComments { get; set; }
    }    // Corrective Plan Approval ViewModel
    public class CorrectivePlanApprovalViewModel
    {
        public CorrectivePlan CorrectivePlan { get; set; } = new CorrectivePlan 
        { 
            IssueIdentified = "",
            CorrectiveAction = "",
            ResponsiblePersonId = "",
            Status = "",
            Priority = ""
        };
        public string ApprovalStatus { get; set; } = "Pending";
        public string? ApprovalComments { get; set; }
        
        [Display(Name = "Approval Decision")]
        public string NewApprovalStatus { get; set; } = "Pending";
        
        [Display(Name = "Comments")]
        public string? NewApprovalComments { get; set; }
    }
}

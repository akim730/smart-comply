using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartComply.data;
using SmartComply.Models;
using SmartComply.ViewModels;

namespace SmartComply.Controllers
{
    [Authorize]
    public class AuditController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AuditController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Auditor")]
        // GET: Audit Dashboard
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userRoles = await _userManager.GetRolesAsync(currentUser);

            IQueryable<Audit> auditsQuery = _context.Audits
                .Include(a => a.Compliance)
                .Include(a => a.Form);

            // Filter based on user role
            if (!userRoles.Contains("superAdmin") && !userRoles.Contains("Admin") && !userRoles.Contains("Manager"))
            {
                // Regular users only see audits they're involved in
                auditsQuery = auditsQuery.Where(a => a.AuditorId == currentUser.Id || a.AuditeeId == currentUser.Id);
            }

            var audits = await auditsQuery
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            var viewModel = new AuditDashboardViewModel
            {
                Audits = audits,
                TotalAudits = audits.Count,
                PendingAudits = audits.Count(a => a.AuditStatus == "Pending"),
                InProgressAudits = audits.Count(a => a.AuditStatus == "InProgress"),
                CompletedAudits = audits.Count(a => a.AuditStatus == "Completed"),
                RequiresCorrection = audits.Count(a => a.AuditStatus == "RequiresCorrection")
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Auditor")]
        // GET: Create Audit
        public async Task<IActionResult> Create()
        {
            var viewModel = new CreateAuditViewModel
            {
                AuditTitle = "",
                AuditDescription = "",
                // REMOVED .Where(c => c.Status == "Active") because AmCompliance has no 'Status'
                Compliances = await _context.AmCompliances.ToListAsync(),
                // Using !f.isArchived for FoForm as it has an 'isArchived' property
                Forms = await _context.FoForms.Where(f => !f.isArchived).ToListAsync(),
                Users = await _userManager.Users.ToListAsync()
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Auditor")]
        // POST: Create Audit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAuditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);

                var audit = new Audit
                {
                    AuditTitle = model.AuditTitle,
                    AuditDescription = model.AuditDescription,
                    ComplianceId = model.ComplianceId,
                    FormId = model.FormId,
                    AuditStatus = model.AuditStatus,
                    AuditDate = model.AuditDate,
                    AuditorId = currentUser?.Id ?? "",
                    AuditeeId = model.AuditeeId,
                    AuditNotes = model.AuditNotes,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Audits.Add(audit);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Audit created successfully!";
                return RedirectToAction(nameof(Index));
            }

            // Reload dropdown data if validation fails
            // REMOVED .Where(c => c.Status == "Active")
            model.Compliances = await _context.AmCompliances.ToListAsync();
            // Using !f.isArchived for FoForm
            model.Forms = await _context.FoForms.Where(f => !f.isArchived).ToListAsync();
            model.Users = await _userManager.Users.ToListAsync();

            return View(model);
        }

        [Authorize(Roles = "Auditor")]
        // GET: Edit Audit
        public async Task<IActionResult> Edit(int id)
        {
            var audit = await _context.Audits.FindAsync(id);
            if (audit == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser ?? new IdentityUser());

            // Check if user has permission to edit this audit
            if (!userRoles.Contains("superAdmin") && !userRoles.Contains("Admin") &&
                audit.AuditorId != currentUser?.Id)
            {
                return Forbid();
            }

            var viewModel = new CreateAuditViewModel
            {
                IsEdit = true,
                Id = audit.Id,
                AuditTitle = audit.AuditTitle,
                AuditDescription = audit.AuditDescription,
                ComplianceId = audit.ComplianceId,
                FormId = audit.FormId,
                AuditDate = audit.AuditDate,
                AuditeeId = audit.AuditeeId,
                AuditStatus = audit.AuditStatus,
                AuditNotes = audit.AuditNotes,
                // REMOVED .Where(c => c.Status == "Active")
                Compliances = await _context.AmCompliances.ToListAsync(),
                // Using !f.isArchived for FoForm
                Forms = await _context.FoForms.Where(f => !f.isArchived).ToListAsync(),
                Users = await _userManager.Users.ToListAsync()
            };

            return View("Create", viewModel);
        }

        [Authorize(Roles = "Auditor")]
        // POST: Edit Audit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CreateAuditViewModel model)
        {
            if (!model.Id.HasValue)
            {
                return BadRequest();
            }

            var audit = await _context.Audits.FindAsync(model.Id.Value);
            if (audit == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser ?? new IdentityUser());

            // Check if user has permission to edit this audit
            if (!userRoles.Contains("superAdmin") && !userRoles.Contains("Admin") &&
                audit.AuditorId != currentUser?.Id)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                audit.AuditTitle = model.AuditTitle;
                audit.AuditDescription = model.AuditDescription;
                audit.ComplianceId = model.ComplianceId;
                audit.FormId = model.FormId;
                audit.AuditDate = model.AuditDate;
                audit.AuditeeId = model.AuditeeId;
                audit.AuditStatus = model.AuditStatus;
                audit.AuditNotes = model.AuditNotes;
                audit.UpdatedAt = DateTime.UtcNow;

                _context.Update(audit);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Audit updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            // Reload dropdown data if validation fails
            model.IsEdit = true;
            model.Id = audit.Id;
            // REMOVED .Where(c => c.Status == "Active")
            model.Compliances = await _context.AmCompliances.ToListAsync();
            // Using !f.isArchived for FoForm
            model.Forms = await _context.FoForms.Where(f => !f.isArchived).ToListAsync();
            model.Users = await _userManager.Users.ToListAsync();

            return View("Create", model);
        }

        [Authorize(Roles = "Auditor")]
        // GET: Conduct Audit
        public async Task<IActionResult> Conduct(int id)
        {
            // 1. Load the Audit with its direct relationships (Compliance, Form, Form.Details, TyItem)
            // We still need to include Form.Details and TyItem for questions to show up.
            var audit = await _context.Audits
                .Include(a => a.Compliance) // Still needed for audit info display
                .Include(a => a.Form)       // Still needed for questions
                    .ThenInclude(f => f.Details)
                        .ThenInclude(fd => fd.TyItem) // Essential for conditional rendering in view
                .FirstOrDefaultAsync(a => a.Id == id);

            if (audit == null)
            {
                return NotFound();
            }

            // 2. Manually fetch Auditee and Auditor User objects using their IDs
            // Assuming AuditeeId and AuditorId properties exist as strings or ints on your Audit model
            // And assuming _userManager can find users by ID.
            IdentityUser auditeeUser = null;
            if (!string.IsNullOrEmpty(audit.AuditeeId)) // Or check for audit.AuditeeId != 0 if int
            {
                auditeeUser = await _userManager.FindByIdAsync(audit.AuditeeId);
            }

            IdentityUser auditorUser = null;
            if (!string.IsNullOrEmpty(audit.AuditorId)) // Or check for audit.AuditorId != 0 if int
            {
                auditorUser = await _userManager.FindByIdAsync(audit.AuditorId);
            }

            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account"); // Redirect to login if user is not authenticated
            }

            var userRoles = await _userManager.GetRolesAsync(currentUser);

            // Check if user has permission to conduct this audit
            if (!userRoles.Contains("superAdmin") && !userRoles.Contains("Admin") &&
                audit.AuditorId != currentUser.Id)
            {
                return Forbid();
            }

            var existingResponses = await _context.AuditResponses
                .Where(ar => ar.AuditId == id)
                .ToListAsync();

            var viewModel = new ConductAuditViewModel
            {
                Audit = audit,
                ExistingResponses = existingResponses,
                AuditeeUser = auditeeUser, // Pass the fetched Auditee User to the ViewModel
                AuditorUser = auditorUser  // Pass the fetched Auditor User to the ViewModel
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Auditor")]
        // POST: Submit Audit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitAudit(int AuditId, List<AuditResponseDto> Responses, decimal? AuditScore, string AuditStatus, string? AuditNotes, string? AuditFindings, string submitType)
        {
            var audit = await _context.Audits.FindAsync(AuditId);
            if (audit == null)
            {
                return NotFound();
            }

            try
            {
                // Remove existing responses
                var existingResponses = _context.AuditResponses.Where(ar => ar.AuditId == AuditId);
                _context.AuditResponses.RemoveRange(existingResponses);

                // Add new responses
                if (Responses != null)
                {
                    foreach (var response in Responses)
                    {
                        if (!string.IsNullOrEmpty(response.QuestionId))
                        {
                            var auditResponse = new AuditResponse
                            {
                                AuditId = AuditId,
                                QuestionId = response.QuestionId,
                                QuestionText = response.QuestionText,
                                ResponseValue = response.ResponseValue,
                                ResponseNotes = response.ResponseNotes,
                                IsCompliant = response.IsCompliant
                            };
                            _context.AuditResponses.Add(auditResponse);
                        }
                    }
                }

                // Update audit
                audit.AuditScore = AuditScore;
                audit.AuditStatus = submitType == "complete" ? "Completed" : AuditStatus;
                audit.AuditNotes = AuditNotes;
                audit.AuditFindings = AuditFindings;
                audit.UpdatedAt = DateTime.UtcNow;

                if (submitType == "complete")
                {
                    audit.CompletedDate = DateTime.UtcNow;
                }

                _context.Update(audit);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = submitType == "complete" ? "Audit completed successfully!" : "Audit progress saved!";
                return RedirectToAction(nameof(Details), new { id = AuditId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error saving audit: " + ex.Message;
                return RedirectToAction(nameof(Conduct), new { id = AuditId });
            }
        }

        [Authorize(Roles = "Auditor")]
        // GET: Audit Details
        public async Task<IActionResult> Details(int id)
        {
            var audit = await _context.Audits
                .Include(a => a.Compliance)
                .Include(a => a.Form)
                .Include(a => a.CorrectivePlans)
                .Include(a => a.FollowUpAudits)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (audit == null)
            {
                return NotFound();
            }

            var responses = await _context.AuditResponses
                .Where(ar => ar.AuditId == id)
                .ToListAsync();

            var viewModel = new AuditDetailsViewModel
            {
                Audit = audit,
                Responses = responses
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Auditor")]
        // GET: Corrective Plans
        public async Task<IActionResult> CorrectivePlans()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser ?? new IdentityUser());

            IQueryable<CorrectivePlan> plansQuery = _context.CorrectivePlans
                .Include(cp => cp.Audit);

            // Filter based on user role
            if (!userRoles.Contains("superAdmin") && !userRoles.Contains("Admin"))
            {
                plansQuery = plansQuery.Where(cp => cp.ResponsiblePersonId == currentUser!.Id);
            }

            var plans = await plansQuery
                .OrderBy(cp => cp.TargetCompletionDate)
                .ToListAsync();

            return View(plans);
        }

        [Authorize(Roles = "Auditor")]
        // GET: Follow-up Audits
        public async Task<IActionResult> FollowUpAudits()
        {
            var followUps = await _context.FollowUpAudits
                .Include(fu => fu.OriginalAudit)
                .OrderBy(fu => fu.ScheduledDate)
                .ToListAsync();

            return View(followUps);
        }

        // AJAX: Get Form Preview
        public async Task<IActionResult> GetFormPreview(int formId)
        {
            var form = await _context.FoForms.FindAsync(formId);
            if (form == null)
            {
                return PartialView("_FormPreview", null);
            }

            // Using 'FoName' as it's the only relevant property available in FoForm
            // and you want to avoid model changes.
            var previewData = new
            {
                FormName = form.FoName // Use the correct property name from FoForm
                // FormDescription and FormType are not in your FoForm model.
                // If needed, they must be added to the model and a migration run.
            };
            return PartialView("_FormPreview", previewData);
        }

        // MANAGER VERIFICATION FEATURES

        [Authorize(Roles = "Manager")]
        // GET: Manager Dashboard - Audits requiring verification
        public async Task<IActionResult> ManagerDashboard()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser ?? new IdentityUser());

            // Get form data for charts
            var foForms = await _context.FoForms
                .Include(f => f.AmCompliance)
                .Include(f => f.Details)
                    .ThenInclude(d => d.TyItem)
                .ToListAsync();
            var formDetails = foForms.SelectMany(f => f.Details).ToList();

            // Get audits requiring verification
            var auditsRequiringVerification = await _context.Audits
                .Include(a => a.Compliance)
                .Include(a => a.Form)
                .Where(a => (a.VerificationStatus == null || a.VerificationStatus == "Pending") &&
                            a.RequiresManagerApproval)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            // Get all submitted audits
            var allSubmittedAudits = await _context.Audits
                .Include(a => a.Compliance)
                .Include(a => a.Form)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            // Get corrective plans requiring approval
            var correctivePlansRequiringApproval = await _context.CorrectivePlans
                .Include(cp => cp.Audit)
                .Where(cp => cp.ApprovalStatus == null || cp.ApprovalStatus == "Pending")
                .OrderByDescending(cp => cp.CreatedAt)
                .ToListAsync();

            var viewModel = new ManagerDashboardViewModel
            {
                AuditsRequiringVerification = auditsRequiringVerification,
                AllSubmittedAudits = allSubmittedAudits,
                CorrectivePlansRequiringApproval = correctivePlansRequiringApproval,
                PendingVerificationCount = auditsRequiringVerification.Count,
                PendingCorrectivePlanApprovalCount = correctivePlansRequiringApproval.Count,
                // Add form-related data
                TotalForms = foForms.Count,
                TotalCompliance = await _context.AmCompliances.CountAsync(),
                FormsPerCompliance = foForms
                    .GroupBy(f => f.AmCompliance?.ComplianceName ?? "Unknown")
                    .ToDictionary(g => g.Key, g => g.Count()),
                FormsPerMonth = Enumerable.Range(1, 12).ToDictionary(
                    month => new DateTime(1, month, 1).ToString("MMM"),
                    month => foForms.Count(f => f.FoDatetime.Month == month && f.FoDatetime.Year == DateTime.Now.Year)
                )
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Manager")]
        // GET: Audit Verification Details
        //[Authorize(Roles = "superAdmin,Admin,Manager")]
        public async Task<IActionResult> VerifyAudit(int id)
        {
            var audit = await _context.Audits
                .Include(a => a.Compliance)
                .Include(a => a.Form)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (audit == null)
            {
                return NotFound();
            }

            // Get audit responses
            var responses = await _context.AuditResponses
                .Where(ar => ar.AuditId == id)
                .ToListAsync();

            var viewModel = new AuditVerificationViewModel
            {
                Audit = audit,
                AuditResponses = responses,
                VerificationStatus = audit.VerificationStatus ?? "Pending",
                VerificationComments = audit.VerificationComments
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Manager")]
        // POST: Submit Audit Verification
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitVerification(AuditVerificationViewModel model) // <--- Change this!
        {
            // Now you access the data through the 'model' parameter
            int auditId = model.Audit.Id; // This will now correctly get the ID from the submitted form
            string verificationComments = model.NewVerificationComments; // Or model.VerificationComments, depending on which field you intend to use
            string verificationStatus = model.NewVerificationStatus;   // Or model.VerificationStatus

            var audit = await _context.Audits.FindAsync(auditId);

            if (audit == null)
            {
                return NotFound(); // Still possible if ID is tampered with or DB record is deleted
            }

            // Apply the verification details
            audit.VerificationStatus = verificationStatus;
            audit.VerificationComments = verificationComments;
            audit.VerifiedAt = DateTime.UtcNow; // Or DateTime.Now, depending on your timezone strategy

            _context.Update(audit);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Audit verification updated successfully.";
            return RedirectToAction(nameof(VerifyAudit), new { id = auditId }); // Redirect to Audit Details or another appropriate page
        }

        [Authorize(Roles = "Manager")]
        // GET: All Submitted Audits (Manager View)
        //[Authorize(Roles = "superAdmin,Admin,Manager")]
        public async Task<IActionResult> SubmittedAudits()
        {
            var submittedAudits = await _context.Audits
                .Include(a => a.Compliance)
                .Include(a => a.Form)
                .Where(a => a.AuditStatus == "Completed" || a.AuditStatus == "Approved" || a.AuditStatus == "RequiresRevision")
                .OrderByDescending(a => a.CompletedDate)
                .ToListAsync();

            return View(submittedAudits);
        }

        [Authorize(Roles = "Auditor")]
        [HttpGet]
        public async Task<IActionResult> GetFormsByCompliance(int complianceId)
        {
            // You'll need to establish a relationship between AmCompliance and FoForm
            // if one doesn't exist, or query based on a linking table/property.
            // For this example, I'll assume FoForm has an AmComplianceId (foreign key).
            // If not, you'll need to adjust your models and database.

            // If FoForm is directly related to Compliance (e.g., FoForm has a ComplianceId)
            var forms = await _context.FoForms
                                      .Where(f => !f.isArchived && f.FoCid == complianceId) // Assuming FoForm has a ComplianceId
                                      .Select(f => new SelectListItem
                                      {
                                          Value = f.Id.ToString(),
                                          Text = f.FoName
                                      })
                                      .ToListAsync();

            // If there's no direct relationship, you need to define how FoForm is linked to Compliance.
            // For instance, if FoForm stores a "compliance type name" that matches AmCompliance.ComplianceName,
            // you'd query like this (less ideal due to string comparison, but works if no FK):
            /*
            var complianceName = await _context.AmCompliances
                                                .Where(c => c.Id == complianceId)
                                                .Select(c => c.ComplianceName)
                                                .FirstOrDefaultAsync();

            if (complianceName == null)
            {
                return Json(new List<SelectListItem>()); // Return empty if compliance not found
            }

            var forms = await _context.FoForms
                                      .Where(f => !f.isArchived && f.FoName.Contains(complianceName)) // Example: if form name contains compliance name
                                      .Select(f => new SelectListItem
                                      {
                                          Value = f.Id.ToString(),
                                          Text = f.FoName
                                      })
                                      .ToListAsync();
            */

            return Json(forms); // Return forms as JSON
        }
    }
}
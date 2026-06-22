using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartComply.data;
using SmartComply.Models;
using SmartComply.ViewModels;

namespace SmartComply.Controllers
{
    [Authorize]
    public class CorrectivePlanController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CorrectivePlanController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: CorrectivePlan
        public async Task<IActionResult> Index()
        {
            var plans = await _context.CorrectivePlans
                .Include(c => c.Audit)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(plans);
        }

        // GET: CorrectivePlan/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var correctivePlan = await _context.CorrectivePlans
                .Include(c => c.Audit)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (correctivePlan == null)
            {
                return NotFound();
            }

            return View(correctivePlan);
        }

        // GET: CorrectivePlan/Create
        public async Task<IActionResult> Create(int? auditId)
        {
            var model = new CorrectivePlanViewModel();
            if (auditId.HasValue)
            {
                model.AuditId = auditId.Value;
            }
            model.Users = await _userManager.Users.ToListAsync();
            ViewBag.Audits = await _context.Audits.ToListAsync();
            return View(model);
        }

        // POST: CorrectivePlan/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CorrectivePlanViewModel model)
        {
            if (ModelState.IsValid)
            {
                var correctivePlan = new CorrectivePlan
                {
                    AuditId = model.AuditId,
                    IssueIdentified = model.IssueIdentified,
                    CorrectiveAction = model.CorrectiveAction,
                    ResponsiblePersonId = model.ResponsiblePersonId,
                    TargetCompletionDate = model.TargetCompletionDate,
                    Priority = model.Priority,
                    ProgressNotes = model.ProgressNotes,
                    Status = "Pending"
                };

                _context.Add(correctivePlan);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            model.Users = await _userManager.Users.ToListAsync();
            return View(model);
        }

        // GET: CorrectivePlan/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var correctivePlan = await _context.CorrectivePlans.FindAsync(id);
            if (correctivePlan == null)
            {
                return NotFound();
            }

            var model = new CorrectivePlanViewModel
            {
                AuditId = correctivePlan.AuditId,
                IssueIdentified = correctivePlan.IssueIdentified,
                CorrectiveAction = correctivePlan.CorrectiveAction,
                ResponsiblePersonId = correctivePlan.ResponsiblePersonId,
                TargetCompletionDate = correctivePlan.TargetCompletionDate,
                Priority = correctivePlan.Priority,
                ProgressNotes = correctivePlan.ProgressNotes,
                Users = await _userManager.Users.ToListAsync()
            };
            ViewBag.Audits = await _context.Audits.ToListAsync();
            return View(model);
        }

        // POST: CorrectivePlan/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CorrectivePlanViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var correctivePlan = await _context.CorrectivePlans.FindAsync(id);
                    if (correctivePlan == null)
                    {
                        return NotFound();
                    }

                    correctivePlan.IssueIdentified = model.IssueIdentified;
                    correctivePlan.CorrectiveAction = model.CorrectiveAction;
                    correctivePlan.ResponsiblePersonId = model.ResponsiblePersonId;
                    correctivePlan.TargetCompletionDate = model.TargetCompletionDate;
                    correctivePlan.Priority = model.Priority;
                    correctivePlan.ProgressNotes = model.ProgressNotes;
                    correctivePlan.UpdatedAt = DateTime.UtcNow;

                    _context.Update(correctivePlan);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CorrectivePlanExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            model.Users = await _userManager.Users.ToListAsync();
            return View(model);
        }

        // POST: CorrectivePlan/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var correctivePlan = await _context.CorrectivePlans.FindAsync(id);
            if (correctivePlan == null)
            {
                return NotFound();
            }

            correctivePlan.Status = status;
            correctivePlan.UpdatedAt = DateTime.UtcNow;

            if (status == "Completed")
            {
                correctivePlan.ActualCompletionDate = DateTime.UtcNow;
            }

            _context.Update(correctivePlan);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = id });
        }

        // GET: CorrectivePlan/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var correctivePlan = await _context.CorrectivePlans
                .Include(c => c.Audit)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (correctivePlan == null)
            {
                return NotFound();
            }

            return View(correctivePlan);
        }

        // POST: CorrectivePlan/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var correctivePlan = await _context.CorrectivePlans.FindAsync(id);
            if (correctivePlan != null)
            {
                _context.CorrectivePlans.Remove(correctivePlan);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }        private bool CorrectivePlanExists(int id)
        {
            return _context.CorrectivePlans.Any(e => e.Id == id);
        }

        // MANAGER APPROVAL FEATURES

        // GET: Manager Approval for Corrective Plan
        [Authorize(Roles = "superAdmin,Admin,Manager")]
        public async Task<IActionResult> ApproveCorrectivePlan(int id)
        {
            var correctivePlan = await _context.CorrectivePlans
                .Include(cp => cp.Audit)
                .FirstOrDefaultAsync(cp => cp.Id == id);

            if (correctivePlan == null)
            {
                return NotFound();
            }

            var viewModel = new CorrectivePlanApprovalViewModel
            {
                CorrectivePlan = correctivePlan,
                ApprovalStatus = correctivePlan.ApprovalStatus ?? "Pending",
                ApprovalComments = correctivePlan.ApprovalComments
            };

            return View(viewModel);
        }

        // POST: Submit Corrective Plan Approval
        [HttpPost]
        [Authorize(Roles = "superAdmin,Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitApproval(int correctivePlanId, string approvalStatus, string approvalComments)
        {
            var correctivePlan = await _context.CorrectivePlans.FindAsync(correctivePlanId);
            if (correctivePlan == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            
            correctivePlan.ApprovalStatus = approvalStatus;
            correctivePlan.ApprovalComments = approvalComments;
            correctivePlan.ApprovedById = currentUser?.Id;
            correctivePlan.ApprovedAt = DateTime.UtcNow;
            correctivePlan.UpdatedAt = DateTime.UtcNow;

            // Update status based on approval
            if (approvalStatus == "Approved")
            {
                correctivePlan.Status = "Approved";
            }
            else if (approvalStatus == "Rejected")
            {
                correctivePlan.Status = "RequiresRevision";
            }

            _context.Update(correctivePlan);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Corrective plan has been {approvalStatus.ToLower()} successfully!";
            return RedirectToAction("ManagerDashboard", "Audit");
        }

        // GET: Corrective Plans requiring Manager approval
        [Authorize(Roles = "superAdmin,Admin,Manager")]
        public async Task<IActionResult> PendingApprovals()
        {
            var pendingPlans = await _context.CorrectivePlans
                .Include(cp => cp.Audit)
                .Where(cp => cp.ApprovalStatus == null || cp.ApprovalStatus == "Pending")
                .OrderByDescending(cp => cp.CreatedAt)
                .ToListAsync();

            return View(pendingPlans);
        }
    }
}

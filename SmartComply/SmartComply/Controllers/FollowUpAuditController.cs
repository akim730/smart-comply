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
    public class FollowUpAuditController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public FollowUpAuditController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: FollowUpAudit
        public async Task<IActionResult> Index()
        {
            var followUpAudits = await _context.FollowUpAudits
                .Include(f => f.OriginalAudit)
                .OrderByDescending(f => f.ScheduledDate)
                .ToListAsync();

            return View(followUpAudits);
        }

        // GET: FollowUpAudit/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var followUpAudit = await _context.FollowUpAudits
                .Include(f => f.OriginalAudit)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (followUpAudit == null)
            {
                return NotFound();
            }

            return View(followUpAudit);
        }

        // GET: FollowUpAudit/Create
        public async Task<IActionResult> Create(int? auditId)
        {
            var model = new FollowUpAuditViewModel();
            
            if (auditId.HasValue)
            {
                model.OriginalAuditId = auditId.Value;
                model.OriginalAudit = await _context.Audits.FindAsync(auditId.Value);
            }

            model.Users = await _userManager.Users.ToListAsync();
            return View(model);
        }

        // POST: FollowUpAudit/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FollowUpAuditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var followUpAudit = new FollowUpAudit
                {
                    OriginalAuditId = model.OriginalAuditId,
                    FollowUpTitle = model.FollowUpTitle,
                    FollowUpDescription = model.FollowUpDescription,
                    ScheduledDate = model.ScheduledDate,
                    AuditorId = model.AuditorId,
                    Status = "Scheduled"
                };

                _context.Add(followUpAudit);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            model.Users = await _userManager.Users.ToListAsync();
            model.OriginalAudit = await _context.Audits.FindAsync(model.OriginalAuditId);
            return View(model);
        }

        // GET: FollowUpAudit/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var followUpAudit = await _context.FollowUpAudits
                .Include(f => f.OriginalAudit)
                .FirstOrDefaultAsync(f => f.Id == id);
            if (followUpAudit == null)
            {
                return NotFound();
            }
            var model = new FollowUpAuditViewModel
            {
                OriginalAuditId = followUpAudit.OriginalAuditId,
                FollowUpTitle = followUpAudit.FollowUpTitle,
                FollowUpDescription = followUpAudit.FollowUpDescription,
                ScheduledDate = followUpAudit.ScheduledDate,
                AuditorId = followUpAudit.AuditorId,
                Users = await _userManager.Users.ToListAsync(),
                OriginalAudit = followUpAudit.OriginalAudit
            };
            var audits = await _context.Audits.ToListAsync();
            ViewBag.Audits = audits ?? new List<SmartComply.Models.Audit>();
            return View(model);
        }

        // POST: FollowUpAudit/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FollowUpAuditViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var followUpAudit = await _context.FollowUpAudits.FindAsync(id);
                    if (followUpAudit == null)
                    {
                        return NotFound();
                    }

                    followUpAudit.FollowUpTitle = model.FollowUpTitle;
                    followUpAudit.FollowUpDescription = model.FollowUpDescription;
                    followUpAudit.ScheduledDate = model.ScheduledDate;
                    followUpAudit.AuditorId = model.AuditorId;
                    followUpAudit.UpdatedAt = DateTime.UtcNow;

                    _context.Update(followUpAudit);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FollowUpAuditExists(id))
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
            model.OriginalAudit = await _context.Audits.FindAsync(model.OriginalAuditId);
            return View(model);
        }

        // GET: FollowUpAudit/Conduct/5
        public async Task<IActionResult> Conduct(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var followUpAudit = await _context.FollowUpAudits
                .Include(f => f.OriginalAudit)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (followUpAudit == null)
            {
                return NotFound();
            }

            return View(followUpAudit);
        }

        // POST: FollowUpAudit/Conduct/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Conduct(int id, string findings, string recommendations)
        {
            var followUpAudit = await _context.FollowUpAudits.FindAsync(id);
            if (followUpAudit == null)
            {
                return NotFound();
            }

            followUpAudit.ConductedDate = DateTime.UtcNow;
            followUpAudit.Status = "Completed";
            followUpAudit.Findings = findings;
            followUpAudit.Recommendations = recommendations;
            followUpAudit.UpdatedAt = DateTime.UtcNow;

            _context.Update(followUpAudit);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = id });
        }

        // GET: FollowUpAudit/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var followUpAudit = await _context.FollowUpAudits
                .Include(f => f.OriginalAudit)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (followUpAudit == null)
            {
                return NotFound();
            }

            return View(followUpAudit);
        }

        // POST: FollowUpAudit/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var followUpAudit = await _context.FollowUpAudits.FindAsync(id);
            if (followUpAudit != null)
            {
                _context.FollowUpAudits.Remove(followUpAudit);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FollowUpAuditExists(int id)
        {
            return _context.FollowUpAudits.Any(e => e.Id == id);
        }
    }
}

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartComply.Models;

namespace SmartComply.data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<FoForm> FoForms { get; set; }
        public DbSet<TyItem> TyItems { get; set; }
        public DbSet<AmCompliance> AmCompliances { get; set; }
        public DbSet<ComplianceFramework> ComplianceFrameworks { get; set; } // 👈 Add this
        public DbSet<FilingDocument> FilingDocuments { get; set; }
        public DbSet<FoFormDetail> FoFormDetails { get; set; }
        public DbSet<ComplianceFolder> ComplianceFolders { get; set; }
        public DbSet<FolderItem> FolderItems { get; set; }

        public DbSet<Audit> Audits { get; set; }
        public DbSet<CorrectivePlan> CorrectivePlans { get; set; }
        public DbSet<FollowUpAudit> FollowUpAudits { get; set; }
        
        public DbSet<AuditResponse> AuditResponses { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure relationships if necessary (EF Core usually infers simple ones)
            // Example: If AmCompliance can exist without a folder, or vice-versa, define CASCADE behavior if needed.
            modelBuilder.Entity<ComplianceFolder>()
                .HasOne(cf => cf.AmCompliance)
                .WithMany() // Assuming AmCompliance doesn't have a direct ICollection<ComplianceFolder>
                .HasForeignKey(cf => cf.AmComplianceId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting Compliance if folders exist, or Cascade if you want to delete folders with compliance

            modelBuilder.Entity<FolderItem>()
                .HasOne(fi => fi.ComplianceFolder)
                .WithMany(cf => cf.FolderItems)
                .HasForeignKey(fi => fi.ComplianceFolderId)
                .OnDelete(DeleteBehavior.Cascade); // Delete items if folder is deleted

            modelBuilder.Entity<FilingDocument>()
                .HasOne(fd => fd.FolderItem)
                .WithMany(fi => fi.FilingDocuments)
                .HasForeignKey(fd => fd.FolderItemId)
                .OnDelete(DeleteBehavior.Cascade); // Delete documents if item is deleted

            // Configure the one-to-many relationship for FoForm and FoFormDetail
            modelBuilder.Entity<FoFormDetail>()
                .HasOne(d => d.FoForm)       // A detail has one parent FoForm
                .WithMany(f => f.Details)    // A FoForm can have many details
                .HasForeignKey(d => d.FoFormId) // The foreign key in FoFormDetail
                .OnDelete(DeleteBehavior.Cascade); // If FoForm is deleted, its details are also deleted

            // Configure relationship for FoFormDetail and TyItem
            modelBuilder.Entity<FoFormDetail>()
                .HasOne(d => d.TyItem)      // A detail has one TyItem
                .WithMany()                 // TyItem doesn't need a collection of FoFormDetail
                .HasForeignKey(d => d.TyItemId); // The foreign key in FoFormDetail

            // Configure relationships
            modelBuilder.Entity<Audit>()
                .HasOne(a => a.Compliance)
                .WithMany(c => c.Audits)
                .HasForeignKey(a => a.ComplianceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Audit>()
                .HasOne(a => a.Form)
                .WithMany(f => f.Audits)
                .HasForeignKey(a => a.FormId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CorrectivePlan>()
                .HasOne(cp => cp.Audit)
                .WithMany(a => a.CorrectivePlans)
                .HasForeignKey(cp => cp.AuditId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FollowUpAudit>()
                .HasOne(fu => fu.OriginalAudit)
                .WithMany(a => a.FollowUpAudits)
                .HasForeignKey(fu => fu.OriginalAuditId)
                .OnDelete(DeleteBehavior.Cascade); modelBuilder.Entity<AuditResponse>()
                .HasOne(ar => ar.Audit)
                .WithMany()
                .HasForeignKey(ar => ar.AuditId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure decimal precision for AuditScore
            modelBuilder.Entity<Audit>()
                .Property(a => a.AuditScore)
                .HasPrecision(5, 2); // 5 digits total, 2 after decimal point (e.g., 999.99)

            // Configure indexes for better performance
            modelBuilder.Entity<Audit>()
                .HasIndex(a => a.AuditStatus);

            modelBuilder.Entity<Audit>()
                .HasIndex(a => a.AuditDate);

            modelBuilder.Entity<CorrectivePlan>()
                .HasIndex(cp => cp.Status);

            modelBuilder.Entity<CorrectivePlan>()
                .HasIndex(cp => cp.TargetCompletionDate);

            // Ensure base Identity configurations are called if you're using Identity
            base.OnModelCreating(modelBuilder);


            // Seed data
            modelBuilder.Entity<TyItem>().HasData(
                new TyItem { Id = 1, TyName = "Text", TyHtmlType = "text", TyValidate = "required" },
                new TyItem { Id = 2, TyName = "range", TyHtmlType = "range", TyValidate = "min=1,max=5" },
                new TyItem { Id = 3, TyName = "radio", TyHtmlType = "radio", TyValidate = "required" }
            );

            modelBuilder.Entity<AmCompliance>().HasData(
                new AmCompliance { Id = 1, ComplianceName = "Halal", ComplianceDescription = "ini berkaitan halal", CreatedAt = new DateTime(2020, 1, 1), UpdatedAt = new DateTime(2024, 1, 1) },
                new AmCompliance { Id = 2, ComplianceName = "ISO4000", ComplianceDescription = "ini berkaitan iso", CreatedAt = new DateTime(2020, 1, 1), UpdatedAt = new DateTime(2024, 1, 1) },
                new AmCompliance { Id = 3, ComplianceName = "BERSIH", ComplianceDescription = "ini berkaitan bersih", CreatedAt = new DateTime(2020, 1, 1), UpdatedAt = new DateTime(2024, 1, 1) }
            );

           
        }
    }
}

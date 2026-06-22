// SmartComply.ViewModels/AdminDashboardViewModel.cs
using SmartComply.Models; // Assuming FoForm, AmCompliance, ComplianceFolder are here
using System.Collections.Generic;

namespace SmartComply.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalComplianceTypes { get; set; }
        public int TotalForms { get; set; }
        public int TotalArchivedForms { get; set; }
        // public int TotalFoFormDetails { get; set; } // REMOVE or rename if you want to keep
        public int TotalFoldersCreated { get; set; } // Add this new property

        public List<FoForm> LatestForms { get; set; } = new List<FoForm>();
        public List<ComplianceOverviewItem> ComplianceOverview { get; set; } = new List<ComplianceOverviewItem>();

        // Add this new property to hold the list of all Compliance Folders
        public List<ComplianceFolder> AllComplianceFolders { get; set; } = new List<ComplianceFolder>();
    }

    // This VM is used within AdminDashboardViewModel
    public class ComplianceOverviewItem
    {
        public int Id { get; set; }
        public string ComplianceName { get; set; }
        public string? ComplianceDescription { get; set; } // Make nullable if not always present
        public int FormsCount { get; set; }
        public DateTime CreatedAt { get; set; } // Assuming you have a CreatedAt property
    }
}
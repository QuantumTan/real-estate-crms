using System.Collections.Generic;

namespace CRMS_Peguit.winforms.Models.ViewModels
{
    public class DashboardSummaryViewModel
    {
        public int TotalCustomers { get; set; }
        public int ActiveProperties { get; set; }
        public int QualifiedLeads { get; set; }
        public int TotalDeals { get; set; }
        public decimal PipelineValue { get; set; }
        public int TotalAgents { get; set; }
        public List<RecentLeadItemViewModel> RecentLeads { get; set; } = new();
    }

    public class RecentLeadItemViewModel
    {
        public string Lead { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Stage { get; set; } = string.Empty;
    }
}

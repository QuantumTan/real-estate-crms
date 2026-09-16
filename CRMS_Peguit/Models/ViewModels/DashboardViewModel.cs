using System;
using System.Collections.Generic;

namespace CRMS_Peguit.winforms.Models.ViewModels
{
    // =========================================================================
    // AGENT DASHBOARD DTO
    // =========================================================================
    public class AgentDashboardDto
    {
        public string Greeting { get; set; } = string.Empty;
        public string DateText { get; set; } = string.Empty;

        // KPI Counts
        public int ActiveLeadsCount { get; set; }
        public int OpenDealsCount { get; set; }
        public int FollowUpsDueTodayCount { get; set; }
        public int OpenSupportTicketsCount { get; set; }

        // Short Lists (max 5 items)
        public List<AgentFollowUpItemDto> FollowUpsToday { get; set; } = new();
        public List<AgentActivityItemDto> RecentActivities { get; set; } = new();
    }

    public class AgentFollowUpItemDto
    {
        public int TaskReminderId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string DueTimeText { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium";
        public string Type { get; set; } = "Call";
        public string RelatedName { get; set; } = string.Empty;
        public bool IsOverdue { get; set; }
    }

    public class AgentActivityItemDto
    {
        public int ActivityId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }

    // =========================================================================
    // MANAGER DASHBOARD DTO
    // =========================================================================
    public class ManagerDashboardDto
    {
        public string Greeting { get; set; } = string.Empty;
        public string DateText { get; set; } = string.Empty;

        // KPI Counts
        public int TeamDealsThisMonthCount { get; set; }
        public int TeamOpenTicketsCount { get; set; }
        public int PendingAssignmentsCount { get; set; }
        public double TeamConversionRate { get; set; }

        // Short Lists (max 5 items)
        public List<PendingAssignmentItemDto> PendingAssignments { get; set; } = new();
        public List<TeamActivityItemDto> TeamRecentActivity { get; set; } = new();
    }

    public class PendingAssignmentItemDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // "Lead" or "Customer"
        public string Name { get; set; } = string.Empty;
        public string SubmitterName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string TimeAgo { get; set; } = string.Empty;
        public int? AssignedAgentId { get; set; }
        public object? OriginalEntity { get; set; }
    }

    public class TeamActivityItemDto
    {
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string TimeAgo { get; set; } = string.Empty;
    }

    // =========================================================================
    // ADMIN DASHBOARD DTO
    // =========================================================================
    public class AdminDashboardDto
    {
        public string Greeting { get; set; } = string.Empty;
        public string DateText { get; set; } = string.Empty;

        // KPI Counts
        public int TotalActiveUsersCount { get; set; }
        public int OpenTicketsCount { get; set; }
        public string SubscriptionStatus { get; set; } = string.Empty;
        public int DealsClosedThisMonthCount { get; set; }

        // Short Lists (max 5 items, empty/omitted if no logs exist)
        public List<SystemActivityItemDto> RecentSystemActivities { get; set; } = new();
    }

    public class SystemActivityItemDto
    {
        public string Title { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string TimeAgo { get; set; } = string.Empty;
        public string Icon { get; set; } = "⚙️";
    }

    // =========================================================================
    // LEGACY VIEW MODELS (Retained for backward compatibility)
    // =========================================================================
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

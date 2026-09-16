using System;
using System.Collections.Generic;
using System.Linq;
using CRMS_Peguit.domain.entities;
using CRMS_Peguit.infrastructure.data;
using CRMS_Peguit.winforms.Auth;
using CRMS_Peguit.winforms.Models.Services;
using CRMS_Peguit.winforms.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CRMS_Peguit.winforms.Controllers
{
    public class DashboardController : IDisposable
    {
        private readonly RealEstateDbContext _db;
        private CustomerController? _customerController;
        private LeadController? _leadController;
        private DealController? _dealController;
        private FollowUpController? _followUpController;
        private SupportTicketController? _supportTicketController;
        private AnalyticsController? _analyticsController;
        private UserController? _userController;
        private ApprovalController? _approvalController;

        private CustomerController CustomerCtrl => _customerController ??= new CustomerController();
        private LeadController LeadCtrl => _leadController ??= new LeadController();
        private DealController DealCtrl => _dealController ??= new DealController();
        private FollowUpController FollowUpCtrl => _followUpController ??= new FollowUpController();
        private SupportTicketController SupportTicketCtrl => _supportTicketController ??= new SupportTicketController();
        private AnalyticsController AnalyticsCtrl => _analyticsController ??= new AnalyticsController();
        private UserController UserCtrl => _userController ??= new UserController();
        private ApprovalController ApprovalCtrl => _approvalController ??= new ApprovalController();

        public DashboardController()
        {
            _db = LocalDb.CreateContext(CurrentSession.TenantId);
        }

        // =========================================================================
        // 1. AGENT SNAPSHOT (Role-scoped to agent's own data only)
        // =========================================================================
        public AgentDashboardDto GetAgentSnapshot(int userId)
        {
            int currentUserId = userId > 0 ? userId : CurrentSession.UserId;
            string fullName = CurrentSession.CurrentUser?.FullName ?? "Agent";
            string todayText = DateTime.Now.ToString("dddd, MMMM d, yyyy");

            try
            {
                // Lightweight COUNT queries calling existing controller methods
                int activeLeads = LeadCtrl.GetActiveLeadsCount(currentUserId);
                int openDeals = DealCtrl.GetOpenDealsCount(currentUserId);
                var followUpCounts = FollowUpCtrl.GetKpiCounts();
                int followUpsTodayCount = followUpCounts.Today;
                int openTickets = SupportTicketCtrl.GetOpenTicketsCount();

                // Top 5 follow-ups due today
                var rawFollowUps = FollowUpCtrl.GetFollowUpsDueToday(5);
                var followUpsToday = rawFollowUps.Select(r => new AgentFollowUpItemDto
                {
                    TaskReminderId = r.TaskReminderId,
                    Title = r.Title,
                    DueTimeText = r.DueDate.ToLocalTime().ToString("h:mm tt"),
                    Priority = r.Priority,
                    Type = r.Type,
                    RelatedName = r.RelatedCustomer?.FullName ?? r.RelatedLead?.FullName ?? string.Empty,
                    IsOverdue = r.Status == "Overdue" || r.DueDate < DateTime.UtcNow
                }).ToList();

                // Last 5 activities logged by this agent
                var rawActivities = CustomerCtrl.GetRecentActivitiesForAgent(currentUserId, 5);
                var recentActivities = rawActivities.Select(a => new AgentActivityItemDto
                {
                    ActivityId = a.ActivityId,
                    Type = a.Type ?? "Activity",
                    Notes = a.Notes ?? string.Empty,
                    TimeAgo = FormatTimeAgo(a.ActivityDate),
                    Date = a.ActivityDate
                }).ToList();

                return new AgentDashboardDto
                {
                    Greeting = $"Welcome back, {fullName}",
                    DateText = todayText,
                    ActiveLeadsCount = activeLeads,
                    OpenDealsCount = openDeals,
                    FollowUpsDueTodayCount = followUpsTodayCount,
                    OpenSupportTicketsCount = openTickets,
                    FollowUpsToday = followUpsToday,
                    RecentActivities = recentActivities
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DashboardController.GetAgentSnapshot] Error: {ex.Message}");
                return new AgentDashboardDto
                {
                    Greeting = $"Welcome back, {fullName}",
                    DateText = todayText
                };
            }
        }

        // =========================================================================
        // 2. MANAGER SNAPSHOT (Team-wide data, no individual follow-up visibility)
        // =========================================================================
        public ManagerDashboardDto GetManagerSnapshot()
        {
            string fullName = CurrentSession.CurrentUser?.FullName ?? "Manager";
            string todayText = DateTime.Now.ToString("dddd, MMMM d, yyyy");

            try
            {
                // Lightweight counts
                int teamDeals = DealCtrl.GetDealsClosedThisMonthCount();
                int teamTickets = SupportTicketCtrl.GetOpenTicketsCount();
                int pendingCust = CustomerCtrl.GetPendingReviewCount();
                int pendingLeads = LeadCtrl.GetPendingReviewCount();
                int totalPending = pendingCust + pendingLeads;
                double conversionRate = LeadCtrl.GetTeamConversionRate();

                // Top 5 pending assignments (Customers and Leads awaiting manager action)
                var pendingApprovals = ApprovalCtrl.GetPendingApprovals()
                    .Where(x => string.Equals(x.Type, "Customer", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(x.Type, "Lead", StringComparison.OrdinalIgnoreCase))
                    .Take(5)
                    .Select(x => new PendingAssignmentItemDto
                    {
                        Id = x.Id,
                        Type = x.Type,
                        Name = x.Title,
                        SubmitterName = x.SubmitterName,
                        CreatedAt = x.CreatedAt,
                        TimeAgo = FormatTimeAgo(x.CreatedAt),
                        AssignedAgentId = x.AssignedAgentId,
                        OriginalEntity = x.OriginalEntity
                    })
                    .ToList();

                // Team recent activities from Analytics feed (last 5 deals closed or tickets resolved)
                var feed = AnalyticsCtrl.GetRecentActivityFeed(5)
                    .Select(f => new TeamActivityItemDto
                    {
                        Description = f.Description,
                        Icon = f.Icon,
                        Timestamp = f.Timestamp,
                        TimeAgo = FormatTimeAgo(f.Timestamp)
                    })
                    .ToList();

                return new ManagerDashboardDto
                {
                    Greeting = $"Welcome back, {fullName}",
                    DateText = todayText,
                    TeamDealsThisMonthCount = teamDeals,
                    TeamOpenTicketsCount = teamTickets,
                    PendingAssignmentsCount = totalPending,
                    TeamConversionRate = conversionRate,
                    PendingAssignments = pendingApprovals,
                    TeamRecentActivity = feed
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DashboardController.GetManagerSnapshot] Error: {ex.Message}");
                return new ManagerDashboardDto
                {
                    Greeting = $"Welcome back, {fullName}",
                    DateText = todayText
                };
            }
        }

        // =========================================================================
        // 3. ADMIN SNAPSHOT (Business oversight, no direct record editing)
        // =========================================================================
        public AdminDashboardDto GetAdminSnapshot()
        {
            string fullName = CurrentSession.CurrentUser?.FullName ?? "Administrator";
            string todayText = DateTime.Now.ToString("dddd, MMMM d, yyyy");

            try
            {
                int totalActiveUsers = UserCtrl.GetActiveUsersCount();
                int openTickets = SupportTicketCtrl.GetOpenTicketsCount();
                int dealsClosed = DealCtrl.GetDealsClosedThisMonthCount();
                string subStatus = GetSubscriptionStatus();

                // Recent System Activity from BackupLogs and SystemSettings
                var systemLogs = new List<SystemActivityItemDto>();

                try
                {
                    var backups = _db.BackupLogs
                        .AsNoTracking()
                        .Include(b => b.PerformedByUser)
                        .OrderByDescending(b => b.BackupDate)
                        .Take(5)
                        .ToList();

                    foreach (var b in backups)
                    {
                        string user = b.PerformedByUser?.FullName ?? "System";
                        systemLogs.Add(new SystemActivityItemDto
                        {
                            Title = "Database Backup",
                            Details = $"Status: {b.Status} · Executed by {user}",
                            Timestamp = b.BackupDate,
                            TimeAgo = FormatTimeAgo(b.BackupDate),
                            Icon = "💾"
                        });
                    }

                    var settings = _db.SystemSettings
                        .AsNoTracking()
                        .Include(s => s.UpdatedByUser)
                        .OrderByDescending(s => s.UpdatedAt)
                        .Take(5)
                        .ToList();

                    foreach (var s in settings)
                    {
                        string user = s.UpdatedByUser?.FullName ?? "Admin";
                        systemLogs.Add(new SystemActivityItemDto
                        {
                            Title = $"Setting: {s.SettingKey}",
                            Details = $"Updated by {user}",
                            Timestamp = s.UpdatedAt,
                            TimeAgo = FormatTimeAgo(s.UpdatedAt),
                            Icon = "⚙️"
                        });
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[DashboardController.GetAdminSnapshot] Logs Query Error: {ex.Message}");
                }

                systemLogs = systemLogs.OrderByDescending(l => l.Timestamp).Take(5).ToList();

                return new AdminDashboardDto
                {
                    Greeting = $"Welcome back, {fullName}",
                    DateText = todayText,
                    TotalActiveUsersCount = totalActiveUsers,
                    OpenTicketsCount = openTickets,
                    SubscriptionStatus = subStatus,
                    DealsClosedThisMonthCount = dealsClosed,
                    RecentSystemActivities = systemLogs
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DashboardController.GetAdminSnapshot] Error: {ex.Message}");
                return new AdminDashboardDto
                {
                    Greeting = $"Welcome back, {fullName}",
                    DateText = todayText,
                    SubscriptionStatus = "Active License"
                };
            }
        }

        private string GetSubscriptionStatus()
        {
            try
            {
                var setting = _db.SystemSettings
                    .AsNoTracking()
                    .FirstOrDefault(s => s.SettingKey == "SubscriptionStatus" || s.SettingKey == "SubscriptionExpiry");

                if (setting != null && !string.IsNullOrWhiteSpace(setting.SettingValue))
                {
                    return setting.SettingValue;
                }

                // Default active status based on current session
                return $"Active until {DateTime.UtcNow.AddMonths(9):MMM dd, yyyy}";
            }
            catch
            {
                return "Active License";
            }
        }

        private static string FormatTimeAgo(DateTime utcTime)
        {
            var span = DateTime.UtcNow - utcTime;
            if (span.TotalMinutes < 1) return "Just now";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
            if (span.TotalDays < 7) return $"{(int)span.TotalDays}d ago";
            return utcTime.ToLocalTime().ToString("MMM d");
        }

        // =========================================================================
        // LEGACY SUMMARY (Retained for backward compatibility)
        // =========================================================================
        public DashboardSummaryViewModel GetSummary()
        {
            try
            {
                var customers = CustomerCtrl.GetAll();
                var leads = LeadCtrl.GetAll();
                var deals = DealCtrl.GetAll();

                int qualifiedLeads = leads.Count(l => string.Equals(l.Stage, "qualified", StringComparison.OrdinalIgnoreCase));
                decimal pipelineSum = deals.Sum(d => d.Value);

                var recentLeads = leads
                    .Take(10)
                    .Select(l => new RecentLeadItemViewModel
                    {
                        Lead = l.FullName,
                        Email = string.IsNullOrWhiteSpace(l.Email) ? "-" : l.Email,
                        Source = string.IsNullOrWhiteSpace(l.Source) ? "Website" : l.Source,
                        Value = l.ExpectedValue.HasValue ? $"₱{l.ExpectedValue.Value:N2}" : "-",
                        Stage = (l.Stage ?? string.Empty).ToUpperInvariant()
                    })
                    .ToList();

                return new DashboardSummaryViewModel
                {
                    TotalCustomers = customers.Count,
                    ActiveProperties = 0,
                    QualifiedLeads = qualifiedLeads,
                    TotalDeals = deals.Count,
                    PipelineValue = pipelineSum,
                    TotalAgents = 0,
                    RecentLeads = recentLeads
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DashboardController.GetSummary] Error: {ex.Message}");
                return new DashboardSummaryViewModel();
            }
        }

        public void Dispose()
        {
            _db.Dispose();
            _customerController?.Dispose();
            _leadController?.Dispose();
            _dealController?.Dispose();
            _followUpController?.Dispose();
            _supportTicketController?.Dispose();
            _analyticsController?.Dispose();
            _userController?.Dispose();
            _approvalController?.Dispose();
        }
    }
}

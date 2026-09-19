using System;
using System.Collections.Generic;
using System.Linq;
using CRMS_Peguit.domain.entities;
using CRMS_Peguit.infrastructure.data;
using CRMS_Peguit.winforms.Auth;
using CRMS_Peguit.winforms.Models.Services;
using CRMS_Peguit.winforms.Models.ViewModels;
using CRMS_Peguit.winforms.Services;
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
        // 1. AGENT SNAPSHOT (Role-scoped to agent's own data only)
        // =========================================================================
        public AgentDashboardDto GetAgentSnapshot(int userId)
        {
            int currentUserId = userId > 0 ? userId : CurrentSession.UserId;
            string fullName = CurrentSession.CurrentUser?.FullName ?? "Agent";
            string firstName = GetFirstName(fullName);
            string todayText = DateTime.Now.ToString("dddd, MMMM d, yyyy");

            try
            {
                // Lightweight COUNT queries calling existing controller methods
                int activeLeads = LeadCtrl.GetActiveLeadsCount(currentUserId);
                int openDeals = DealCtrl.GetOpenDealsCount(currentUserId);
                var followUpCounts = FollowUpCtrl.GetKpiCounts();
                int followUpsTodayCount = followUpCounts.Today;
                int openTickets = SupportTicketCtrl.GetOpenTicketsCount();

                // Sparkline: My Deals Closed over the last 30 days
                var sparkline = new List<double>();
                var thirtyDaysAgo = DateTime.UtcNow.Date.AddDays(-29);
                var closedDeals = _db.Deals
                    .AsNoTracking()
                    .Where(d => d.AgentId == currentUserId && (d.Stage == "Closed" || d.Stage == "Closed-Won" || d.Stage == "Won") && ((d.ContractSignedDate != null && d.ContractSignedDate >= thirtyDaysAgo) || d.CreatedAt >= thirtyDaysAgo))
                    .ToList();

                for (int i = 0; i < 30; i++)
                {
                    var targetDate = thirtyDaysAgo.AddDays(i);
                    int dayCount = closedDeals.Count(d => (d.ContractSignedDate?.Date ?? d.CreatedAt.Date) == targetDate);
                    sparkline.Add((double)dayCount);
                }

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
                    Status = r.Status == "Overdue" || r.DueDate < DateTime.UtcNow ? "Overdue" : (r.DueDate <= DateTime.UtcNow.AddHours(2) ? "Pending" : "Active"),
                    IsOverdue = r.Status == "Overdue" || r.DueDate < DateTime.UtcNow
                }).ToList();

                // Last 5 activities logged by this agent
                var rawActivities = CustomerCtrl.GetRecentActivitiesForAgent(currentUserId, 5);
                var recentActivities = rawActivities.Select(a => new AgentActivityItemDto
                {
                    ActivityId = a.ActivityId,
                    Type = a.Type ?? "Activity",
                    RelatedName = a.RelatedCustomer?.FullName ?? a.RelatedLead?.FullName ?? (string.IsNullOrWhiteSpace(a.Notes) ? "Client Contact" : a.Notes),
                    Notes = a.Notes ?? string.Empty,
                    Status = "Completed",
                    TimeAgo = FormatTimeAgo(a.ActivityDate),
                    Date = a.ActivityDate
                }).ToList();

                return new AgentDashboardDto
                {
                    Greeting = $"Welcome back, {firstName}",
                    DateText = todayText,
                    ActiveLeadsCount = activeLeads,
                    OpenDealsCount = openDeals,
                    FollowUpsDueTodayCount = followUpsTodayCount,
                    OpenSupportTicketsCount = openTickets,
                    SparklineDealsClosed = sparkline,
                    FollowUpsToday = followUpsToday,
                    RecentActivities = recentActivities
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DashboardController.GetAgentSnapshot] Error: {ex.Message}");
                return new AgentDashboardDto
                {
                    Greeting = $"Welcome back, {firstName}",
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
            string firstName = GetFirstName(fullName);
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

                // Glanceable Chart: Team Deals Won vs. Lost this month
                var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                int dealsWon = _db.Deals.AsNoTracking()
                    .Count(d => (d.Stage == "Closed" || d.Stage == "Closed-Won" || d.Stage == "Won") && ((d.ContractSignedDate != null && d.ContractSignedDate >= startOfMonth) || d.CreatedAt >= startOfMonth));
                int dealsLost = _db.Deals.AsNoTracking()
                    .Count(d => (d.Stage == "Lost" || d.Stage == "Closed-Lost") && d.CreatedAt >= startOfMonth);

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
                        Status = "Pending",
                        CreatedAt = x.CreatedAt,
                        TimeAgo = FormatTimeAgo(x.CreatedAt),
                        AssignedAgentId = x.AssignedAgentId,
                        OriginalEntity = x.OriginalEntity
                    })
                    .ToList();

                // Team recent activities: last 5 deals closed or tickets resolved
                var recentClosedDeals = _db.Deals
                    .AsNoTracking()
                    .Include(d => d.Agent)
                    .Where(d => d.Stage == "Closed" || d.Stage == "Closed-Won" || d.Stage == "Won" || d.Stage == "Lost" || d.Stage == "Closed-Lost")
                    .OrderByDescending(d => d.ContractSignedDate ?? d.CreatedAt)
                    .Take(5)
                    .ToList();

                var recentResolvedTickets = _db.SupportTickets
                    .AsNoTracking()
                    .Include(t => t.AssignedToUser)
                    .Where(t => t.Status == "Resolved" || t.Status == "Closed")
                    .OrderByDescending(t => t.ResolvedAt ?? t.CreatedAt)
                    .Take(5)
                    .ToList();

                var teamEvents = new List<TeamActivityItemDto>();
                foreach (var d in recentClosedDeals)
                {
                    bool isWon = d.Stage != "Lost" && d.Stage != "Closed-Lost";
                    teamEvents.Add(new TeamActivityItemDto
                    {
                        AgentName = d.Agent?.FullName ?? "Agent",
                        ActionTitle = $"Deal #{d.DealId}",
                        Description = $"Value: {AppFormat.FormatCurrency(d.Value)}",
                        Outcome = isWon ? "Won" : "Lost",
                        Icon = "💼",
                        Timestamp = d.ContractSignedDate ?? d.CreatedAt,
                        TimeAgo = FormatTimeAgo(d.ContractSignedDate ?? d.CreatedAt)
                    });
                }
                foreach (var t in recentResolvedTickets)
                {
                    teamEvents.Add(new TeamActivityItemDto
                    {
                        AgentName = t.AssignedToUser?.FullName ?? "Support Staff",
                        ActionTitle = $"Ticket #{t.TicketNumber}",
                        Description = t.Description ?? "Ticket issue resolved",
                        Outcome = "Resolved",
                        Icon = "🎟",
                        Timestamp = t.ResolvedAt ?? t.CreatedAt,
                        TimeAgo = FormatTimeAgo(t.ResolvedAt ?? t.CreatedAt)
                    });
                }
                var feed = teamEvents.OrderByDescending(e => e.Timestamp).Take(5).ToList();

                return new ManagerDashboardDto
                {
                    Greeting = $"Welcome back, {firstName}",
                    DateText = todayText,
                    TeamDealsThisMonthCount = teamDeals,
                    TeamOpenTicketsCount = teamTickets,
                    PendingAssignmentsCount = totalPending,
                    TeamConversionRate = conversionRate,
                    DealsWonThisMonthCount = dealsWon,
                    DealsLostThisMonthCount = dealsLost,
                    PendingAssignments = pendingApprovals,
                    TeamRecentActivity = feed
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DashboardController.GetManagerSnapshot] Error: {ex.Message}");
                return new ManagerDashboardDto
                {
                    Greeting = $"Welcome back, {firstName}",
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
            string firstName = GetFirstName(fullName);
            string todayText = DateTime.Now.ToString("dddd, MMMM d, yyyy");

            try
            {
                int totalActiveUsers = UserCtrl.GetActiveUsersCount();
                int openTickets = SupportTicketCtrl.GetOpenTicketsCount();
                int dealsClosed = DealCtrl.GetDealsClosedThisMonthCount();
                var (subStatus, subExpiry) = GetSubscriptionDetails();

                // Glanceable Chart: Ticket Status Breakdown (Open / In Progress / Resolved)
                int openT = _db.SupportTickets.AsNoTracking().Count(t => t.Status == "Open" || t.Status == "New");
                int inProgT = _db.SupportTickets.AsNoTracking().Count(t => t.Status == "In Progress" || t.Status == "InProgress");
                int resT = _db.SupportTickets.AsNoTracking().Count(t => t.Status == "Resolved" || t.Status == "Closed");

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
                            Status = b.Status,
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
                            Status = "Active",
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
                    Greeting = $"Welcome back, {firstName}",
                    DateText = todayText,
                    TotalActiveUsersCount = totalActiveUsers,
                    OpenTicketsCount = openTickets,
                    SubscriptionStatus = subStatus,
                    SubscriptionExpiryText = subExpiry,
                    DealsClosedThisMonthCount = dealsClosed,
                    OpenTicketsBreakdown = openT,
                    InProgressTicketsBreakdown = inProgT,
                    ResolvedTicketsBreakdown = resT,
                    RecentSystemActivities = systemLogs
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DashboardController.GetAdminSnapshot] Error: {ex.Message}");
                return new AdminDashboardDto
                {
                    Greeting = $"Welcome back, {firstName}",
                    DateText = todayText,
                    SubscriptionStatus = "Active",
                    SubscriptionExpiryText = $"Expires {DateTime.UtcNow.AddMonths(9):MMM dd, yyyy}"
                };
            }
        }

        // =========================================================================
        // 4. SUPER ADMIN SNAPSHOT (Platform-level oversight)
        // =========================================================================
        public SuperAdminDashboardDto GetSuperAdminSnapshot()
        {
            string fullName = CurrentSession.CurrentUser?.FullName ?? "Super Admin";
            string firstName = GetFirstName(fullName);
            string todayText = DateTime.Now.ToString("dddd, MMMM d, yyyy");

            try
            {
                int totalTenants = 1;

                int activeSubs = 1;
                int expiringSubs = 0;
                int expiredSubs = 0;

                string backupStatus = "Active";
                string backupTime = "Up to date";
                try
                {
                    var lastBackup = _db.BackupLogs.AsNoTracking().OrderByDescending(b => b.BackupDate).FirstOrDefault();
                    if (lastBackup != null && !string.IsNullOrWhiteSpace(lastBackup.Status))
                    {
                        backupStatus = lastBackup.Status;
                        backupTime = FormatTimeAgo(lastBackup.BackupDate);
                    }
                }
                catch { }

                var platformEvents = new List<PlatformActivityItemDto>();
                try
                {
                    var backups = _db.BackupLogs.AsNoTracking().Include(b => b.PerformedByUser).OrderByDescending(b => b.BackupDate).Take(3).ToList();
                    foreach (var b in backups)
                    {
                        platformEvents.Add(new PlatformActivityItemDto
                        {
                            Title = "Platform Database Backup",
                            Details = $"Status: {b.Status} · Executed by {b.PerformedByUser?.FullName ?? "System"}",
                            Status = b.Status,
                            Timestamp = b.BackupDate,
                            TimeAgo = FormatTimeAgo(b.BackupDate),
                            Icon = "💾"
                        });
                    }

                    var recentUsers = _db.Users.AsNoTracking().Include(u => u.Role).OrderByDescending(u => u.CreatedAt).Take(2).ToList();
                    foreach (var u in recentUsers)
                    {
                        platformEvents.Add(new PlatformActivityItemDto
                        {
                            Title = $"User Registration: {u.FullName}",
                            Details = $"Role: {u.Role?.RoleName ?? "User"} · {u.Email}",
                            Status = u.Status ?? "Active",
                            Timestamp = u.CreatedAt,
                            TimeAgo = FormatTimeAgo(u.CreatedAt),
                            Icon = "👤"
                        });
                    }
                }
                catch { }

                platformEvents = platformEvents.OrderByDescending(p => p.Timestamp).Take(5).ToList();

                return new SuperAdminDashboardDto
                {
                    Greeting = $"Welcome back, {firstName}",
                    DateText = todayText,
                    TotalTenantsCount = totalTenants,
                    ActiveSubscriptionsCount = activeSubs,
                    SubscriptionsExpiringThisMonthCount = expiringSubs,
                    ExpiredSubscriptionsCount = expiredSubs,
                    LastBackupStatus = backupStatus,
                    LastBackupTimeText = backupTime,
                    RecentPlatformActivities = platformEvents
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DashboardController.GetSuperAdminSnapshot] Error: {ex.Message}");
                return new SuperAdminDashboardDto
                {
                    Greeting = $"Welcome back, {firstName}",
                    DateText = todayText
                };
            }
        }

        private static string GetFirstName(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "User";
            var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 0 ? parts[0] : fullName.Trim();
        }

        private (string status, string expiry) GetSubscriptionDetails()
        {
            try
            {
                var setting = _db.SystemSettings
                    .AsNoTracking()
                    .FirstOrDefault(s => s.SettingKey == "SubscriptionStatus" || s.SettingKey == "SubscriptionExpiry");

                string status = "Active";
                string expiry = $"Expires {DateTime.UtcNow.AddMonths(9):MMM dd, yyyy}";

                if (setting != null && !string.IsNullOrWhiteSpace(setting.SettingValue))
                {
                    if (setting.SettingKey == "SubscriptionStatus")
                    {
                        status = setting.SettingValue;
                    }
                    else
                    {
                        expiry = $"Expires {setting.SettingValue}";
                    }
                }

                return (status, expiry);
            }
            catch
            {
                return ("Active", $"Expires {DateTime.UtcNow.AddMonths(9):MMM dd, yyyy}");
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

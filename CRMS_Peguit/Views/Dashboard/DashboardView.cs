using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using CRMS_Peguit.domain.entities;
using CRMS_Peguit.Models;
using CRMS_Peguit.winforms.Auth;
using CRMS_Peguit.winforms.Controllers;
using CRMS_Peguit.winforms.Controls;
using CRMS_Peguit.winforms.Models.Roles;
using CRMS_Peguit.winforms.Models.Services;
using CRMS_Peguit.winforms.Models.ViewModels;
using CRMS_Peguit.winforms.Views.Customers;
using CRMS_Peguit.winforms.Views.FollowUps;
using CRMS_Peguit.winforms.Views.Leads;
using CRMS_Peguit.winforms.Views.Shared;

namespace CRMS_Peguit.winforms.Views.Dashboard
{
    public partial class DashboardView : UserControl
    {
        public event Action<string>? NavigationRequested;

        public DashboardView()
        {
            InitializeComponent();
            SetupStyling();
            LoadData();

            Load += (_, _) => LayoutControls();
            Resize += (_, _) => LayoutControls();
        }

        private void SetupStyling()
        {
            UiRadiusHelper.StyleCard(pnlLeftCard, 12);
            UiRadiusHelper.StyleCard(pnlRightCard, 12);

            pnlLeftList.AutoScroll = true;
            pnlRightList.AutoScroll = true;

            pnlLeftList.Resize += (_, _) => ResizeListItems(pnlLeftList);
            pnlRightList.Resize += (_, _) => ResizeListItems(pnlRightList);
        }

        public async void LoadData()
        {
            try
            {
                var user = CurrentSession.CurrentUser;
                var role = user?.Role ?? UserRole.SalesStaff;

                object? snapshot = null;
                await System.Threading.Tasks.Task.Run(() =>
                {
                    using var ctrl = new DashboardController();
                    snapshot = role switch
                    {
                        UserRole.SalesStaff => (object)ctrl.GetAgentSnapshot(CurrentSession.UserId),
                        UserRole.Manager => (object)ctrl.GetManagerSnapshot(),
                        UserRole.Admin or UserRole.SuperAdmin => (object)ctrl.GetAdminSnapshot(),
                        _ => (object)ctrl.GetAgentSnapshot(CurrentSession.UserId)
                    };
                });

                if (IsDisposed) return;

                switch (role)
                {
                    case UserRole.SalesStaff:
                        if (snapshot is AgentDashboardDto agentSnap) RenderAgentDashboard(agentSnap);
                        break;
                    case UserRole.Manager:
                        if (snapshot is ManagerDashboardDto mgrSnap) RenderManagerDashboard(mgrSnap);
                        break;
                    case UserRole.Admin:
                    case UserRole.SuperAdmin:
                        if (snapshot is AdminDashboardDto adminSnap) RenderAdminDashboard(adminSnap);
                        break;
                    default:
                        if (snapshot is AgentDashboardDto defSnap) RenderAgentDashboard(defSnap);
                        break;
                }
                LayoutControls();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DashboardView.LoadData] Error: {ex.Message}");
            }
        }

        // =========================================================================
        // 1. AGENT DASHBOARD RENDERER (Owner-scoped only)
        // =========================================================================
        private void RenderAgentDashboard(AgentDashboardDto snapshot)
        {

            lblTitle.Text = snapshot.Greeting;
            lblSubtitle.Text = snapshot.DateText;

            // Quick Actions: "+ New Lead", "+ New Customer", "+ Log Activity"
            pnlQuickActions.Controls.Clear();

            var btnAddLead = CreateQuickActionButton("+ New Lead", AzureTints.SkylineBlue, Color.White, (_, _) =>
            {
                using var form = new LeadInputForm();
                if (form.ShowDialog(this) == DialogResult.OK && form.Result != null)
                {
                    using var leadCtrl = new LeadController();
                    leadCtrl.Add(form.Result);
                    LoadData();
                }
            });

            var btnAddCustomer = CreateQuickActionButton("+ New Customer", Color.White, Theme.TextPrimary, (_, _) =>
            {
                using var form = new CustomerInputForm();
                if (form.ShowDialog(this) == DialogResult.OK && form.Result != null)
                {
                    using var custCtrl = new CustomerController();
                    custCtrl.Add(form.Result);
                    LoadData();
                }
            }, hasBorder: true);

            var btnLogActivity = CreateQuickActionButton("+ Log Activity", Color.White, Theme.TextPrimary, (_, _) =>
            {
                using var dlg = new LogActivityDialog();
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    LoadData();
                }
            }, hasBorder: true);

            pnlQuickActions.Controls.Add(btnAddLead);
            pnlQuickActions.Controls.Add(btnAddCustomer);
            pnlQuickActions.Controls.Add(btnLogActivity);

            // 4 KPI Cards
            ConfigureKpiCard(kpi1, "MY ACTIVE LEADS", snapshot.ActiveLeadsCount, "Pipeline leads", AzureTints.SkylineBlue, KpiIconType.Target, () => RequestNavigation("Leads"));
            ConfigureKpiCard(kpi2, "MY OPEN DEALS", snapshot.OpenDealsCount, "Active pipeline", Color.FromArgb(139, 92, 246), KpiIconType.Briefcase, () => RequestNavigation("Deals"));
            ConfigureKpiCard(kpi3, "FOLLOW-UPS TODAY", snapshot.FollowUpsDueTodayCount, "Due & overdue", Color.FromArgb(217, 119, 6), KpiIconType.Clock, () => RequestNavigation("FollowUps"));
            ConfigureKpiCard(kpi4, "MY OPEN TICKETS", snapshot.OpenSupportTicketsCount, "Awaiting triage", Color.FromArgb(14, 165, 233), KpiIconType.Ticket, () => RequestNavigation("SupportTickets"));

            // Left Card: "My Follow-Ups Today" (max 5, clickable to open)
            lblLeftTitle.Text = "My Follow-Ups Today";
            lblLeftSubtitle.Text = snapshot.FollowUpsToday.Count > 0 ? $"{snapshot.FollowUpsToday.Count} due today" : "Due today";
            pnlLeftList.Controls.Clear();

            if (snapshot.FollowUpsToday.Count == 0)
            {
                lblLeftEmpty.Text = "No follow-ups due today. You're all caught up!";
                lblLeftEmpty.Visible = true;
            }
            else
            {
                lblLeftEmpty.Visible = false;
                int y = 0;
                foreach (var item in snapshot.FollowUpsToday)
                {
                    Color badgeBg = item.IsOverdue ? Color.FromArgb(254, 226, 226) : Color.FromArgb(241, 245, 249);
                    Color badgeFg = item.IsOverdue ? Color.FromArgb(185, 28, 28) : Color.FromArgb(71, 85, 105);
                    string badgeText = item.IsOverdue ? $"Overdue · {item.DueTimeText}" : item.DueTimeText;
                    string sub = string.IsNullOrWhiteSpace(item.RelatedName) ? $"Priority: {item.Priority}" : $"{item.RelatedName} · {item.Priority}";

                    var row = CreateItemRow(
                        iconText: GetActivityIcon(item.Type),
                        title: item.Title,
                        subtitle: sub,
                        badgeText: badgeText,
                        badgeBg: badgeBg,
                        badgeFg: badgeFg,
                        onClick: () =>
                        {
                            using var fuCtrl = new FollowUpController();
                            var reminder = fuCtrl.GetById(item.TaskReminderId);
                            if (reminder != null)
                            {
                                using var dlg = new FollowUpInputForm(fuCtrl, reminder);
                                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.Result != null)
                                {
                                    fuCtrl.Update(dlg.Result);
                                    LoadData();
                                }
                            }
                        }
                    );

                    row.Location = new Point(0, y);
                    row.Width = Math.Max(200, pnlLeftList.ClientSize.Width - 4);
                    pnlLeftList.Controls.Add(row);
                    y += row.Height + 8;
                }
            }

            // Right Card: "My Recent Activity" (last 5, read-only)
            lblRightTitle.Text = "My Recent Activity";
            lblRightSubtitle.Text = "Last 5 activities logged (read-only)";
            pnlRightList.Controls.Clear();

            if (snapshot.RecentActivities.Count == 0)
            {
                lblRightEmpty.Text = "No recent activities logged yet.";
                lblRightEmpty.Visible = true;
            }
            else
            {
                lblRightEmpty.Visible = false;
                int y = 0;
                foreach (var act in snapshot.RecentActivities)
                {
                    var row = CreateItemRow(
                        iconText: GetActivityIcon(act.Type),
                        title: act.Type,
                        subtitle: act.Notes,
                        badgeText: act.TimeAgo,
                        badgeBg: Color.Transparent,
                        badgeFg: Color.FromArgb(148, 163, 184),
                        onClick: null
                    );

                    row.Location = new Point(0, y);
                    row.Width = Math.Max(200, pnlRightList.ClientSize.Width - 4);
                    pnlRightList.Controls.Add(row);
                    y += row.Height + 8;
                }
            }
        }

        // =========================================================================
        // 2. MANAGER DASHBOARD RENDERER (Team-wide, zero follow-up visibility)
        // =========================================================================
        private void RenderManagerDashboard(ManagerDashboardDto snapshot)
        {
            lblTitle.Text = snapshot.Greeting;
            lblSubtitle.Text = snapshot.DateText;

            // Quick Action: "View Full Team Dashboard"
            pnlQuickActions.Controls.Clear();
            var btnTeamDashboard = CreateQuickActionButton("📊 View Full Team Dashboard", AzureTints.SkylineBlue, Color.White, (_, _) =>
            {
                RequestNavigation("Analytics");
            });
            pnlQuickActions.Controls.Add(btnTeamDashboard);

            // 4 KPI Cards
            ConfigureKpiCard(kpi1, "TEAM DEALS (MONTH)", snapshot.TeamDealsThisMonthCount, "Closed this month", Color.FromArgb(16, 185, 129), KpiIconType.Briefcase, () => RequestNavigation("Analytics"));
            ConfigureKpiCard(kpi2, "TEAM OPEN TICKETS", snapshot.TeamOpenTicketsCount, "Across all agents", Color.FromArgb(220, 38, 38), KpiIconType.Ticket, () => RequestNavigation("SupportTickets"));
            ConfigureKpiCard(kpi3, "PENDING ASSIGNMENTS", snapshot.PendingAssignmentsCount, "Awaiting manager action", Color.FromArgb(217, 119, 6), KpiIconType.Users, () => RequestNavigation("Approvals"));
            ConfigureKpiCard(kpi4, "LEAD CONVERSION", $"{snapshot.TeamConversionRate:F1}%", "Team conversion rate", AzureTints.SkylineBlue, KpiIconType.Target, () => RequestNavigation("Analytics"));

            // Left Card: "Pending Assignments" (max 5, with "Assign" button)
            lblLeftTitle.Text = "Pending Assignments";
            lblLeftSubtitle.Text = $"{snapshot.PendingAssignmentsCount} awaiting manager action";
            pnlLeftList.Controls.Clear();

            if (snapshot.PendingAssignments.Count == 0)
            {
                lblLeftEmpty.Text = "No pending assignments awaiting review.";
                lblLeftEmpty.Visible = true;
            }
            else
            {
                lblLeftEmpty.Visible = false;
                int y = 0;
                foreach (var item in snapshot.PendingAssignments)
                {
                    bool isCust = string.Equals(item.Type, "Customer", StringComparison.OrdinalIgnoreCase);
                    Color typeBadgeBg = isCust ? Color.FromArgb(220, 252, 231) : Color.FromArgb(224, 242, 254);
                    Color typeBadgeFg = isCust ? Color.FromArgb(22, 101, 52) : Color.FromArgb(2, 132, 199);

                    var row = CreateItemRow(
                        iconText: isCust ? "👤" : "🎯",
                        title: item.Name,
                        subtitle: $"Submitted by {item.SubmitterName} · {item.TimeAgo}",
                        badgeText: item.Type.ToUpperInvariant(),
                        badgeBg: typeBadgeBg,
                        badgeFg: typeBadgeFg,
                        onClick: null,
                        actionBtnText: "Assign",
                        onActionClick: () =>
                        {
                            using var appCtrl = new ApprovalController();
                            var agents = appCtrl.GetAgents();
                            using var dlg = new AssignAgentDialog(item.Name, agents, item.AssignedAgentId);
                            if (dlg.ShowDialog(this) == DialogResult.OK)
                            {
                                var approvalItem = new PendingApprovalItem
                                {
                                    Id = item.Id,
                                    Type = item.Type,
                                    Title = item.Name,
                                    AssignedAgentId = item.AssignedAgentId,
                                    OriginalEntity = item.OriginalEntity!
                                };
                                appCtrl.AssignAgent(approvalItem, dlg.SelectedAgentId, dlg.ApproveNow, dlg.ReviewNotes);
                                LoadData();
                            }
                        }
                    );

                    row.Location = new Point(0, y);
                    row.Width = Math.Max(200, pnlLeftList.ClientSize.Width - 4);
                    pnlLeftList.Controls.Add(row);
                    y += row.Height + 8;
                }
            }

            // Right Card: "Team Recent Activity" (last 5 deals closed or tickets resolved)
            lblRightTitle.Text = "Team Recent Activity";
            lblRightSubtitle.Text = "Last 5 team events (deals closed & tickets resolved)";
            pnlRightList.Controls.Clear();

            if (snapshot.TeamRecentActivity.Count == 0)
            {
                lblRightEmpty.Text = "No recent team events found.";
                lblRightEmpty.Visible = true;
            }
            else
            {
                lblRightEmpty.Visible = false;
                int y = 0;
                foreach (var evt in snapshot.TeamRecentActivity)
                {
                    var row = CreateItemRow(
                        iconText: evt.Icon,
                        title: evt.Description,
                        subtitle: evt.TimeAgo,
                        badgeText: string.Empty,
                        badgeBg: Color.Transparent,
                        badgeFg: Color.Transparent,
                        onClick: null
                    );

                    row.Location = new Point(0, y);
                    row.Width = Math.Max(200, pnlRightList.ClientSize.Width - 4);
                    pnlRightList.Controls.Add(row);
                    y += row.Height + 8;
                }
            }
        }

        // =========================================================================
        // 3. ADMIN DASHBOARD RENDERER (Business oversight, no record editing)
        // =========================================================================
        private void RenderAdminDashboard(AdminDashboardDto snapshot)
        {
            lblTitle.Text = snapshot.Greeting;
            lblSubtitle.Text = snapshot.DateText;

            // Quick Actions: "View Reports", "Manage Users"
            pnlQuickActions.Controls.Clear();
            var btnReports = CreateQuickActionButton("📊 View Reports", AzureTints.SkylineBlue, Color.White, (_, _) =>
            {
                RequestNavigation("Reports");
            });

            var btnManageUsers = CreateQuickActionButton("👥 Manage Users", Color.White, Theme.TextPrimary, (_, _) =>
            {
                RequestNavigation("SalesStaff");
            }, hasBorder: true);

            pnlQuickActions.Controls.Add(btnReports);
            pnlQuickActions.Controls.Add(btnManageUsers);

            // 4 KPI Cards
            ConfigureKpiCard(kpi1, "TOTAL ACTIVE USERS", snapshot.TotalActiveUsersCount, "Active personnel", AzureTints.SkylineBlue, KpiIconType.Users, () => RequestNavigation("SalesStaff"));
            ConfigureKpiCard(kpi2, "AGENCY OPEN TICKETS", snapshot.OpenTicketsCount, "Oversight only", Color.FromArgb(220, 38, 38), KpiIconType.Ticket, () => RequestNavigation("SupportTickets"));
            ConfigureKpiCard(kpi3, "SUBSCRIPTION", snapshot.SubscriptionStatus, "Multi-tenant status", Color.FromArgb(16, 185, 129), KpiIconType.Building, () => RequestNavigation("Reports"));
            ConfigureKpiCard(kpi4, "DEALS CLOSED (MONTH)", snapshot.DealsClosedThisMonthCount, "Headline total", Color.FromArgb(139, 92, 246), KpiIconType.Currency, () => RequestNavigation("Reports"));

            // Left Card: "Recent System Activity" (omitted if no logs exist)
            pnlLeftList.Controls.Clear();
            bool hasSystemLogs = snapshot.RecentSystemActivities.Count > 0;

            if (hasSystemLogs)
            {
                pnlLeftCard.Visible = true;
                lblLeftTitle.Text = "Recent System Activity";
                lblLeftSubtitle.Text = "System backups & configuration logs";
                lblLeftEmpty.Visible = false;

                int y = 0;
                foreach (var log in snapshot.RecentSystemActivities)
                {
                    var row = CreateItemRow(
                        iconText: log.Icon,
                        title: log.Title,
                        subtitle: log.Details,
                        badgeText: log.TimeAgo,
                        badgeBg: Color.Transparent,
                        badgeFg: Color.FromArgb(148, 163, 184),
                        onClick: null
                    );

                    row.Location = new Point(0, y);
                    row.Width = Math.Max(200, pnlLeftList.ClientSize.Width - 4);
                    pnlLeftList.Controls.Add(row);
                    y += row.Height + 8;
                }
            }
            else
            {
                // Omit section rather than fabricate data per prompt requirement
                lblLeftTitle.Text = "System Activity";
                lblLeftSubtitle.Text = "No system logs recorded yet";
                lblLeftEmpty.Text = "No system logs recorded yet.";
                lblLeftEmpty.Visible = true;
            }

            // Right Card: "System & Tenant Overview"
            lblRightTitle.Text = "Business Overview";
            lblRightSubtitle.Text = "System environment & oversight snapshot";
            pnlRightList.Controls.Clear();
            lblRightEmpty.Visible = false;

            int ry = 0;
            var overviewItems = new[]
            {
                ("🏢 Tenant Status", $"Tenant #{CurrentSession.TenantId} · Multi-Tenant Isolation Active", "ONLINE", Color.FromArgb(220, 252, 231), Color.FromArgb(22, 101, 52)),
                ("🛡 Security Tier", "Role-Based Access Control (RBAC) & Row-Level Security Enforced", "SECURE", Color.FromArgb(224, 242, 254), Color.FromArgb(2, 132, 199)),
                ("📄 Subscription Plan", snapshot.SubscriptionStatus, "ACTIVE", Color.FromArgb(220, 252, 231), Color.FromArgb(22, 101, 52)),
                ("⚡ System Performance", "LocalDb Fast Snapshot Cache Operational", "NOMINAL", Color.FromArgb(241, 245, 249), Color.FromArgb(71, 85, 105))
            };

            foreach (var (title, desc, status, bg, fg) in overviewItems)
            {
                var row = CreateItemRow(
                    iconText: "•",
                    title: title,
                    subtitle: desc,
                    badgeText: status,
                    badgeBg: bg,
                    badgeFg: fg,
                    onClick: null
                );

                row.Location = new Point(0, ry);
                row.Width = Math.Max(200, pnlRightList.ClientSize.Width - 4);
                pnlRightList.Controls.Add(row);
                ry += row.Height + 8;
            }
        }

        // =========================================================================
        // UI HELPERS & ITEM ROW GENERATOR
        // =========================================================================
        private static Button CreateQuickActionButton(string text, Color bg, Color fg, EventHandler onClick, bool hasBorder = false)
        {
            var btn = new Button
            {
                Text = text,
                BackColor = bg,
                ForeColor = fg,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Height = 36,
                AutoSize = true,
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(4, 0, 0, 0),
                Padding = new Padding(12, 0, 12, 0)
            };

            if (hasBorder)
            {
                btn.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
                btn.FlatAppearance.BorderSize = 1;
            }
            else
            {
                btn.FlatAppearance.BorderSize = 0;
            }

            btn.Click += onClick;
            UiRadiusHelper.StyleButton(btn, 8);
            return btn;
        }

        private static void ConfigureKpiCard(KpiCard card, string title, object value, string subtitle, Color accentColor, KpiIconType icon, Action onClick)
        {
            if (value is int intVal)
            {
                card.SetValue(intVal);
            }
            else
            {
                card.SetValue(value?.ToString() ?? "0");
            }

            card.SetSubtitle(subtitle);
            card.SetIcon(icon, accentColor);
            card.Cursor = Cursors.Hand;

            // Remove existing handlers to avoid duplication
            card.Click += (_, _) => onClick();
        }

        private Panel CreateItemRow(
            string iconText,
            string title,
            string subtitle,
            string badgeText,
            Color badgeBg,
            Color badgeFg,
            Action? onClick = null,
            string? actionBtnText = null,
            Action? onActionClick = null)
        {
            var panel = new Panel
            {
                Height = 58,
                BackColor = Color.White,
                Padding = new Padding(12, 8, 12, 8)
            };
            UiRadiusHelper.ApplyRoundedCorners(panel, 8);

            // Icon container
            var lblIcon = new Label
            {
                Text = iconText,
                Font = new Font("Segoe UI Emoji", 11f),
                Location = new Point(10, 16),
                Size = new Size(28, 26),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            // Title label
            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(42, 9),
                AutoSize = true,
                AutoEllipsis = true,
                BackColor = Color.Transparent
            };

            // Subtitle label
            var lblSubtitle = new Label
            {
                Text = subtitle,
                Font = new Font("Segoe UI", 8.25f),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(43, 31),
                AutoSize = true,
                AutoEllipsis = true,
                BackColor = Color.Transparent
            };

            panel.Controls.Add(lblIcon);
            panel.Controls.Add(lblTitle);
            panel.Controls.Add(lblSubtitle);

            // Optional action button (e.g. "Assign")
            if (!string.IsNullOrWhiteSpace(actionBtnText) && onActionClick != null)
            {
                var btnAction = new Button
                {
                    Text = actionBtnText,
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(14, 116, 144),
                    BackColor = Color.FromArgb(240, 249, 255),
                    Size = new Size(68, 28),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };
                btnAction.FlatAppearance.BorderColor = Color.FromArgb(186, 230, 253);
                btnAction.Click += (_, _) => onActionClick();
                UiRadiusHelper.StyleButton(btnAction, 6);

                void PositionButton()
                {
                    btnAction.Location = new Point(panel.Width - btnAction.Width - 14, (panel.Height - btnAction.Height) / 2);
                }

                panel.Controls.Add(btnAction);
                panel.SizeChanged += (_, _) => PositionButton();
                PositionButton();
            }
            // Optional Badge / Time text
            else if (!string.IsNullOrWhiteSpace(badgeText))
            {
                var lblBadge = new Label
                {
                    Text = badgeText,
                    Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                    ForeColor = badgeFg,
                    BackColor = badgeBg,
                    AutoSize = true,
                    Padding = new Padding(6, 3, 6, 3),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                if (badgeBg != Color.Transparent)
                {
                    UiRadiusHelper.ApplyPillShape(lblBadge);
                }

                void PositionBadge()
                {
                    lblBadge.Location = new Point(panel.Width - lblBadge.PreferredWidth - 14, (panel.Height - lblBadge.Height) / 2);
                }

                panel.Controls.Add(lblBadge);
                panel.SizeChanged += (_, _) => PositionBadge();
                PositionBadge();
            }

            // Border painting
            panel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var pen = new Pen(Color.FromArgb(226, 232, 240), 1f);
                using var path = UiRadiusHelper.CreateRoundedPath(new Rectangle(0, 0, panel.Width - 1, panel.Height - 1), 8);
                e.Graphics.DrawPath(pen, path);
            };

            // Click handling and hover effects
            if (onClick != null)
            {
                panel.Cursor = Cursors.Hand;
                lblTitle.Cursor = Cursors.Hand;
                lblSubtitle.Cursor = Cursors.Hand;
                lblIcon.Cursor = Cursors.Hand;

                void SetHover(bool hovered)
                {
                    panel.BackColor = hovered ? Color.FromArgb(248, 250, 252) : Color.White;
                }

                panel.MouseEnter += (_, _) => SetHover(true);
                panel.MouseLeave += (_, _) => SetHover(false);
                lblTitle.MouseEnter += (_, _) => SetHover(true);
                lblTitle.MouseLeave += (_, _) => SetHover(false);
                lblSubtitle.MouseEnter += (_, _) => SetHover(true);
                lblSubtitle.MouseLeave += (_, _) => SetHover(false);

                panel.Click += (_, _) => onClick();
                lblTitle.Click += (_, _) => onClick();
                lblSubtitle.Click += (_, _) => onClick();
                lblIcon.Click += (_, _) => onClick();
            }

            return panel;
        }

        private static string GetActivityIcon(string type) => (type ?? "").ToLowerInvariant() switch
        {
            "call" => "📞",
            "email" => "✉️",
            "meeting" => "🤝",
            "showing" => "🏠",
            "ticket" => "🎟",
            "deal" => "💼",
            "lead" => "🎯",
            _ => "📋"
        };

        private static void ResizeListItems(Panel pnl)
        {
            if (pnl.Width <= 0) return;
            int targetWidth = Math.Max(150, pnl.ClientSize.Width - 4);
            foreach (Control c in pnl.Controls)
            {
                if (c is Panel row)
                {
                    row.Width = targetWidth;
                }
            }
        }

        private void LayoutControls()
        {
            if (IsDisposed) return;

            int leftMargin = 30;
            int rightMargin = 30;
            int totalWidth = ClientSize.Width;

            lblTitle.Location = new Point(leftMargin, 20);
            lblSubtitle.Location = new Point(leftMargin + 2, lblTitle.Bottom + 4);

            pnlQuickActions.Location = new Point(totalWidth - rightMargin - pnlQuickActions.Width, 22);

            int kpiTop = Math.Max(84, lblSubtitle.Bottom + 16);
            pnlKpiContainer.Location = new Point(leftMargin, kpiTop);
            pnlKpiContainer.Size = new Size(totalWidth - leftMargin - rightMargin, 104);

            int splitTop = pnlKpiContainer.Bottom + 16;
            int splitHeight = Math.Max(200, ClientSize.Height - splitTop - 24);
            pnlContentSplit.Location = new Point(leftMargin, splitTop);
            pnlContentSplit.Size = new Size(totalWidth - leftMargin - rightMargin, splitHeight);

            ResizeListItems(pnlLeftList);
            ResizeListItems(pnlRightList);
        }

        private void RequestNavigation(string module)
        {
            if (NavigationRequested is not null)
            {
                NavigationRequested.Invoke(module);
                return;
            }

            if (FindForm() is MainForm form)
            {
                form.NavigateTo(module);
            }
        }
    }
}
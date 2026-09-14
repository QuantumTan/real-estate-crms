using CRMS_Peguit.winforms.Auth;
using CRMS_Peguit.winforms.Views.Dashboard;
using CRMS_Peguit.winforms.Views.Customers;
using CRMS_Peguit.winforms.Views.Leads;
using CRMS_Peguit.winforms.Views.Properties;
using CRMS_Peguit.winforms.Views.Deals;
using CRMS_Peguit.winforms.Views.Marketing;
using CRMS_Peguit.winforms.Views.Shared;
using CRMS_Peguit.winforms.Views.Users;
using CRMS_Peguit.Models;
using CRMS_Peguit.winforms.Models.Services;
using CRMS_Peguit.winforms.Services;
using ReaLTaiizor.Forms;
using System.Linq;

namespace CRMS_Peguit.winforms
{
    public partial class MainForm : Form
    {
        private Button? _activeNavButton;
        private readonly List<Button> _navButtons = new();
        private readonly Dictionary<Button, (string Icon, string Title)> _navButtonInfo = new();
        private bool _sidebarCollapsed = false;
        private System.Windows.Forms.Timer? _sidebarAnimationTimer;
        private int _targetSidebarWidth = 240;

        // Global search debounce + floating dropdown
        private System.Windows.Forms.Timer? _searchDebounce;
        private ToolStripDropDown? _searchDropDown;

        public MainForm()
        {
            InitializeComponent();
            ApplyBranding();
            ApplyTheme();
            InitNavButtons();
            BindEvents();
            ApplyRolePermissions();
            SetActiveNavButton(btnDashboard);
            BtnDashboardClick(btnDashboard, EventArgs.Empty);
        }

        private void ApplyBranding()
        {
            AppBrand.ApplyAppIcon(this);
            if (AppBrand.Logo != null)
            {
                picLogo.Image = AppBrand.Logo;
            }

            pnlLogoHeader.Cursor = Cursors.Hand;
            picLogo.Cursor = Cursors.Hand;
            lblLogo.Cursor = Cursors.Hand;
            EventHandler navDashboard = (s, e) => { SetActiveNavButton(btnDashboard); BtnDashboardClick(s, e); };
            picLogo.Click += navDashboard;
            lblLogo.Click += navDashboard;
            pnlLogoHeader.Click += navDashboard;
            mainToolTip.SetToolTip(pnlLogoHeader, "NEXA CRM SYSTEM — Go to Dashboard");
        }

        private void InitNavButtons()
        {
            _navButtonInfo[btnDashboard] = ("⊞", "Dashboard");
            _navButtonInfo[btnLeads] = ("◎", "Leads");
            _navButtonInfo[btnCustomers] = ("👥", "Customers");
            _navButtonInfo[btnProperties] = ("🏢", "Properties");
            _navButtonInfo[btnDeals] = ("💼", "Deals");
            _navButtonInfo[btnCampaigns] = ("📣", "Campaigns");
            _navButtonInfo[btnActivities] = ("📈", "Activities");
            _navButtonInfo[btnFollowUps] = ("⏱", "Follow-Ups");
            _navButtonInfo[btnSupportTickets] = ("🎟", "Support Tickets");
            _navButtonInfo[btnReports] = ("📊", "Reports");
            _navButtonInfo[btnApprovals] = ("✓", "Approvals & Review");
            _navButtonInfo[btnManageManagers] = ("🛡", "Manage Managers");
            _navButtonInfo[btnManageAgents] = ("👥", "Manage Agents");

            _navButtons.AddRange(_navButtonInfo.Keys);

            foreach (var btn in _navButtons)
            {
                UiRadiusHelper.StyleButton(btn, 6);
                var info = _navButtonInfo[btn];
                mainToolTip.SetToolTip(btn, info.Title);
                btn.Text = $"   {info.Icon,-2}   {info.Title}";
                btn.Padding = new Padding(16, 0, 0, 0);
                btn.TextAlign = ContentAlignment.MiddleLeft;

                btn.Paint += (s, e) =>
                {
                    if (s is Button b && b == _activeNavButton)
                    {
                        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        using var accentBrush = new SolidBrush(Color.FromArgb(96, 165, 250));
                        using var accentPath = UiRadiusHelper.CreateRoundedPath(new Rectangle(2, 6, 4, b.Height - 12), 2);
                        e.Graphics.FillPath(accentBrush, accentPath);
                    }
                };

                btn.MouseEnter += (s, e) =>
                {
                    if (s is Button b && b != _activeNavButton)
                        b.BackColor = Theme.SidebarHover;
                };
                btn.MouseLeave += (s, e) =>
                {
                    if (s is Button b && b != _activeNavButton)
                        b.BackColor = Color.Transparent;
                };
            }
        }

        private void SetActiveNavButton(Button button)
        {
            _activeNavButton = button;
            foreach (var btn in _navButtons)
            {
                if (btn == button)
                {
                    btn.BackColor = Theme.SidebarSelected;
                    btn.ForeColor = Color.White;
                }
                else
                {
                    btn.BackColor = Color.Transparent;
                    btn.ForeColor = Theme.SidebarText;
                }
                btn.Invalidate();
            }
        }

        private void BindEvents()
        {
            btnToggleSidebar.Click += (_, _) => ToggleSidebar();
            mainToolTip.SetToolTip(btnToggleSidebar, "Toggle Sidebar (Ctrl+B)");
            mainToolTip.SetToolTip(lblBellIcon, "System Status & Notifications");
            mainToolTip.SetToolTip(txtGlobalSearch, "Global search (Ctrl+K or Ctrl+F)");
            mainToolTip.SetToolTip(lblStatusDot, "Online — Active Session");
            mainToolTip.SetToolTip(lblRoleBadge, "Current Role Scope");

            btnLogout.Click += BtnLogoutClick;
            btnDashboard.Click += (s, e) => { SetActiveNavButton(btnDashboard); BtnDashboardClick(s, e); };
            btnManageManagers.Click += (s, e) => { SetActiveNavButton(btnManageManagers); BtnManageManagersClick(s, e); };
            btnManageAgents.Click += (s, e) => { SetActiveNavButton(btnManageAgents); BtnManageAgentsClick(s, e); };
            btnCustomers.Click += (s, e) => { SetActiveNavButton(btnCustomers); BtnCustomersClick(s, e); };
            btnLeads.Click += (s, e) => { SetActiveNavButton(btnLeads); BtnLeadsClick(s, e); };
            btnProperties.Click += (s, e) => { SetActiveNavButton(btnProperties); BtnPropertiesClick(s, e); };
            btnDeals.Click += (s, e) => { SetActiveNavButton(btnDeals); BtnDealsClick(s, e); };
            btnCampaigns.Click += (s, e) => { SetActiveNavButton(btnCampaigns); BtnCampaignsClick(s, e); };
            btnActivities.Click += (s, e) => { SetActiveNavButton(btnActivities); BtnActivitiesClick(s, e); };
            btnFollowUps.Click += (s, e) => { SetActiveNavButton(btnFollowUps); BtnFollowUpsClick(s, e); };
            btnReports.Click += (s, e) => { SetActiveNavButton(btnReports); BtnReportsClick(s, e); };
            btnApprovals.Click += (s, e) => { SetActiveNavButton(btnApprovals); BtnApprovalsClick(s, e); };
            btnSupportTickets.Click += (s, e) => { SetActiveNavButton(btnSupportTickets); BtnSupportTicketsClick(s, e); };

            lblBellIcon.Click += (_, _) => ShowNotificationMenu();

            // ── Global Search: debounced TextChanged → floating results dropdown ──
            _searchDebounce = new System.Windows.Forms.Timer { Interval = 300 };
            _searchDebounce.Tick += (_, _) =>
            {
                _searchDebounce.Stop();
                ShowGlobalSearchResults();
            };

            txtGlobalSearch.TextChanged += (_, _) =>
            {
                _searchDebounce.Stop();
                if (string.IsNullOrWhiteSpace(txtGlobalSearch.Text))
                {
                    DismissSearchDropDown();
                    return;
                }
                _searchDebounce.Start();
            };

            txtGlobalSearch.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    DismissSearchDropDown();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Enter && _searchDropDown?.Items.Count > 0)
                {
                    // Activate top result
                    (_searchDropDown.Items[0] as ToolStripMenuItem)?.PerformClick();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            };

            txtGlobalSearch.Leave += (_, _) =>
            {
                // Small delay so clicking a result registers before the dropdown closes
                Task.Delay(200).ContinueWith(_ => BeginInvoke(DismissSearchDropDown));
            };

        }

        private void ToggleSidebar()
        {
            _sidebarCollapsed = !_sidebarCollapsed;
            _targetSidebarWidth = _sidebarCollapsed ? 64 : 240;

            if (_sidebarCollapsed)
            {
                lblLogo.Visible = false;
                pnlUserContainer.Visible = false;
                lblSalesSection.Visible = false;
                lblSupportSection.Visible = false;
                lblInsightsSection.Visible = false;
                lblAdminSection.Visible = false;
                foreach (var btn in _navButtons)
                {
                    if (_navButtonInfo.TryGetValue(btn, out var info))
                    {
                        btn.Text = info.Icon;
                        btn.Padding = new Padding(0);
                        btn.TextAlign = ContentAlignment.MiddleCenter;
                    }
                }
                btnLogout.Text = "↪";
                btnLogout.Padding = new Padding(0);
                btnLogout.TextAlign = ContentAlignment.MiddleCenter;
                mainToolTip.SetToolTip(btnLogout, "Sign out");
                mainToolTip.SetToolTip(picLogo, "NEXA CRM SYSTEM");
            }

            _sidebarAnimationTimer?.Stop();
            _sidebarAnimationTimer?.Dispose();
            _sidebarAnimationTimer = new System.Windows.Forms.Timer { Interval = 10 };
            _sidebarAnimationTimer.Tick += (_, _) =>
            {
                int diff = _targetSidebarWidth - sidebarPanel.Width;
                int step = Math.Sign(diff) * Math.Max(16, Math.Abs(diff) / 2);
                if (Math.Abs(diff) <= Math.Abs(step))
                {
                    sidebarPanel.Width = _targetSidebarWidth;
                    _sidebarAnimationTimer.Stop();
                    _sidebarAnimationTimer.Dispose();
                    _sidebarAnimationTimer = null;
                    FinalizeSidebarState();
                }
                else
                {
                    sidebarPanel.Width += step;
                }
            };
            _sidebarAnimationTimer.Start();
        }

        private void FinalizeSidebarState()
        {
            if (_sidebarCollapsed)
            {
                picLogo.Location = new Point((64 - picLogo.Width) / 2, 9);
            }
            else
            {
                picLogo.Location = new Point(14, 9);
                lblLogo.Visible = true;
                mainToolTip.SetToolTip(picLogo, null);
                pnlUserContainer.Visible = true;
                lblSalesSection.Visible = true;
                lblSupportSection.Visible = true;
                lblInsightsSection.Visible = true;
                lblAdminSection.Visible = btnManageManagers.Visible || btnManageAgents.Visible || btnApprovals.Visible;
                foreach (var btn in _navButtons)
                {
                    if (_navButtonInfo.TryGetValue(btn, out var info))
                    {
                        btn.Text = $"   {info.Icon,-2}   {info.Title}";
                        btn.Padding = new Padding(16, 0, 0, 0);
                        btn.TextAlign = ContentAlignment.MiddleLeft;
                    }
                }
                btnLogout.Text = "   ↪   Sign out";
                btnLogout.Padding = new Padding(16, 0, 0, 0);
                btnLogout.TextAlign = ContentAlignment.MiddleLeft;
                mainToolTip.SetToolTip(btnLogout, null);
            }
        }

        private void ShowNotificationMenu()
        {
            var menu = new ContextMenuStrip
            {
                BackColor = Theme.Surface,
                ForeColor = Theme.TextPrimary,
                Font = new Font("Segoe UI", 9.5f)
            };

            var user = CurrentSession.CurrentUser;
            var header = new ToolStripMenuItem($"Session: {user?.FullName ?? "User"} ({user?.GetDashboardType() ?? "Active"})")
            {
                Enabled = false,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            menu.Items.Add(header);
            menu.Items.Add(new ToolStripSeparator());

            var statusItem = new ToolStripMenuItem("🟢 System Status: Connected & Operational")
            {
                Enabled = false
            };
            menu.Items.Add(statusItem);

            if (CurrentSession.CanAccess("Approvals") && RbacService.CanApproveAssignments)
            {
                var approvalsItem = new ToolStripMenuItem("📋 Pending Approvals — Click to Review");
                approvalsItem.Click += (_, _) => NavigateTo("approvals");
                menu.Items.Add(approvalsItem);
            }

            var quickAction = new ToolStripMenuItem("🔍 Jump to Search (Ctrl+K or Ctrl+F)");
            quickAction.Click += (_, _) => txtGlobalSearch.Focus();
            menu.Items.Add(quickAction);

            menu.Show(lblBellIcon, new Point(lblBellIcon.Width - 260, lblBellIcon.Height + 4));
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.B))
            {
                ToggleSidebar();
                return true;
            }
            if (keyData == (Keys.Control | Keys.K) || keyData == (Keys.Control | Keys.F))
            {
                txtGlobalSearch.Focus();
                txtGlobalSearch.SelectAll();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        // =====================================================
        // GLOBAL SEARCH — floating results dropdown
        // =====================================================

        private void ShowGlobalSearchResults()
        {
            string query = txtGlobalSearch.Text.Trim();
            if (string.IsNullOrWhiteSpace(query)) return;

            DismissSearchDropDown();

            var results = GlobalSearchService.Search(query, maxPerModule: 5);

            _searchDropDown = new ToolStripDropDown
            {
                AutoClose = false,
                BackColor = Color.White,
                Padding = new Padding(0, 4, 0, 4)
            };
            _searchDropDown.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;

            if (results.Count == 0)
            {
                var emptyItem = new ToolStripMenuItem($"🔍  No results found for \"{query}\"")
                {
                    Enabled = false,
                    Font = new Font("Segoe UI", 9.5f),
                    ForeColor = Color.FromArgb(100, 116, 139),
                    BackColor = Color.White
                };
                _searchDropDown.Items.Add(emptyItem);
            }
            else
            {
                string? lastModule = null;
                foreach (var r in results)
                {
                    // Module header separator
                    if (r.Module != lastModule)
                    {
                        if (lastModule != null) _searchDropDown.Items.Add(new ToolStripSeparator());

                        var header = new ToolStripMenuItem(GetModuleLabel(r.Module))
                        {
                            Enabled = false,
                            Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                            ForeColor = Color.FromArgb(100, 116, 139),
                            BackColor = Color.White,
                            Padding = new Padding(14, 2, 8, 2)
                        };
                        _searchDropDown.Items.Add(header);
                        lastModule = r.Module;
                    }

                    var result = r; // capture for lambda
                    var item = new ToolStripMenuItem
                    {
                        Text = $"  {result.Icon}  {result.Title}",
                        ToolTipText = result.Subtitle,
                        Font = new Font("Segoe UI", 9.5f),
                        ForeColor = Color.FromArgb(15, 23, 42),
                        BackColor = Color.White,
                        Padding = new Padding(12, 4, 12, 4),
                        AutoToolTip = false
                    };

                    // Show subtitle as secondary text by adding a label
                    var subtitleItem = new ToolStripMenuItem
                    {
                        Text = $"      {result.Subtitle}",
                        Enabled = false,
                        Font = new Font("Segoe UI", 8f),
                        ForeColor = Color.FromArgb(100, 116, 139),
                        BackColor = Color.White,
                        Padding = new Padding(12, 0, 12, 2)
                    };

                    item.Click += (_, _) =>
                    {
                        DismissSearchDropDown();
                        txtGlobalSearch.Clear();
                        NavigateTo(result.Module);
                    };

                    _searchDropDown.Items.Add(item);
                    _searchDropDown.Items.Add(subtitleItem);
                }
            }

            // Footer hint
            _searchDropDown.Items.Add(new ToolStripSeparator());
            var hint = new ToolStripMenuItem("↑↓ navigate · Enter to open · Esc to close")
            {
                Enabled = false,
                Font = new Font("Segoe UI", 7.5f),
                ForeColor = Color.FromArgb(148, 163, 184),
                BackColor = Color.White,
                Padding = new Padding(12, 2, 8, 2)
            };
            _searchDropDown.Items.Add(hint);

            // Style the dropdown container
            _searchDropDown.Width = Math.Max(txtGlobalSearch.Width, 340);

            var screenPt = txtGlobalSearch.PointToScreen(new Point(0, txtGlobalSearch.Height + 2));
            _searchDropDown.Show(screenPt);
        }

        private static string GetModuleLabel(string module) => module switch
        {
            "customers" => "CUSTOMERS",
            "leads"     => "LEADS",
            "properties"=> "PROPERTIES",
            "deals"     => "DEALS",
            _           => module.ToUpper()
        };

        private void DismissSearchDropDown()
        {
            _searchDropDown?.Close();
            _searchDropDown?.Dispose();
            _searchDropDown = null;
        }

        private void ApplyTheme()
        {
            sidebarPanel.BackColor = Theme.SidebarBackground;
            pnlUserProfile.BackColor = Theme.SidebarProfileCard;
            topHeaderPanel.BackColor = Theme.HeaderBackground;
            mainPanel.BackColor = Theme.Background;
            BackColor = Theme.Background;
            txtGlobalSearch.BackColor = AzureTints.BackgroundWash;
            txtGlobalSearch.ForeColor = Theme.TextPrimary;
            lblHeaderUserName.ForeColor = Theme.TextPrimary;
            lblHeaderAvatar.BackColor = AzureTints.SkylineBlue;
            lblHeaderAvatar.ForeColor = AzureTints.PureWhite;
            lblBellIcon.ForeColor = Theme.TextSecondary;

            // WCAG AA Compliant Section Headings (≥ 4.5:1 on dark sidebar)
            lblSalesSection.ForeColor = Theme.SidebarTextMuted;
            lblSupportSection.ForeColor = Theme.SidebarTextMuted;
            lblInsightsSection.ForeColor = Theme.SidebarTextMuted;
            lblAdminSection.ForeColor = Theme.SidebarTextMuted;

            UiRadiusHelper.ApplyRoundedCorners(pnlUserProfile, 10);
            UiRadiusHelper.MakeCircularAvatar(lblUserAvatar);
            UiRadiusHelper.MakeCircularAvatar(lblHeaderAvatar);
            UiRadiusHelper.MakeStatusDot(lblStatusDot, Color.FromArgb(34, 197, 94));
            UiRadiusHelper.SetPadding(txtGlobalSearch, 12, 12);
            UiRadiusHelper.StyleButton(btnLogout, 6);
            UiRadiusHelper.StyleButton(btnToggleSidebar, 6);

            topHeaderPanel.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(226, 232, 240), 1f);
                e.Graphics.DrawLine(pen, 0, topHeaderPanel.Height - 1, topHeaderPanel.Width, topHeaderPanel.Height - 1);

                // Soft multi-layered elevation shadow separating header and content
                using var shadowBrush1 = new SolidBrush(Color.FromArgb(12, 0, 0, 0));
                e.Graphics.FillRectangle(shadowBrush1, 0, topHeaderPanel.Height - 3, topHeaderPanel.Width, 1);
                using var shadowBrush2 = new SolidBrush(Color.FromArgb(6, 0, 0, 0));
                e.Graphics.FillRectangle(shadowBrush2, 0, topHeaderPanel.Height - 2, topHeaderPanel.Width, 1);
            };
        }

        // =====================================================
        // ROLE PERMISSIONS
        // =====================================================

        private void ApplyRolePermissions()
        {
            if (CurrentSession.CurrentUser is null)
            {
                MessageBox.Show(
                    "No active session. Please sign in again.",
                    "Session Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                Close();
                return;
            }

            var user = CurrentSession.CurrentUser;
            string initials = !string.IsNullOrWhiteSpace(user.FullName)
                ? string.Join("", user.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(s => s[0])).ToUpper()
                : "U";
            if (initials.Length > 2) initials = initials.Substring(0, 2);

            string roleDisplay = user.Role switch
            {
                CRMS_Peguit.winforms.Models.Roles.UserRole.SuperAdmin => "Super Admin",
                CRMS_Peguit.winforms.Models.Roles.UserRole.Admin => "Admin",
                CRMS_Peguit.winforms.Models.Roles.UserRole.Manager => "Manager",
                CRMS_Peguit.winforms.Models.Roles.UserRole.SalesStaff => "Sales Staff",
                _ => user.Role.ToString()
            };

            lblUserAvatar.Text = initials;
            lblUserName.Text = user.FullName;
            lblUserRole.Text = roleDisplay;

            lblHeaderAvatar.Text = initials;
            lblHeaderUserName.Text = $"{user.FullName}\r\n{roleDisplay}";
            lblRoleBadge.Text = $"• {roleDisplay}";
            var badgeSize = TextRenderer.MeasureText(lblRoleBadge.Text, lblRoleBadge.Font);
            lblRoleBadge.Width = Math.Max(72, badgeSize.Width + 16);
            UiRadiusHelper.ApplyPillShape(lblRoleBadge);
            txtGlobalSearch.Location = new Point(lblRoleBadge.Right + 12, 16);

            btnApprovals.Visible = CurrentSession.CanAccess("Approvals") && RbacService.CanApproveAssignments;
            btnManageManagers.Visible = CurrentSession.CanAccess("Managers");
            btnManageAgents.Visible = CurrentSession.CanAccess("SalesStaff");
            lblAdminSection.Text = RbacService.IsAdmin ? "ADMINISTRATION" : "MANAGEMENT";
            lblAdminSection.Visible = btnManageManagers.Visible || btnManageAgents.Visible || btnApprovals.Visible;

            btnCustomers.Visible = CurrentSession.CanAccess("Customers");
            btnLeads.Visible = CurrentSession.CanAccess("Leads");
            btnProperties.Visible = CurrentSession.CanAccess("Properties");
            btnDeals.Visible = CurrentSession.CanAccess("Deals");
            btnCampaigns.Visible = CurrentSession.CanAccess("Campaigns");
            btnActivities.Visible = CurrentSession.CanAccess("Activities");
            btnFollowUps.Visible = CurrentSession.CanAccess("TasksReminders");
            btnReports.Visible = CurrentSession.CanAccess("Reports");
            btnSupportTickets.Visible = CurrentSession.CanAccess("SupportTickets");

            Text = $"NEXA CRM SYSTEM — {user.FullName} ({roleDisplay})";
        }

        // =====================================================
        // VIEW MANAGEMENT
        // =====================================================

        private void ShowView(UserControl view)
        {
            foreach (Control control in mainPanel.Controls)
                control.Dispose();

            mainPanel.Controls.Clear();
            view.Dock = DockStyle.Fill;
            mainPanel.Controls.Add(view);
        }

        // =====================================================
        // NAVIGATION
        // =====================================================

        public void NavigateTo(string module)
        {
            switch (module.ToLowerInvariant())
            {
                case "customers":
                    if (!CurrentSession.CanAccess("Customers")) return;
                    SetActiveNavButton(btnCustomers);
                    BtnCustomersClick(btnCustomers, EventArgs.Empty);
                    break;
                case "leads":
                    if (!CurrentSession.CanAccess("Leads")) return;
                    SetActiveNavButton(btnLeads);
                    BtnLeadsClick(btnLeads, EventArgs.Empty);
                    break;
                case "properties":
                    if (!CurrentSession.CanAccess("Properties")) return;
                    SetActiveNavButton(btnProperties);
                    BtnPropertiesClick(btnProperties, EventArgs.Empty);
                    break;
                case "deals":
                    if (!CurrentSession.CanAccess("Deals")) return;
                    SetActiveNavButton(btnDeals);
                    BtnDealsClick(btnDeals, EventArgs.Empty);
                    break;
                case "campaigns":
                    if (!CurrentSession.CanAccess("Campaigns")) return;
                    SetActiveNavButton(btnCampaigns);
                    BtnCampaignsClick(btnCampaigns, EventArgs.Empty);
                    break;
                case "approvals":
                    if (!CurrentSession.CanAccess("Approvals") && !RbacService.CanApproveAssignments) return;
                    SetActiveNavButton(btnApprovals);
                    BtnApprovalsClick(btnApprovals, EventArgs.Empty);
                    break;
            }
        }

        private void BtnCampaignsClick(object? sender, EventArgs e)
        {
            if (!CurrentSession.CanAccess("Campaigns")) return;
            ShowView(new CampaignsView());
        }

        private void BtnDashboardClick(object? sender, EventArgs e)
        {
            var dashboard = new DashboardView();
            dashboard.NavigationRequested += module => NavigateTo(module);
            ShowView(dashboard);
        }

        private void BtnApprovalsClick(object? sender, EventArgs e)
        {
            if (!CurrentSession.CanAccess("Approvals") && !RbacService.CanApproveAssignments) return;
            ShowView(new CRMS_Peguit.winforms.Views.Management.ApprovalsView());
        }

        private void BtnManageManagersClick(object? sender, EventArgs e)
        {
            if (!CurrentSession.CanAccess("Managers")) return;
            ShowView(new AdminUserListForm("Manager"));
        }

        private void BtnManageAgentsClick(object? sender, EventArgs e)
        {
            if (!CurrentSession.CanAccess("SalesStaff")) return;
            ShowView(new AdminUserListForm("Agent"));
        }

        private void BtnCustomersClick(object? sender, EventArgs e)
        {
            if (!CurrentSession.CanAccess("Customers")) return;
            ShowView(new CustomersView());
        }

        private void BtnLeadsClick(object? sender, EventArgs e)
        {
            if (!CurrentSession.CanAccess("Leads")) return;
            ShowView(new LeadsView());
        }

        private void BtnPropertiesClick(object? sender, EventArgs e)
        {
            if (!CurrentSession.CanAccess("Properties")) return;
            ShowView(new PropertiesView());
        }

        private void BtnDealsClick(object? sender, EventArgs e)
        {
            if (!CurrentSession.CanAccess("Deals")) return;
            ShowView(new DealsView());
        }

        private void BtnActivitiesClick(object? sender, EventArgs e)
        {
            if (!CurrentSession.CanAccess("Activities")) return;
            ShowView(new PlaceholderView(
                "Activities",
                "Activity management placeholder. Recent email and lifecycle activity is already recorded on leads and customers."));
        }

        private void BtnFollowUpsClick(object? sender, EventArgs e)
        {
            if (!CurrentSession.CanAccess("TasksReminders")) return;
            ShowView(new PlaceholderView(
                "Follow Ups",
                "Follow-up and reminder workflow placeholder for agent activities."));
        }

        private void BtnReportsClick(object? sender, EventArgs e)
        {
            if (!CurrentSession.CanAccess("Reports")) return;
            ShowView(new PlaceholderView(
                "Reports",
                "Reports placeholder. All roles have dashboard/report access, with role-specific restrictions applied in navigation."));
        }

        private void BtnSupportTicketsClick(object? sender, EventArgs e)
        {
            if (!CurrentSession.CanAccess("SupportTickets")) return;
            ShowView(new PlaceholderView(
                "Support Tickets",
                "Support ticket oversight placeholder for admin, manager, and agent workflows."));
        }

        // =====================================================
        // LOGOUT
        // =====================================================

        private void BtnLogoutClick(object? sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to logout?",
                "Confirm Logout",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result != DialogResult.Yes)
                return;

            CurrentSession.SignOut();

            var loginForm = Application.OpenForms.OfType<LoginForm>().FirstOrDefault();

            if (loginForm != null)
            {
                loginForm.PrepareForLogout();
            }
            else
            {
                Application.Exit();   // fallback
                return;
            }

            Close();
        }
    }
}
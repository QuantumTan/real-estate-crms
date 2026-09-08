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
using ReaLTaiizor.Forms;
using System.Linq;

namespace CRMS_Peguit.winforms
{
    public partial class Form1 : Form
    {
        private Button? _activeNavButton;
        private readonly List<Button> _navButtons = new();

        public Form1()
        {
            InitializeComponent();
            ApplyTheme();
            InitNavButtons();
            BindEvents();
            ApplyRolePermissions();
            SetActiveNavButton(btnDashboard);
            BtnDashboardClick(btnDashboard, EventArgs.Empty);
        }

        private void InitNavButtons()
        {
            _navButtons.AddRange(new[]
            {
                btnDashboard,
                btnLeads,
                btnCustomers,
                btnProperties,
                btnDeals,
                btnCampaigns,
                btnActivities,
                btnFollowUps,
                btnSupportTickets,
                btnReports,
                btnApprovals,
                btnManageManagers,
                btnManageAgents
            });

            foreach (var btn in _navButtons)
            {
                UiRadiusHelper.ApplyRoundedCorners(btn, 6);
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
            }
        }

        private void BindEvents()
        {
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

            lblBellIcon.Click += (_, _) => MessageBox.Show("No new notifications.", "Notifications", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

            UiRadiusHelper.ApplyRoundedCorners(pnlUserProfile, 10);
            UiRadiusHelper.ApplyPillShape(lblUserAvatar);
            UiRadiusHelper.ApplyPillShape(lblHeaderAvatar);
            UiRadiusHelper.StyleButton(btnLogout, 6);
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

            lblUserAvatar.Text = initials;
            lblUserName.Text = user.FullName;
            lblUserRole.Text = user.GetDashboardType();

            lblHeaderAvatar.Text = initials;
            lblHeaderUserName.Text = $"{user.FullName}\r\n{user.GetDashboardType()}";
            lblRoleBadge.Text = $"• {user.GetDashboardType()}";

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

            Text = $"NEXA CRM SYSTEM — {user.FullName} ({user.GetDashboardType()})";
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
using CRMS_Peguit.winforms.Auth;
using CRMS_Peguit.winforms.Views.Dashboard;
using CRMS_Peguit.winforms.Views.Customers;
using CRMS_Peguit.winforms.Views.Leads;
using CRMS_Peguit.winforms.Views.Properties;
using CRMS_Peguit.winforms.Views.Deals;
using CRMS_Peguit.winforms.Views.Shared;
using CRMS_Peguit.winforms.Models.Services;
using ReaLTaiizor.Forms;
using System.Linq;

namespace CRMS_Peguit.winforms
{
    public partial class Form1 : MaterialForm
    {

        public Form1()
        {
            InitializeComponent();
            BindEvents();
            ApplyRolePermissions();
            ShowView(new DashboardView());
        }

        private void BindEvents()
        {
            btnLogout.Click += BtnLogoutClick;
            btnDashboard.Click += BtnDashboardClick;
            btnManageManagers.Click += BtnManageManagersClick;
            btnManageAgents.Click += BtnManageAgentsClick;
            btnCustomers.Click += BtnCustomersClick;
            btnLeads.Click += BtnLeadsClick;
            btnProperties.Click += BtnPropertiesClick;
            btnDeals.Click += BtnDealsClick;
            btnActivities.Click += BtnActivitiesClick;
            btnFollowUps.Click += BtnFollowUpsClick;
            btnReports.Click += BtnReportsClick;
            btnSupportTickets.Click += BtnSupportTicketsClick;
        }

        // ==========================================
        // CREATE NAVIGATION BUTTON
        // ==========================================

        private Button NavButton(string text, int y)
        {
            var button = new Button
            {
                Text = "  " + text,
                FlatStyle = FlatStyle.Flat,
                BackColor = Theme.Surface,
                ForeColor = Theme.TextPrimary,
                Font = new Font("Segoe UI", 10.5f),
                Location = new Point(10, y),
                Size = new Size(200, 45),
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand
            };

            button.FlatAppearance.BorderSize = 0;
            return button;
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

            btnManageManagers.Visible = CurrentSession.CanAccess("Managers");
            btnManageAgents.Visible = CurrentSession.CanAccess("SalesStaff");
            btnCustomers.Visible = CurrentSession.CanAccess("Customers");
            btnLeads.Visible = CurrentSession.CanAccess("Leads");
            btnProperties.Visible = CurrentSession.CanAccess("Properties");
            btnDeals.Visible = CurrentSession.CanAccess("Deals");
            btnActivities.Visible = CurrentSession.CanAccess("Activities");
            btnFollowUps.Visible = CurrentSession.CanAccess("TasksReminders");
            btnReports.Visible = CurrentSession.CanAccess("Reports");
            btnSupportTickets.Visible = CurrentSession.CanAccess("SupportTickets");

            Text = $"CRMS - {CurrentSession.CurrentUser.FullName} " +
                   $"({CurrentSession.CurrentUser.GetDashboardType()})";
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

        private void BtnDashboardClick(object? sender, EventArgs e) =>
            ShowView(new DashboardView());

        private void BtnManageManagersClick(object? sender, EventArgs e)
        {
            if (!CurrentSession.CanAccess("Managers")) return;
            ShowView(new CRMS_Peguit.winforms.Forms.AdminUserListForm("Manager"));
        }

        private void BtnManageAgentsClick(object? sender, EventArgs e)
        {
            if (!CurrentSession.CanAccess("SalesStaff")) return;
            ShowView(new CRMS_Peguit.winforms.Forms.AdminUserListForm("Agent"));
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
namespace CRMS_Peguit.winforms
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel sidebarPanel = null!;
        private System.Windows.Forms.Panel mainPanel = null!;

        private System.Windows.Forms.Button btnDashboard = null!;
        private System.Windows.Forms.Button btnManageManagers = null!;
        private System.Windows.Forms.Button btnManageAgents = null!;
        private System.Windows.Forms.Button btnCustomers = null!;
        private System.Windows.Forms.Button btnLeads = null!;
        private System.Windows.Forms.Button btnProperties = null!;
        private System.Windows.Forms.Button btnDeals = null!;
        private System.Windows.Forms.Button btnActivities = null!;
        private System.Windows.Forms.Button btnFollowUps = null!;
        private System.Windows.Forms.Button btnReports = null!;
        private System.Windows.Forms.Button btnSupportTickets = null!;
        private System.Windows.Forms.Button btnLogout = null!;

        private System.Windows.Forms.Label lblLogo = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.sidebarPanel = new System.Windows.Forms.Panel();
            this.lblLogo = new System.Windows.Forms.Label();
            this.btnDashboard = new System.Windows.Forms.Button();
            this.btnManageManagers = new System.Windows.Forms.Button();
            this.btnManageAgents = new System.Windows.Forms.Button();
            this.btnCustomers = new System.Windows.Forms.Button();
            this.btnLeads = new System.Windows.Forms.Button();
            this.btnProperties = new System.Windows.Forms.Button();
            this.btnDeals = new System.Windows.Forms.Button();
            this.btnActivities = new System.Windows.Forms.Button();
            this.btnFollowUps = new System.Windows.Forms.Button();
            this.btnReports = new System.Windows.Forms.Button();
            this.btnSupportTickets = new System.Windows.Forms.Button();
            this.btnLogout = new System.Windows.Forms.Button();
            this.mainPanel = new System.Windows.Forms.Panel();
            this.sidebarPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // sidebarPanel
            // 
            this.sidebarPanel.BackColor = System.Drawing.Color.White;
            this.sidebarPanel.Controls.Add(this.btnLogout);
            this.sidebarPanel.Controls.Add(this.btnSupportTickets);
            this.sidebarPanel.Controls.Add(this.btnReports);
            this.sidebarPanel.Controls.Add(this.btnFollowUps);
            this.sidebarPanel.Controls.Add(this.btnActivities);
            this.sidebarPanel.Controls.Add(this.btnDeals);
            this.sidebarPanel.Controls.Add(this.btnProperties);
            this.sidebarPanel.Controls.Add(this.btnLeads);
            this.sidebarPanel.Controls.Add(this.btnCustomers);
            this.sidebarPanel.Controls.Add(this.btnManageAgents);
            this.sidebarPanel.Controls.Add(this.btnManageManagers);
            this.sidebarPanel.Controls.Add(this.btnDashboard);
            this.sidebarPanel.Controls.Add(this.lblLogo);
            this.sidebarPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.sidebarPanel.Location = new System.Drawing.Point(0, 0);
            this.sidebarPanel.Name = "sidebarPanel";
            this.sidebarPanel.Padding = new System.Windows.Forms.Padding(0, 10, 0, 10);
            this.sidebarPanel.Size = new System.Drawing.Size(220, 680);
            this.sidebarPanel.TabIndex = 0;
            // 
            // lblLogo
            // 
            this.lblLogo.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblLogo.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblLogo.ForeColor = System.Drawing.Color.FromArgb(37, 103, 156);
            this.lblLogo.Location = new System.Drawing.Point(0, 10);
            this.lblLogo.Name = "lblLogo";
            this.lblLogo.Size = new System.Drawing.Size(220, 50);
            this.lblLogo.TabIndex = 0;
            this.lblLogo.Text = "  NEXA CRM";
            this.lblLogo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnDashboard
            // 
            this.btnDashboard.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnDashboard.FlatAppearance.BorderSize = 0;
            this.btnDashboard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDashboard.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnDashboard.ForeColor = System.Drawing.Color.FromArgb(8, 52, 87);
            this.btnDashboard.Location = new System.Drawing.Point(0, 60);
            this.btnDashboard.Name = "btnDashboard";
            this.btnDashboard.Size = new System.Drawing.Size(220, 42);
            this.btnDashboard.TabIndex = 1;
            this.btnDashboard.Text = "  Dashboard";
            this.btnDashboard.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnDashboard.UseVisualStyleBackColor = true;
            // 
            // btnManageManagers
            // 
            this.btnManageManagers.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnManageManagers.FlatAppearance.BorderSize = 0;
            this.btnManageManagers.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnManageManagers.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnManageManagers.ForeColor = System.Drawing.Color.FromArgb(8, 52, 87);
            this.btnManageManagers.Location = new System.Drawing.Point(0, 102);
            this.btnManageManagers.Name = "btnManageManagers";
            this.btnManageManagers.Size = new System.Drawing.Size(220, 42);
            this.btnManageManagers.TabIndex = 2;
            this.btnManageManagers.Text = "  Manage Managers";
            this.btnManageManagers.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnManageManagers.UseVisualStyleBackColor = true;
            // 
            // btnManageAgents
            // 
            this.btnManageAgents.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnManageAgents.FlatAppearance.BorderSize = 0;
            this.btnManageAgents.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnManageAgents.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnManageAgents.ForeColor = System.Drawing.Color.FromArgb(8, 52, 87);
            this.btnManageAgents.Location = new System.Drawing.Point(0, 144);
            this.btnManageAgents.Name = "btnManageAgents";
            this.btnManageAgents.Size = new System.Drawing.Size(220, 42);
            this.btnManageAgents.TabIndex = 3;
            this.btnManageAgents.Text = "  Manage Agents";
            this.btnManageAgents.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnManageAgents.UseVisualStyleBackColor = true;
            // 
            // btnCustomers
            // 
            this.btnCustomers.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnCustomers.FlatAppearance.BorderSize = 0;
            this.btnCustomers.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCustomers.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnCustomers.ForeColor = System.Drawing.Color.FromArgb(8, 52, 87);
            this.btnCustomers.Location = new System.Drawing.Point(0, 186);
            this.btnCustomers.Name = "btnCustomers";
            this.btnCustomers.Size = new System.Drawing.Size(220, 42);
            this.btnCustomers.TabIndex = 4;
            this.btnCustomers.Text = "  Customers";
            this.btnCustomers.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCustomers.UseVisualStyleBackColor = true;
            // 
            // btnLeads
            // 
            this.btnLeads.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnLeads.FlatAppearance.BorderSize = 0;
            this.btnLeads.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLeads.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnLeads.ForeColor = System.Drawing.Color.FromArgb(8, 52, 87);
            this.btnLeads.Location = new System.Drawing.Point(0, 228);
            this.btnLeads.Name = "btnLeads";
            this.btnLeads.Size = new System.Drawing.Size(220, 42);
            this.btnLeads.TabIndex = 5;
            this.btnLeads.Text = "  Leads";
            this.btnLeads.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnLeads.UseVisualStyleBackColor = true;
            // 
            // btnProperties
            // 
            this.btnProperties.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnProperties.FlatAppearance.BorderSize = 0;
            this.btnProperties.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnProperties.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnProperties.ForeColor = System.Drawing.Color.FromArgb(8, 52, 87);
            this.btnProperties.Location = new System.Drawing.Point(0, 270);
            this.btnProperties.Name = "btnProperties";
            this.btnProperties.Size = new System.Drawing.Size(220, 42);
            this.btnProperties.TabIndex = 6;
            this.btnProperties.Text = "  Properties";
            this.btnProperties.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnProperties.UseVisualStyleBackColor = true;
            // 
            // btnDeals
            // 
            this.btnDeals.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnDeals.FlatAppearance.BorderSize = 0;
            this.btnDeals.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeals.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnDeals.ForeColor = System.Drawing.Color.FromArgb(8, 52, 87);
            this.btnDeals.Location = new System.Drawing.Point(0, 312);
            this.btnDeals.Name = "btnDeals";
            this.btnDeals.Size = new System.Drawing.Size(220, 42);
            this.btnDeals.TabIndex = 7;
            this.btnDeals.Text = "  Deals";
            this.btnDeals.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnDeals.UseVisualStyleBackColor = true;
            // 
            // btnActivities
            // 
            this.btnActivities.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnActivities.FlatAppearance.BorderSize = 0;
            this.btnActivities.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnActivities.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnActivities.ForeColor = System.Drawing.Color.FromArgb(8, 52, 87);
            this.btnActivities.Location = new System.Drawing.Point(0, 354);
            this.btnActivities.Name = "btnActivities";
            this.btnActivities.Size = new System.Drawing.Size(220, 42);
            this.btnActivities.TabIndex = 8;
            this.btnActivities.Text = "  Activities";
            this.btnActivities.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnActivities.UseVisualStyleBackColor = true;
            // 
            // btnFollowUps
            // 
            this.btnFollowUps.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnFollowUps.FlatAppearance.BorderSize = 0;
            this.btnFollowUps.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFollowUps.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnFollowUps.ForeColor = System.Drawing.Color.FromArgb(8, 52, 87);
            this.btnFollowUps.Location = new System.Drawing.Point(0, 396);
            this.btnFollowUps.Name = "btnFollowUps";
            this.btnFollowUps.Size = new System.Drawing.Size(220, 42);
            this.btnFollowUps.TabIndex = 9;
            this.btnFollowUps.Text = "  Follow-Ups";
            this.btnFollowUps.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnFollowUps.UseVisualStyleBackColor = true;
            // 
            // btnReports
            // 
            this.btnReports.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnReports.FlatAppearance.BorderSize = 0;
            this.btnReports.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReports.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnReports.ForeColor = System.Drawing.Color.FromArgb(8, 52, 87);
            this.btnReports.Location = new System.Drawing.Point(0, 438);
            this.btnReports.Name = "btnReports";
            this.btnReports.Size = new System.Drawing.Size(220, 42);
            this.btnReports.TabIndex = 10;
            this.btnReports.Text = "  Reports";
            this.btnReports.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnReports.UseVisualStyleBackColor = true;
            // 
            // btnSupportTickets
            // 
            this.btnSupportTickets.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnSupportTickets.FlatAppearance.BorderSize = 0;
            this.btnSupportTickets.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSupportTickets.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnSupportTickets.ForeColor = System.Drawing.Color.FromArgb(8, 52, 87);
            this.btnSupportTickets.Location = new System.Drawing.Point(0, 480);
            this.btnSupportTickets.Name = "btnSupportTickets";
            this.btnSupportTickets.Size = new System.Drawing.Size(220, 42);
            this.btnSupportTickets.TabIndex = 11;
            this.btnSupportTickets.Text = "  Support Tickets";
            this.btnSupportTickets.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSupportTickets.UseVisualStyleBackColor = true;
            // 
            // btnLogout
            // 
            this.btnLogout.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnLogout.FlatAppearance.BorderSize = 0;
            this.btnLogout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogout.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnLogout.ForeColor = System.Drawing.Color.IndianRed;
            this.btnLogout.Location = new System.Drawing.Point(0, 628);
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.Size = new System.Drawing.Size(220, 42);
            this.btnLogout.TabIndex = 12;
            this.btnLogout.Text = "  Sign Out";
            this.btnLogout.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnLogout.UseVisualStyleBackColor = true;
            // 
            // mainPanel
            // 
            this.mainPanel.BackColor = System.Drawing.Color.FromArgb(243, 247, 250);
            this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainPanel.Location = new System.Drawing.Point(220, 0);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Size = new System.Drawing.Size(980, 680);
            this.mainPanel.TabIndex = 1;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(243, 247, 250);
            this.ClientSize = new System.Drawing.Size(1200, 680);
            this.Controls.Add(this.mainPanel);
            this.Controls.Add(this.sidebarPanel);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "NEXA CRM";
            this.sidebarPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion
    }
}

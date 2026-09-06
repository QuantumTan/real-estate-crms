namespace CRMS_Peguit.winforms.Views.Management
{
    partial class ApprovalsView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblTitle = null!;
        private System.Windows.Forms.Label lblSubtitle = null!;
        private System.Windows.Forms.Button btnRefresh = null!;
        private System.Windows.Forms.TextBox txtSearch = null!;
        private System.Windows.Forms.Button btnFilterAll = null!;
        private System.Windows.Forms.Button btnFilterLeads = null!;
        private System.Windows.Forms.Button btnFilterCustomers = null!;
        private System.Windows.Forms.Button btnFilterProperties = null!;
        private System.Windows.Forms.Panel pnlCard = null!;
        private System.Windows.Forms.DataGridView grid = null!;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.btnFilterAll = new System.Windows.Forms.Button();
            this.btnFilterLeads = new System.Windows.Forms.Button();
            this.btnFilterCustomers = new System.Windows.Forms.Button();
            this.btnFilterProperties = new System.Windows.Forms.Button();
            this.pnlCard = new System.Windows.Forms.Panel();
            this.grid = new System.Windows.Forms.DataGridView();
            this.pnlCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.lblTitle.Location = new System.Drawing.Point(30, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(262, 37);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Review & Approvals";
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblSubtitle.Location = new System.Drawing.Point(32, 58);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(430, 17);
            this.lblSubtitle.TabIndex = 1;
            this.lblSubtitle.Text = "Review pending submissions, assign sales staff, and approve listings and leads.";
            // 
            // btnRefresh
            // 
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefresh.BackColor = System.Drawing.Color.White;
            this.btnRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRefresh.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(226, 232, 240);
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnRefresh.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.btnRefresh.Location = new System.Drawing.Point(880, 24);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(120, 36);
            this.btnRefresh.TabIndex = 2;
            this.btnRefresh.Text = "⟳ Refresh";
            this.btnRefresh.UseVisualStyleBackColor = false;
            // 
            // txtSearch
            // 
            this.txtSearch.BackColor = System.Drawing.Color.White;
            this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtSearch.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.txtSearch.Location = new System.Drawing.Point(30, 90);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.PlaceholderText = "🔍 Search pending items by name or submitter...";
            this.txtSearch.Size = new System.Drawing.Size(360, 24);
            this.txtSearch.TabIndex = 3;
            // 
            // btnFilterAll
            // 
            this.btnFilterAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFilterAll.BackColor = System.Drawing.Color.FromArgb(15, 91, 158);
            this.btnFilterAll.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnFilterAll.FlatAppearance.BorderSize = 0;
            this.btnFilterAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFilterAll.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnFilterAll.ForeColor = System.Drawing.Color.White;
            this.btnFilterAll.Location = new System.Drawing.Point(630, 88);
            this.btnFilterAll.Name = "btnFilterAll";
            this.btnFilterAll.Size = new System.Drawing.Size(95, 28);
            this.btnFilterAll.TabIndex = 4;
            this.btnFilterAll.Text = "All Pending";
            this.btnFilterAll.UseVisualStyleBackColor = false;
            // 
            // btnFilterLeads
            // 
            this.btnFilterLeads.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFilterLeads.BackColor = System.Drawing.Color.White;
            this.btnFilterLeads.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnFilterLeads.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(226, 232, 240);
            this.btnFilterLeads.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFilterLeads.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnFilterLeads.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.btnFilterLeads.Location = new System.Drawing.Point(731, 88);
            this.btnFilterLeads.Name = "btnFilterLeads";
            this.btnFilterLeads.Size = new System.Drawing.Size(75, 28);
            this.btnFilterLeads.TabIndex = 5;
            this.btnFilterLeads.Text = "Leads";
            this.btnFilterLeads.UseVisualStyleBackColor = false;
            // 
            // btnFilterCustomers
            // 
            this.btnFilterCustomers.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFilterCustomers.BackColor = System.Drawing.Color.White;
            this.btnFilterCustomers.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnFilterCustomers.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(226, 232, 240);
            this.btnFilterCustomers.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFilterCustomers.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnFilterCustomers.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.btnFilterCustomers.Location = new System.Drawing.Point(812, 88);
            this.btnFilterCustomers.Name = "btnFilterCustomers";
            this.btnFilterCustomers.Size = new System.Drawing.Size(95, 28);
            this.btnFilterCustomers.TabIndex = 6;
            this.btnFilterCustomers.Text = "Customers";
            this.btnFilterCustomers.UseVisualStyleBackColor = false;
            // 
            // btnFilterProperties
            // 
            this.btnFilterProperties.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFilterProperties.BackColor = System.Drawing.Color.White;
            this.btnFilterProperties.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnFilterProperties.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(226, 232, 240);
            this.btnFilterProperties.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFilterProperties.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnFilterProperties.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.btnFilterProperties.Location = new System.Drawing.Point(913, 88);
            this.btnFilterProperties.Name = "btnFilterProperties";
            this.btnFilterProperties.Size = new System.Drawing.Size(87, 28);
            this.btnFilterProperties.TabIndex = 7;
            this.btnFilterProperties.Text = "Properties";
            this.btnFilterProperties.UseVisualStyleBackColor = false;
            // 
            // pnlCard
            // 
            this.pnlCard.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlCard.BackColor = System.Drawing.Color.White;
            this.pnlCard.Controls.Add(this.grid);
            this.pnlCard.Location = new System.Drawing.Point(30, 126);
            this.pnlCard.Name = "pnlCard";
            this.pnlCard.Padding = new System.Windows.Forms.Padding(1);
            this.pnlCard.Size = new System.Drawing.Size(970, 520);
            this.pnlCard.TabIndex = 8;
            // 
            // grid
            // 
            this.grid.AllowUserToAddRows = false;
            this.grid.AllowUserToDeleteRows = false;
            this.grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grid.BackgroundColor = System.Drawing.Color.White;
            this.grid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.grid.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.grid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.grid.ColumnHeadersHeight = 44;
            this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.GridColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.grid.Location = new System.Drawing.Point(1, 1);
            this.grid.MultiSelect = false;
            this.grid.Name = "grid";
            this.grid.ReadOnly = true;
            this.grid.RowHeadersVisible = false;
            this.grid.RowTemplate.Height = 52;
            this.grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grid.Size = new System.Drawing.Size(968, 518);
            this.grid.TabIndex = 0;
            // 
            // ApprovalsView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(248, 249, 250);
            this.Controls.Add(this.pnlCard);
            this.Controls.Add(this.btnFilterProperties);
            this.Controls.Add(this.btnFilterCustomers);
            this.Controls.Add(this.btnFilterLeads);
            this.Controls.Add(this.btnFilterAll);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.lblSubtitle);
            this.Controls.Add(this.lblTitle);
            this.Name = "ApprovalsView";
            this.Size = new System.Drawing.Size(1030, 670);
            this.pnlCard.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}

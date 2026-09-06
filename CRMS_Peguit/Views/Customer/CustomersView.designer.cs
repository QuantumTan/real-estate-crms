namespace CRMS_Peguit.winforms.Views.Customers
{
    partial class CustomersView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblTitle = null!;
        private System.Windows.Forms.Label lblSubtitle = null!;
        private System.Windows.Forms.Button btnAdd = null!;
        private System.Windows.Forms.TextBox txtSearch = null!;
        private System.Windows.Forms.Button btnFilterAll = null!;
        private System.Windows.Forms.Button btnFilterActive = null!;
        private System.Windows.Forms.Button btnFilterFollowUp = null!;
        private System.Windows.Forms.Button btnFilterInactive = null!;
        private System.Windows.Forms.Panel pnlCard = null!;
        private System.Windows.Forms.DataGridView grid = null!;

        // Backward compatibility fields (hidden)
        private System.Windows.Forms.ComboBox cmbFilter = null!;
        private CRMS_Peguit.winforms.Controls.KpiCard kpiTotal = null!;
        private CRMS_Peguit.winforms.Controls.KpiCard kpiActive = null!;
        private CRMS_Peguit.winforms.Controls.KpiCard kpiInactive = null!;
        private CRMS_Peguit.winforms.Controls.KpiCard kpiThisMonth = null!;

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
            this.btnAdd = new System.Windows.Forms.Button();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.btnFilterAll = new System.Windows.Forms.Button();
            this.btnFilterActive = new System.Windows.Forms.Button();
            this.btnFilterFollowUp = new System.Windows.Forms.Button();
            this.btnFilterInactive = new System.Windows.Forms.Button();
            this.pnlCard = new System.Windows.Forms.Panel();
            this.grid = new System.Windows.Forms.DataGridView();
            this.cmbFilter = new System.Windows.Forms.ComboBox();
            this.kpiTotal = new CRMS_Peguit.winforms.Controls.KpiCard();
            this.kpiActive = new CRMS_Peguit.winforms.Controls.KpiCard();
            this.kpiInactive = new CRMS_Peguit.winforms.Controls.KpiCard();
            this.kpiThisMonth = new CRMS_Peguit.winforms.Controls.KpiCard();
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
            this.lblTitle.Size = new System.Drawing.Size(153, 37);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Customers";
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblSubtitle.Location = new System.Drawing.Point(32, 58);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(99, 17);
            this.lblSubtitle.TabIndex = 1;
            this.lblSubtitle.Text = "8 total · 5 active";
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdd.BackColor = System.Drawing.Color.FromArgb(15, 91, 158);
            this.btnAdd.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAdd.FlatAppearance.BorderSize = 0;
            this.btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAdd.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnAdd.ForeColor = System.Drawing.Color.White;
            this.btnAdd.Location = new System.Drawing.Point(860, 24);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(140, 36);
            this.btnAdd.TabIndex = 2;
            this.btnAdd.Text = "+ Add Customer";
            this.btnAdd.UseVisualStyleBackColor = false;
            // 
            // txtSearch
            // 
            this.txtSearch.BackColor = System.Drawing.Color.White;
            this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtSearch.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.txtSearch.Location = new System.Drawing.Point(30, 90);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.PlaceholderText = "🔍 Search by name, company, or email...";
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
            this.btnFilterAll.Location = new System.Drawing.Point(680, 88);
            this.btnFilterAll.Name = "btnFilterAll";
            this.btnFilterAll.Size = new System.Drawing.Size(55, 28);
            this.btnFilterAll.TabIndex = 4;
            this.btnFilterAll.Text = "All";
            this.btnFilterAll.UseVisualStyleBackColor = false;
            // 
            // btnFilterActive
            // 
            this.btnFilterActive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFilterActive.BackColor = System.Drawing.Color.White;
            this.btnFilterActive.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnFilterActive.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(226, 232, 240);
            this.btnFilterActive.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFilterActive.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnFilterActive.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.btnFilterActive.Location = new System.Drawing.Point(741, 88);
            this.btnFilterActive.Name = "btnFilterActive";
            this.btnFilterActive.Size = new System.Drawing.Size(70, 28);
            this.btnFilterActive.TabIndex = 5;
            this.btnFilterActive.Text = "Active";
            this.btnFilterActive.UseVisualStyleBackColor = false;
            // 
            // btnFilterFollowUp
            // 
            this.btnFilterFollowUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFilterFollowUp.BackColor = System.Drawing.Color.White;
            this.btnFilterFollowUp.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnFilterFollowUp.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(226, 232, 240);
            this.btnFilterFollowUp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFilterFollowUp.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnFilterFollowUp.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.btnFilterFollowUp.Location = new System.Drawing.Point(817, 88);
            this.btnFilterFollowUp.Name = "btnFilterFollowUp";
            this.btnFilterFollowUp.Size = new System.Drawing.Size(95, 28);
            this.btnFilterFollowUp.TabIndex = 6;
            this.btnFilterFollowUp.Text = "Follow Up";
            this.btnFilterFollowUp.UseVisualStyleBackColor = false;
            // 
            // btnFilterInactive
            // 
            this.btnFilterInactive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFilterInactive.BackColor = System.Drawing.Color.White;
            this.btnFilterInactive.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnFilterInactive.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(226, 232, 240);
            this.btnFilterInactive.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFilterInactive.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnFilterInactive.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.btnFilterInactive.Location = new System.Drawing.Point(918, 88);
            this.btnFilterInactive.Name = "btnFilterInactive";
            this.btnFilterInactive.Size = new System.Drawing.Size(82, 28);
            this.btnFilterInactive.TabIndex = 7;
            this.btnFilterInactive.Text = "Inactive";
            this.btnFilterInactive.UseVisualStyleBackColor = false;
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
            // cmbFilter
            // 
            this.cmbFilter.Location = new System.Drawing.Point(0, 0);
            this.cmbFilter.Name = "cmbFilter";
            this.cmbFilter.Size = new System.Drawing.Size(0, 21);
            this.cmbFilter.TabIndex = 10;
            this.cmbFilter.Visible = false;
            // 
            // kpiTotal
            // 
            this.kpiTotal.Location = new System.Drawing.Point(0, 0);
            this.kpiTotal.Name = "kpiTotal";
            this.kpiTotal.Size = new System.Drawing.Size(0, 0);
            this.kpiTotal.TabIndex = 11;
            this.kpiTotal.Visible = false;
            // 
            // kpiActive
            // 
            this.kpiActive.Location = new System.Drawing.Point(0, 0);
            this.kpiActive.Name = "kpiActive";
            this.kpiActive.Size = new System.Drawing.Size(0, 0);
            this.kpiActive.TabIndex = 12;
            this.kpiActive.Visible = false;
            // 
            // kpiInactive
            // 
            this.kpiInactive.Location = new System.Drawing.Point(0, 0);
            this.kpiInactive.Name = "kpiInactive";
            this.kpiInactive.Size = new System.Drawing.Size(0, 0);
            this.kpiInactive.TabIndex = 13;
            this.kpiInactive.Visible = false;
            // 
            // kpiThisMonth
            // 
            this.kpiThisMonth.Location = new System.Drawing.Point(0, 0);
            this.kpiThisMonth.Name = "kpiThisMonth";
            this.kpiThisMonth.Size = new System.Drawing.Size(0, 0);
            this.kpiThisMonth.TabIndex = 14;
            this.kpiThisMonth.Visible = false;
            // 
            // CustomersView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(244, 247, 251);
            this.Controls.Add(this.pnlCard);
            this.Controls.Add(this.btnFilterInactive);
            this.Controls.Add(this.btnFilterFollowUp);
            this.Controls.Add(this.btnFilterActive);
            this.Controls.Add(this.btnFilterAll);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.lblSubtitle);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.cmbFilter);
            this.Controls.Add(this.kpiTotal);
            this.Controls.Add(this.kpiActive);
            this.Controls.Add(this.kpiInactive);
            this.Controls.Add(this.kpiThisMonth);
            this.Name = "CustomersView";
            this.Size = new System.Drawing.Size(1030, 700);
            this.pnlCard.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
    }
}

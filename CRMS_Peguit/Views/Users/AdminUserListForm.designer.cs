namespace CRMS_Peguit.winforms.Views.Users
{
    partial class AdminUserListForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblTitle = null!;
        private System.Windows.Forms.DataGridView grid = null!;
        private System.Windows.Forms.TextBox txtSearch = null!;
        private System.Windows.Forms.ComboBox cmbRoleFilter = null!;
        private System.Windows.Forms.CheckBox chkIncludeInactive = null!;
        private System.Windows.Forms.Button btnAdd = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _controller?.Dispose();
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.grid = new System.Windows.Forms.DataGridView();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.cmbRoleFilter = new System.Windows.Forms.ComboBox();
            this.chkIncludeInactive = new System.Windows.Forms.CheckBox();
            this.btnAdd = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 22F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(8, 52, 87);
            this.lblTitle.Location = new System.Drawing.Point(30, 25);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(211, 41);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Manage Users";
            // 
            // txtSearch
            // 
            this.txtSearch.BackColor = System.Drawing.Color.White;
            this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtSearch.ForeColor = System.Drawing.Color.FromArgb(8, 52, 87);
            this.txtSearch.Location = new System.Drawing.Point(30, 80);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.PlaceholderText = "Search name or email...";
            this.txtSearch.Size = new System.Drawing.Size(280, 27);
            this.txtSearch.TabIndex = 1;
            // 
            // cmbRoleFilter
            // 
            this.cmbRoleFilter.BackColor = System.Drawing.Color.White;
            this.cmbRoleFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRoleFilter.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.cmbRoleFilter.ForeColor = System.Drawing.Color.FromArgb(8, 52, 87);
            this.cmbRoleFilter.FormattingEnabled = true;
            this.cmbRoleFilter.Location = new System.Drawing.Point(325, 80);
            this.cmbRoleFilter.Name = "cmbRoleFilter";
            this.cmbRoleFilter.Size = new System.Drawing.Size(150, 28);
            this.cmbRoleFilter.TabIndex = 2;
            // 
            // chkIncludeInactive
            // 
            this.chkIncludeInactive.AutoSize = true;
            this.chkIncludeInactive.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.chkIncludeInactive.ForeColor = System.Drawing.Color.FromArgb(8, 52, 87);
            this.chkIncludeInactive.Location = new System.Drawing.Point(490, 83);
            this.chkIncludeInactive.Name = "chkIncludeInactive";
            this.chkIncludeInactive.Size = new System.Drawing.Size(158, 23);
            this.chkIncludeInactive.TabIndex = 3;
            this.chkIncludeInactive.Text = "Include inactive users";
            this.chkIncludeInactive.UseVisualStyleBackColor = true;
            // 
            // btnAdd
            // 
            this.btnAdd.BackColor = System.Drawing.Color.FromArgb(37, 103, 156);
            this.btnAdd.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAdd.FlatAppearance.BorderSize = 0;
            this.btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAdd.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnAdd.ForeColor = System.Drawing.Color.White;
            this.btnAdd.Location = new System.Drawing.Point(660, 78);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(130, 35);
            this.btnAdd.TabIndex = 4;
            this.btnAdd.Text = "+ Add User";
            this.btnAdd.UseVisualStyleBackColor = false;
            // 
            // grid
            // 
            this.grid.AllowUserToAddRows = false;
            this.grid.AllowUserToDeleteRows = false;
            this.grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grid.BackgroundColor = System.Drawing.Color.White;
            this.grid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.grid.ColumnHeadersHeight = 45;
            this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.grid.Location = new System.Drawing.Point(30, 135);
            this.grid.MultiSelect = false;
            this.grid.Name = "grid";
            this.grid.ReadOnly = true;
            this.grid.RowHeadersVisible = false;
            this.grid.RowTemplate.Height = 45;
            this.grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grid.Size = new System.Drawing.Size(800, 430);
            this.grid.TabIndex = 5;
            // 
            // AdminUserListForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(243, 247, 250);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.cmbRoleFilter);
            this.Controls.Add(this.chkIncludeInactive);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.grid);
            this.Name = "AdminUserListForm";
            this.Padding = new System.Windows.Forms.Padding(30);
            this.Size = new System.Drawing.Size(900, 600);
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}

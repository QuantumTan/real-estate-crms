namespace CRMS_Peguit.winforms.Views.Leads
{
    partial class LeadsView
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblTitle = null!;
        private System.Windows.Forms.DataGridView grid = null!;
        private System.Windows.Forms.TextBox txtSearch = null!;
        private System.Windows.Forms.ComboBox cmbFilter = null!;
        private System.Windows.Forms.Button btnAdd = null!;

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
            lblTitle = new Label();
            grid = new DataGridView();
            txtSearch = new TextBox();
            cmbFilter = new ComboBox();
            btnAdd = new Button();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(8, 52, 87);
            lblTitle.Location = new Point(34, 33);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(120, 50);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Leads";
            // 
            // grid
            // 
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.BackgroundColor = Color.FromArgb(255, 255, 255);
            grid.BorderStyle = BorderStyle.None;
            grid.ColumnHeadersHeight = 45;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.Location = new Point(34, 180);
            grid.Margin = new Padding(3, 4, 3, 4);
            grid.MultiSelect = false;
            grid.Name = "grid";
            grid.ReadOnly = true;
            grid.RowHeadersVisible = false;
            grid.RowHeadersWidth = 51;
            grid.RowTemplate.Height = 45;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.Size = new Size(914, 600);
            grid.TabIndex = 4;
            // 
            // txtSearch
            // 
            txtSearch.BackColor = Color.FromArgb(255, 255, 255);
            txtSearch.BorderStyle = BorderStyle.FixedSingle;
            txtSearch.Font = new Font("Segoe UI", 11F);
            txtSearch.ForeColor = Color.FromArgb(8, 52, 87);
            txtSearch.Location = new Point(34, 107);
            txtSearch.Margin = new Padding(3, 4, 3, 4);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "Search first name, last name, email, phone...";
            txtSearch.Size = new Size(343, 32);
            txtSearch.TabIndex = 1;
            txtSearch.TextChanged += txtSearch_TextChanged;
            // 
            // cmbFilter
            // 
            cmbFilter.BackColor = Color.FromArgb(255, 255, 255);
            cmbFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFilter.Font = new Font("Segoe UI", 11F);
            cmbFilter.ForeColor = Color.FromArgb(8, 52, 87);
            cmbFilter.FormattingEnabled = true;
            cmbFilter.Location = new Point(394, 107);
            cmbFilter.Margin = new Padding(3, 4, 3, 4);
            cmbFilter.Name = "cmbFilter";
            cmbFilter.Size = new Size(171, 33);
            cmbFilter.TabIndex = 2;
            // 
            // btnAdd
            // 
            btnAdd.BackColor = Color.FromArgb(37, 103, 156);
            btnAdd.Cursor = Cursors.Hand;
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnAdd.ForeColor = Color.FromArgb(255, 255, 255);
            btnAdd.Location = new Point(777, 107);
            btnAdd.Margin = new Padding(3, 4, 3, 4);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(171, 51);
            btnAdd.TabIndex = 3;
            btnAdd.Text = "+ Add Lead";
            btnAdd.UseVisualStyleBackColor = false;
            // 
            // LeadsView
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(243, 247, 250);
            Controls.Add(lblTitle);
            Controls.Add(txtSearch);
            Controls.Add(cmbFilter);
            Controls.Add(btnAdd);
            Controls.Add(grid);
            Margin = new Padding(3, 4, 3, 4);
            Name = "LeadsView";
            Padding = new Padding(34, 40, 34, 40);
            Size = new Size(1029, 827);
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}

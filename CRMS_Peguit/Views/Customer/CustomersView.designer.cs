namespace CRMS_Peguit.winforms.Views.Customers
{
    partial class CustomersView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblTitle = null!;
        private System.Windows.Forms.DataGridView grid = null!;
        private System.Windows.Forms.TextBox txtSearch = null!;
        private System.Windows.Forms.ComboBox cmbFilter = null!;
        private System.Windows.Forms.Button btnAdd = null!;
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

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
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
            lblTitle.Size = new Size(205, 50);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Customers";
            lblTitle.Click += lblTitle_Click;
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
            grid.Location = new Point(34, 313);
            grid.Margin = new Padding(3, 4, 3, 4);
            grid.MultiSelect = false;
            grid.Name = "grid";
            grid.ReadOnly = true;
            grid.RowHeadersVisible = false;
            grid.RowHeadersWidth = 51;
            grid.RowTemplate.Height = 45;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.Size = new Size(914, 467);
            grid.TabIndex = 4;
            // 
            // txtSearch
            // 
            txtSearch.BackColor = Color.FromArgb(255, 255, 255);
            txtSearch.BorderStyle = BorderStyle.FixedSingle;
            txtSearch.Font = new Font("Segoe UI", 11F);
            txtSearch.ForeColor = Color.FromArgb(8, 52, 87);
            txtSearch.Location = new Point(34, 240);
            txtSearch.Margin = new Padding(3, 4, 3, 4);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "Search first name, last name, email, phone...";
            txtSearch.Size = new Size(343, 32);
            txtSearch.TabIndex = 1;
            // 
            // cmbFilter
            // 
            cmbFilter.BackColor = Color.FromArgb(255, 255, 255);
            cmbFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFilter.Font = new Font("Segoe UI", 11F);
            cmbFilter.ForeColor = Color.FromArgb(8, 52, 87);
            cmbFilter.FormattingEnabled = true;
            cmbFilter.Location = new Point(394, 240);
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
            btnAdd.Location = new Point(777, 240);
            btnAdd.Margin = new Padding(3, 4, 3, 4);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(171, 51);
            btnAdd.TabIndex = 3;
            btnAdd.Text = "+ Add Customer";
            btnAdd.UseVisualStyleBackColor = false;
            // 
            // kpiTotal
            // 
            kpiTotal.BackColor = Color.FromArgb(255, 255, 255);
            kpiTotal.Location = new Point(0, 0);
            kpiTotal.Margin = new Padding(3, 4, 3, 4);
            kpiTotal.Name = "kpiTotal";
            kpiTotal.Size = new Size(229, 120);
            kpiTotal.TabIndex = 1;
            // 
            // kpiActive
            // 
            kpiActive.BackColor = Color.FromArgb(255, 255, 255);
            kpiActive.Location = new Point(0, 0);
            kpiActive.Margin = new Padding(3, 4, 3, 4);
            kpiActive.Name = "kpiActive";
            kpiActive.Size = new Size(229, 120);
            kpiActive.TabIndex = 2;
            // 
            // kpiInactive
            // 
            kpiInactive.BackColor = Color.FromArgb(255, 255, 255);
            kpiInactive.Location = new Point(0, 0);
            kpiInactive.Margin = new Padding(3, 4, 3, 4);
            kpiInactive.Name = "kpiInactive";
            kpiInactive.Size = new Size(229, 120);
            kpiInactive.TabIndex = 3;
            // 
            // kpiThisMonth
            // 
            kpiThisMonth.BackColor = Color.FromArgb(255, 255, 255);
            kpiThisMonth.Location = new Point(0, 0);
            kpiThisMonth.Margin = new Padding(3, 4, 3, 4);
            kpiThisMonth.Name = "kpiThisMonth";
            kpiThisMonth.Size = new Size(229, 120);
            kpiThisMonth.TabIndex = 4;
            // 
            // CustomersView
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(243, 247, 250);
            Controls.Add(lblTitle);
            Controls.Add(kpiTotal);
            Controls.Add(kpiActive);
            Controls.Add(kpiInactive);
            Controls.Add(kpiThisMonth);
            Controls.Add(txtSearch);
            Controls.Add(cmbFilter);
            Controls.Add(btnAdd);
            Controls.Add(grid);
            Margin = new Padding(3, 4, 3, 4);
            Name = "CustomersView";
            Padding = new Padding(34, 40, 34, 40);
            Size = new Size(1029, 827);
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion
    }
}

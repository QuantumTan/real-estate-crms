namespace CRMS_Peguit.winforms.Views.Dashboard
{
    partial class DashboardView
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblTitle = null!;
        private System.Windows.Forms.Label lblSubtitle = null!;

        private System.Windows.Forms.TableLayoutPanel pnlKpiContainer = null!;
        private CRMS_Peguit.winforms.Controls.KpiCard kpiCustomers = null!;
        private CRMS_Peguit.winforms.Controls.KpiCard kpiProperties = null!;
        private CRMS_Peguit.winforms.Controls.KpiCard kpiLeads = null!;
        private CRMS_Peguit.winforms.Controls.KpiCard kpiDeals = null!;

        private System.Windows.Forms.TableLayoutPanel pnlContentSplit = null!;
        private System.Windows.Forms.Panel pnlLeftCard = null!;
        private System.Windows.Forms.Label lblLeftTitle = null!;
        private System.Windows.Forms.DataGridView gridRecent = null!;
        private System.Windows.Forms.Label lblRecentEmpty = null!;

        private System.Windows.Forms.Panel pnlRightCard = null!;
        private System.Windows.Forms.Label lblRightTitle = null!;
        private System.Windows.Forms.Label lblStat1Title = null!;
        private System.Windows.Forms.Label lblStat1Value = null!;
        private System.Windows.Forms.Label lblStat2Title = null!;
        private System.Windows.Forms.Label lblStat2Value = null!;
        private System.Windows.Forms.Label lblStat3Title = null!;
        private System.Windows.Forms.Label lblStat3Value = null!;

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
            this.pnlKpiContainer = new System.Windows.Forms.TableLayoutPanel();
            this.kpiCustomers = new CRMS_Peguit.winforms.Controls.KpiCard("TOTAL CUSTOMERS", "customers", System.Drawing.Color.FromArgb(15, 91, 158));
            this.kpiProperties = new CRMS_Peguit.winforms.Controls.KpiCard("ACTIVE PROPERTIES", "properties", System.Drawing.Color.FromArgb(16, 185, 129));
            this.kpiLeads = new CRMS_Peguit.winforms.Controls.KpiCard("QUALIFIED LEADS", "leads", System.Drawing.Color.FromArgb(14, 165, 233));
            this.kpiDeals = new CRMS_Peguit.winforms.Controls.KpiCard("TOTAL DEALS", "deals", System.Drawing.Color.FromArgb(139, 92, 246));
            this.pnlContentSplit = new System.Windows.Forms.TableLayoutPanel();
            this.pnlLeftCard = new System.Windows.Forms.Panel();
            this.lblLeftTitle = new System.Windows.Forms.Label();
            this.gridRecent = new System.Windows.Forms.DataGridView();
            this.lblRecentEmpty = new System.Windows.Forms.Label();
            this.pnlRightCard = new System.Windows.Forms.Panel();
            this.lblRightTitle = new System.Windows.Forms.Label();
            this.lblStat1Title = new System.Windows.Forms.Label();
            this.lblStat1Value = new System.Windows.Forms.Label();
            this.lblStat2Title = new System.Windows.Forms.Label();
            this.lblStat2Value = new System.Windows.Forms.Label();
            this.lblStat3Title = new System.Windows.Forms.Label();
            this.lblStat3Value = new System.Windows.Forms.Label();
            this.pnlKpiContainer.SuspendLayout();
            this.pnlLeftCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridRecent)).BeginInit();
            this.pnlRightCard.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.lblTitle.Location = new System.Drawing.Point(30, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(155, 37);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Dashboard";
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblSubtitle.Location = new System.Drawing.Point(32, 58);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(260, 17);
            this.lblSubtitle.TabIndex = 1;
            this.lblSubtitle.Text = "Overview of your real estate sales operations";
            // 
            // pnlKpiContainer
            // 
            this.pnlKpiContainer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlKpiContainer.ColumnCount = 4;
            this.pnlKpiContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.pnlKpiContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.pnlKpiContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.pnlKpiContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.pnlKpiContainer.Controls.Add(this.kpiCustomers, 0, 0);
            this.pnlKpiContainer.Controls.Add(this.kpiProperties, 1, 0);
            this.pnlKpiContainer.Controls.Add(this.kpiLeads, 2, 0);
            this.pnlKpiContainer.Controls.Add(this.kpiDeals, 3, 0);
            this.pnlKpiContainer.Location = new System.Drawing.Point(30, 88);
            this.pnlKpiContainer.Name = "pnlKpiContainer";
            this.pnlKpiContainer.RowCount = 1;
            this.pnlKpiContainer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlKpiContainer.Size = new System.Drawing.Size(970, 104);
            this.pnlKpiContainer.TabIndex = 3;
            // 
            // kpiCustomers
            // 
            this.kpiCustomers.BackColor = System.Drawing.Color.White;
            this.kpiCustomers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kpiCustomers.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.kpiCustomers.Name = "kpiCustomers";
            this.kpiCustomers.Size = new System.Drawing.Size(234, 104);
            this.kpiCustomers.TabIndex = 0;
            // 
            // kpiProperties
            // 
            this.kpiProperties.BackColor = System.Drawing.Color.White;
            this.kpiProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kpiProperties.Margin = new System.Windows.Forms.Padding(4, 0, 6, 0);
            this.kpiProperties.Name = "kpiProperties";
            this.kpiProperties.Size = new System.Drawing.Size(234, 104);
            this.kpiProperties.TabIndex = 1;
            // 
            // kpiLeads
            // 
            this.kpiLeads.BackColor = System.Drawing.Color.White;
            this.kpiLeads.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kpiLeads.Margin = new System.Windows.Forms.Padding(6, 0, 4, 0);
            this.kpiLeads.Name = "kpiLeads";
            this.kpiLeads.Size = new System.Drawing.Size(234, 104);
            this.kpiLeads.TabIndex = 2;
            // 
            // kpiDeals
            // 
            this.kpiDeals.BackColor = System.Drawing.Color.White;
            this.kpiDeals.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kpiDeals.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.kpiDeals.Name = "kpiDeals";
            this.kpiDeals.Size = new System.Drawing.Size(234, 104);
            this.kpiDeals.TabIndex = 3;
            // 
            // pnlContentSplit
            // 
            this.pnlContentSplit.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlContentSplit.ColumnCount = 2;
            this.pnlContentSplit.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 68F));
            this.pnlContentSplit.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32F));
            this.pnlContentSplit.Controls.Add(this.pnlLeftCard, 0, 0);
            this.pnlContentSplit.Controls.Add(this.pnlRightCard, 1, 0);
            this.pnlContentSplit.Location = new System.Drawing.Point(30, 206);
            this.pnlContentSplit.Name = "pnlContentSplit";
            this.pnlContentSplit.RowCount = 1;
            this.pnlContentSplit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlContentSplit.Size = new System.Drawing.Size(970, 466);
            this.pnlContentSplit.TabIndex = 4;
            // 
            // pnlLeftCard
            // 
            this.pnlLeftCard.BackColor = System.Drawing.Color.White;
            this.pnlLeftCard.Controls.Add(this.lblRecentEmpty);
            this.pnlLeftCard.Controls.Add(this.gridRecent);
            this.pnlLeftCard.Controls.Add(this.lblLeftTitle);
            this.pnlLeftCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLeftCard.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.pnlLeftCard.Name = "pnlLeftCard";
            this.pnlLeftCard.Padding = new System.Windows.Forms.Padding(16);
            this.pnlLeftCard.Size = new System.Drawing.Size(651, 470);
            this.pnlLeftCard.TabIndex = 0;
            // 
            // lblRecentEmpty
            // 
            this.lblRecentEmpty.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRecentEmpty.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.lblRecentEmpty.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblRecentEmpty.Location = new System.Drawing.Point(16, 50);
            this.lblRecentEmpty.Name = "lblRecentEmpty";
            this.lblRecentEmpty.Size = new System.Drawing.Size(618, 404);
            this.lblRecentEmpty.TabIndex = 2;
            this.lblRecentEmpty.Text = "No recent pipeline leads found.";
            this.lblRecentEmpty.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblRecentEmpty.Visible = false;
            // 
            // lblLeftTitle
            // 
            this.lblLeftTitle.AutoSize = true;
            this.lblLeftTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblLeftTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.lblLeftTitle.Location = new System.Drawing.Point(16, 16);
            this.lblLeftTitle.Name = "lblLeftTitle";
            this.lblLeftTitle.Size = new System.Drawing.Size(170, 21);
            this.lblLeftTitle.TabIndex = 0;
            this.lblLeftTitle.Text = "Recent Pipeline Leads";
            // 
            // gridRecent
            // 
            this.gridRecent.AllowUserToAddRows = false;
            this.gridRecent.AllowUserToDeleteRows = false;
            this.gridRecent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridRecent.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gridRecent.BackgroundColor = System.Drawing.Color.White;
            this.gridRecent.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gridRecent.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.gridRecent.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.gridRecent.ColumnHeadersHeight = 36;
            this.gridRecent.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.gridRecent.GridColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.gridRecent.Location = new System.Drawing.Point(16, 50);
            this.gridRecent.MultiSelect = false;
            this.gridRecent.Name = "gridRecent";
            this.gridRecent.ReadOnly = true;
            this.gridRecent.RowHeadersVisible = false;
            this.gridRecent.RowTemplate.Height = 44;
            this.gridRecent.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridRecent.ShowCellToolTips = true;
            this.gridRecent.Size = new System.Drawing.Size(618, 404);
            this.gridRecent.TabIndex = 1;
            // 
            // pnlRightCard
            // 
            this.pnlRightCard.BackColor = System.Drawing.Color.White;
            this.pnlRightCard.Controls.Add(this.lblStat3Value);
            this.pnlRightCard.Controls.Add(this.lblStat3Title);
            this.pnlRightCard.Controls.Add(this.lblStat2Value);
            this.pnlRightCard.Controls.Add(this.lblStat2Title);
            this.pnlRightCard.Controls.Add(this.lblStat1Value);
            this.pnlRightCard.Controls.Add(this.lblStat1Title);
            this.pnlRightCard.Controls.Add(this.lblRightTitle);
            this.pnlRightCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRightCard.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.pnlRightCard.Name = "pnlRightCard";
            this.pnlRightCard.Padding = new System.Windows.Forms.Padding(20);
            this.pnlRightCard.Size = new System.Drawing.Size(303, 470);
            this.pnlRightCard.TabIndex = 1;
            // 
            // lblRightTitle
            // 
            this.lblRightTitle.AutoSize = true;
            this.lblRightTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblRightTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.lblRightTitle.Location = new System.Drawing.Point(20, 16);
            this.lblRightTitle.Name = "lblRightTitle";
            this.lblRightTitle.Size = new System.Drawing.Size(157, 21);
            this.lblRightTitle.TabIndex = 0;
            this.lblRightTitle.Text = "Pipeline Highlights";
            // 
            // lblStat1Title
            // 
            this.lblStat1Title.AutoSize = true;
            this.lblStat1Title.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStat1Title.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblStat1Title.Location = new System.Drawing.Point(20, 65);
            this.lblStat1Title.Name = "lblStat1Title";
            this.lblStat1Title.Size = new System.Drawing.Size(147, 15);
            this.lblStat1Title.TabIndex = 1;
            this.lblStat1Title.Text = "TOTAL PIPELINE VOLUME";
            // 
            // lblStat1Value
            // 
            this.lblStat1Value.AutoSize = true;
            this.lblStat1Value.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblStat1Value.ForeColor = System.Drawing.Color.FromArgb(15, 91, 158);
            this.lblStat1Value.Location = new System.Drawing.Point(20, 85);
            this.lblStat1Value.Name = "lblStat1Value";
            this.lblStat1Value.Size = new System.Drawing.Size(42, 32);
            this.lblStat1Value.TabIndex = 2;
            this.lblStat1Value.Text = "₱0.00";
            // 
            // lblStat2Title
            // 
            this.lblStat2Title.AutoSize = true;
            this.lblStat2Title.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStat2Title.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblStat2Title.Location = new System.Drawing.Point(20, 145);
            this.lblStat2Title.Name = "lblStat2Title";
            this.lblStat2Title.Size = new System.Drawing.Size(130, 15);
            this.lblStat2Title.TabIndex = 3;
            this.lblStat2Title.Text = "AVAILABLE INVENTORY";
            // 
            // lblStat2Value
            // 
            this.lblStat2Value.AutoSize = true;
            this.lblStat2Value.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblStat2Value.ForeColor = System.Drawing.Color.FromArgb(16, 185, 129);
            this.lblStat2Value.Location = new System.Drawing.Point(20, 165);
            this.lblStat2Value.Name = "lblStat2Value";
            this.lblStat2Value.Size = new System.Drawing.Size(117, 32);
            this.lblStat2Value.TabIndex = 4;
            this.lblStat2Value.Text = "0 listings";
            // 
            // lblStat3Title
            // 
            this.lblStat3Title.AutoSize = true;
            this.lblStat3Title.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStat3Title.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblStat3Title.Location = new System.Drawing.Point(20, 225);
            this.lblStat3Title.Name = "lblStat3Title";
            this.lblStat3Title.Size = new System.Drawing.Size(124, 15);
            this.lblStat3Title.TabIndex = 5;
            this.lblStat3Title.Text = "ACTIVE SALES AGENTS";
            // 
            // lblStat3Value
            // 
            this.lblStat3Value.AutoSize = true;
            this.lblStat3Value.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblStat3Value.ForeColor = System.Drawing.Color.FromArgb(139, 92, 246);
            this.lblStat3Value.Location = new System.Drawing.Point(20, 245);
            this.lblStat3Value.Name = "lblStat3Value";
            this.lblStat3Value.Size = new System.Drawing.Size(95, 32);
            this.lblStat3Value.TabIndex = 6;
            this.lblStat3Value.Text = "0 active";
            // 
            // DashboardView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(244, 247, 251);
            this.Controls.Add(this.pnlContentSplit);
            this.Controls.Add(this.pnlKpiContainer);
            this.Controls.Add(this.lblSubtitle);
            this.Controls.Add(this.lblTitle);
            this.Name = "DashboardView";
            this.Size = new System.Drawing.Size(1030, 700);
            this.pnlKpiContainer.ResumeLayout(false);
            this.pnlContentSplit.ResumeLayout(false);
            this.pnlLeftCard.ResumeLayout(false);
            this.pnlLeftCard.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridRecent)).EndInit();
            this.pnlRightCard.ResumeLayout(false);
            this.pnlRightCard.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}

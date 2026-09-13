namespace CRMS_Peguit.winforms.Views.Properties
{
    partial class PropertyInputForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblAddress = null!;
        private System.Windows.Forms.TextBox txtAddress = null!;
        private System.Windows.Forms.Label lblType = null!;
        private System.Windows.Forms.ComboBox cmbPropertyType = null!;
        private System.Windows.Forms.Label lblStatus = null!;
        private System.Windows.Forms.ComboBox cmbStatus = null!;
        private System.Windows.Forms.Label lblPrice = null!;
        private System.Windows.Forms.TextBox txtPrice = null!;
        private System.Windows.Forms.Label lblOwner = null!;
        private System.Windows.Forms.ComboBox cmbOwner = null!;
        private System.Windows.Forms.Label lblAgent = null!;
        private System.Windows.Forms.ComboBox cmbListedByAgent = null!;
        private System.Windows.Forms.Button btnSave = null!;
        private System.Windows.Forms.Button btnCancel = null!;

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
            this.lblAddress = new System.Windows.Forms.Label();
            this.txtAddress = new System.Windows.Forms.TextBox();
            this.lblType = new System.Windows.Forms.Label();
            this.cmbPropertyType = new System.Windows.Forms.ComboBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.cmbStatus = new System.Windows.Forms.ComboBox();
            this.lblPrice = new System.Windows.Forms.Label();
            this.txtPrice = new System.Windows.Forms.TextBox();
            this.lblOwner = new System.Windows.Forms.Label();
            this.cmbOwner = new System.Windows.Forms.ComboBox();
            this.lblAgent = new System.Windows.Forms.Label();
            this.cmbListedByAgent = new System.Windows.Forms.ComboBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblAddress
            // 
            this.lblAddress.AutoSize = true;
            this.lblAddress.ForeColor = System.Drawing.Color.FromArgb(8, 52, 87);
            this.lblAddress.Location = new System.Drawing.Point(20, 20);
            this.lblAddress.Name = "lblAddress";
            this.lblAddress.Size = new System.Drawing.Size(64, 19);
            this.lblAddress.TabIndex = 0;
            this.lblAddress.Text = "Address *";
            // 
            // txtAddress
            // 
            this.txtAddress.BackColor = System.Drawing.Color.White;
            this.txtAddress.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtAddress.ForeColor = System.Drawing.Color.FromArgb(8, 52, 87);
            this.txtAddress.Location = new System.Drawing.Point(20, 45);
            this.txtAddress.MaxLength = 500;
            this.txtAddress.Multiline = true;
            this.txtAddress.Name = "txtAddress";
            this.txtAddress.Size = new System.Drawing.Size(500, 70);
            this.txtAddress.TabIndex = 1;
            // 
            // lblType
            // 
            this.lblType.AutoSize = true;
            this.lblType.ForeColor = System.Drawing.Color.FromArgb(8, 52, 87);
            this.lblType.Location = new System.Drawing.Point(20, 130);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(95, 19);
            this.lblType.TabIndex = 2;
            this.lblType.Text = "Property Type";
            // 
            // cmbPropertyType
            // 
            this.cmbPropertyType.BackColor = System.Drawing.Color.White;
            this.cmbPropertyType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPropertyType.ForeColor = System.Drawing.Color.FromArgb(8, 52, 87);
            this.cmbPropertyType.FormattingEnabled = true;
            this.cmbPropertyType.Items.AddRange(new object[] {
            "house",
            "condo",
            "lot",
            "townhouse",
            "commercial",
            "other"});
            this.cmbPropertyType.Location = new System.Drawing.Point(20, 155);
            this.cmbPropertyType.Name = "cmbPropertyType";
            this.cmbPropertyType.Size = new System.Drawing.Size(230, 27);
            this.cmbPropertyType.TabIndex = 3;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(8, 52, 87);
            this.lblStatus.Location = new System.Drawing.Point(290, 130);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(47, 19);
            this.lblStatus.TabIndex = 4;
            this.lblStatus.Text = "Status";
            // 
            // cmbStatus
            // 
            this.cmbStatus.BackColor = System.Drawing.Color.White;
            this.cmbStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStatus.ForeColor = System.Drawing.Color.FromArgb(8, 52, 87);
            this.cmbStatus.FormattingEnabled = true;
            this.cmbStatus.Items.AddRange(new object[] {
            "available",
            "under_contract",
            "sold",
            "off_market"});
            this.cmbStatus.Location = new System.Drawing.Point(290, 155);
            this.cmbStatus.Name = "cmbStatus";
            this.cmbStatus.Size = new System.Drawing.Size(230, 27);
            this.cmbStatus.TabIndex = 5;
            // 
            // lblPrice
            // 
            this.lblPrice.AutoSize = true;
            this.lblPrice.ForeColor = System.Drawing.Color.FromArgb(8, 52, 87);
            this.lblPrice.Location = new System.Drawing.Point(20, 200);
            this.lblPrice.Name = "lblPrice";
            this.lblPrice.Size = new System.Drawing.Size(76, 19);
            this.lblPrice.TabIndex = 6;
            this.lblPrice.Text = "Price (PHP)";
            // 
            // txtPrice
            // 
            this.txtPrice.BackColor = System.Drawing.Color.White;
            this.txtPrice.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPrice.ForeColor = System.Drawing.Color.FromArgb(8, 52, 87);
            this.txtPrice.Location = new System.Drawing.Point(20, 225);
            this.txtPrice.MaxLength = 18;
            this.txtPrice.Name = "txtPrice";
            this.txtPrice.Size = new System.Drawing.Size(230, 25);
            this.txtPrice.TabIndex = 7;
            // 
            // lblOwner
            // 
            this.lblOwner.AutoSize = true;
            this.lblOwner.ForeColor = System.Drawing.Color.FromArgb(8, 52, 87);
            this.lblOwner.Location = new System.Drawing.Point(20, 270);
            this.lblOwner.Name = "lblOwner";
            this.lblOwner.Size = new System.Drawing.Size(161, 19);
            this.lblOwner.TabIndex = 8;
            this.lblOwner.Text = "Owner (Seller Customer)";
            // 
            // cmbOwner
            // 
            this.cmbOwner.BackColor = System.Drawing.Color.White;
            this.cmbOwner.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbOwner.ForeColor = System.Drawing.Color.FromArgb(8, 52, 87);
            this.cmbOwner.FormattingEnabled = true;
            this.cmbOwner.Location = new System.Drawing.Point(20, 295);
            this.cmbOwner.Name = "cmbOwner";
            this.cmbOwner.Size = new System.Drawing.Size(500, 27);
            this.cmbOwner.TabIndex = 9;
            // 
            // lblAgent
            // 
            this.lblAgent.AutoSize = true;
            this.lblAgent.ForeColor = System.Drawing.Color.FromArgb(8, 52, 87);
            this.lblAgent.Location = new System.Drawing.Point(20, 340);
            this.lblAgent.Name = "lblAgent";
            this.lblAgent.Size = new System.Drawing.Size(107, 19);
            this.lblAgent.TabIndex = 10;
            this.lblAgent.Text = "Listed By Agent";
            // 
            // cmbListedByAgent
            // 
            this.cmbListedByAgent.BackColor = System.Drawing.Color.White;
            this.cmbListedByAgent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbListedByAgent.ForeColor = System.Drawing.Color.FromArgb(8, 52, 87);
            this.cmbListedByAgent.FormattingEnabled = true;
            this.cmbListedByAgent.Location = new System.Drawing.Point(20, 365);
            this.cmbListedByAgent.Name = "cmbListedByAgent";
            this.cmbListedByAgent.Size = new System.Drawing.Size(500, 27);
            this.cmbListedByAgent.TabIndex = 11;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(37, 103, 156);
            this.btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(335, 440);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(85, 38);
            this.btnSave.TabIndex = 12;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = false;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.BackColor = System.Drawing.Color.White;
            this.btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 198, 217);
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCancel.ForeColor = System.Drawing.Color.FromArgb(8, 52, 87);
            this.btnCancel.Location = new System.Drawing.Point(435, 440);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(85, 38);
            this.btnCancel.TabIndex = 13;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            // 
            // PropertyInputForm
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.FromArgb(243, 247, 250);
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(544, 521);
            this.Controls.Add(this.lblAddress);
            this.Controls.Add(this.txtAddress);
            this.Controls.Add(this.lblType);
            this.Controls.Add(this.cmbPropertyType);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.cmbStatus);
            this.Controls.Add(this.lblPrice);
            this.Controls.Add(this.txtPrice);
            this.Controls.Add(this.lblOwner);
            this.Controls.Add(this.cmbOwner);
            this.Controls.Add(this.lblAgent);
            this.Controls.Add(this.cmbListedByAgent);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnCancel);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(544, 480);
            this.Name = "PropertyInputForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Property Details";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}

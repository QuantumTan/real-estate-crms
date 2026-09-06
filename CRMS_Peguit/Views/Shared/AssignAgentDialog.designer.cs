namespace CRMS_Peguit.winforms.Views.Shared
{
    partial class AssignAgentDialog
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblHeader = null!;
        private System.Windows.Forms.Label lblSubtitle = null!;
        private System.Windows.Forms.Label lblRecord = null!;
        private System.Windows.Forms.Label lblAgent = null!;
        private System.Windows.Forms.ComboBox cmbAgents = null!;
        private System.Windows.Forms.Label lblNotes = null!;
        private System.Windows.Forms.TextBox txtNotes = null!;
        private System.Windows.Forms.CheckBox chkApprove = null!;
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
            this.lblHeader = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.lblRecord = new System.Windows.Forms.Label();
            this.lblAgent = new System.Windows.Forms.Label();
            this.cmbAgents = new System.Windows.Forms.ComboBox();
            this.lblNotes = new System.Windows.Forms.Label();
            this.txtNotes = new System.Windows.Forms.TextBox();
            this.chkApprove = new System.Windows.Forms.CheckBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblHeader
            // 
            this.lblHeader.AutoSize = true;
            this.lblHeader.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblHeader.ForeColor = System.Drawing.Color.FromArgb(8, 52, 87);
            this.lblHeader.Location = new System.Drawing.Point(20, 18);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(209, 25);
            this.lblHeader.TabIndex = 0;
            this.lblHeader.Text = "Assign Agent && Review";
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblSubtitle.Location = new System.Drawing.Point(22, 46);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(325, 15);
            this.lblSubtitle.TabIndex = 1;
            this.lblSubtitle.Text = "Assign a sales agent to this record and complete review.";
            // 
            // lblRecord
            // 
            this.lblRecord.AutoEllipsis = true;
            this.lblRecord.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblRecord.ForeColor = System.Drawing.Color.FromArgb(30, 41, 59);
            this.lblRecord.Location = new System.Drawing.Point(22, 75);
            this.lblRecord.Name = "lblRecord";
            this.lblRecord.Size = new System.Drawing.Size(390, 22);
            this.lblRecord.TabIndex = 2;
            this.lblRecord.Text = "Record: -";
            // 
            // lblAgent
            // 
            this.lblAgent.AutoSize = true;
            this.lblAgent.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblAgent.ForeColor = System.Drawing.Color.FromArgb(8, 52, 87);
            this.lblAgent.Location = new System.Drawing.Point(22, 105);
            this.lblAgent.Name = "lblAgent";
            this.lblAgent.Size = new System.Drawing.Size(124, 15);
            this.lblAgent.TabIndex = 3;
            this.lblAgent.Text = "Assign To Agent *";
            // 
            // cmbAgents
            // 
            this.cmbAgents.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAgents.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbAgents.FormattingEnabled = true;
            this.cmbAgents.Location = new System.Drawing.Point(24, 125);
            this.cmbAgents.Name = "cmbAgents";
            this.cmbAgents.Size = new System.Drawing.Size(388, 25);
            this.cmbAgents.TabIndex = 4;
            // 
            // lblNotes
            // 
            this.lblNotes.AutoSize = true;
            this.lblNotes.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblNotes.ForeColor = System.Drawing.Color.FromArgb(8, 52, 87);
            this.lblNotes.Location = new System.Drawing.Point(22, 160);
            this.lblNotes.Name = "lblNotes";
            this.lblNotes.Size = new System.Drawing.Size(147, 15);
            this.lblNotes.TabIndex = 5;
            this.lblNotes.Text = "Review Notes (Optional)";
            // 
            // txtNotes
            // 
            this.txtNotes.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtNotes.Location = new System.Drawing.Point(24, 180);
            this.txtNotes.Multiline = true;
            this.txtNotes.Name = "txtNotes";
            this.txtNotes.Size = new System.Drawing.Size(388, 60);
            this.txtNotes.TabIndex = 6;
            // 
            // chkApprove
            // 
            this.chkApprove.AutoSize = true;
            this.chkApprove.Checked = true;
            this.chkApprove.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkApprove.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkApprove.ForeColor = System.Drawing.Color.FromArgb(30, 41, 59);
            this.chkApprove.Location = new System.Drawing.Point(24, 252);
            this.chkApprove.Name = "chkApprove";
            this.chkApprove.Size = new System.Drawing.Size(225, 19);
            this.chkApprove.TabIndex = 7;
            this.chkApprove.Text = "Approve record upon assignment";
            this.chkApprove.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(8, 52, 87);
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(212, 290);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(200, 36);
            this.btnSave.TabIndex = 8;
            this.btnSave.Text = "Confirm Assignment";
            this.btnSave.UseVisualStyleBackColor = false;
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatAppearance.BorderSize = 0;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnCancel.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.btnCancel.Location = new System.Drawing.Point(106, 290);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 36);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            // 
            // AssignAgentDialog
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(434, 345);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.chkApprove);
            this.Controls.Add(this.txtNotes);
            this.Controls.Add(this.lblNotes);
            this.Controls.Add(this.cmbAgents);
            this.Controls.Add(this.lblAgent);
            this.Controls.Add(this.lblRecord);
            this.Controls.Add(this.lblSubtitle);
            this.Controls.Add(this.lblHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AssignAgentDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Assign Agent";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}

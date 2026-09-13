namespace CRMS_Peguit.winforms
{
    partial class LoginForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel pnlHero = null!;
        private System.Windows.Forms.Label lblBrandLogo = null!;
        private System.Windows.Forms.Label lblBrandSub = null!;
        private System.Windows.Forms.Label lblHeroTitle = null!;
        private System.Windows.Forms.Label lblHeroSubtitle = null!;
        private System.Windows.Forms.Label lblHeroFooter = null!;

        private System.Windows.Forms.Panel pnlForm = null!;
        private System.Windows.Forms.Label lblWelcome = null!;
        private System.Windows.Forms.Label lblWelcomeSub = null!;
        private System.Windows.Forms.Label lblCompanyId = null!;
        private System.Windows.Forms.TextBox txtCompanyId = null!;
        private System.Windows.Forms.Label lblEmail = null!;
        private System.Windows.Forms.TextBox txtEmail = null!;
        private System.Windows.Forms.Label lblPassword = null!;
        private System.Windows.Forms.TextBox txtPassword = null!;
        private System.Windows.Forms.CheckBox chkShowPassword = null!;
        private System.Windows.Forms.LinkLabel lnkForgotPassword = null!;
        private System.Windows.Forms.Label lblError = null!;
        private System.Windows.Forms.Button btnLogin = null!;

        // Backward compatibility field
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
            this.pnlHero = new System.Windows.Forms.Panel();
            this.lblBrandLogo = new System.Windows.Forms.Label();
            this.lblBrandSub = new System.Windows.Forms.Label();
            this.lblHeroTitle = new System.Windows.Forms.Label();
            this.lblHeroSubtitle = new System.Windows.Forms.Label();
            this.lblHeroFooter = new System.Windows.Forms.Label();
            this.pnlForm = new System.Windows.Forms.Panel();
            this.lblWelcome = new System.Windows.Forms.Label();
            this.lblWelcomeSub = new System.Windows.Forms.Label();
            this.lblCompanyId = new System.Windows.Forms.Label();
            this.txtCompanyId = new System.Windows.Forms.TextBox();
            this.lblEmail = new System.Windows.Forms.Label();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.chkShowPassword = new System.Windows.Forms.CheckBox();
            this.lnkForgotPassword = new System.Windows.Forms.LinkLabel();
            this.lblError = new System.Windows.Forms.Label();
            this.btnLogin = new System.Windows.Forms.Button();
            this.lblLogo = new System.Windows.Forms.Label();
            this.pnlHero.SuspendLayout();
            this.pnlForm.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHero
            // 
            this.pnlHero.BackColor = System.Drawing.Color.FromArgb(6, 29, 51);
            this.pnlHero.Controls.Add(this.lblBrandLogo);
            this.pnlHero.Controls.Add(this.lblBrandSub);
            this.pnlHero.Controls.Add(this.lblHeroTitle);
            this.pnlHero.Controls.Add(this.lblHeroSubtitle);
            this.pnlHero.Controls.Add(this.lblHeroFooter);
            this.pnlHero.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlHero.Location = new System.Drawing.Point(0, 0);
            this.pnlHero.Name = "pnlHero";
            this.pnlHero.Size = new System.Drawing.Size(390, 600);
            this.pnlHero.TabIndex = 0;
            // 
            // lblBrandLogo
            // 
            this.lblBrandLogo.AutoSize = true;
            this.lblBrandLogo.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblBrandLogo.ForeColor = System.Drawing.Color.White;
            this.lblBrandLogo.Location = new System.Drawing.Point(40, 40);
            this.lblBrandLogo.Name = "lblBrandLogo";
            this.lblBrandLogo.Size = new System.Drawing.Size(95, 28);
            this.lblBrandLogo.TabIndex = 0;
            this.lblBrandLogo.Text = "⛛ NEXA";
            // 
            // lblBrandSub
            // 
            this.lblBrandSub.AutoSize = true;
            this.lblBrandSub.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblBrandSub.ForeColor = System.Drawing.Color.FromArgb(140, 163, 186);
            this.lblBrandSub.Location = new System.Drawing.Point(43, 70);
            this.lblBrandSub.Name = "lblBrandSub";
            this.lblBrandSub.Size = new System.Drawing.Size(76, 13);
            this.lblBrandSub.TabIndex = 1;
            this.lblBrandSub.Text = "CRM SYSTEM";
            // 
            // lblHeroTitle
            // 
            this.lblHeroTitle.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold);
            this.lblHeroTitle.ForeColor = System.Drawing.Color.White;
            this.lblHeroTitle.Location = new System.Drawing.Point(40, 200);
            this.lblHeroTitle.Name = "lblHeroTitle";
            this.lblHeroTitle.Size = new System.Drawing.Size(320, 100);
            this.lblHeroTitle.TabIndex = 2;
            this.lblHeroTitle.Text = "Built for\r\nreal estate teams.";
            // 
            // lblHeroSubtitle
            // 
            this.lblHeroSubtitle.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.lblHeroSubtitle.ForeColor = System.Drawing.Color.FromArgb(140, 163, 186);
            this.lblHeroSubtitle.Location = new System.Drawing.Point(40, 310);
            this.lblHeroSubtitle.Name = "lblHeroSubtitle";
            this.lblHeroSubtitle.Size = new System.Drawing.Size(310, 80);
            this.lblHeroSubtitle.TabIndex = 3;
            this.lblHeroSubtitle.Text = "Manage leads, properties, and client relationships in one place. Access is automatically scoped to your role.";
            // 
            // lblHeroFooter
            // 
            this.lblHeroFooter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblHeroFooter.AutoSize = true;
            this.lblHeroFooter.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblHeroFooter.ForeColor = System.Drawing.Color.FromArgb(150, 178, 204);
            this.lblHeroFooter.Location = new System.Drawing.Point(40, 560);
            this.lblHeroFooter.Name = "lblHeroFooter";
            this.lblHeroFooter.Size = new System.Drawing.Size(262, 15);
            this.lblHeroFooter.TabIndex = 4;
            this.lblHeroFooter.Text = "© 2026 NEXA CRM · Role-based access control";
            // 
            // pnlForm
            // 
            this.pnlForm.AutoScroll = true;
            this.pnlForm.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            this.pnlForm.Controls.Add(this.lblWelcome);
            this.pnlForm.Controls.Add(this.lblWelcomeSub);
            this.pnlForm.Controls.Add(this.lblCompanyId);
            this.pnlForm.Controls.Add(this.txtCompanyId);
            this.pnlForm.Controls.Add(this.lblEmail);
            this.pnlForm.Controls.Add(this.txtEmail);
            this.pnlForm.Controls.Add(this.lblPassword);
            this.pnlForm.Controls.Add(this.txtPassword);
            this.pnlForm.Controls.Add(this.chkShowPassword);
            this.pnlForm.Controls.Add(this.lnkForgotPassword);
            this.pnlForm.Controls.Add(this.lblError);
            this.pnlForm.Controls.Add(this.btnLogin);
            this.pnlForm.Controls.Add(this.lblLogo);
            this.pnlForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlForm.Location = new System.Drawing.Point(390, 0);
            this.pnlForm.Name = "pnlForm";
            this.pnlForm.Size = new System.Drawing.Size(570, 600);
            this.pnlForm.TabIndex = 1;
            // 
            // lblWelcome
            // 
            this.lblWelcome.AutoSize = true;
            this.lblWelcome.Font = new System.Drawing.Font("Segoe UI", 22F, System.Drawing.FontStyle.Bold);
            this.lblWelcome.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.lblWelcome.Location = new System.Drawing.Point(80, 60);
            this.lblWelcome.Name = "lblWelcome";
            this.lblWelcome.Size = new System.Drawing.Size(225, 41);
            this.lblWelcome.TabIndex = 0;
            this.lblWelcome.Text = "Welcome back";
            // 
            // lblWelcomeSub
            // 
            this.lblWelcomeSub.AutoSize = true;
            this.lblWelcomeSub.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.lblWelcomeSub.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblWelcomeSub.Location = new System.Drawing.Point(82, 105);
            this.lblWelcomeSub.Name = "lblWelcomeSub";
            this.lblWelcomeSub.Size = new System.Drawing.Size(232, 19);
            this.lblWelcomeSub.TabIndex = 1;
            this.lblWelcomeSub.Text = "Sign in to your account to continue.";
            // 
            // lblCompanyId
            // 
            this.lblCompanyId.AutoSize = true;
            this.lblCompanyId.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblCompanyId.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.lblCompanyId.Location = new System.Drawing.Point(82, 148);
            this.lblCompanyId.Name = "lblCompanyId";
            this.lblCompanyId.Size = new System.Drawing.Size(91, 17);
            this.lblCompanyId.TabIndex = 2;
            this.lblCompanyId.Text = "Company ID *";
            // 
            // txtCompanyId
            // 
            this.txtCompanyId.BackColor = System.Drawing.Color.White;
            this.txtCompanyId.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtCompanyId.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.txtCompanyId.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.txtCompanyId.Location = new System.Drawing.Point(82, 170);
            this.txtCompanyId.Name = "txtCompanyId";
            this.txtCompanyId.PlaceholderText = "e.g. 1";
            this.txtCompanyId.Size = new System.Drawing.Size(390, 26);
            this.txtCompanyId.TabIndex = 3;
            // 
            // lblEmail
            // 
            this.lblEmail.AutoSize = true;
            this.lblEmail.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblEmail.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.lblEmail.Location = new System.Drawing.Point(82, 212);
            this.lblEmail.Name = "lblEmail";
            this.lblEmail.Size = new System.Drawing.Size(102, 17);
            this.lblEmail.TabIndex = 4;
            this.lblEmail.Text = "Email address *";
            // 
            // txtEmail
            // 
            this.txtEmail.BackColor = System.Drawing.Color.White;
            this.txtEmail.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtEmail.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.txtEmail.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.txtEmail.Location = new System.Drawing.Point(82, 234);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.PlaceholderText = "you@company.com";
            this.txtEmail.Size = new System.Drawing.Size(390, 26);
            this.txtEmail.TabIndex = 5;
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblPassword.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.lblPassword.Location = new System.Drawing.Point(82, 276);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(74, 17);
            this.lblPassword.TabIndex = 6;
            this.lblPassword.Text = "Password *";
            // 
            // txtPassword
            // 
            this.txtPassword.BackColor = System.Drawing.Color.White;
            this.txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPassword.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.txtPassword.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.txtPassword.Location = new System.Drawing.Point(82, 298);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PlaceholderText = "••••••••";
            this.txtPassword.Size = new System.Drawing.Size(390, 26);
            this.txtPassword.TabIndex = 7;
            this.txtPassword.UseSystemPasswordChar = true;
            // 
            // chkShowPassword
            // 
            this.chkShowPassword.AutoSize = true;
            this.chkShowPassword.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkShowPassword.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.chkShowPassword.Location = new System.Drawing.Point(82, 335);
            this.chkShowPassword.Name = "chkShowPassword";
            this.chkShowPassword.Size = new System.Drawing.Size(107, 19);
            this.chkShowPassword.TabIndex = 8;
            this.chkShowPassword.Text = "Show password";
            this.chkShowPassword.UseVisualStyleBackColor = true;
            // 
            // lnkForgotPassword
            // 
            this.lnkForgotPassword.ActiveLinkColor = System.Drawing.Color.FromArgb(15, 91, 158);
            this.lnkForgotPassword.AutoSize = true;
            this.lnkForgotPassword.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
            this.lnkForgotPassword.LinkColor = System.Drawing.Color.FromArgb(15, 91, 158);
            this.lnkForgotPassword.Location = new System.Drawing.Point(368, 336);
            this.lnkForgotPassword.Name = "lnkForgotPassword";
            this.lnkForgotPassword.Size = new System.Drawing.Size(104, 15);
            this.lnkForgotPassword.TabIndex = 9;
            this.lnkForgotPassword.TabStop = true;
            this.lnkForgotPassword.Text = "Forgot password?";
            // 
            // lblError
            // 
            this.lblError.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblError.ForeColor = System.Drawing.Color.FromArgb(180, 40, 40);
            this.lblError.Location = new System.Drawing.Point(82, 362);
            this.lblError.Name = "lblError";
            this.lblError.Size = new System.Drawing.Size(390, 30);
            this.lblError.TabIndex = 10;
            // 
            // btnLogin
            // 
            this.btnLogin.BackColor = System.Drawing.Color.FromArgb(15, 91, 158);
            this.btnLogin.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLogin.FlatAppearance.BorderSize = 0;
            this.btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogin.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.btnLogin.ForeColor = System.Drawing.Color.White;
            this.btnLogin.Location = new System.Drawing.Point(82, 398);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(390, 42);
            this.btnLogin.TabIndex = 11;
            this.btnLogin.Text = "Sign in  →";
            this.btnLogin.UseVisualStyleBackColor = false;
            // 
            // lblLogo
            // 
            this.lblLogo.Location = new System.Drawing.Point(0, 0);
            this.lblLogo.Name = "lblLogo";
            this.lblLogo.Size = new System.Drawing.Size(0, 0);
            this.lblLogo.TabIndex = 13;
            this.lblLogo.Visible = false;
            // 
            // LoginForm
            // 
            this.AcceptButton = this.btnLogin;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            this.ClientSize = new System.Drawing.Size(960, 600);
            this.Controls.Add(this.pnlForm);
            this.Controls.Add(this.pnlHero);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "LoginForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "NEXA CRM SYSTEM — Sign In";
            this.pnlHero.ResumeLayout(false);
            this.pnlHero.PerformLayout();
            this.pnlForm.ResumeLayout(false);
            this.pnlForm.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
    }
}

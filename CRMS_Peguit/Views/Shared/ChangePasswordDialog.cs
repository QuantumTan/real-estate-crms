using System;
using System.Drawing;
using System.Windows.Forms;
using CRMS_Peguit.winforms.Models.Services;

namespace CRMS_Peguit.winforms.Views.Shared
{
    public partial class ChangePasswordDialog : Form
    {
        public string NewPassword { get; private set; } = string.Empty;

        public ChangePasswordDialog(string userName)
        {
            InitializeComponent();
            lblSubtitle.Text = $"Set a new secure password for {userName}.";

            UiRadiusHelper.StyleButton(btnSave, 8);
            UiRadiusHelper.StyleButton(btnCancel, 8);

            chkShowPassword.CheckedChanged += (_, _) =>
            {
                char mask = chkShowPassword.Checked ? '\0' : '●';
                txtNewPassword.PasswordChar = mask;
                txtConfirmPassword.PasswordChar = mask;
            };

            btnSave.Click += (_, _) =>
            {
                string pass = txtNewPassword.Text;
                string confirm = txtConfirmPassword.Text;

                if (string.IsNullOrWhiteSpace(pass))
                {
                    ShowError("Password cannot be empty.");
                    txtNewPassword.Focus();
                    return;
                }

                if (pass.Length < 6)
                {
                    ShowError("Password must be at least 6 characters long.");
                    txtNewPassword.Focus();
                    return;
                }

                if (pass != confirm)
                {
                    ShowError("Passwords do not match.");
                    txtConfirmPassword.Focus();
                    return;
                }

                NewPassword = pass;
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            txtNewPassword.TextChanged += (_, _) => ClearError();
            txtConfirmPassword.TextChanged += (_, _) => ClearError();
        }

        private void ShowError(string message)
        {
            lblError.Text = message;
            lblError.Visible = true;
        }

        private void ClearError()
        {
            if (lblError.Visible)
            {
                lblError.Text = string.Empty;
                lblError.Visible = false;
            }
        }
    }
}

using CRMS_Peguit.winforms.Auth;
using CRMS_Peguit.winforms.Models.Services;
using ReaLTaiizor.Forms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CRMS_Peguit.winforms
{
    public partial class LoginForm : Form
    {
        private readonly AuthService _authService;

        // Store main form and its FormClosed handler.
        private Form1? _mainForm;
        private FormClosedEventHandler? _mainFormClosedHandler;   // <-- fixed type

        // ==========================================================
        // DEFAULT CONSTRUCTOR
        // ==========================================================

        public LoginForm()
            : this("https://localhost:7259/")
        {
        }

        // ==========================================================
        // API CONSTRUCTOR
        // ==========================================================

        public LoginForm(string apiBaseUrl)
        {
            _authService = new AuthService(apiBaseUrl);
            InitializeComponent();
            BindEvents();
        }

        private void BindEvents()
        {
            chkShowPassword.CheckedChanged += (_, _) =>
            {
                txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
            };

            lnkForgotPassword.Click += LnkForgotPasswordClick;
            btnLogin.Click += BtnLogin_Click;
        }

        // ==========================================================
        // LOGIN BUTTON
        // ==========================================================

        private async void BtnLogin_Click(object? sender, EventArgs e)
        {
            lblError.Text = "";

            if (string.IsNullOrWhiteSpace(txtCompanyId.Text))
            {
                lblError.Text = "Company ID is required.";
                return;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                lblError.Text = "Email is required.";
                return;
            }

            if (!ContactEmailService.IsValidEmail(txtEmail.Text))
            {
                lblError.Text = "Enter a valid email address.";
                txtEmail.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                lblError.Text = "Password is required.";
                return;
            }

            btnLogin.Enabled = false;
            btnLogin.Text = "Signing in...";

            try
            {
                var result = await _authService.LoginAsync(
                    txtCompanyId.Text.Trim(),
                    txtEmail.Text.Trim(),
                    txtPassword.Text
                );

                if (!result.Success)
                {
                    lblError.Text = result.ErrorMessage ?? "Login failed.";
                    return;
                }

                if (result.WasOffline)
                {
                    MessageBox.Show(
                        "You're offline. Signed in using your last saved credentials.",
                        "Offline Mode",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }

                // Create main form and store it.
                _mainForm = new Form1();

                // Store the handler – type now matches FormClosedEventHandler.
                _mainFormClosedHandler = (s, args) => Close();

                _mainForm.FormClosed += _mainFormClosedHandler;

                _mainForm.Show();
                Hide();
            }
            catch (Exception ex)
            {
                lblError.Text = $"Unexpected error: {ex.Message}";
            }
            finally
            {
                btnLogin.Enabled = true;
                btnLogin.Text = "Sign In";
            }
        }

        private async void LnkForgotPasswordClick(object? sender, EventArgs e)
        {
            lblError.Text = "";

            if (string.IsNullOrWhiteSpace(txtCompanyId.Text))
            {
                lblError.Text = "Company ID is required before requesting a reset.";
                txtCompanyId.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                lblError.Text = "Email is required before requesting a reset.";
                txtEmail.Focus();
                return;
            }

            if (!ContactEmailService.IsValidEmail(txtEmail.Text))
            {
                lblError.Text = "Enter a valid email before requesting a reset.";
                txtEmail.Focus();
                return;
            }

            lnkForgotPassword.Enabled = false;

            try
            {
                var result = await ContactEmailService.SendForgotPasswordAsync(
                    txtEmail.Text.Trim(),
                    txtCompanyId.Text.Trim());

                MessageBox.Show(
                    result.Message,
                    result.Success ? "Password Reset Email" : "Email Error",
                    MessageBoxButtons.OK,
                    result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning
                );
            }
            finally
            {
                lnkForgotPassword.Enabled = true;
            }
        }

        // ==========================================================
        // Called by Form1 during logout to detach and show login form.
        // ==========================================================

        public void PrepareForLogout()
        {
            if (_mainForm != null && _mainFormClosedHandler != null)
            {
                _mainForm.FormClosed -= _mainFormClosedHandler;
                _mainForm = null;
                _mainFormClosedHandler = null;
            }

            Show();
            Activate();
        }
    }
}

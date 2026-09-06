using CRMS_Peguit.winforms.Auth;
using CRMS_Peguit.winforms.Models.Services;
using ReaLTaiizor.Forms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CRMS_Peguit.winforms
{
    public partial class LoginForm : MaterialForm
    {
        private readonly AuthService _authService;

        private TextBox txtCompanyId = null!;
        private TextBox txtEmail = null!;
        private TextBox txtPassword = null!;
        private Button btnLogin = null!;
        private CheckBox chkShowPassword = null!;
        private LinkLabel lnkForgotPassword = null!;
        private Label lblError = null!;
        private Label lblLogo = null!;

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
            BuildUi();
        }

        // ==========================================================
        // BUILD LOGIN UI
        // ==========================================================

        private void BuildUi()
        {
            ClientSize = new Size(420, 475);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "NEXA - Sign In";
            BackColor = Theme.Background;

            // Logo
            lblLogo = new Label
            {
                Text = "NEXA",
                ForeColor = Theme.Primary,
                Font = new Font("Segoe UI", 26, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(150, 30)
            };

            // Company ID
            var lblCompanyId = new Label
            {
                Text = "Company ID",
                ForeColor = Theme.TextPrimary,
                Location = new Point(50, 100),
                AutoSize = true
            };

            txtCompanyId = new TextBox
            {
                Location = new Point(50, 120),
                Size = new Size(320, 28),
                Font = new Font("Segoe UI", 10.5f)
            };

            // Email
            var lblEmail = new Label
            {
                Text = "Email",
                ForeColor = Theme.TextPrimary,
                Location = new Point(50, 160),
                AutoSize = true
            };

            txtEmail = new TextBox
            {
                Location = new Point(50, 180),
                Size = new Size(320, 28),
                Font = new Font("Segoe UI", 10.5f),
                PlaceholderText = "name@example.com"
            };

            // Password
            var lblPassword = new Label
            {
                Text = "Password",
                ForeColor = Theme.TextPrimary,
                Location = new Point(50, 220),
                AutoSize = true
            };

            txtPassword = new TextBox
            {
                Location = new Point(50, 240),
                Size = new Size(320, 28),
                Font = new Font("Segoe UI", 10.5f),
                UseSystemPasswordChar = true
            };

            chkShowPassword = new CheckBox
            {
                Text = "Show password",
                ForeColor = Theme.TextPrimary,
                BackColor = Theme.Background,
                Location = new Point(50, 273),
                AutoSize = true
            };
            chkShowPassword.CheckedChanged += (_, _) =>
            {
                txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
            };

            lnkForgotPassword = new LinkLabel
            {
                Text = "Forgot password?",
                Location = new Point(263, 273),
                AutoSize = true,
                LinkColor = Theme.Primary,
                ActiveLinkColor = Theme.Primary
            };
            lnkForgotPassword.Click += LnkForgotPasswordClick;

            // Error label
            lblError = new Label
            {
                ForeColor = Color.IndianRed,
                Location = new Point(50, 305),
                Size = new Size(320, 40),
                Font = new Font("Segoe UI", 9f)
            };

            // Login button
            btnLogin = new Button
            {
                Text = "Sign In",
                BackColor = Theme.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(50, 360),
                Size = new Size(320, 40),
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };
            btnLogin.Click += BtnLogin_Click;

            // Add controls
            Controls.Add(lblLogo);
            Controls.Add(lblCompanyId);
            Controls.Add(txtCompanyId);
            Controls.Add(lblEmail);
            Controls.Add(txtEmail);
            Controls.Add(lblPassword);
            Controls.Add(txtPassword);
            Controls.Add(chkShowPassword);
            Controls.Add(lnkForgotPassword);
            Controls.Add(lblError);
            Controls.Add(btnLogin);
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

        private void LnkForgotPasswordClick(object? sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtEmail.Text) &&
                !ContactEmailService.IsValidEmail(txtEmail.Text))
            {
                lblError.Text = "Enter a valid email before requesting a reset.";
                txtEmail.Focus();
                return;
            }

            MessageBox.Show(
                "Password reset email is ready for SMTP integration. Once SMTP settings are configured, NEXA will send a reset link to the account email.",
                "Forgot Password",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
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

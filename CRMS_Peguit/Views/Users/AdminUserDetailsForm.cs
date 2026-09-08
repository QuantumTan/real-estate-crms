using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using CRMS_Peguit.domain.entities;
using CRMS_Peguit.winforms.Controllers;
using CRMS_Peguit.winforms.Models.Services;
using CRMS_Peguit.winforms.Auth;

namespace CRMS_Peguit.winforms.Views.Users
{
    public partial class AdminUserDetailsForm : Form
    {
        private readonly UserController _controller;
        private readonly User _user;
        private readonly Dictionary<int, string> _roles;

        public AdminUserDetailsForm() : this(new UserController(), new User(), new Dictionary<int, string>())
        {
        }

        public AdminUserDetailsForm(UserController controller, User user, Dictionary<int, string> roles)
        {
            _controller = controller;
            _user = user;
            _roles = roles;
            InitializeComponent();

            lblNameValue.Text = _user.FullName;
            lblEmailValue.Text = _user.Email;
            lblRoleValue.Text = _roles.ContainsKey(_user.RoleId) ? _roles[_user.RoleId] : "Unknown";
            lblStatusValue.Text = _user.Status.ToUpper();

            UiRadiusHelper.StyleButton(btnChangePassword, 8);

            UiRadiusHelper.StyleButton(btnToggleStatus, 8);

            UiRadiusHelper.StyleButton(btnClose, 8);

            btnChangePassword.Click += async (s, e) => await ChangePasswordAsync();
            btnToggleStatus.Click += async (s, e) => await ToggleStatusAsync();

            btnToggleStatus.Text = _user.Status == "active" ? "Deactivate User" : "Reactivate User";
            btnToggleStatus.BackColor = _user.Status == "active" ? Color.IndianRed : Color.MediumSeaGreen;

            if (_user.UserId == CurrentSession.UserId)
            {
                btnToggleStatus.Enabled = false;
                btnToggleStatus.BackColor = Color.Gray;
            }

            btnClose.Click += (s, e) => { this.DialogResult = DialogResult.OK; this.Close(); };
        }

        private async Task ChangePasswordAsync()
        {
            var newPassword = Microsoft.VisualBasic.Interaction.InputBox("Enter new password for the user:", "Change Password", "");
            if (string.IsNullOrWhiteSpace(newPassword)) return;

            var confirm = MessageBox.Show($"Are you sure you want to change the password for {_user.FullName}?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            try
            {
                await _controller.ChangePasswordAsync(_user.UserId, newPassword);
                MessageBox.Show("Password changed successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task ToggleStatusAsync()
        {
            try
            {
                if (_user.Status == "active")
                {
                    var confirm = MessageBox.Show($"Are you sure you want to deactivate {_user.FullName}?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (confirm != DialogResult.Yes) return;
                    
                    await _controller.DeactivateAsync(_user.UserId);
                    _user.Status = "inactive";
                }
                else
                {
                    await _controller.ReactivateAsync(_user.UserId);
                    _user.Status = "active";
                }

                lblStatusValue.Text = _user.Status.ToUpper();
                btnToggleStatus.Text = _user.Status == "active" ? "Deactivate User" : "Reactivate User";
                btnToggleStatus.BackColor = _user.Status == "active" ? Color.IndianRed : Color.MediumSeaGreen;
                
                MessageBox.Show($"User is now {_user.Status}.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}


using CRMS_Peguit.domain.entities;
using CRMS_Peguit.winforms.Controllers;
using CRMS_Peguit.winforms.Models.Services;

namespace CRMS_Peguit.winforms.Views.Leads
{
    /// <summary>
    /// Dialog form for creating or editing a lead.
    /// </summary>
    public partial class LeadInputForm : Form
    {
        public Lead? Result { get; private set; }

        private readonly Lead? _existingLead;

        public LeadInputForm(Lead? lead = null)
        {
            _existingLead = lead;

            InitializeComponent();
            UiRadiusHelper.StyleButton(btnSave, 8);
            UiRadiusHelper.StyleButton(btnCancel, 8);
            UiRadiusHelper.AttachHoverFeedback(btnCancel, Color.White, Color.FromArgb(241, 245, 249));
            UiRadiusHelper.AttachHoverFeedback(btnSave, Theme.Primary, Theme.PrimaryDark);
            btnSave.Click += BtnSaveClick;
            LoadData();
        }

        private void LoadData()
        {
            Text = _existingLead is null
                ? "Add New Lead"
                : $"Edit Lead - {_existingLead.FullName}";

            if (_existingLead is not null)
            {
                txtFirstName.Text = _existingLead.FirstName;
                txtMiddleName.Text = _existingLead.MiddleName ?? string.Empty;
                txtLastName.Text = _existingLead.LastName;
                txtSuffix.Text = _existingLead.Suffix ?? string.Empty;
                txtEmail.Text = _existingLead.Email ?? string.Empty;
                txtPhone.Text = _existingLead.Phone ?? string.Empty;
                txtSource.Text = _existingLead.Source ?? string.Empty;
                txtNotes.Text = _existingLead.Notes ?? string.Empty;
                txtExpectedValue.Text = _existingLead.ExpectedValue.HasValue
                    ? _existingLead.ExpectedValue.Value.ToString("F2")
                    : string.Empty;
                SelectComboValue(cmbStage, _existingLead.Stage, "new");
                SelectComboValue(cmbPriority, _existingLead.Priority, "medium");
            }
            else
            {
                cmbStage.SelectedItem = "new";
                cmbPriority.SelectedItem = "medium";
            }
        }

        private void BtnSaveClick(object? sender, EventArgs e)
        {
            string firstName = txtFirstName.Text.Trim();
            string lastName = txtLastName.Text.Trim();

            if (!LeadController.ValidateLeadInput(
                firstName,
                lastName,
                txtEmail.Text,
                txtExpectedValue.Text,
                out decimal? expectedValue,
                out string? errorMessage,
                out string? errorField))
            {
                Control target = errorField switch
                {
                    "FirstName" => txtFirstName,
                    "LastName" => txtLastName,
                    "Email" => txtEmail,
                    "ExpectedValue" => txtExpectedValue,
                    _ => txtFirstName
                };
                ShowValidationError(errorMessage ?? "Validation error.", target);
                return;
            }

            if (_existingLead is not null)
            {
                _existingLead.FirstName = firstName;
                _existingLead.MiddleName = NullIfEmpty(txtMiddleName.Text);
                _existingLead.LastName = lastName;
                _existingLead.Suffix = NullIfEmpty(txtSuffix.Text);
                _existingLead.Email = NullIfEmpty(txtEmail.Text);
                _existingLead.Phone = NullIfEmpty(txtPhone.Text);
                _existingLead.Source = NullIfEmpty(txtSource.Text);
                _existingLead.Stage = cmbStage.SelectedItem?.ToString() ?? "new";
                _existingLead.Priority = cmbPriority.SelectedItem?.ToString() ?? "medium";
                _existingLead.ExpectedValue = expectedValue;
                _existingLead.Notes = NullIfEmpty(txtNotes.Text);

                Result = _existingLead;
            }
            else
            {
                Result = new Lead
                {
                    FirstName = firstName,
                    MiddleName = NullIfEmpty(txtMiddleName.Text),
                    LastName = lastName,
                    Suffix = NullIfEmpty(txtSuffix.Text),
                    Email = NullIfEmpty(txtEmail.Text),
                    Phone = NullIfEmpty(txtPhone.Text),
                    Source = NullIfEmpty(txtSource.Text),
                    Stage = cmbStage.SelectedItem?.ToString() ?? "new",
                    Priority = cmbPriority.SelectedItem?.ToString() ?? "medium",
                    ExpectedValue = expectedValue,
                    Notes = NullIfEmpty(txtNotes.Text),
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedAt = null
                };
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private static string? NullIfEmpty(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static void SelectComboValue(ComboBox comboBox, string? value, string fallback)
        {
            string selectedValue = string.IsNullOrWhiteSpace(value) ? fallback : value;
            comboBox.SelectedItem = comboBox.Items.Contains(selectedValue) ? selectedValue : fallback;
        }

        private static void ShowValidationError(string message, Control control)
        {
            MessageBox.Show(message, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            control.Focus();
        }

        private void txtSuffix_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

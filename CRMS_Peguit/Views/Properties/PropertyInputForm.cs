using CRMS_Peguit.domain.entities;
using CRMS_Peguit.winforms.Auth;
using CRMS_Peguit.winforms.Controllers;
using CRMS_Peguit.winforms.Models.Services;

namespace CRMS_Peguit.winforms.Views.Properties
{
    public class PropertyInputForm : Form
    {
        private readonly Property? _existingProperty;
        private readonly List<CustomerPickerItem> _owners;
        private readonly List<AgentPickerItem> _agents;

        private TextBox txtAddress = null!;
        private TextBox txtPrice = null!;
        private ComboBox cmbPropertyType = null!;
        private ComboBox cmbStatus = null!;
        private ComboBox cmbOwner = null!;
        private ComboBox cmbAgent = null!;
        private Button btnSave = null!;
        private Button btnCancel = null!;

        public Property? Result { get; private set; }

        public PropertyInputForm(
            List<CustomerPickerItem> owners,
            List<AgentPickerItem> agents,
            Property? property = null)
        {
            _owners = owners;
            _agents = agents;
            _existingProperty = property;

            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            Width = 560;
            Height = 560;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Theme.Background;
            ForeColor = Theme.TextPrimary;
            Font = new Font("Segoe UI", 10);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;

            Text = _existingProperty is null
                ? "Add New Property"
                : $"Edit Property #{_existingProperty.PropertyId}";

            var lblAddress = CreateLabel("Address *", 20, 20);
            txtAddress = CreateTextBox(20, 45, 500);
            txtAddress.Height = 70;
            txtAddress.Multiline = true;
            txtAddress.MaxLength = 500;

            var lblType = CreateLabel("Property Type", 20, 130);
            cmbPropertyType = CreateComboBox(20, 155, 230);
            cmbPropertyType.Items.AddRange(new[] { "house", "condo", "lot", "townhouse", "commercial", "other" });

            var lblStatus = CreateLabel("Status", 290, 130);
            cmbStatus = CreateComboBox(290, 155, 230);
            cmbStatus.Items.AddRange(new[] { "available", "reserved", "sold", "inactive" });

            var lblPrice = CreateLabel("Price *", 20, 210);
            txtPrice = CreateTextBox(20, 235, 230);

            var lblOwner = CreateLabel("Owner Customer *", 20, 290);
            cmbOwner = CreateComboBox(20, 315, 500);
            cmbOwner.DataSource = _owners;

            var lblAgent = CreateLabel("Listed By Agent *", 20, 370);
            cmbAgent = CreateComboBox(20, 395, 500);
            cmbAgent.DataSource = _agents;
            cmbAgent.Enabled = !RbacService.ShouldAutoAssignCreatedRecord;

            btnSave = new Button
            {
                Text = "Save",
                Location = new Point(330, 465),
                Width = 90,
                Height = 38,
                BackColor = Theme.Primary,
                ForeColor = Theme.Surface,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSaveClick;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(430, 465),
                Width = 90,
                Height = 38,
                BackColor = Theme.Surface,
                ForeColor = Theme.TextPrimary,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };

            Controls.AddRange(new Control[]
            {
                lblAddress,
                txtAddress,
                lblType,
                cmbPropertyType,
                lblStatus,
                cmbStatus,
                lblPrice,
                txtPrice,
                lblOwner,
                cmbOwner,
                lblAgent,
                cmbAgent,
                btnSave,
                btnCancel
            });

            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        private void LoadData()
        {
            cmbPropertyType.SelectedItem = "house";
            cmbStatus.SelectedItem = "available";

            if (RbacService.ShouldAutoAssignCreatedRecord)
            {
                var currentAgent = _agents.FirstOrDefault(a => a.UserId == CurrentSession.UserId);
                if (currentAgent is not null)
                {
                    cmbAgent.SelectedItem = currentAgent;
                }
            }

            if (_existingProperty is null)
            {
                return;
            }

            txtAddress.Text = _existingProperty.Address;
            txtPrice.Text = _existingProperty.Price.ToString("0.##");
            SelectComboValue(cmbPropertyType, _existingProperty.PropertyType, "house");
            SelectComboValue(cmbStatus, _existingProperty.Status, "available");
            SelectOwner(_existingProperty.OwnerCustomerId);
            SelectAgent(_existingProperty.ListedByAgentId);
        }

        private void BtnSaveClick(object? sender, EventArgs e)
        {
            string address = txtAddress.Text.Trim();
            if (string.IsNullOrWhiteSpace(address))
            {
                ShowValidationError("Address is required.", txtAddress);
                return;
            }

            if (!decimal.TryParse(txtPrice.Text.Trim(), out decimal price) || price < 0)
            {
                ShowValidationError("Enter a valid price.", txtPrice);
                return;
            }

            if (cmbOwner.SelectedItem is not CustomerPickerItem owner)
            {
                ShowValidationError("Select an owner customer.", cmbOwner);
                return;
            }

            if (cmbAgent.SelectedItem is not AgentPickerItem agent)
            {
                ShowValidationError("Select a listing agent.", cmbAgent);
                return;
            }

            if (RbacService.ShouldAutoAssignCreatedRecord)
            {
                agent = _agents.FirstOrDefault(a => a.UserId == CurrentSession.UserId) ?? agent;
            }

            if (_existingProperty is not null)
            {
                _existingProperty.Address = address;
                _existingProperty.PropertyType = cmbPropertyType.SelectedItem?.ToString() ?? "house";
                _existingProperty.Price = price;
                _existingProperty.Status = cmbStatus.SelectedItem?.ToString() ?? "available";
                _existingProperty.OwnerCustomerId = owner.CustomerId;
                _existingProperty.ListedByAgentId = agent.UserId;

                Result = _existingProperty;
            }
            else
            {
                Result = new Property
                {
                    Address = address,
                    PropertyType = cmbPropertyType.SelectedItem?.ToString() ?? "house",
                    Price = price,
                    Status = cmbStatus.SelectedItem?.ToString() ?? "available",
                    OwnerCustomerId = owner.CustomerId,
                    ListedByAgentId = agent.UserId,
                    CreatedAt = DateTime.UtcNow
                };
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private static Label CreateLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                ForeColor = Theme.TextPrimary
            };
        }

        private static TextBox CreateTextBox(int x, int y, int width)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Width = width,
                BackColor = Theme.Surface,
                ForeColor = Theme.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private static ComboBox CreateComboBox(int x, int y, int width)
        {
            return new ComboBox
            {
                Location = new Point(x, y),
                Width = width,
                BackColor = Theme.Surface,
                ForeColor = Theme.TextPrimary,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
        }

        private static void SelectComboValue(ComboBox comboBox, string? value, string fallback)
        {
            string selectedValue = string.IsNullOrWhiteSpace(value)
                ? fallback
                : value;

            comboBox.SelectedItem = comboBox.Items.Contains(selectedValue)
                ? selectedValue
                : fallback;
        }

        private void SelectOwner(int ownerCustomerId)
        {
            var owner = _owners.FirstOrDefault(x => x.CustomerId == ownerCustomerId);
            if (owner is not null)
            {
                cmbOwner.SelectedItem = owner;
            }
        }

        private void SelectAgent(int listedByAgentId)
        {
            var agent = _agents.FirstOrDefault(x => x.UserId == listedByAgentId);
            if (agent is not null)
            {
                cmbAgent.SelectedItem = agent;
            }
        }

        private static void ShowValidationError(string message, Control control)
        {
            MessageBox.Show(
                message,
                "Validation Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            control.Focus();
        }
    }
}

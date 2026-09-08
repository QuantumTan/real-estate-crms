using CRMS_Peguit.domain.entities;
using CRMS_Peguit.winforms.Controllers;
using CRMS_Peguit.winforms.Models.Services;

namespace CRMS_Peguit.winforms.Views.Properties
{
    public partial class PropertyDetailForm : Form
    {
        private readonly Property? _property;
        private readonly PropertyController? _controller;

        public PropertyDetailForm()
        {
            InitializeComponent();
        }

        public PropertyDetailForm(Property property, PropertyController controller)
        {
            _property = property;
            _controller = controller;
            InitializeComponent();
            BuildFormControls();
        }

        private void BuildFormControls()
        {
            if (_property == null || _controller == null) return;

            Width = 620;
            Height = 440;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Theme.Background;
            ForeColor = Theme.TextPrimary;
            Font = new Font("Segoe UI", 10);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Text = $"Property #{_property.PropertyId}";

            int y = 20;
            AddHeading(_property.Address, ref y);
            AddStatusBadge(_property.Status, ref y);

            y += 10;
            AddSectionTitle("Listing Information", ref y);
            AddField("Type", string.IsNullOrWhiteSpace(_property.PropertyType) ? "-" : _property.PropertyType, ref y);
            AddField("Price", _property.Price.ToString("C"), ref y);
            AddField("Created", _property.CreatedAt.ToLocalTime().ToString("MMM d, yyyy"), ref y);

            y += 10;
            AddSectionTitle("Assignments", ref y);
            AddField("Owner", _controller.GetOwnerName(_property.OwnerCustomerId) ?? $"Customer #{_property.OwnerCustomerId}", ref y);
            AddField("Listed By", _controller.GetListedAgentName(_property.ListedByAgentId) ?? (_property.ListedByAgentId.HasValue ? $"User #{_property.ListedByAgentId.Value}" : "Unassigned"), ref y);
            AddField("Assignment Review", _property.AssignmentStatus, ref y);

            var btnClose = new Button
            {
                Text = "Close",
                Location = new Point(Width - 140, y + 20),
                Size = new Size(90, 36),
                BackColor = Theme.Primary,
                ForeColor = Theme.Surface,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK
            };
            btnClose.FlatAppearance.BorderSize = 0;
            UiRadiusHelper.StyleButton(btnClose, 8);
            Controls.Add(btnClose);
        }

        private void AddHeading(string text, ref int y)
        {
            Controls.Add(new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Theme.TextPrimary,
                Location = new Point(20, y),
                AutoSize = true,
                MaximumSize = new Size(560, 0)
            });
            y += 60;
        }

        private void AddStatusBadge(string? status, ref int y)
        {
            var color = string.Equals(status, "available", StringComparison.OrdinalIgnoreCase)
                ? Color.MediumSeaGreen
                : string.Equals(status, "sold", StringComparison.OrdinalIgnoreCase)
                    ? Color.IndianRed
                    : Color.Goldenrod;

            Controls.Add(new Label
            {
                Text = (status ?? "-").ToUpperInvariant(),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = color,
                Location = new Point(20, y),
                AutoSize = true
            });
            y += 30;
        }

        private void AddSectionTitle(string text, ref int y)
        {
            Controls.Add(new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Theme.Primary,
                Location = new Point(20, y),
                AutoSize = true
            });
            y += 30;
        }

        private void AddField(string label, string value, ref int y)
        {
            Controls.Add(new Label
            {
                Text = $"{label}:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Theme.TextPrimary,
                Location = new Point(30, y),
                AutoSize = true
            });
            Controls.Add(new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 10),
                ForeColor = Theme.TextPrimary,
                Location = new Point(180, y),
                AutoSize = true,
                MaximumSize = new Size(380, 0)
            });
            y += 28;
        }
    }
}


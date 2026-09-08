using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CRMS_Peguit.domain.entities;
using CRMS_Peguit.winforms.Controllers;
using CRMS_Peguit.winforms.Models.Services;

namespace CRMS_Peguit.winforms.Views.Customers
{
    public partial class CustomerDetailForm : Form
    {
        private readonly Customer? _customer;
        private readonly CustomerController? _controller;

        public CustomerDetailForm()
        {
            InitializeComponent();
        }

        public CustomerDetailForm(Customer customer, CustomerController controller)
        {
            _customer = customer;
            _controller = controller;
            InitializeComponent();
            BuildUi();
        }

        private void BuildUi()
        {
            if (_customer == null || _controller == null) return;

            int y = 20;
            AddHeading(_customer.FullName, ref y);
            AddStatusBadge(_customer.Status, ref y);

            y += 10;
            AddSectionTitle("Contact Information", ref y);
            AddField("Email", string.IsNullOrWhiteSpace(_customer.Email) ? "-" : _customer.Email, ref y);
            AddField("Phone", string.IsNullOrWhiteSpace(_customer.Phone) ? "-" : _customer.Phone, ref y);
            AddField("Type", _customer.Type, ref y);

            y += 10;
            AddSectionTitle("Ownership", ref y);
            var agentName = _controller.GetAssignedAgentName(_customer.AssignedAgentId);
            AddField("Assigned Agent", agentName ?? "Unassigned", ref y);
            AddField("Assignment Review", _customer.AssignmentStatus, ref y);
            AddField("Status", _customer.Status, ref y);

            bool isSeller = string.Equals(_customer.Type, "seller", StringComparison.OrdinalIgnoreCase)
                || string.Equals(_customer.Type, "both", StringComparison.OrdinalIgnoreCase);

            if (isSeller)
            {
                y += 10;
                AddSectionTitle("Owned Properties", ref y);
                var properties = _controller.GetOwnedProperties(_customer.CustomerId);
                if (properties.Count == 0)
                {
                    AddPlainText("No properties on file.", ref y);
                }
                else
                {
                    foreach (var p in properties)
                    {
                        AddPlainText($"• {p.Address} — {p.PropertyType}, {p.Status}", ref y);
                    }
                }
            }

            y += 10;
            AddSectionTitle("Recent Activities", ref y);
            var activities = _controller.GetActivityHistory(_customer.CustomerId);
            if (activities.Count == 0)
            {
                AddPlainText("No activity logged yet.", ref y);
            }
            else
            {
                foreach (var a in activities.Take(15))
                {
                    AddPlainText($"[{a.ActivityDate:MMM d, yyyy}] {a.Type} - {a.Notes}", ref y);
                }
            }

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

            Height = Math.Min(y + 100, 900);
            AutoScroll = true;
        }

        private void AddHeading(string text, ref int y)
        {
            Controls.Add(new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Theme.TextPrimary,
                Location = new Point(20, y),
                AutoSize = true
            });
            y += 40;
        }

        private void AddStatusBadge(string? status, ref int y)
        {
            var color = string.Equals(status, "active", StringComparison.OrdinalIgnoreCase)
                ? Color.MediumSeaGreen
                : string.Equals(status, "inactive", StringComparison.OrdinalIgnoreCase)
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
                AutoSize = true
            });
            y += 26;
        }

        private void AddPlainText(string text, ref int y)
        {
            Controls.Add(new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Theme.TextPrimary,
                Location = new Point(30, y),
                AutoSize = true,
                MaximumSize = new Size(550, 0)
            });
            y += 24;
        }
    }
}

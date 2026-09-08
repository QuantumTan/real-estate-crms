using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CRMS_Peguit.domain.entities;
using CRMS_Peguit.winforms.Controllers;
using CRMS_Peguit.winforms.Models.Services;

using Lead = CRMS_Peguit.domain.entities.Lead;

namespace CRMS_Peguit.winforms.Views.Leads
{
    public partial class LeadDetailForm : Form
    {
        private static readonly string[] PipelineStages =
            { "new", "contacted", "qualified", "converted" };

        private readonly Lead? _lead;
        private readonly LeadController? _controller;

        public LeadDetailForm()
        {
            InitializeComponent();
        }

        public LeadDetailForm(Lead lead, LeadController controller)
        {
            _lead = lead;
            _controller = controller;
            InitializeComponent();
            BuildUi();
        }

        private void BuildUi()
        {
            if (_lead == null || _controller == null) return;
            Text = $"Lead - {_lead.FullName}";

            int y = 20;

            AddHeading(_lead.FullName, ref y);
            AddStageAndPriorityBadges(ref y);

            y += 10;
            AddSectionTitle("Lead Information", ref y);
            AddField("Email", string.IsNullOrWhiteSpace(_lead.Email) ? "-" : _lead.Email, ref y);
            AddField("Phone", string.IsNullOrWhiteSpace(_lead.Phone) ? "-" : _lead.Phone, ref y);
            AddField("Source", string.IsNullOrWhiteSpace(_lead.Source) ? "-" : _lead.Source, ref y);
            AddField("Priority", string.IsNullOrWhiteSpace(_lead.Priority) ? "-" : _lead.Priority, ref y);
            AddField("Expected Value", _lead.ExpectedValue?.ToString("C") ?? "-", ref y);
            var agentName = _controller.GetAssignedAgentName(_lead.AssignedAgentId);
            AddField("Assigned Agent", agentName ?? "Unassigned", ref y);
            AddField("Assignment Review", _lead.AssignmentStatus, ref y);

            y += 10;
            AddSectionTitle("Notes", ref y);
            AddPlainText(string.IsNullOrWhiteSpace(_lead.Notes) ? "No notes yet." : _lead.Notes, ref y);

            y += 10;
            AddSectionTitle("Pipeline Summary", ref y);
            AddPipelineSummary(ref y);

            y += 10;
            AddSectionTitle("Recent Activities", ref y);
            var activities = _controller.GetActivityHistory(_lead.LeadId);
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

        private void AddStageAndPriorityBadges(ref int y)
        {
            bool isLost = string.Equals(_lead.Stage, "lost", StringComparison.OrdinalIgnoreCase);
            bool isConverted = string.Equals(_lead.Stage, "converted", StringComparison.OrdinalIgnoreCase);

            var stageColor = isLost ? Color.IndianRed
                : isConverted ? Color.MediumSeaGreen
                : Theme.Primary;

            Controls.Add(new Label
            {
                Text = (_lead.Stage ?? "-").ToUpperInvariant(),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = stageColor,
                Location = new Point(20, y),
                AutoSize = true
            });
            y += 30;
        }

        private void AddPipelineSummary(ref int y)
        {
            bool isLost = string.Equals(_lead.Stage, "lost", StringComparison.OrdinalIgnoreCase);

            if (isLost)
            {
                AddPlainText("This lead was marked LOST and is no longer progressing.", ref y);
                return;
            }

            int currentIndex = Array.FindIndex(PipelineStages,
                s => string.Equals(s, _lead.Stage, StringComparison.OrdinalIgnoreCase));

            for (int i = 0; i < PipelineStages.Length; i++)
            {
                bool reached = i <= currentIndex;
                var marker = reached ? "●" : "○";
                var color = reached ? Theme.Primary : Theme.Border;

                Controls.Add(new Label
                {
                    Text = $"{marker} {PipelineStages[i]}",
                    Font = new Font("Segoe UI", 10, i == currentIndex ? FontStyle.Bold : FontStyle.Regular),
                    ForeColor = color,
                    Location = new Point(30, y),
                    AutoSize = true
                });
                y += 24;
            }
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

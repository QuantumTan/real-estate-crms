using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CRMS_Peguit.domain.entities;
using CRMS_Peguit.winforms.Auth;
using CRMS_Peguit.winforms.Controllers;
using CRMS_Peguit.winforms.Models.Services;
using Lead = CRMS_Peguit.domain.entities.Lead;

namespace CRMS_Peguit.winforms.Views.Marketing
{
    public partial class CampaignsView : UserControl
    {
        private readonly LeadController _leadController;
        private List<string> _activeSources = new();
        private string _selectedSource = "All";

        public CampaignsView()
        {
            _leadController = new LeadController();
            InitializeComponent();
            ApplyModernStyling();
            BindEvents();
            LoadData();
        }

        private void ApplyModernStyling()
        {
            UiRadiusHelper.StyleButton(btnAddCampaign, 8);
            UiRadiusHelper.StyleButton(btnRefresh, 8);
            UiRadiusHelper.StyleCard(pnlStats, 10);
            UiRadiusHelper.StyleCard(pnlGridCard, 12);

            gridLeads.EnableHeadersVisualStyles = false;
            gridLeads.GridColor = Color.FromArgb(241, 245, 249);
            gridLeads.RowTemplate.Height = 48;
            gridLeads.DefaultCellStyle.BackColor = Color.White;
            gridLeads.DefaultCellStyle.ForeColor = Color.FromArgb(15, 23, 42);
            gridLeads.DefaultCellStyle.SelectionBackColor = Color.FromArgb(241, 245, 249);
            gridLeads.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
            gridLeads.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f);
            gridLeads.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            gridLeads.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(100, 116, 139);
            gridLeads.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            gridLeads.ColumnHeadersHeight = 40;
        }

        private void BindEvents()
        {
            btnAddCampaign.Click += BtnAddCampaign_Click;
            btnRefresh.Click += (_, _) => LoadData();
        }

        private void LoadData()
        {
            var leads = _leadController.GetAll();

            var sources = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Facebook Ad",
                "Referral",
                "Walk-in",
                "Website",
                "Property Portal"
            };

            foreach (var l in leads)
            {
                if (!string.IsNullOrWhiteSpace(l.Source))
                {
                    sources.Add(l.Source.Trim());
                }
            }

            foreach (var s in _activeSources)
            {
                sources.Add(s);
            }

            _activeSources = sources.OrderBy(s => s).ToList();

            int totalLeads = leads.Count;
            lblStatChannels.Text = $"{_activeSources.Count} Active Lead Channels";
            lblStatTotalLeads.Text = $"{totalLeads} Total Leads Attributed";

            var topSource = leads
                .Where(l => !string.IsNullOrWhiteSpace(l.Source))
                .GroupBy(l => l.Source!.Trim(), StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            lblStatConversion.Text = topSource != null
                ? $"Top Channel: {topSource.Key} ({topSource.Count()} leads)"
                : "Top Channel: None";

            BuildFilterPills(leads);
            DisplayLeads(leads);
        }

        private void BuildFilterPills(List<Lead> leads)
        {
            pnlSourcePills.Controls.Clear();

            var btnAll = CreatePillButton($"All ({leads.Count})", "All");
            pnlSourcePills.Controls.Add(btnAll);

            foreach (var source in _activeSources)
            {
                int count = leads.Count(l => string.Equals(l.Source?.Trim(), source, StringComparison.OrdinalIgnoreCase));
                var btn = CreatePillButton($"{source} ({count})", source);
                pnlSourcePills.Controls.Add(btn);
            }
        }

        private Button CreatePillButton(string label, string sourceValue)
        {
            bool isSelected = string.Equals(_selectedSource, sourceValue, StringComparison.OrdinalIgnoreCase);

            var btn = new Button
            {
                Text = label,
                Tag = sourceValue,
                AutoSize = true,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9f, isSelected ? FontStyle.Bold : FontStyle.Regular),
                BackColor = isSelected ? Color.FromArgb(15, 91, 158) : Color.White,
                ForeColor = isSelected ? Color.White : Color.FromArgb(71, 85, 105),
                Margin = new Padding(0, 0, 8, 0)
            };
            btn.FlatAppearance.BorderSize = 0;

            // Apply modern pill radius
            UiRadiusHelper.ApplyPillShape(btn);

            btn.Click += (_, _) =>
            {
                _selectedSource = sourceValue;
                LoadData();
            };

            return btn;
        }

        private void DisplayLeads(List<Lead> allLeads)
        {
            var filtered = string.Equals(_selectedSource, "All", StringComparison.OrdinalIgnoreCase)
                ? allLeads
                : allLeads.Where(l => string.Equals(l.Source?.Trim(), _selectedSource, StringComparison.OrdinalIgnoreCase)).ToList();

            gridLeads.Columns.Clear();

            gridLeads.DataSource = filtered.Select(l => new
            {
                l.LeadId,
                Name = l.FullName,
                CampaignSource = string.IsNullOrWhiteSpace(l.Source) ? "Untagged" : l.Source,
                Stage = (l.Stage ?? "New").ToUpper(),
                Value = l.ExpectedValue.HasValue ? $"${l.ExpectedValue.Value:N0}" : "-",
                Contact = string.IsNullOrWhiteSpace(l.Phone) ? l.Email ?? "-" : l.Phone,
                AssignedAgent = _leadController.GetAssignedAgentName(l.AssignedAgentId) ?? "Unassigned",
                CapturedDate = l.CreatedAt.ToString("MMM dd, yyyy")
            }).ToList();

            if (gridLeads.Columns["LeadId"] is DataGridViewColumn idCol)
                idCol.Visible = false;

            if (gridLeads.Columns["Name"] is DataGridViewColumn nameCol)
            {
                nameCol.HeaderText = "LEAD NAME";
                nameCol.FillWeight = 140;
            }
            if (gridLeads.Columns["CampaignSource"] is DataGridViewColumn srcCol)
            {
                srcCol.HeaderText = "SOURCE / CAMPAIGN";
                srcCol.FillWeight = 130;
            }
            if (gridLeads.Columns["Stage"] is DataGridViewColumn stageCol)
            {
                stageCol.HeaderText = "STAGE";
                stageCol.FillWeight = 90;
            }
            if (gridLeads.Columns["Value"] is DataGridViewColumn valCol)
            {
                valCol.HeaderText = "EXPECTED VALUE";
                valCol.FillWeight = 100;
            }
            if (gridLeads.Columns["Contact"] is DataGridViewColumn conCol)
            {
                conCol.HeaderText = "CONTACT";
                conCol.FillWeight = 120;
            }
            if (gridLeads.Columns["AssignedAgent"] is DataGridViewColumn agentCol)
            {
                agentCol.HeaderText = "ASSIGNED AGENT";
                agentCol.FillWeight = 110;
            }
            if (gridLeads.Columns["CapturedDate"] is DataGridViewColumn dateCol)
            {
                dateCol.HeaderText = "DATE CAPTURED";
                dateCol.FillWeight = 100;
            }
        }

        private void BtnAddCampaign_Click(object? sender, EventArgs e)
        {
            using var inputForm = new Form
            {
                Text = "Create New Lead Source / Campaign",
                Size = new Size(420, 210),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White
            };

            var lblPrompt = new Label
            {
                Text = "Campaign / Source Name (e.g., Google Ads, Billboard, Event):",
                Location = new Point(24, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f)
            };

            var txtName = new TextBox
            {
                Location = new Point(24, 48),
                Size = new Size(350, 30),
                Font = new Font("Segoe UI", 10f)
            };

            var btnSubmit = new Button
            {
                Text = "Save Campaign",
                DialogResult = DialogResult.OK,
                Location = new Point(234, 105),
                Size = new Size(140, 38),
                BackColor = Color.FromArgb(15, 91, 158),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            UiRadiusHelper.StyleButton(btnSubmit, 8);

            inputForm.Controls.Add(lblPrompt);
            inputForm.Controls.Add(txtName);
            inputForm.Controls.Add(btnSubmit);
            inputForm.AcceptButton = btnSubmit;

            if (inputForm.ShowDialog(this) == DialogResult.OK)
            {
                string newSource = txtName.Text.Trim();
                if (!string.IsNullOrWhiteSpace(newSource))
                {
                    if (!_activeSources.Contains(newSource, StringComparer.OrdinalIgnoreCase))
                    {
                        _activeSources.Add(newSource);
                    }
                    _selectedSource = newSource;
                    LoadData();

                    MessageBox.Show(
                        $"Lead source / campaign '{newSource}' is now active and ready for lead tagging.",
                        "Campaign Created",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
        }
    }
}

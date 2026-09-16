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
        private readonly CampaignController _campaignController;
        private readonly LeadController _leadController;
        private string _selectedSource = "All";

        public CampaignsView()
        {
            _campaignController = new CampaignController();
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

            UiGridHelper.ApplyModernGridStyle(gridLeads, 48);
        }

        private void BindEvents()
        {
            btnAddCampaign.Click += BtnAddCampaign_Click;
            btnRefresh.Click += (_, _) => LoadData();
        }

        private void LoadData()
        {
            var summary = _campaignController.GetCampaignSummary(_selectedSource);

            lblStatChannels.Text = $"{summary.ActiveChannelCount} Active Lead Channels";
            lblStatTotalLeads.Text = $"{summary.TotalLeads} Total Leads Attributed";
            lblStatConversion.Text = summary.TopChannelText;

            BuildFilterPills(summary);
            DisplayLeads(summary.FilteredLeads);
        }

        private void BuildFilterPills(CRMS_Peguit.winforms.Models.ViewModels.CampaignSummaryViewModel summary)
        {
            pnlSourcePills.Controls.Clear();

            var btnAll = CreatePillButton($"All ({summary.TotalLeads})", "All");
            pnlSourcePills.Controls.Add(btnAll);

            foreach (var source in summary.Channels)
            {
                int count = summary.ChannelCounts.TryGetValue(source, out int c) ? c : 0;
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

        private void DisplayLeads(List<Lead> filteredLeads)
        {
            var filtered = filteredLeads;

            gridLeads.Columns.Clear();

            gridLeads.DataSource = filtered.Select(l => new
            {
                l.LeadId,
                Name = l.FullName,
                CampaignSource = string.IsNullOrWhiteSpace(l.Source) ? "Untagged" : l.Source,
                Stage = (l.Stage ?? "New").ToUpper(),
                Value = l.ExpectedValue.HasValue ? $"₱{l.ExpectedValue.Value:N2}" : "-",
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
                Text = "Create New Campaign / Lead Source",
                Size = new Size(460, 300),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White
            };

            var lblPrompt = new Label
            {
                Text = "Campaign / Source Name * (e.g., Summer Promo 2026, Ayala Expo):",
                Location = new Point(24, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f)
            };

            var txtName = new TextBox
            {
                Location = new Point(24, 48),
                Size = new Size(390, 30),
                Font = new Font("Segoe UI", 10f)
            };

            var lblChannel = new Label
            {
                Text = "Channel / Category (e.g., Social Media, Portal, Event, Direct):",
                Location = new Point(24, 90),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f)
            };

            var cmbChannel = new ComboBox
            {
                Location = new Point(24, 118),
                Size = new Size(390, 30),
                Font = new Font("Segoe UI", 10f),
                DropDownStyle = ComboBoxStyle.DropDown
            };
            cmbChannel.Items.AddRange(new object[]
            {
                "Social Media",
                "Property Portal",
                "Search Engine / Paid Ads",
                "Referral",
                "Event / Expo",
                "Outdoor / Billboard",
                "Direct / Walk-in",
                "Website"
            });
            cmbChannel.SelectedIndex = 0;

            var btnCancel = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new Point(174, 195),
                Size = new Size(100, 38),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(8, 52, 87),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f)
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(180, 198, 217);
            UiRadiusHelper.StyleButton(btnCancel, 8);

            var btnSubmit = new Button
            {
                Text = "Save Campaign",
                DialogResult = DialogResult.OK,
                Location = new Point(284, 195),
                Size = new Size(130, 38),
                BackColor = Color.FromArgb(15, 91, 158),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            UiRadiusHelper.StyleButton(btnSubmit, 8);

            inputForm.Controls.Add(lblPrompt);
            inputForm.Controls.Add(txtName);
            inputForm.Controls.Add(lblChannel);
            inputForm.Controls.Add(cmbChannel);
            inputForm.Controls.Add(btnCancel);
            inputForm.Controls.Add(btnSubmit);
            inputForm.AcceptButton = btnSubmit;
            inputForm.CancelButton = btnCancel;

            if (inputForm.ShowDialog(this) == DialogResult.OK)
            {
                string newSource = txtName.Text.Trim();
                string channel = cmbChannel.Text.Trim();
                if (!string.IsNullOrWhiteSpace(newSource))
                {
                    bool saved = _campaignController.AddCampaign(newSource, string.IsNullOrWhiteSpace(channel) ? "Direct" : channel);
                    if (saved)
                    {
                        _selectedSource = newSource;
                        LoadData();

                        MessageBox.Show(
                            $"Campaign '{newSource}' has been successfully saved to the database.\n\nIt is now immediately available in the Agent's Lead Source dropdown for lead attribution.",
                            "Campaign Saved",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show(
                            "Unable to save the campaign. Please check connection and try again.",
                            "Error Saving Campaign",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                }
            }
        }
    }
}

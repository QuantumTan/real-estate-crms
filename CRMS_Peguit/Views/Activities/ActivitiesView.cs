using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using CRMS_Peguit.domain.entities;
using CRMS_Peguit.winforms.Auth;
using CRMS_Peguit.winforms.Controllers;
using CRMS_Peguit.winforms.Models.Services;
using CRMS_Peguit.winforms.Views.Customers;
using CRMS_Peguit.winforms.Views.FollowUps;
using CRMS_Peguit.winforms.Views.Leads;
using CRMS_Peguit.winforms.Views.Shared;

namespace CRMS_Peguit.winforms.Views.Activities
{
    public partial class ActivitiesView : UserControl
    {
        private readonly ActivityController _controller;
        private string _filterCategory = "All"; // "All", "Calls", "Emails", "Meetings", "System Events"
        private List<TimelineItemDto> _items = new();
        private Panel _pnlEmptyState = null!;

        public ActivitiesView()
        {
            InitializeComponent();
            _controller = new ActivityController();

            InitGridColumns();
            InitEmptyState();
            ApplyStyling();
            BindEvents();
            UpdateFilterPillStyles();
            RefreshData();
        }

        private void InitGridColumns()
        {
            grid.Columns.Clear();
            grid.AutoGenerateColumns = false;

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Id",
                HeaderText = "ID",
                Visible = false
            });

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Type",
                HeaderText = "TYPE",
                Width = 95,
                MinimumWidth = 85,
                FillWeight = 12,
                ReadOnly = true
            });

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Client",
                HeaderText = "CONTACT / CLIENT",
                Width = 190,
                MinimumWidth = 150,
                FillWeight = 22,
                ReadOnly = true
            });

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Title",
                HeaderText = "ACTIVITY SUMMARY",
                Width = 220,
                MinimumWidth = 180,
                FillWeight = 26,
                ReadOnly = true
            });

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Notes",
                HeaderText = "DETAILS & NOTES",
                Width = 260,
                MinimumWidth = 180,
                FillWeight = 30,
                ReadOnly = true
            });

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Date",
                HeaderText = "DATE / TIME",
                Width = 150,
                MinimumWidth = 130,
                FillWeight = 16,
                ReadOnly = true
            });

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Actor",
                HeaderText = "LOGGED BY",
                Width = 130,
                MinimumWidth = 110,
                FillWeight = 14,
                ReadOnly = true
            });

            UiGridHelper.ApplyModernGridStyle(grid, 48);
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void InitEmptyState()
        {
            _pnlEmptyState = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Visible = false
            };

            var lblEmptyIcon = new Label
            {
                Text = "⚡",
                Font = new Font("Segoe UI Emoji", 36f),
                ForeColor = Color.FromArgb(148, 163, 184),
                Size = new Size(80, 60),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblEmptyTitle = new Label
            {
                Text = "No activities or interactions found",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                AutoSize = true
            };

            var lblEmptySub = new Label
            {
                Text = "Log calls, emails, and meetings to maintain a complete client relationship history.",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(100, 116, 139),
                AutoSize = true
            };

            var btnEmptyAdd = new Button
            {
                Text = "+ Log First Activity",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                BackColor = Theme.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(160, 36),
                Cursor = Cursors.Hand
            };
            btnEmptyAdd.FlatAppearance.BorderSize = 0;
            UiRadiusHelper.StyleButton(btnEmptyAdd, 8);
            UiRadiusHelper.AttachHoverFeedback(btnEmptyAdd, Theme.Primary, Theme.PrimaryDark);
            btnEmptyAdd.Click += (_, _) => BtnAddClick();

            _pnlEmptyState.Resize += (_, _) =>
            {
                int cx = _pnlEmptyState.Width / 2;
                int cy = _pnlEmptyState.Height / 2 - 40;
                lblEmptyIcon.Location = new Point(cx - lblEmptyIcon.Width / 2, cy - 60);
                lblEmptyTitle.Location = new Point(cx - lblEmptyTitle.Width / 2, cy + 5);
                lblEmptySub.Location = new Point(cx - lblEmptySub.Width / 2, cy + 32);
                btnEmptyAdd.Location = new Point(cx - btnEmptyAdd.Width / 2, cy + 65);
            };

            _pnlEmptyState.Controls.Add(lblEmptyIcon);
            _pnlEmptyState.Controls.Add(lblEmptyTitle);
            _pnlEmptyState.Controls.Add(lblEmptySub);
            _pnlEmptyState.Controls.Add(btnEmptyAdd);

            pnlCard.Controls.Add(_pnlEmptyState);
        }

        private void ApplyStyling()
        {
            UiRadiusHelper.StyleButton(btnAdd, 8);
            UiRadiusHelper.AttachHoverFeedback(btnAdd, Theme.Primary, Theme.PrimaryDark);

            UiRadiusHelper.ApplyRoundedCorners(pnlCard, 10);
            pnlCard.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, pnlCard.Width - 1, pnlCard.Height - 1);
                using var path = UiRadiusHelper.CreateRoundedPath(rect, 10);
                using var pen = new Pen(Color.FromArgb(226, 232, 240), 1f);
                e.Graphics.DrawPath(pen, path);
            };

            var filterPills = new[] { btnFilterAll, btnFilterCalls, btnFilterEmails, btnFilterMeetings, btnFilterSystem };
            foreach (var pill in filterPills)
            {
                UiRadiusHelper.StyleButton(pill, 6);
            }
        }

        private void BindEvents()
        {
            btnAdd.Click += (_, _) => BtnAddClick();

            btnFilterAll.Click += (_, _) => SetFilter("All");
            btnFilterCalls.Click += (_, _) => SetFilter("Calls");
            btnFilterEmails.Click += (_, _) => SetFilter("Emails");
            btnFilterMeetings.Click += (_, _) => SetFilter("Meetings");
            btnFilterSystem.Click += (_, _) => SetFilter("System Events");

            kpiTotal.Click += (_, _) => SetFilter("All");
            kpiCalls.Click += (_, _) => SetFilter("Calls");
            kpiEmails.Click += (_, _) => SetFilter("Emails");
            kpiMeetings.Click += (_, _) => SetFilter("Meetings");

            txtSearch.TextChanged += (_, _) => RefreshData();

            grid.CellPainting += Grid_CellPainting;
            grid.CellDoubleClick += Grid_CellDoubleClick;
            grid.CellMouseDown += Grid_CellMouseDown;
        }

        private void SetFilter(string filter)
        {
            _filterCategory = filter;
            UpdateFilterPillStyles();
            RefreshData();
        }

        private void UpdateFilterPillStyles()
        {
            var mapping = new[]
            {
                (btnFilterAll, "All"),
                (btnFilterCalls, "Calls"),
                (btnFilterEmails, "Emails"),
                (btnFilterMeetings, "Meetings"),
                (btnFilterSystem, "System Events")
            };

            foreach (var (btn, key) in mapping)
            {
                bool active = string.Equals(_filterCategory, key, StringComparison.OrdinalIgnoreCase);
                btn.BackColor = active ? Theme.Primary : Color.FromArgb(241, 245, 249);
                btn.ForeColor = active ? Color.White : Color.FromArgb(71, 85, 105);
                btn.Font = new Font("Segoe UI", 9f, active ? FontStyle.Bold : FontStyle.Regular);
            }
        }

        public void RefreshData()
        {
            int agentId = CurrentSession.UserId;
            if (agentId <= 0) return;

            // Load items
            _items = _controller.GetAllForAgent(agentId, _filterCategory, txtSearch.Text);

            // Update KPI cards
            var (total, calls, emails, meetings) = _controller.GetActivityStatsForAgent(agentId);
            kpiTotal.SetValue(total.ToString("N0"));
            kpiCalls.SetValue(calls.ToString("N0"));
            kpiEmails.SetValue(emails.ToString("N0"));
            kpiMeetings.SetValue(meetings.ToString("N0"));

            // Populate Grid
            grid.Rows.Clear();
            foreach (var item in _items)
            {
                int rowIndex = grid.Rows.Add(
                    item.Id,
                    item.Type,
                    item.ClientName,
                    item.Title,
                    item.Notes ?? string.Empty,
                    item.Timestamp.ToLocalTime().ToString("MMM d, yyyy h:mm tt"),
                    item.ActorName
                );
                grid.Rows[rowIndex].Tag = item;
            }

            bool hasData = _items.Count > 0;
            _pnlEmptyState.Visible = !hasData;
            grid.Visible = hasData;
        }

        private void BtnAddClick()
        {
            using var dlg = new LogActivityDialog();
            if (dlg.ShowDialog(this.FindForm()) == DialogResult.OK)
            {
                RefreshData();
            }
        }

        private void Grid_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            OpenContactDetailForSelectedRow(e.RowIndex);
        }

        private void Grid_CellMouseDown(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right || e.RowIndex < 0) return;

            grid.ClearSelection();
            grid.Rows[e.RowIndex].Selected = true;

            if (grid.Rows[e.RowIndex].Tag is not TimelineItemDto item) return;

            var menu = new ContextMenuStrip
            {
                BackColor = Theme.Surface,
                ForeColor = Theme.TextPrimary,
                Font = new Font("Segoe UI", 10)
            };

            var viewContactItem = new ToolStripMenuItem($"📄 View {item.ClientType} Details");
            viewContactItem.Click += (_, _) => OpenContactDetailForSelectedRow(e.RowIndex);
            menu.Items.Add(viewContactItem);

            if (RbacService.IsAgent && item.CanCreateFollowUp)
            {
                var fuItem = new ToolStripMenuItem("➕ Schedule Follow-Up");
                fuItem.Click += (_, _) =>
                {
                    using var fuCtrl = new FollowUpController();
                    var template = _controller.CreateFollowUpTemplate(item);
                    using var form = new FollowUpInputForm(fuCtrl, template);
                    if (form.ShowDialog(this.FindForm()) == DialogResult.OK && form.Result != null)
                    {
                        fuCtrl.Add(form.Result);
                        MessageBox.Show("Follow-up task scheduled successfully.", "Follow-Up Scheduled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        RefreshData();
                    }
                };
                menu.Items.Add(fuItem);
            }

            menu.Show(grid, grid.PointToClient(Cursor.Position));
        }

        private void OpenContactDetailForSelectedRow(int rowIndex)
        {
            if (grid.Rows[rowIndex].Tag is not TimelineItemDto item) return;

            if (item.RelatedCustomerId.HasValue)
            {
                using var custCtrl = new CustomerController();
                var customer = custCtrl.GetById(item.RelatedCustomerId.Value);
                if (customer != null)
                {
                    using var form = new CustomerDetailForm(customer, custCtrl);
                    form.ShowDialog(this.FindForm());
                    RefreshData();
                }
            }
            else if (item.RelatedLeadId.HasValue)
            {
                using var leadCtrl = new LeadController();
                var lead = leadCtrl.GetById(item.RelatedLeadId.Value);
                if (lead != null)
                {
                    using var form = new LeadDetailForm(lead, leadCtrl);
                    form.ShowDialog(this.FindForm());
                    RefreshData();
                }
            }
        }

        private void Grid_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.Graphics == null) return;

            // Render type column with icons and badges
            if (grid.Columns[e.ColumnIndex].Name == "Type" && e.Value != null)
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);

                string type = e.Value.ToString() ?? "";
                string icon = type switch
                {
                    "Call" => "📞",
                    "Email" => "✉️",
                    "Meeting" => "📅",
                    _ => "⚙️"
                };

                string display = $"{icon} {type}";
                using var font = new Font("Segoe UI", 9f, FontStyle.Bold);
                using var brush = new SolidBrush(Color.FromArgb(15, 23, 42));
                var sf = new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Near };
                var textRect = new Rectangle(e.CellBounds.X + 8, e.CellBounds.Y, e.CellBounds.Width - 16, e.CellBounds.Height);
                e.Graphics.DrawString(display, font, brush, textRect, sf);
            }
        }
    }
}

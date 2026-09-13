using CRMS_Peguit.domain.entities;
using CRMS_Peguit.winforms.Auth;
using CRMS_Peguit.winforms.Controllers;
using CRMS_Peguit.winforms.Controls;
using CRMS_Peguit.winforms.Models.Services;
using CRMS_Peguit.winforms.Views.Shared;

using Lead = CRMS_Peguit.domain.entities.Lead;

namespace CRMS_Peguit.winforms.Views.Leads
{
    public partial class LeadsView : UserControl
    {
        private readonly LeadController _controller;
        private string _filterStage = "All";
        private Button? _btnExport;
        private Label _lblEmptyState = null!;

        public LeadsView()
        {
            InitializeComponent();
            _controller = new LeadController();

            InitEmptyState();
            ApplyStyling();
            BindEvents();
            UpdateFilterPillStyles();
            RefreshGrid();

            this.Load += (_, _) => LayoutToolbar();
            this.Resize += (_, _) => LayoutToolbar();
        }

        private void InitEmptyState()
        {
            _lblEmptyState = new Label
            {
                Text = "🔍 No leads match your search or filter criteria.\nTry adjusting your search terms or filter.",
                Font = new Font("Segoe UI", 11f),
                ForeColor = Theme.TextSecondary,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Visible = false
            };
            pnlCard.Controls.Add(_lblEmptyState);
            _lblEmptyState.BringToFront();
        }

        private void ApplyStyling()
        {
            UiRadiusHelper.StyleCard(pnlCard, 12);
            UiRadiusHelper.StyleButton(btnAdd, 8);
            UiRadiusHelper.ApplyPillShape(btnFilterAll);
            UiRadiusHelper.ApplyPillShape(btnFilterNew);
            UiRadiusHelper.ApplyPillShape(btnFilterContacted);
            UiRadiusHelper.ApplyPillShape(btnFilterQualified);
            UiRadiusHelper.ApplyPillShape(btnFilterConverted);
        }

        private void BindEvents()
        {
            btnAdd.Visible = RbacService.CanCreateSalesRecord;
            btnAdd.Click += BtnAddClick;
            txtSearch.TextChanged += (_, _) => RefreshGrid();

            if (RbacService.CanExportData)
            {
                _btnExport = new Button
                {
                    Text = "📥 Export CSV",
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(15, 91, 158),
                    Cursor = Cursors.Hand,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Size = new Size(130, 36)
                };
                _btnExport.FlatAppearance.BorderColor = Color.FromArgb(15, 91, 158);
                _btnExport.Click += (_, _) => ExportToCsv();
                UiRadiusHelper.StyleButton(_btnExport, 8);
                Controls.Add(_btnExport);
                _btnExport.BringToFront();
            }

            btnFilterAll.Click += (_, _) => SetFilter("All");
            btnFilterNew.Click += (_, _) => SetFilter("New");
            btnFilterContacted.Click += (_, _) => SetFilter("Contacted");
            btnFilterQualified.Click += (_, _) => SetFilter("Qualified");
            btnFilterConverted.Click += (_, _) => SetFilter("Converted");

            // Modern Grid Styling & Search Padding
            UiGridHelper.ApplyModernGridStyle(grid, 52);
            UiRadiusHelper.SetPadding(txtSearch, 10, 10);

            grid.CellPainting += Grid_CellPainting;
            grid.CellContentClick += GridCellContentClick;
            grid.CellDoubleClick += (_, e) =>
            {
                if (e.RowIndex < 0) return;
                var lead = GetLeadAtRow(e.RowIndex);
                if (lead is not null) ViewLead(lead);
            };
        }

        private void SetFilter(string stage)
        {
            _filterStage = stage;
            UpdateFilterPillStyles();
            RefreshGrid();
        }

        private void UpdateFilterPillStyles()
        {
            var pills = new[]
            {
                (btnFilterAll, "All"),
                (btnFilterNew, "New"),
                (btnFilterContacted, "Contacted"),
                (btnFilterQualified, "Qualified"),
                (btnFilterConverted, "Converted")
            };

            foreach (var (btn, name) in pills)
            {
                bool isSelected = string.Equals(_filterStage, name, StringComparison.OrdinalIgnoreCase);
                if (isSelected)
                {
                    btn.BackColor = Color.FromArgb(15, 91, 158);
                    btn.ForeColor = Color.White;
                    btn.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                }
                else
                {
                    btn.BackColor = Color.White;
                    btn.ForeColor = Color.FromArgb(71, 85, 105);
                    btn.Font = new Font("Segoe UI", 9f, FontStyle.Regular);
                }
            }
        }

        private void RefreshGrid()
        {
            grid.Columns.Clear();
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            var allList = _controller.GetAll().ToList();
            int total = allList.Count;
            int qualified = allList.Count(l => string.Equals(l.Stage, "qualified", StringComparison.OrdinalIgnoreCase));
            lblSubtitle.Text = $"{total} total · {qualified} qualified";

            IEnumerable<Lead> query = allList;

            if (!string.Equals(_filterStage, "All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(l => string.Equals(l.Stage, _filterStage, StringComparison.OrdinalIgnoreCase));
            }

            string search = txtSearch.Text.Trim();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(lead =>
                    ContainsText(lead.FirstName, search) ||
                    ContainsText(lead.MiddleName, search) ||
                    ContainsText(lead.LastName, search) ||
                    ContainsText(lead.Suffix, search) ||
                    ContainsText(lead.FullName, search) ||
                    ContainsText(lead.Email, search) ||
                    ContainsText(lead.Phone, search));
            }

            grid.DataSource = query
                .Select(lead => new
                {
                    lead.LeadId,
                    Name = lead.FullName,
                    Email = string.IsNullOrWhiteSpace(lead.Email) ? "-" : lead.Email,
                    Phone = string.IsNullOrWhiteSpace(lead.Phone) ? "-" : lead.Phone,
                    Source = string.IsNullOrWhiteSpace(lead.Source) ? "Website" : lead.Source,
                    ExpectedValue = lead.ExpectedValue.HasValue ? $"₱{lead.ExpectedValue.Value:N0}" : "-",
                    Stage = lead.Stage.ToUpper(),
                    Assignment = lead.AssignmentStatus.ToUpper()
                })
                .ToList();

            var idCol = grid.Columns["LeadId"];
            if (idCol is not null) idCol.Visible = false;

            grid.ShowCellToolTips = true;

            if (grid.Columns["Name"] is DataGridViewColumn nameCol)
            {
                nameCol.HeaderText = "NAME";
                nameCol.FillWeight = 160;
                nameCol.MinimumWidth = 140;
            }
            if (grid.Columns["Email"] is DataGridViewColumn emailCol)
            {
                emailCol.HeaderText = "EMAIL";
                emailCol.FillWeight = 140;
                emailCol.MinimumWidth = 120;
            }
            if (grid.Columns["Phone"] is DataGridViewColumn phoneCol)
            {
                phoneCol.HeaderText = "PHONE";
                phoneCol.FillWeight = 100;
                phoneCol.MinimumWidth = 90;
            }
            if (grid.Columns["Source"] is DataGridViewColumn srcCol)
            {
                srcCol.HeaderText = "SOURCE";
                srcCol.FillWeight = 90;
                srcCol.MinimumWidth = 80;
            }
            if (grid.Columns["ExpectedValue"] is DataGridViewColumn valCol)
            {
                valCol.HeaderText = "EXPECTED VALUE";
                valCol.FillWeight = 110;
                valCol.MinimumWidth = 100;
            }
            if (grid.Columns["Stage"] is DataGridViewColumn stageCol)
            {
                stageCol.HeaderText = "STAGE";
                stageCol.FillWeight = 95;
                stageCol.MinimumWidth = 80;
            }
            if (grid.Columns["Assignment"] is DataGridViewColumn assignCol)
            {
                assignCol.HeaderText = "ASSIGNMENT";
                assignCol.FillWeight = 100;
                assignCol.MinimumWidth = 90;
            }

            UiGridHelper.AddActionsColumn(grid, 64);
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _lblEmptyState.Visible = (grid.Rows.Count == 0);
        }

        private void Grid_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.Graphics is null) return;

            // Custom render Stage pill badge
            if (grid.Columns[e.ColumnIndex].Name == "Stage" && e.Value != null)
            {
                e.PaintBackground(e.CellBounds, true);
                string stage = e.Value.ToString() ?? "";
                Color bgColor;
                Color textColor;

                if (stage.Equals("NEW", StringComparison.OrdinalIgnoreCase))
                {
                    bgColor = Color.FromArgb(224, 242, 254);
                    textColor = Color.FromArgb(3, 105, 161);
                }
                else if (stage.Equals("CONTACTED", StringComparison.OrdinalIgnoreCase))
                {
                    bgColor = Color.FromArgb(243, 232, 255);
                    textColor = Color.FromArgb(107, 33, 168);
                }
                else if (stage.Equals("QUALIFIED", StringComparison.OrdinalIgnoreCase))
                {
                    bgColor = Color.FromArgb(220, 252, 231);
                    textColor = Color.FromArgb(22, 101, 52);
                }
                else if (stage.Equals("CONVERTED", StringComparison.OrdinalIgnoreCase))
                {
                    bgColor = Color.FromArgb(187, 247, 208);
                    textColor = Color.FromArgb(20, 83, 45);
                }
                else
                {
                    bgColor = Color.FromArgb(254, 226, 226);
                    textColor = Color.FromArgb(153, 27, 27);
                }

                using (var font = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                {
                    var size = TextRenderer.MeasureText(stage, font);
                    int pillWidth = size.Width + 16;
                    int pillHeight = 22;
                    int pillX = e.CellBounds.X + (e.CellBounds.Width - pillWidth) / 2;
                    int pillY = e.CellBounds.Y + (e.CellBounds.Height - pillHeight) / 2;
                    var pillRect = new Rectangle(pillX, pillY, pillWidth, pillHeight);

                    using (var brush = new SolidBrush(bgColor))
                    using (var path = GetRoundedRectangle(pillRect, 8))
                    {
                        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        e.Graphics.FillPath(brush, path);
                    }

                    TextRenderer.DrawText(e.Graphics, stage, font, pillRect, textColor,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }

                e.Handled = true;
            }
            // Custom render Name with circular initials badge
            else if (grid.Columns[e.ColumnIndex].Name == "Name" && e.Value != null)
            {
                e.PaintBackground(e.CellBounds, true);
                string name = e.Value.ToString() ?? "";
                string initials = GetInitials(name);

                int avatarSize = 28;
                int avatarX = e.CellBounds.X + 8;
                int avatarY = e.CellBounds.Y + (e.CellBounds.Height - avatarSize) / 2;
                var avatarRect = new Rectangle(avatarX, avatarY, avatarSize, avatarSize);

                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(Color.FromArgb(71, 118, 153)))
                {
                    e.Graphics.FillEllipse(brush, avatarRect);
                }

                using (var font = new Font("Segoe UI", 8f, FontStyle.Bold))
                {
                    TextRenderer.DrawText(e.Graphics, initials, font, avatarRect, Color.White,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }

                var textRect = new Rectangle(avatarX + avatarSize + 10, e.CellBounds.Y,
                    e.CellBounds.Width - avatarSize - 18, e.CellBounds.Height);

                using (var font = new Font("Segoe UI", 9.5f, FontStyle.Bold))
                {
                    TextRenderer.DrawText(e.Graphics, name, font, textRect, Theme.TextPrimary,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                }

                e.Handled = true;
            }
            // Custom render Email as clickable blue
            else if (grid.Columns[e.ColumnIndex].Name == "Email" && e.Value != null)
            {
                e.PaintBackground(e.CellBounds, true);
                string email = e.Value.ToString() ?? "";
                using (var font = new Font("Segoe UI", 9.5f))
                {
                    TextRenderer.DrawText(e.Graphics, email, font, e.CellBounds, Theme.Primary,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                }
                e.Handled = true;
            }
        }

        private static string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "U";
            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
            return $"{char.ToUpper(parts[0][0])}{char.ToUpper(parts[^1][0])}";
        }

        private static System.Drawing.Drawing2D.GraphicsPath GetRoundedRectangle(Rectangle bounds, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            int d = radius * 2;
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private Lead? GetLeadAtRow(int rowIndex)
        {
            object? idValue = grid.Rows[rowIndex].Cells["LeadId"].Value;
            if (idValue is null || !int.TryParse(idValue.ToString(), out int leadId))
                return null;

            return _controller.GetAll().FirstOrDefault(lead => lead.LeadId == leadId);
        }

        private Lead? GetSelectedLead()
        {
            if (grid.CurrentRow is null) return null;
            return GetLeadAtRow(grid.CurrentRow.Index);
        }

        private void GridCellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (grid.Columns[e.ColumnIndex] is not ActionsColumn) return;

            grid.Rows[e.RowIndex].Selected = true;
            grid.CurrentCell = grid.Rows[e.RowIndex].Cells[e.ColumnIndex];

            Lead? lead = GetSelectedLead();
            if (lead is null) return;

            var menu = new ContextMenuStrip
            {
                BackColor = Theme.Surface,
                ForeColor = Theme.TextPrimary,
                Font = new Font("Segoe UI", 10)
            };

            var viewItem = new ToolStripMenuItem("View");
            viewItem.Click += (_, _) => ViewLead(lead);
            menu.Items.Add(viewItem);

            var messageItem = new ToolStripMenuItem("Message");
            messageItem.Click += (_, _) => MessageLead(lead);
            messageItem.Enabled = CRMS_Peguit.winforms.Models.Services.ContactEmailService.IsValidEmail(lead.Email);
            menu.Items.Add(messageItem);

            if (CRMS_Peguit.winforms.Auth.RbacService.CanEditRecord(lead.AssignedAgentId, lead.CreatedByUserId))
            {
                var editItem = new ToolStripMenuItem("Edit");
                editItem.Click += (_, _) => EditLead(lead);
                menu.Items.Add(editItem);
            }

            if (CRMS_Peguit.winforms.Auth.RbacService.CanEditRecord(lead.AssignedAgentId, lead.CreatedByUserId) &&
                !string.Equals(lead.Stage, "converted", StringComparison.OrdinalIgnoreCase))
            {
                var convertItem = new ToolStripMenuItem("Convert to Customer");
                convertItem.Click += (_, _) => ConvertLead(lead);
                menu.Items.Add(convertItem);
            }

            if (CRMS_Peguit.winforms.Auth.RbacService.CanEditRecord(lead.AssignedAgentId, lead.CreatedByUserId) &&
                !string.Equals(lead.Stage, "lost", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(lead.Stage, "converted", StringComparison.OrdinalIgnoreCase))
            {
                var lostItem = new ToolStripMenuItem("Mark as Lost");
                lostItem.Click += (_, _) => MarkLeadLost(lead);
                menu.Items.Add(lostItem);
            }

            if (CRMS_Peguit.winforms.Auth.RbacService.CanEditRecord(lead.AssignedAgentId, lead.CreatedByUserId) &&
                string.Equals(lead.Stage, "lost", StringComparison.OrdinalIgnoreCase))
            {
                var restoreItem = new ToolStripMenuItem("Restore Lead");
                restoreItem.Click += (_, _) => RestoreLostLead(lead);
                menu.Items.Add(restoreItem);
            }

            if (CRMS_Peguit.winforms.Auth.RbacService.CanAssignRecords)
            {
                var assignItem = new ToolStripMenuItem("Assign Agent");
                assignItem.Click += (_, _) =>
                {
                    using var dlg = new AssignAgentDialog(lead.FullName, _controller.GetAgents(), lead.AssignedAgentId);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        _controller.AssignAgent(lead, dlg.SelectedAgentId, dlg.ApproveNow, dlg.ReviewNotes);
                        RefreshGrid();
                    }
                };
                menu.Items.Add(assignItem);
            }

            if (CRMS_Peguit.winforms.Auth.RbacService.CanApproveAssignments &&
                string.Equals(lead.AssignmentStatus, "pending_review", StringComparison.OrdinalIgnoreCase))
            {
                var approveItem = new ToolStripMenuItem("Approve Assignment");
                approveItem.Click += (_, _) => ApproveLeadAssignment(lead);
                menu.Items.Add(approveItem);
            }

            if (CRMS_Peguit.winforms.Auth.RbacService.CanArchiveRecord(lead.AssignedAgentId, lead.CreatedByUserId))
            {
                var archiveItem = new ToolStripMenuItem("Archive");
                archiveItem.Click += (_, _) => ArchiveLead(lead);
                menu.Items.Add(archiveItem);
            }

            menu.Show(grid, grid.PointToClient(Cursor.Position));
        }

        private void ViewLead(Lead lead)
        {
            using var form = new LeadDetailForm(lead, _controller);
            form.ShowDialog();
        }

        private void EditLead(Lead lead)
        {
            using var form = new LeadInputForm(lead);
            if (form.ShowDialog() == DialogResult.OK && form.Result is not null)
            {
                _controller.Update(form.Result);
                RefreshGrid();
            }
        }

        private void ArchiveLead(Lead lead)
        {
            var confirmation = MessageBox.Show(
                $"Archive '{lead.FullName}'?", "Archive Lead",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmation != DialogResult.Yes) return;

            _controller.SoftDelete(lead);
            RefreshGrid();
        }

        private void BtnAddClick(object? sender, EventArgs e)
        {
            using var form = new LeadInputForm();
            if (form.ShowDialog() == DialogResult.OK && form.Result is not null)
            {
                _controller.Add(form.Result);
                RefreshGrid();
            }
        }

        private void MessageLead(Lead lead)
        {
            using var form = new CRMS_Peguit.winforms.Views.Shared.EmailMessageForm(lead.FullName, lead.Email);
            if (form.ShowDialog() == DialogResult.OK)
            {
                _controller.LogEmail(lead, form.SentSubject);
                RefreshGrid();
            }
        }

        private void MarkLeadLost(Lead lead)
        {
            var confirmation = MessageBox.Show(
                $"Mark '{lead.FullName}' as lost?",
                "Lead Lifecycle",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmation != DialogResult.Yes) return;

            _controller.MarkLost(lead);
            RefreshGrid();
        }

        private void RestoreLostLead(Lead lead)
        {
            var confirmation = MessageBox.Show(
                $"Restore '{lead.FullName}' back to active follow-up?",
                "Restore Lead",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmation != DialogResult.Yes) return;

            _controller.RestoreFromLost(lead);
            RefreshGrid();
        }

        private void ApproveLeadAssignment(Lead lead)
        {
            _controller.ApproveAssignment(lead, "Reviewed from Leads module.");
            MessageBox.Show(
                $"Assignment for '{lead.FullName}' has been approved.",
                "Assignment Approved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            RefreshGrid();
        }

        private void ConvertLead(Lead lead)
        {
            var confirmation = MessageBox.Show(
                $"Convert '{lead.FullName}' into a customer?\n\n" +
                "A new customer record will be created and this lead will be marked as converted.",
                "Convert Lead", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmation != DialogResult.Yes) return;

            Customer customer = _controller.ConvertToCustomer(lead);

            MessageBox.Show(
                $"'{lead.FullName}' is now customer #{customer.CustomerId}.",
                "Conversion Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

            RefreshGrid();
        }

        private void ExportToCsv()
        {
            using var sfd = new SaveFileDialog
            {
                Filter = "CSV File (*.csv)|*.csv",
                FileName = $"Leads_Export_{DateTime.Now:yyyyMMdd_HHmm}.csv"
            };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var leads = _controller.GetAll().ToList();
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("LeadId,FullName,Source,Stage,Priority,ExpectedValue,Phone,Email,AssignedAgent,CreatedAt");
                foreach (var l in leads)
                {
                    sb.AppendLine($"\"{l.LeadId}\",\"{l.FullName}\",\"{l.Source}\",\"{l.Stage}\",\"{l.Priority}\",\"{l.ExpectedValue}\",\"{l.Phone}\",\"{l.Email}\",\"{_controller.GetAssignedAgentName(l.AssignedAgentId)}\",\"{l.CreatedAt:yyyy-MM-dd}\"");
                }
                System.IO.File.WriteAllText(sfd.FileName, sb.ToString());
                MessageBox.Show("Leads exported successfully.", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static bool ContainsText(string? value, string search)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   value.Contains(search, StringComparison.OrdinalIgnoreCase);
        }

        private void LayoutToolbar()
        {
            if (this.IsDisposed) return;

            int rightPadding = 30;
            int leftMargin = 30;
            int totalWidth = ClientSize.Width;
            int y = 88;

            // Position header action buttons
            int rightEdge = totalWidth - rightPadding;
            if (btnAdd.Visible)
            {
                btnAdd.Left = rightEdge - btnAdd.Width;
                btnAdd.Top = 24;
                rightEdge = btnAdd.Left - 10;
            }
            if (_btnExport != null && _btnExport.Visible)
            {
                _btnExport.Left = rightEdge - _btnExport.Width;
                _btnExport.Top = 24;
            }

            // Layout filter pills
            var pills = new[] { btnFilterConverted, btnFilterQualified, btnFilterContacted, btnFilterNew, btnFilterAll };
            int filterRight = totalWidth - rightPadding;
            int totalFilterWidth = 0;
            foreach (var p in pills) totalFilterWidth += p.Width + 6;

            int availableForSearch = totalWidth - leftMargin - rightPadding - totalFilterWidth - 20;

            if (availableForSearch >= 180)
            {
                // Single row: search on left, filters aligned to right
                foreach (var p in pills)
                {
                    p.Top = y;
                    p.Left = filterRight - p.Width;
                    filterRight = p.Left - 6;
                }

                txtSearch.Top = y;
                txtSearch.Left = leftMargin;
                txtSearch.Width = Math.Min(340, availableForSearch);

                pnlCard.Top = 126;
                pnlCard.Height = Math.Max(100, ClientSize.Height - 126 - 30);
            }
            else
            {
                // Two rows: search on row 1, filter pills wrapped to row 2
                txtSearch.Top = y;
                txtSearch.Left = leftMargin;
                txtSearch.Width = Math.Max(180, totalWidth - leftMargin - rightPadding);

                int filterX = leftMargin;
                int pillY = y + 36;
                var forwardPills = new[] { btnFilterAll, btnFilterNew, btnFilterContacted, btnFilterQualified, btnFilterConverted };
                foreach (var p in forwardPills)
                {
                    p.Top = pillY;
                    p.Left = filterX;
                    filterX += p.Width + 6;
                }

                pnlCard.Top = pillY + 38;
                pnlCard.Height = Math.Max(100, ClientSize.Height - pnlCard.Top - 20);
            }

            pnlCard.Left = leftMargin;
            pnlCard.Width = Math.Max(100, totalWidth - leftMargin - rightPadding);
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

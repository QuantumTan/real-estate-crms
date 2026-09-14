using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using CRMS_Peguit.domain.entities;
using CRMS_Peguit.winforms.Auth;
using CRMS_Peguit.winforms.Controllers;
using CRMS_Peguit.winforms.Controls;
using CRMS_Peguit.winforms.Models.Services;
using CRMS_Peguit.winforms.Views.Shared;

namespace CRMS_Peguit.winforms.Views.SupportTickets
{
    public partial class SupportTicketsView : UserControl
    {
        private readonly SupportTicketController _controller;
        private string _filterStatus = "All";
        private Button? _btnExport;
        private Label _lblEmptyState = null!;

        public SupportTicketsView()
        {
            InitializeComponent();
            _controller = new SupportTicketController();

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
                Text = "No support tickets match your search or filter criteria.\nTry selecting another filter or adjusting your search term.",
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
            UiRadiusHelper.ApplyPillShape(btnFilterOpen);
            UiRadiusHelper.ApplyPillShape(btnFilterInProgress);
            UiRadiusHelper.ApplyPillShape(btnFilterResolved);
            UiRadiusHelper.ApplyPillShape(btnFilterOverdue);
        }

        private void BindEvents()
        {
            // Business Rule: Admin has read-only oversight; Agent & Manager can log tickets
            btnAdd.Visible = RbacService.CanCreateSalesRecord;
            btnAdd.Click += BtnAddClick;
            txtSearch.TextChanged += (_, _) => RefreshGrid();

            if (RbacService.CanExportData)
            {
                _btnExport = new Button
                {
                    Text = "Export CSV",
                    BackColor = Color.White,
                    ForeColor = Theme.Primary,
                    Cursor = Cursors.Hand,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Size = new Size(130, 36)
                };
                _btnExport.FlatAppearance.BorderColor = Theme.Primary;
                _btnExport.Click += (_, _) => ExportToCsv();
                UiRadiusHelper.StyleButton(_btnExport, 8);
                Controls.Add(_btnExport);
                _btnExport.BringToFront();
            }

            // Wire KPI card click-to-filter interaction
            kpiTotal.Click += (_, _) => SetFilter("All");
            kpiOpen.Click += (_, _) => SetFilter("Open");
            kpiInProgress.Click += (_, _) => SetFilter("In Progress");
            kpiOverdue.Click += (_, _) => SetFilter("Overdue");

            // Filter pills
            btnFilterAll.Click += (_, _) => SetFilter("All");
            btnFilterOpen.Click += (_, _) => SetFilter("Open");
            btnFilterInProgress.Click += (_, _) => SetFilter("In Progress");
            btnFilterResolved.Click += (_, _) => SetFilter("Resolved");
            btnFilterOverdue.Click += (_, _) => SetFilter("Overdue");

            // Modern Grid Styling & Search Padding
            UiGridHelper.ApplyModernGridStyle(grid, 52);
            UiRadiusHelper.SetPadding(txtSearch, 10, 10);

            grid.CellPainting += Grid_CellPainting;
            grid.CellContentClick += GridCellContentClick;
            grid.CellDoubleClick += (_, e) =>
            {
                if (e.RowIndex < 0) return;
                var ticket = GetTicketAtRow(e.RowIndex);
                if (ticket is not null) ViewTicket(ticket);
            };
        }

        private void SetFilter(string filter)
        {
            _filterStatus = filter;
            UpdateFilterPillStyles();
            RefreshGrid();
        }

        private void UpdateFilterPillStyles()
        {
            var kpis = _controller.GetKpiCounts();

            // Refresh KPI card values
            kpiTotal.SetValue(kpis.Total);
            kpiOpen.SetValue(kpis.Open);
            kpiInProgress.SetValue(kpis.InProgress);
            kpiOverdue.SetValue(kpis.Overdue);

            kpiTotal.SetSelected(string.Equals(_filterStatus, "All", StringComparison.OrdinalIgnoreCase));
            kpiOpen.SetSelected(string.Equals(_filterStatus, "Open", StringComparison.OrdinalIgnoreCase));
            kpiInProgress.SetSelected(string.Equals(_filterStatus, "In Progress", StringComparison.OrdinalIgnoreCase));
            kpiOverdue.SetSelected(string.Equals(_filterStatus, "Overdue", StringComparison.OrdinalIgnoreCase));

            lblSubtitle.Text = $"{kpis.Total} total · {kpis.Open} open · {kpis.InProgress} in progress · {kpis.Overdue} overdue";

            var pills = new[]
            {
                (btnFilterAll, "All"),
                (btnFilterOpen, "Open"),
                (btnFilterInProgress, "In Progress"),
                (btnFilterResolved, "Resolved"),
                (btnFilterOverdue, "Overdue")
            };

            foreach (var (btn, name) in pills)
            {
                bool isSelected = string.Equals(_filterStatus, name, StringComparison.OrdinalIgnoreCase);
                if (isSelected)
                {
                    btn.BackColor = string.Equals(name, "Overdue", StringComparison.OrdinalIgnoreCase)
                        ? Theme.Danger
                        : Theme.Primary;
                    btn.ForeColor = Color.White;
                    btn.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                }
                else
                {
                    btn.BackColor = Color.White;
                    btn.ForeColor = string.Equals(name, "Overdue", StringComparison.OrdinalIgnoreCase)
                        ? Theme.Danger
                        : Color.FromArgb(71, 85, 105);
                    btn.Font = new Font("Segoe UI", 9f, FontStyle.Regular);
                }
            }
        }

        private void RefreshGrid()
        {
            grid.Columns.Clear();
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            var allList = _controller.GetAll();
            var now = DateTime.UtcNow;

            IEnumerable<SupportTicket> query = allList;

            if (string.Equals(_filterStatus, "Open", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(t => string.Equals(t.Status, "Open", StringComparison.OrdinalIgnoreCase));
            }
            else if (string.Equals(_filterStatus, "In Progress", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(t => string.Equals(t.Status, "In Progress", StringComparison.OrdinalIgnoreCase));
            }
            else if (string.Equals(_filterStatus, "Resolved", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(t => string.Equals(t.Status, "Resolved", StringComparison.OrdinalIgnoreCase));
            }
            else if (string.Equals(_filterStatus, "Overdue", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(t => !string.Equals(t.Status, "Resolved", StringComparison.OrdinalIgnoreCase)
                                         && t.DueDate.HasValue && t.DueDate.Value < now);
            }

            string search = txtSearch.Text.Trim();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(t =>
                    ContainsText(t.TicketNumber, search) ||
                    ContainsText(t.Category, search) ||
                    ContainsText(t.Description, search) ||
                    ContainsText(t.Customer?.FullName, search) ||
                    ContainsText(t.Customer?.Email, search));
            }

            grid.DataSource = query
                .Select(t => new
                {
                    t.TicketId,
                    TicketNumber = t.TicketNumber,
                    Customer = t.Customer?.FullName ?? "Unknown",
                    Category = t.Category,
                    Priority = t.Priority.ToUpper(),
                    Status = t.Status.ToUpper(),
                    DueDate = t.DueDate.HasValue
                        ? (!string.Equals(t.Status, "Resolved", StringComparison.OrdinalIgnoreCase) && t.DueDate.Value < now
                            ? $"[OVERDUE] {t.DueDate.Value.ToLocalTime():MMM dd, yyyy}"
                            : t.DueDate.Value.ToLocalTime().ToString("MMM dd, yyyy"))
                        : "-",
                    AssignedTo = _controller.GetAssignedAgentName(t.AssignedToUserId) ?? "Unassigned",
                    Logged = t.CreatedAt.ToLocalTime().ToString("MMM dd, yyyy")
                })
                .ToList();

            var idCol = grid.Columns["TicketId"];
            if (idCol is not null) idCol.Visible = false;

            grid.ShowCellToolTips = true;

            if (grid.Columns["TicketNumber"] is DataGridViewColumn numCol)
            {
                numCol.HeaderText = "TICKET #";
                numCol.FillWeight = 95;
                numCol.MinimumWidth = 85;
            }
            if (grid.Columns["Customer"] is DataGridViewColumn custCol)
            {
                custCol.HeaderText = "CUSTOMER";
                custCol.FillWeight = 150;
                custCol.MinimumWidth = 130;
            }
            if (grid.Columns["Category"] is DataGridViewColumn catCol)
            {
                catCol.HeaderText = "CATEGORY";
                catCol.FillWeight = 110;
                catCol.MinimumWidth = 95;
            }
            if (grid.Columns["Priority"] is DataGridViewColumn priCol)
            {
                priCol.HeaderText = "PRIORITY";
                priCol.FillWeight = 90;
                priCol.MinimumWidth = 80;
                priCol.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            if (grid.Columns["Status"] is DataGridViewColumn statCol)
            {
                statCol.HeaderText = "STATUS";
                statCol.FillWeight = 95;
                statCol.MinimumWidth = 85;
                statCol.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            if (grid.Columns["DueDate"] is DataGridViewColumn dueCol)
            {
                dueCol.HeaderText = "DUE DATE (SLA)";
                dueCol.FillWeight = 115;
                dueCol.MinimumWidth = 100;
            }
            if (grid.Columns["AssignedTo"] is DataGridViewColumn assignCol)
            {
                assignCol.HeaderText = "ASSIGNED TO";
                assignCol.FillWeight = 120;
                assignCol.MinimumWidth = 100;
            }
            if (grid.Columns["Logged"] is DataGridViewColumn logCol)
            {
                logCol.HeaderText = "LOGGED";
                logCol.FillWeight = 95;
                logCol.MinimumWidth = 85;
            }

            UiGridHelper.AddActionsColumn(grid, 64);
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _lblEmptyState.Visible = (grid.Rows.Count == 0);
        }

        private void Grid_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.Graphics is null) return;
            string colName = grid.Columns[e.ColumnIndex].Name;

            // Custom render Ticket Number (bold navy)
            if (colName == "TicketNumber" && e.Value != null)
            {
                e.PaintBackground(e.CellBounds, true);
                string num = e.Value.ToString() ?? "";
                using var font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
                TextRenderer.DrawText(e.Graphics, num, font, e.CellBounds, Theme.PrimaryDark,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
                e.Handled = true;
            }
            // Custom render Status pill badge
            else if (colName == "Status" && e.Value != null)
            {
                e.PaintBackground(e.CellBounds, true);
                string status = e.Value.ToString() ?? "";
                var (bgColor, textColor, strokeColor) = UiDetailCardHelper.GetStatusColors(status);

                using (var font = new Font("Segoe UI", 8f, FontStyle.Bold))
                {
                    var size = TextRenderer.MeasureText(status, font);
                    int pillWidth = size.Width + 14;
                    int pillHeight = 22;
                    int pillX = e.CellBounds.X + (e.CellBounds.Width - pillWidth) / 2;
                    int pillY = e.CellBounds.Y + (e.CellBounds.Height - pillHeight) / 2;
                    var pillRect = new Rectangle(pillX, pillY, pillWidth, pillHeight);

                    using (var brush = new SolidBrush(bgColor))
                    using (var pen = new Pen(strokeColor, 1f))
                    using (var path = GetRoundedRectangle(pillRect, 8))
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        e.Graphics.FillPath(brush, path);
                        e.Graphics.DrawPath(pen, path);
                    }

                    TextRenderer.DrawText(e.Graphics, status, font, pillRect, textColor,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }

                e.Handled = true;
            }
            // Custom render Priority pill badge
            else if (colName == "Priority" && e.Value != null)
            {
                e.PaintBackground(e.CellBounds, true);
                string priority = e.Value.ToString() ?? "";
                var (bgColor, textColor, strokeColor) = UiDetailCardHelper.GetPriorityColors(priority);

                using (var font = new Font("Segoe UI", 8f, FontStyle.Bold))
                {
                    var size = TextRenderer.MeasureText(priority, font);
                    int pillWidth = size.Width + 14;
                    int pillHeight = 22;
                    int pillX = e.CellBounds.X + (e.CellBounds.Width - pillWidth) / 2;
                    int pillY = e.CellBounds.Y + (e.CellBounds.Height - pillHeight) / 2;
                    var pillRect = new Rectangle(pillX, pillY, pillWidth, pillHeight);

                    using (var brush = new SolidBrush(bgColor))
                    using (var pen = new Pen(strokeColor, 1f))
                    using (var path = GetRoundedRectangle(pillRect, 8))
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        e.Graphics.FillPath(brush, path);
                        e.Graphics.DrawPath(pen, path);
                    }

                    TextRenderer.DrawText(e.Graphics, priority, font, pillRect, textColor,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }

                e.Handled = true;
            }
            // Custom render DueDate with red warning highlighting if overdue
            else if (colName == "DueDate" && e.Value != null)
            {
                e.PaintBackground(e.CellBounds, true);
                string text = e.Value.ToString() ?? "";
                bool isOverdue = text.StartsWith("[OVERDUE]") || text.Contains("OVERDUE");

                Color textColor = isOverdue ? Theme.Danger : Theme.TextPrimary;
                using var font = new Font("Segoe UI", 9f, isOverdue ? FontStyle.Bold : FontStyle.Regular);

                TextRenderer.DrawText(e.Graphics, text, font, e.CellBounds, textColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

                e.Handled = true;
            }
            // Custom render Customer with initial badge
            else if (colName == "Customer" && e.Value != null)
            {
                e.PaintBackground(e.CellBounds, true);
                string name = e.Value.ToString() ?? "";
                string initials = UiDetailCardHelper.GetInitials(name);

                int avatarSize = 26;
                int avatarX = e.CellBounds.X + 6;
                int avatarY = e.CellBounds.Y + (e.CellBounds.Height - avatarSize) / 2;
                var avatarRect = new Rectangle(avatarX, avatarY, avatarSize, avatarSize);

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(Theme.PrimaryLight))
                {
                    e.Graphics.FillEllipse(brush, avatarRect);
                }

                using (var font = new Font("Segoe UI", 7.5f, FontStyle.Bold))
                {
                    TextRenderer.DrawText(e.Graphics, initials, font, avatarRect, Theme.PrimaryDark,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }

                var textRect = new Rectangle(avatarX + avatarSize + 8, e.CellBounds.Y,
                    e.CellBounds.Width - avatarSize - 16, e.CellBounds.Height);

                using (var font = new Font("Segoe UI", 9.5f, FontStyle.Regular))
                {
                    TextRenderer.DrawText(e.Graphics, name, font, textRect, Theme.TextPrimary,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                }

                e.Handled = true;
            }
        }

        private static GraphicsPath GetRoundedRectangle(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static bool ContainsText(string? value, string search) =>
            !string.IsNullOrWhiteSpace(value) &&
            value.Contains(search, StringComparison.OrdinalIgnoreCase);

        private void GridCellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (grid.Columns[e.ColumnIndex] is not ActionsColumn) return;

            grid.Rows[e.RowIndex].Selected = true;
            grid.CurrentCell = grid.Rows[e.RowIndex].Cells[e.ColumnIndex];

            var ticket = GetTicketAtRow(e.RowIndex);
            if (ticket is null) return;

            var menu = new ContextMenuStrip
            {
                BackColor = Theme.Surface,
                ForeColor = Theme.TextPrimary,
                Font = new Font("Segoe UI", 10)
            };

            var viewItem = new ToolStripMenuItem("View Ticket");
            viewItem.Click += (_, _) => ViewTicket(ticket);
            menu.Items.Add(viewItem);

            // Update Status (Manager or owning Agent)
            bool canEdit = _controller.CanUserEditTicket(ticket);
            bool isResolved = string.Equals(ticket.Status, "Resolved", StringComparison.OrdinalIgnoreCase);

            if (canEdit && !isResolved)
            {
                var statusMenu = new ToolStripMenuItem("Update Status");

                if (string.Equals(ticket.Status, "Open", StringComparison.OrdinalIgnoreCase))
                {
                    var inProgressItem = new ToolStripMenuItem("Mark In Progress");
                    inProgressItem.Click += (_, _) =>
                    {
                        _controller.UpdateStatus(ticket.TicketId, "In Progress");
                        UpdateFilterPillStyles();
                        RefreshGrid();
                    };
                    statusMenu.DropDownItems.Add(inProgressItem);
                }

                var resolveItem = new ToolStripMenuItem("Resolve Ticket");
                resolveItem.Click += (_, _) =>
                {
                    _controller.UpdateStatus(ticket.TicketId, "Resolved");
                    UpdateFilterPillStyles();
                    RefreshGrid();
                };
                statusMenu.DropDownItems.Add(resolveItem);

                menu.Items.Add(statusMenu);
            }

            // Reopen (Explicit action if Resolved)
            if (canEdit && isResolved)
            {
                var reopenItem = new ToolStripMenuItem("Reopen Ticket");
                reopenItem.Click += (_, _) =>
                {
                    _controller.Reopen(ticket.TicketId, "Reopened from ticket list.");
                    UpdateFilterPillStyles();
                    RefreshGrid();
                };
                menu.Items.Add(reopenItem);
            }

            // Assign / Reassign (Manager / Admin ONLY)
            if (RbacService.CanAssignRecords)
            {
                var assignItem = new ToolStripMenuItem("Assign / Reassign Agent");
                assignItem.Click += (_, _) =>
                {
                    using var dlg = new AssignAgentDialog(ticket.TicketNumber, _controller.GetAgents(), ticket.AssignedToUserId);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        _controller.AssignTo(ticket.TicketId, dlg.SelectedAgentId, dlg.ReviewNotes);
                        UpdateFilterPillStyles();
                        RefreshGrid();
                    }
                };
                menu.Items.Add(assignItem);
            }

            // Add Note / Comment
            var commentItem = new ToolStripMenuItem("Add Internal Note");
            commentItem.Click += (_, _) =>
            {
                PromptAddComment(ticket);
            };
            menu.Items.Add(commentItem);

            // Archive (Manager only)
            if (RbacService.IsManager || RbacService.IsSuperAdmin)
            {
                var archiveItem = new ToolStripMenuItem("Archive");
                archiveItem.Click += (_, _) =>
                {
                    var confirm = MessageBox.Show(
                        $"Are you sure you want to archive ticket {ticket.TicketNumber}?",
                        "Confirm Archive",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (confirm == DialogResult.Yes)
                    {
                        _controller.SoftDelete(ticket.TicketId);
                        UpdateFilterPillStyles();
                        RefreshGrid();
                    }
                };
                menu.Items.Add(archiveItem);
            }

            menu.Show(grid, grid.PointToClient(Cursor.Position));
        }

        private void PromptAddComment(SupportTicket ticket)
        {
            using var prompt = new Form
            {
                Text = $"Add Note — {ticket.TicketNumber}",
                Size = new Size(420, 200),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Theme.Surface
            };

            var lbl = new Label { Text = "Internal note / reply text:", Location = new Point(20, 16), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            var txt = new TextBox { Location = new Point(20, 38), Width = 360, Height = 60, Multiline = true };
            var btnOk = new Button { Text = "Post Note", Location = new Point(210, 110), Width = 84, Height = 32, DialogResult = DialogResult.OK, BackColor = Theme.Primary, ForeColor = Color.White };
            var btnCan = new Button { Text = "Cancel", Location = new Point(300, 110), Width = 80, Height = 32, DialogResult = DialogResult.Cancel };
            UiRadiusHelper.StyleButton(btnOk, 6);
            UiRadiusHelper.StyleButton(btnCan, 6);

            prompt.Controls.AddRange(new Control[] { lbl, txt, btnOk, btnCan });
            prompt.AcceptButton = btnOk;
            prompt.CancelButton = btnCan;

            if (prompt.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(txt.Text))
            {
                _controller.AddComment(ticket.TicketId, txt.Text.Trim());
                RefreshGrid();
            }
        }

        private SupportTicket? GetTicketAtRow(int rowIndex)
        {
            if (grid.Rows[rowIndex].Cells["TicketId"].Value is int id)
            {
                return _controller.GetById(id);
            }
            return null;
        }

        private void BtnAddClick(object? sender, EventArgs e)
        {
            using var form = new SupportTicketInputForm(_controller);
            if (form.ShowDialog() == DialogResult.OK && form.Result is not null)
            {
                _controller.Add(form.Result);
                UpdateFilterPillStyles();
                RefreshGrid();
            }
        }

        private void ViewTicket(SupportTicket ticket)
        {
            using var form = new SupportTicketDetailForm(ticket, _controller);
            form.ShowDialog();
            UpdateFilterPillStyles();
            RefreshGrid();
        }

        private void ExportToCsv()
        {
            using var sfd = new SaveFileDialog
            {
                Filter = "CSV File (*.csv)|*.csv",
                FileName = $"SupportTickets_Export_{DateTime.Now:yyyyMMdd_HHmm}.csv"
            };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var tickets = _controller.GetAll();
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("TicketId,TicketNumber,Category,Customer,Priority,Status,DueDate,AssignedTo,CreatedAt");
                foreach (var t in tickets)
                {
                    sb.AppendLine($"\"{t.TicketId}\",\"{t.TicketNumber}\",\"{t.Category}\",\"{t.Customer?.FullName}\",\"{t.Priority}\",\"{t.Status}\",\"{t.DueDate:yyyy-MM-dd}\",\"{_controller.GetAssignedAgentName(t.AssignedToUserId)}\",\"{t.CreatedAt:yyyy-MM-dd}\"");
                }
                System.IO.File.WriteAllText(sfd.FileName, sb.ToString());
                MessageBox.Show("Support tickets exported successfully.", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void LayoutToolbar()
        {
            if (this.IsDisposed) return;

            int rightPadding = 30;
            int leftMargin = 30;
            int totalWidth = ClientSize.Width;

            // 1. Position header action buttons
            int rightEdge = totalWidth - rightPadding;
            if (btnAdd.Visible)
            {
                btnAdd.Left = rightEdge - btnAdd.Width;
                btnAdd.Top = 22;
                rightEdge = btnAdd.Left - 10;
            }
            if (_btnExport != null && _btnExport.Visible)
            {
                _btnExport.Left = rightEdge - _btnExport.Width;
                _btnExport.Top = 22;
            }

            // 2. Position and size KPI container explicitly
            pnlKpiContainer.Left = leftMargin;
            pnlKpiContainer.Top = 84;
            pnlKpiContainer.Width = Math.Max(100, totalWidth - leftMargin - rightPadding);
            pnlKpiContainer.Height = 88;

            // 3. Position search & filter pills ALWAYS below the KPI container
            int y = pnlKpiContainer.Bottom + 16;

            var pills = new[] { btnFilterOverdue, btnFilterResolved, btnFilterInProgress, btnFilterOpen, btnFilterAll };
            int filterRight = totalWidth - rightPadding;
            int totalFilterWidth = 0;
            foreach (var p in pills) totalFilterWidth += p.Width + 6;

            int availableForSearch = totalWidth - leftMargin - rightPadding - totalFilterWidth - 20;

            int cardTop;
            if (availableForSearch >= 180)
            {
                // Single row
                foreach (var p in pills)
                {
                    p.Top = y + 2;
                    p.Left = filterRight - p.Width;
                    filterRight = p.Left - 6;
                }

                txtSearch.Top = y;
                txtSearch.Left = leftMargin;
                txtSearch.Width = Math.Min(380, availableForSearch);

                cardTop = y + 42;
            }
            else
            {
                // Two rows
                txtSearch.Top = y;
                txtSearch.Left = leftMargin;
                txtSearch.Width = Math.Max(180, totalWidth - leftMargin - rightPadding);

                int filterX = leftMargin;
                int pillY = y + 36;
                var forwardPills = new[] { btnFilterAll, btnFilterOpen, btnFilterInProgress, btnFilterResolved, btnFilterOverdue };
                foreach (var p in forwardPills)
                {
                    p.Top = pillY;
                    p.Left = filterX;
                    filterX += p.Width + 6;
                }

                cardTop = pillY + 40;
            }

            // 4. Position DataGridView card
            pnlCard.Top = cardTop;
            pnlCard.Left = leftMargin;
            pnlCard.Width = Math.Max(100, totalWidth - leftMargin - rightPadding);
            pnlCard.Height = Math.Max(100, ClientSize.Height - cardTop - 24);
        }
    }
}

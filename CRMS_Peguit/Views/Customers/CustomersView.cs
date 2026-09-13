using CRMS_Peguit.domain.entities;
using CRMS_Peguit.winforms.Auth;
using CRMS_Peguit.winforms.Controllers;
using CRMS_Peguit.winforms.Controls;
using CRMS_Peguit.winforms.Models.Services;
using CRMS_Peguit.winforms.Views.Shared;

namespace CRMS_Peguit.winforms.Views.Customers
{
    public partial class CustomersView : UserControl
    {
        private readonly CustomerController _controller;
        private string _filterStatus = "All";
        private Button? _btnExport;
        private Label _lblEmptyState = null!;

        public CustomersView()
        {
            InitializeComponent();
            _controller = new CustomerController();

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
                Text = "🔍 No customers match your search or filter criteria.\nTry adjusting your search terms or filter.",
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
            UiRadiusHelper.ApplyPillShape(btnFilterActive);
            UiRadiusHelper.ApplyPillShape(btnFilterFollowUp);
            UiRadiusHelper.ApplyPillShape(btnFilterInactive);
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
            btnFilterActive.Click += (_, _) => SetFilter("Active");
            btnFilterFollowUp.Click += (_, _) => SetFilter("Follow Up");
            btnFilterInactive.Click += (_, _) => SetFilter("Inactive");

            // Modern Grid Styling & Search Padding
            UiGridHelper.ApplyModernGridStyle(grid, 52);
            UiRadiusHelper.SetPadding(txtSearch, 10, 10);

            grid.CellPainting += Grid_CellPainting;
            grid.CellContentClick += GridCellContentClick;
            grid.CellDoubleClick += (_, e) =>
            {
                if (e.RowIndex < 0) return;
                var customer = GetCustomerAtRow(e.RowIndex);
                if (customer is not null) ViewCustomer(customer);
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
            var pills = new[]
            {
                (btnFilterAll, "All"),
                (btnFilterActive, "Active"),
                (btnFilterFollowUp, "Follow Up"),
                (btnFilterInactive, "Inactive")
            };

            foreach (var (btn, name) in pills)
            {
                bool isSelected = string.Equals(_filterStatus, name, StringComparison.OrdinalIgnoreCase);
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
            int active = allList.Count(c => string.Equals(c.Status, "active", StringComparison.OrdinalIgnoreCase));
            lblSubtitle.Text = $"{total} total · {active} active";

            IEnumerable<Customer> query = allList;

            if (string.Equals(_filterStatus, "Active", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(c => string.Equals(c.Status, "active", StringComparison.OrdinalIgnoreCase));
            }
            else if (string.Equals(_filterStatus, "Inactive", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(c => string.Equals(c.Status, "inactive", StringComparison.OrdinalIgnoreCase));
            }
            else if (string.Equals(_filterStatus, "Follow Up", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(c => string.Equals(c.Status, "prospect", StringComparison.OrdinalIgnoreCase) ||
                                         string.Equals(c.AssignmentStatus, "pending", StringComparison.OrdinalIgnoreCase));
            }

            string search = txtSearch.Text.Trim();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c =>
                    ContainsText(c.FirstName, search) ||
                    ContainsText(c.MiddleName, search) ||
                    ContainsText(c.LastName, search) ||
                    ContainsText(c.Suffix, search) ||
                    ContainsText(c.FullName, search) ||
                    ContainsText(c.Email, search) ||
                    ContainsText(c.Phone, search));
            }

            grid.DataSource = query
                .Select(c => new
                {
                    c.CustomerId,
                    Name = c.FullName,
                    Company = string.IsNullOrWhiteSpace(c.Type) ? "Client" : char.ToUpper(c.Type[0]) + c.Type.Substring(1).ToLower(),
                    Phone = string.IsNullOrWhiteSpace(c.Phone) ? "-" : c.Phone,
                    Email = string.IsNullOrWhiteSpace(c.Email) ? "-" : c.Email,
                    AssignedTo = _controller.GetAssignedAgentName(c.AssignedAgentId) ?? "Unassigned",
                    Status = c.Status.ToUpper(),
                    LastContacted = c.CreatedAt.ToString("MMM dd, yyyy")
                })
                .ToList();

            var idCol = grid.Columns["CustomerId"];
            if (idCol is not null) idCol.Visible = false;

            grid.ShowCellToolTips = true;

            if (grid.Columns["Name"] is DataGridViewColumn nameCol)
            {
                nameCol.HeaderText = "NAME";
                nameCol.FillWeight = 160;
                nameCol.MinimumWidth = 140;
            }
            if (grid.Columns["Company"] is DataGridViewColumn compCol)
            {
                compCol.HeaderText = "COMPANY";
                compCol.FillWeight = 90;
                compCol.MinimumWidth = 80;
            }
            if (grid.Columns["Phone"] is DataGridViewColumn phoneCol)
            {
                phoneCol.HeaderText = "PHONE";
                phoneCol.FillWeight = 100;
                phoneCol.MinimumWidth = 90;
            }
            if (grid.Columns["Email"] is DataGridViewColumn emailCol)
            {
                emailCol.HeaderText = "EMAIL";
                emailCol.FillWeight = 140;
                emailCol.MinimumWidth = 120;
            }
            if (grid.Columns["AssignedTo"] is DataGridViewColumn assignCol)
            {
                assignCol.HeaderText = "ASSIGNED TO";
                assignCol.FillWeight = 110;
                assignCol.MinimumWidth = 100;
            }
            if (grid.Columns["Status"] is DataGridViewColumn statusCol)
            {
                statusCol.HeaderText = "STATUS";
                statusCol.FillWeight = 90;
                statusCol.MinimumWidth = 80;
            }
            if (grid.Columns["LastContacted"] is DataGridViewColumn lastCol)
            {
                lastCol.HeaderText = "LAST CONTACTED";
                lastCol.FillWeight = 100;
                lastCol.MinimumWidth = 90;
            }

            UiGridHelper.AddActionsColumn(grid, 64);
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _lblEmptyState.Visible = (grid.Rows.Count == 0);
        }

        private void Grid_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.Graphics is null) return;

            // Custom render Status pill badge
            if (grid.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                e.PaintBackground(e.CellBounds, true);
                string status = e.Value.ToString() ?? "";
                Color bgColor;
                Color textColor;

                if (status.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase))
                {
                    bgColor = Theme.StatusActiveBg;
                    textColor = Theme.StatusActiveText;
                }
                else if (status.Equals("PROSPECT", StringComparison.OrdinalIgnoreCase) || status.Contains("FOLLOW"))
                {
                    bgColor = Theme.StatusFollowUpBg;
                    textColor = Theme.StatusFollowUpText;
                }
                else
                {
                    bgColor = Theme.StatusInactiveBg;
                    textColor = Theme.StatusInactiveText;
                }

                using (var font = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                {
                    var size = TextRenderer.MeasureText(status, font);
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

                    TextRenderer.DrawText(e.Graphics, status, font, pillRect, textColor,
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

        private static bool ContainsText(string? value, string search) =>
            !string.IsNullOrWhiteSpace(value) &&
            value.Contains(search, StringComparison.OrdinalIgnoreCase);

        private void GridCellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (grid.Columns[e.ColumnIndex] is not ActionsColumn) return;

            grid.Rows[e.RowIndex].Selected = true;
            grid.CurrentCell = grid.Rows[e.RowIndex].Cells[e.ColumnIndex];

            var customer = GetCustomerAtRow(e.RowIndex);
            if (customer is null) return;

            var menu = new ContextMenuStrip
            {
                BackColor = Theme.Surface,
                ForeColor = Theme.TextPrimary,
                Font = new Font("Segoe UI", 10)
            };

            var viewItem = new ToolStripMenuItem("View");
            viewItem.Click += (_, _) => ViewCustomer(customer);
            menu.Items.Add(viewItem);

            var messageItem = new ToolStripMenuItem("Message");
            messageItem.Click += (_, _) => MessageCustomer(customer);
            messageItem.Enabled = ContactEmailService.IsValidEmail(customer.Email);
            menu.Items.Add(messageItem);

            if (RbacService.CanEditRecord(customer.AssignedAgentId, customer.CreatedByUserId))
            {
                var editItem = new ToolStripMenuItem("Edit");
                editItem.Click += (_, _) => EditCustomer(customer);
                menu.Items.Add(editItem);
            }

            if (RbacService.CanAssignRecords)
            {
                var assignItem = new ToolStripMenuItem("Assign Agent");
                assignItem.Click += (_, _) =>
                {
                    using var dlg = new AssignAgentDialog(customer.FullName, _controller.GetAgents(), customer.AssignedAgentId);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        _controller.AssignAgent(customer, dlg.SelectedAgentId, dlg.ApproveNow, dlg.ReviewNotes);
                        RefreshGrid();
                    }
                };
                menu.Items.Add(assignItem);
            }

            if (RbacService.CanApproveAssignments &&
                string.Equals(customer.AssignmentStatus, "pending_review", StringComparison.OrdinalIgnoreCase))
            {
                var approveItem = new ToolStripMenuItem("Approve Assignment");
                approveItem.Click += (_, _) => ApproveCustomerAssignment(customer);
                menu.Items.Add(approveItem);
            }

            if (RbacService.CanArchiveRecord(customer.AssignedAgentId, customer.CreatedByUserId))
            {
                var archiveItem = new ToolStripMenuItem("Archive");
                archiveItem.Click += (_, _) => ArchiveCustomer(customer);
                menu.Items.Add(archiveItem);
            }

            menu.Show(grid, grid.PointToClient(Cursor.Position));
        }

        private void MessageCustomer(Customer customer)
        {
            using var form = new CRMS_Peguit.winforms.Views.Shared.EmailMessageForm(customer.FullName, customer.Email);
            if (form.ShowDialog() == DialogResult.OK)
            {
                _controller.LogEmail(customer, form.SentSubject);
                RefreshGrid();
            }
        }

        private void ApproveCustomerAssignment(Customer customer)
        {
            _controller.ApproveAssignment(customer, "Reviewed from Customers module.");
            MessageBox.Show(
                $"Assignment for '{customer.FullName}' has been approved.",
                "Assignment Approved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            RefreshGrid();
        }

        private Customer? GetCustomerAtRow(int rowIndex)
        {
            if (grid.Rows[rowIndex].Cells["CustomerId"].Value is int id)
            {
                return _controller.GetAll().FirstOrDefault(c => c.CustomerId == id) ?? _controller.GetById(id);
            }
            return null;
        }

        private void BtnAddClick(object? sender, EventArgs e)
        {
            using var form = new CustomerInputForm();
            if (form.ShowDialog() == DialogResult.OK && form.Result is not null)
            {
                _controller.Add(form.Result);
                RefreshKpis();
                RefreshGrid();
            }
        }

        private void ViewCustomer(Customer customer)
        {
            using var form = new CustomerDetailForm(customer, _controller);
            form.ShowDialog();
            RefreshKpis();
            RefreshGrid();
        }

        private void EditCustomer(Customer customer)
        {
            using var form = new CustomerInputForm(customer);
            if (form.ShowDialog() == DialogResult.OK && form.Result is not null)
            {
                _controller.Update(form.Result);
                RefreshKpis();
                RefreshGrid();
            }
        }

        private void RefreshKpis()
        {
            // Dynamic totals are now displayed in lblSubtitle
        }

        private void ArchiveCustomer(Customer customer)
        {
            var confirm = MessageBox.Show(
                $"Are you sure you want to archive '{customer.FullName}'?",
                "Confirm Archive",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                _controller.SoftDelete(customer);
                RefreshGrid();
            }
        }

        private void ExportToCsv()
        {
            using var sfd = new SaveFileDialog
            {
                Filter = "CSV File (*.csv)|*.csv",
                FileName = $"Customers_Export_{DateTime.Now:yyyyMMdd_HHmm}.csv"
            };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var customers = _controller.GetAll().ToList();
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("CustomerId,FullName,Type,Phone,Email,Status,AssignedAgent,CreatedAt");
                foreach (var c in customers)
                {
                    sb.AppendLine($"\"{c.CustomerId}\",\"{c.FullName}\",\"{c.Type}\",\"{c.Phone}\",\"{c.Email}\",\"{c.Status}\",\"{_controller.GetAssignedAgentName(c.AssignedAgentId)}\",\"{c.CreatedAt:yyyy-MM-dd}\"");
                }
                System.IO.File.WriteAllText(sfd.FileName, sb.ToString());
                MessageBox.Show("Customers exported successfully.", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
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
            var pills = new[] { btnFilterInactive, btnFilterFollowUp, btnFilterActive, btnFilterAll };
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
                txtSearch.Width = Math.Min(360, availableForSearch);

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
                var forwardPills = new[] { btnFilterAll, btnFilterActive, btnFilterFollowUp, btnFilterInactive };
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

        private void lblTitle_Click(object sender, EventArgs e)
        {

        }
    }
}

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
        private Panel _pnlEmptyState = null!;
        private int _hoverRowIndex = -1;
        private string _sortColumn = "Name";
        private SortOrder _sortDirection = SortOrder.Ascending;
        private bool _isSkeletonLoading = false;
        private System.Windows.Forms.Timer? _skeletonTimer;
        private PaginationControl _pagination = null!;
        private List<Lead> _allLeads = new();
        private List<Lead> _filteredLeads = new();

        public LeadsView()
        {
            InitializeComponent();
            _controller = new LeadController();

            InitPagination();
            InitEmptyState();
            ApplyStyling();
            BindEvents();
            RefreshGrid(reloadFromDb: true, animate: false);

            this.Load += (_, _) => LayoutToolbar();
            this.Resize += (_, _) => LayoutToolbar();
        }

        private void InitPagination()
        {
            _pagination = new PaginationControl();
            _pagination.SetItemLabel("leads");
            _pagination.PageChanged += (_, _) => BindCurrentPage();
            _pagination.PageSizeChanged += (_, _) => BindCurrentPage();
            pnlCard.Controls.Add(_pagination);
            _pagination.BringToFront();
        }

        private void InitEmptyState()
        {
            _pnlEmptyState = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Visible = false
            };

            var innerPanel = new Panel
            {
                Size = new Size(420, 240),
                BackColor = Color.Transparent
            };

            var lblIcon = new Label
            {
                Text = "🔍",
                Font = new Font("Segoe UI Emoji", 34f),
                ForeColor = Color.FromArgb(148, 163, 184),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(70, 70),
                Location = new Point((420 - 70) / 2, 8)
            };

            var lblTitle = new Label
            {
                Text = "No leads found",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(400, 28),
                Location = new Point(10, 85)
            };

            var lblDesc = new Label
            {
                Text = "We couldn't find any leads matching your criteria.\nTry searching with different terms or selecting another stage.",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(100, 116, 139),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(400, 42),
                Location = new Point(10, 116)
            };

            var btnReset = new Button
            {
                Text = "Clear Filters & Search",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 91, 158),
                BackColor = Color.FromArgb(239, 246, 255),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(180, 36),
                Location = new Point((420 - 180) / 2, 172)
            };
            btnReset.FlatAppearance.BorderColor = Color.FromArgb(191, 219, 254);
            UiRadiusHelper.StyleButton(btnReset, 8);
            btnReset.Click += (_, _) =>
            {
                txtSearch.Clear();
                SetFilter("All");
            };

            innerPanel.Controls.Add(lblIcon);
            innerPanel.Controls.Add(lblTitle);
            innerPanel.Controls.Add(lblDesc);
            innerPanel.Controls.Add(btnReset);

            _pnlEmptyState.Controls.Add(innerPanel);
            _pnlEmptyState.Resize += (_, _) =>
            {
                innerPanel.Location = new Point((_pnlEmptyState.Width - innerPanel.Width) / 2, Math.Max(20, (_pnlEmptyState.Height - innerPanel.Height) / 2));
            };

            pnlCard.Controls.Add(_pnlEmptyState);
            _pnlEmptyState.BringToFront();
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
            txtSearch.TextChanged += (_, _) => RefreshGrid(reloadFromDb: false, animate: false);

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

            // Modern Grid Styling (56px row height for optimal readability and touch targets)
            UiGridHelper.ApplyModernGridStyle(grid, 56);
            UiRadiusHelper.SetPadding(txtSearch, 12, 12);

            grid.CellPainting += Grid_CellPainting;
            grid.CellContentClick += GridCellContentClick;
            grid.ColumnHeaderMouseClick += Grid_ColumnHeaderMouseClick;

            grid.CellMouseEnter += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.RowIndex != _hoverRowIndex)
                {
                    int old = _hoverRowIndex;
                    _hoverRowIndex = e.RowIndex;
                    if (old >= 0 && old < grid.RowCount) grid.InvalidateRow(old);
                    if (_hoverRowIndex < grid.RowCount) grid.InvalidateRow(_hoverRowIndex);
                }
            };

            grid.CellMouseLeave += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.RowIndex == _hoverRowIndex)
                {
                    int old = _hoverRowIndex;
                    _hoverRowIndex = -1;
                    if (old >= 0 && old < grid.RowCount) grid.InvalidateRow(old);
                }
            };

            grid.CellDoubleClick += (_, e) =>
            {
                if (e.RowIndex < 0 || _isSkeletonLoading) return;
                var lead = GetLeadAtRow(e.RowIndex);
                if (lead is not null) ViewLead(lead);
            };
        }

        private void SetFilter(string stage)
        {
            if (string.Equals(_filterStage, stage, StringComparison.OrdinalIgnoreCase)) return;
            _filterStage = stage;
            UpdateFilterPillStyles();
            RefreshGrid(reloadFromDb: false, animate: true);
        }

        private void UpdateFilterPillStyles()
        {
            var allList = _allLeads;
            int total = allList.Count;
            int countNew = allList.Count(l => string.Equals(l.Stage, "new", StringComparison.OrdinalIgnoreCase));
            int countContacted = allList.Count(l => string.Equals(l.Stage, "contacted", StringComparison.OrdinalIgnoreCase));
            int countQualified = allList.Count(l => string.Equals(l.Stage, "qualified", StringComparison.OrdinalIgnoreCase));
            int countConverted = allList.Count(l => string.Equals(l.Stage, "converted", StringComparison.OrdinalIgnoreCase));

            lblSubtitle.Text = $"{total} total · {countQualified} qualified";

            var pills = new[]
            {
                (btnFilterAll, "All", total),
                (btnFilterNew, "New", countNew),
                (btnFilterContacted, "Contacted", countContacted),
                (btnFilterQualified, "Qualified", countQualified),
                (btnFilterConverted, "Converted", countConverted)
            };

            foreach (var (btn, name, count) in pills)
            {
                bool isSelected = string.Equals(_filterStage, name, StringComparison.OrdinalIgnoreCase);
                btn.Text = $"{name}  {count}";
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
                btn.Width = TextRenderer.MeasureText(btn.Text, btn.Font).Width + 24;
                UiRadiusHelper.ApplyPillShape(btn);
            }

            LayoutToolbar();
        }

        private void Grid_ColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex < 0 || _isSkeletonLoading) return;
            string colName = grid.Columns[e.ColumnIndex].Name;
            if (colName == "Actions" || colName == "LeadId") return;

            if (string.Equals(_sortColumn, colName, StringComparison.OrdinalIgnoreCase))
            {
                _sortDirection = (_sortDirection == SortOrder.Ascending) ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                _sortColumn = colName;
                _sortDirection = SortOrder.Ascending;
            }

            RefreshGrid(reloadFromDb: false, animate: false);
        }

        private void RefreshGrid(bool reloadFromDb = true, bool animate = false)
        {
            if (reloadFromDb || _allLeads.Count == 0)
            {
                _allLeads = _controller.GetAll().ToList();
                UpdateFilterPillStyles();
            }

            IEnumerable<Lead> query = _allLeads;

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

            // Sorting
            query = _sortColumn switch
            {
                "Name" => _sortDirection == SortOrder.Ascending ? query.OrderBy(l => l.FullName) : query.OrderByDescending(l => l.FullName),
                "Email" => _sortDirection == SortOrder.Ascending ? query.OrderBy(l => l.Email ?? "") : query.OrderByDescending(l => l.Email ?? ""),
                "Phone" => _sortDirection == SortOrder.Ascending ? query.OrderBy(l => l.Phone ?? "") : query.OrderByDescending(l => l.Phone ?? ""),
                "Source" => _sortDirection == SortOrder.Ascending ? query.OrderBy(l => l.Source ?? "") : query.OrderByDescending(l => l.Source ?? ""),
                "ExpectedValue" => _sortDirection == SortOrder.Ascending ? query.OrderBy(l => l.ExpectedValue ?? 0m) : query.OrderByDescending(l => l.ExpectedValue ?? 0m),
                "Stage" => _sortDirection == SortOrder.Ascending ? query.OrderBy(l => l.Stage ?? "") : query.OrderByDescending(l => l.Stage ?? ""),
                "Assignment" => _sortDirection == SortOrder.Ascending ? query.OrderBy(l => l.AssignmentStatus ?? "") : query.OrderByDescending(l => l.AssignmentStatus ?? ""),
                _ => query
            };

            _filteredLeads = query.ToList();
            _pagination.UpdatePagination(_filteredLeads.Count, 1, _pagination.PageSize);

            if (animate)
            {
                _isSkeletonLoading = true;
                _skeletonTimer?.Stop();
                _skeletonTimer?.Dispose();

                BindCurrentPage();
                grid.Invalidate();

                _skeletonTimer = new System.Windows.Forms.Timer { Interval = 130 };
                _skeletonTimer.Tick += (_, _) =>
                {
                    _skeletonTimer.Stop();
                    _isSkeletonLoading = false;
                    grid.Invalidate();
                };
                _skeletonTimer.Start();
            }
            else
            {
                _isSkeletonLoading = false;
                BindCurrentPage();
            }
        }

        private void BindCurrentPage()
        {
            grid.Columns.Clear();
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            var pageItems = _filteredLeads
                .Skip((_pagination.CurrentPage - 1) * _pagination.PageSize)
                .Take(_pagination.PageSize)
                .Select(lead => new
                {
                    lead.LeadId,
                    Name = lead.FullName,
                    Email = string.IsNullOrWhiteSpace(lead.Email) ? "-" : lead.Email,
                    Phone = string.IsNullOrWhiteSpace(lead.Phone) ? "-" : lead.Phone,
                    Source = string.IsNullOrWhiteSpace(lead.Source) ? "Website" : lead.Source,
                    ExpectedValue = lead.ExpectedValue.HasValue ? $"₱{lead.ExpectedValue.Value:N2}" : "-",
                    Stage = lead.Stage.ToUpper(),
                    Assignment = lead.AssignmentStatus.ToUpper()
                })
                .ToList();

            grid.DataSource = pageItems;

            var idCol = grid.Columns["LeadId"];
            if (idCol is not null) idCol.Visible = false;

            grid.ShowCellToolTips = true;

            if (grid.Columns["Name"] is DataGridViewColumn nameCol)
            {
                nameCol.HeaderText = "NAME";
                nameCol.FillWeight = 160;
                nameCol.MinimumWidth = 140;
                nameCol.SortMode = DataGridViewColumnSortMode.Programmatic;
            }
            if (grid.Columns["Email"] is DataGridViewColumn emailCol)
            {
                emailCol.HeaderText = "EMAIL";
                emailCol.FillWeight = 140;
                emailCol.MinimumWidth = 120;
                emailCol.SortMode = DataGridViewColumnSortMode.Programmatic;
            }
            if (grid.Columns["Phone"] is DataGridViewColumn phoneCol)
            {
                phoneCol.HeaderText = "PHONE";
                phoneCol.FillWeight = 100;
                phoneCol.MinimumWidth = 90;
                phoneCol.SortMode = DataGridViewColumnSortMode.Programmatic;
            }
            if (grid.Columns["Source"] is DataGridViewColumn srcCol)
            {
                srcCol.HeaderText = "SOURCE";
                srcCol.FillWeight = 90;
                srcCol.MinimumWidth = 80;
                srcCol.SortMode = DataGridViewColumnSortMode.Programmatic;
            }
            if (grid.Columns["ExpectedValue"] is DataGridViewColumn valCol)
            {
                valCol.HeaderText = "EXPECTED VALUE";
                valCol.FillWeight = 110;
                valCol.MinimumWidth = 105;
                valCol.SortMode = DataGridViewColumnSortMode.Programmatic;
                valCol.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
                valCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            if (grid.Columns["Stage"] is DataGridViewColumn stageCol)
            {
                stageCol.HeaderText = "STAGE";
                stageCol.FillWeight = 95;
                stageCol.MinimumWidth = 85;
                stageCol.SortMode = DataGridViewColumnSortMode.Programmatic;
                stageCol.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                stageCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            if (grid.Columns["Assignment"] is DataGridViewColumn assignCol)
            {
                assignCol.HeaderText = "ASSIGNMENT";
                assignCol.FillWeight = 110;
                assignCol.MinimumWidth = 95;
                assignCol.SortMode = DataGridViewColumnSortMode.Programmatic;
                assignCol.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                assignCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            UiGridHelper.AddActionsColumn(grid, 64);
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _pnlEmptyState.Visible = (_filteredLeads.Count == 0);
        }

        private void Grid_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.Graphics is null) return;

            // Column Header Sort Indicator (Uniform header #F8FAFC with bottom border #E2E8F0, no blue cell glitch)
            if (e.RowIndex == -1 && e.ColumnIndex >= 0)
            {
                using (var hBrush = new SolidBrush(UiGridHelper.HeaderBg))
                {
                    e.Graphics.FillRectangle(hBrush, e.CellBounds);
                }

                var col = grid.Columns[e.ColumnIndex];
                var formatFlags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
                if (col.HeaderCell.Style.Alignment == DataGridViewContentAlignment.MiddleRight)
                    formatFlags |= TextFormatFlags.Right;
                else if (col.HeaderCell.Style.Alignment == DataGridViewContentAlignment.MiddleCenter)
                    formatFlags |= TextFormatFlags.HorizontalCenter;
                else
                    formatFlags |= TextFormatFlags.Left;

                var headerTextRect = new Rectangle(e.CellBounds.X + 12, e.CellBounds.Y, e.CellBounds.Width - 24, e.CellBounds.Height);
                TextRenderer.DrawText(e.Graphics, col.HeaderText, grid.ColumnHeadersDefaultCellStyle.Font, headerTextRect, UiGridHelper.HeaderText, formatFlags);

                using (var bPen = new Pen(Color.FromArgb(226, 232, 240), 1f))
                {
                    e.Graphics.DrawLine(bPen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
                }

                string colName = col.Name;
                if (string.Equals(colName, _sortColumn, StringComparison.OrdinalIgnoreCase))
                {
                    string arrow = _sortDirection == SortOrder.Ascending ? " ▲" : " ▼";
                    using var sortFont = new Font("Segoe UI", 7f, FontStyle.Bold);
                    
                    var textSize = TextRenderer.MeasureText(e.Graphics, col.HeaderText, grid.ColumnHeadersDefaultCellStyle.Font);
                    int arrowX = (col.HeaderCell.Style.Alignment == DataGridViewContentAlignment.MiddleRight)
                        ? e.CellBounds.Right - 16
                        : e.CellBounds.Left + textSize.Width + 16;
                    int arrowY = e.CellBounds.Y + (e.CellBounds.Height - 12) / 2;

                    TextRenderer.DrawText(e.Graphics, arrow, sortFont, new Point(arrowX, arrowY), Color.FromArgb(15, 91, 158));
                }
                e.Handled = true;
                return;
            }

            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            // Determine row background with seamless hover and selection
            Color rowBg = grid.Rows[e.RowIndex].Selected
                ? UiGridHelper.SelectionBg
                : (e.RowIndex == _hoverRowIndex
                    ? UiGridHelper.RowHover
                    : (e.RowIndex % 2 == 1 ? UiGridHelper.RowAlternate : UiGridHelper.RowNormal));

            // Skeleton Loading State
            if (_isSkeletonLoading)
            {
                using (var bgBrush = new SolidBrush(rowBg))
                {
                    e.Graphics.FillRectangle(bgBrush, e.CellBounds);
                }

                // Shimmering placeholder bar
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                int barHeight = 14;
                int barY = e.CellBounds.Y + (e.CellBounds.Height - barHeight) / 2;
                int barWidth = Math.Max(30, e.CellBounds.Width - 36);
                int barX = e.CellBounds.X + 16;

                using (var shimmerBrush = new SolidBrush(Color.FromArgb(226, 232, 240)))
                using (var path = GetRoundedRectangle(new Rectangle(barX, barY, barWidth, barHeight), 4))
                {
                    e.Graphics.FillPath(shimmerBrush, path);
                }

                using (var dividerPen = new Pen(UiGridHelper.GridBorder, 1f))
                {
                    e.Graphics.DrawLine(dividerPen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
                }

                e.Handled = true;
                return;
            }

            string columnName = grid.Columns[e.ColumnIndex].Name;

            // ── STAGE INDICATOR (Minimalist 6px dot + text, NO pills) ──
            if (columnName == "Stage" && e.Value != null)
            {
                string stage = e.Value.ToString() ?? "";
                UiGridHelper.PaintStatusIndicator(grid, e, stage, center: true);
                e.Handled = true;
            }
            // ── ASSIGNMENT STATUS INDICATOR (Minimalist 6px dot + text, NO pills) ──
            else if (columnName == "Assignment" && e.Value != null)
            {
                string rawAssign = e.Value.ToString() ?? "";
                string displayLabel = rawAssign.Contains("PENDING", StringComparison.OrdinalIgnoreCase)
                    ? "PENDING REVIEW"
                    : (rawAssign.Contains("APPROVED", StringComparison.OrdinalIgnoreCase)
                        ? "APPROVED"
                        : (string.IsNullOrWhiteSpace(rawAssign) ? "UNASSIGNED" : rawAssign));

                UiGridHelper.PaintStatusIndicator(grid, e, displayLabel, center: true);
                e.Handled = true;
            }
            // ── NAME COLUMN (Initial avatar + text) ──
            else if (columnName == "Name" && e.Value != null)
            {
                using (var bgBrush = new SolidBrush(rowBg))
                {
                    e.Graphics.FillRectangle(bgBrush, e.CellBounds);
                }

                string name = e.Value.ToString() ?? "";
                string initials = GetInitials(name);

                int avatarSize = 30;
                int avatarX = e.CellBounds.X + 10;
                int avatarY = e.CellBounds.Y + (e.CellBounds.Height - avatarSize) / 2;
                var avatarRect = new Rectangle(avatarX, avatarY, avatarSize, avatarSize);

                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(Color.FromArgb(27, 80, 136))) // Brand Navy
                {
                    e.Graphics.FillEllipse(brush, avatarRect);
                }

                using (var font = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                {
                    TextRenderer.DrawText(e.Graphics, initials, font, avatarRect, Color.White,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }

                var textRect = new Rectangle(avatarX + avatarSize + 10, e.CellBounds.Y,
                    e.CellBounds.Width - avatarSize - 20, e.CellBounds.Height);

                using (var font = new Font("Segoe UI", 9.5f, FontStyle.Bold))
                {
                    TextRenderer.DrawText(e.Graphics, name, font, textRect, Theme.TextPrimary,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                }

                using (var dividerPen = new Pen(UiGridHelper.GridBorder, 1f))
                {
                    e.Graphics.DrawLine(dividerPen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
                }

                e.Handled = true;
            }
            // ── EMAIL COLUMN ──
            else if (columnName == "Email" && e.Value != null)
            {
                using (var bgBrush = new SolidBrush(rowBg))
                {
                    e.Graphics.FillRectangle(bgBrush, e.CellBounds);
                }

                string email = e.Value.ToString() ?? "";
                using (var font = new Font("Segoe UI", 9.5f))
                {
                    var textRect = new Rectangle(e.CellBounds.X + 10, e.CellBounds.Y, e.CellBounds.Width - 16, e.CellBounds.Height);
                    TextRenderer.DrawText(e.Graphics, email, font, textRect, Color.FromArgb(15, 91, 158),
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                }

                using (var dividerPen = new Pen(UiGridHelper.GridBorder, 1f))
                {
                    e.Graphics.DrawLine(dividerPen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
                }

                e.Handled = true;
            }
            // ── EXPECTED VALUE COLUMN (Right-aligned currency) ──
            else if (columnName == "ExpectedValue" && e.Value != null)
            {
                using (var bgBrush = new SolidBrush(rowBg))
                {
                    e.Graphics.FillRectangle(bgBrush, e.CellBounds);
                }

                string valStr = e.Value.ToString() ?? "-";
                using (var font = new Font("Segoe UI", 9.5f, FontStyle.Bold))
                {
                    var textRect = new Rectangle(e.CellBounds.X + 8, e.CellBounds.Y, e.CellBounds.Width - 22, e.CellBounds.Height);
                    TextRenderer.DrawText(e.Graphics, valStr, font, textRect, Theme.TextPrimary,
                        TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
                }

                using (var dividerPen = new Pen(UiGridHelper.GridBorder, 1f))
                {
                    e.Graphics.DrawLine(dividerPen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
                }

                e.Handled = true;
            }
            // ── STANDARD COLUMNS (Phone, Source) ──
            else if (columnName != "Actions")
            {
                using (var bgBrush = new SolidBrush(rowBg))
                {
                    e.Graphics.FillRectangle(bgBrush, e.CellBounds);
                }

                string text = e.Value?.ToString() ?? "";
                using (var font = new Font("Segoe UI", 9.5f))
                {
                    var textRect = new Rectangle(e.CellBounds.X + 10, e.CellBounds.Y, e.CellBounds.Width - 16, e.CellBounds.Height);
                    TextRenderer.DrawText(e.Graphics, text, font, textRect, Theme.TextPrimary,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                }

                using (var dividerPen = new Pen(UiGridHelper.GridBorder, 1f))
                {
                    e.Graphics.DrawLine(dividerPen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
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
            if (rowIndex < 0 || rowIndex >= grid.Rows.Count) return null;
            object? idValue = grid.Rows[rowIndex].Cells["LeadId"].Value;
            if (idValue is null || !int.TryParse(idValue.ToString(), out int leadId))
                return null;

            return _allLeads.FirstOrDefault(lead => lead.LeadId == leadId) ?? _controller.GetById(leadId);
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

            if (CRMS_Peguit.winforms.Auth.RbacService.CanEditRecord(lead.AssignedAgentId, lead.CreatedByUserId, lead.AssignmentStatus))
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

            int rightPadding = 32;
            int leftMargin = 32;
            int totalWidth = ClientSize.Width;

            // Reposition title and subtitle inline next to each other
            lblTitle.Location = new Point(leftMargin, 20);
            lblSubtitle.Location = new Point(lblTitle.Right + 14, lblTitle.Top + 12);
            lblSubtitle.BringToFront();

            // Position header action buttons
            int rightEdge = totalWidth - rightPadding;
            if (btnAdd.Visible)
            {
                btnAdd.Left = rightEdge - btnAdd.Width;
                btnAdd.Top = 20;
                rightEdge = btnAdd.Left - 10;
            }
            if (_btnExport != null && _btnExport.Visible)
            {
                _btnExport.Left = rightEdge - _btnExport.Width;
                _btnExport.Top = 20;
            }

            int y = 78;

            // Layout filter pills
            var pills = new[] { btnFilterConverted, btnFilterQualified, btnFilterContacted, btnFilterNew, btnFilterAll };
            int filterRight = totalWidth - rightPadding;
            int totalFilterWidth = 0;
            foreach (var p in pills) totalFilterWidth += p.Width + 8;

            int availableForSearch = totalWidth - leftMargin - rightPadding - totalFilterWidth - 20;

            if (availableForSearch >= 200)
            {
                // Single row: search on left, filters aligned to right
                foreach (var p in pills)
                {
                    p.Top = y;
                    p.Height = 32;
                    p.Left = filterRight - p.Width;
                    filterRight = p.Left - 8;
                }

                txtSearch.Top = y;
                txtSearch.Left = leftMargin;
                txtSearch.Height = 32;
                txtSearch.Width = Math.Min(360, availableForSearch);

                pnlCard.Top = 126;
                pnlCard.Height = Math.Max(100, ClientSize.Height - 126 - 24);
            }
            else
            {
                // Two rows: search on row 1, filter pills wrapped to row 2
                txtSearch.Top = y;
                txtSearch.Left = leftMargin;
                txtSearch.Height = 32;
                txtSearch.Width = Math.Max(180, totalWidth - leftMargin - rightPadding);

                int filterX = leftMargin;
                int pillY = y + 40;
                var forwardPills = new[] { btnFilterAll, btnFilterNew, btnFilterContacted, btnFilterQualified, btnFilterConverted };
                foreach (var p in forwardPills)
                {
                    p.Top = pillY;
                    p.Height = 32;
                    p.Left = filterX;
                    filterX += p.Width + 8;
                }

                pnlCard.Top = pillY + 42;
                pnlCard.Height = Math.Max(100, ClientSize.Height - pnlCard.Top - 24);
            }

            pnlCard.Left = leftMargin;
            pnlCard.Width = Math.Max(100, totalWidth - leftMargin - rightPadding);
        }
    }
}

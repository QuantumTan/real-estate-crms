using System.Drawing.Drawing2D;
using CRMS_Peguit.domain.entities;
using CRMS_Peguit.winforms.Auth;
using CRMS_Peguit.winforms.Controllers;
using CRMS_Peguit.winforms.Controls;
using CRMS_Peguit.winforms.Models.Services;

namespace CRMS_Peguit.winforms.Views.Deals
{
    public partial class DealsView : UserControl
    {
        private readonly DealController _controller;
        private string _filterStage = "All";
        private Label _lblEmptyState = null!;
        private Button? _btnExport;
        private Button? _btnAdd;
        private PaginationControl _pagination = null!;
        private List<Deal> _allDeals = new();
        private List<Deal> _filteredDeals = new();
        private Dictionary<int, string> _customers = new();
        private Dictionary<int, string> _properties = new();
        private Dictionary<int, string> _agents = new();

        public DealsView()
        {
            InitializeComponent();
            _controller = new DealController();

            InitPagination();
            InitEmptyState();
            ApplyStyling();
            BindEvents();
            UpdateFilterPillStyles();
            RefreshGrid();

            this.Load += (_, _) => LayoutToolbar();
            this.Resize += (_, _) => LayoutToolbar();
        }

        private void InitPagination()
        {
            _pagination = new PaginationControl();
            _pagination.SetItemLabel("deals");
            _pagination.PageChanged += (_, _) => BindCurrentPage();
            _pagination.PageSizeChanged += (_, _) => BindCurrentPage();
            pnlCard.Controls.Add(_pagination);
            _pagination.BringToFront();
        }

        private void InitEmptyState()
        {
            _lblEmptyState = new Label
            {
                Text = "🔍 No deals match your search or filter criteria.\nTry adjusting your search terms or filter.",
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
            UiRadiusHelper.ApplyPillShape(btnFilterAll);
            UiRadiusHelper.ApplyPillShape(btnFilterOffer);
            UiRadiusHelper.ApplyPillShape(btnFilterContract);
            UiRadiusHelper.ApplyPillShape(btnFilterClosed);
            UiRadiusHelper.ApplyPillShape(btnFilterLost);
        }

        private void BindEvents()
        {
            txtSearch.TextChanged += (_, _) => RefreshGrid(reloadFromDb: false);

            if (RbacService.CanCreateSalesRecord)
            {
                _btnAdd = new Button
                {
                    Text = "+ New Deal",
                    BackColor = Theme.Primary,
                    ForeColor = Color.White,
                    Cursor = Cursors.Hand,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Size = new Size(120, 36)
                };
                _btnAdd.Click += (_, _) =>
                {
                    using var form = new DealInputForm(_controller);
                    if (form.ShowDialog(this.FindForm()) == DialogResult.OK && form.Result != null)
                    {
                        _controller.Add(form.Result);
                        RefreshGrid();
                    }
                };
                UiRadiusHelper.StyleButton(_btnAdd, 8);
                Controls.Add(_btnAdd);
                _btnAdd.BringToFront();
            }

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
            btnFilterOffer.Click += (_, _) => SetFilter("Offer");
            btnFilterContract.Click += (_, _) => SetFilter("Contract");
            btnFilterClosed.Click += (_, _) => SetFilter("Closed");
            btnFilterLost.Click += (_, _) => SetFilter("Lost");

            // Modern Grid Styling & Search Padding
            UiGridHelper.ApplyModernGridStyle(grid, 52);
            UiRadiusHelper.SetPadding(txtSearch, 10, 10);

            grid.CellPainting += Grid_CellPainting;
            grid.CellContentClick += GridCellContentClick;
            grid.CellDoubleClick += (_, e) =>
            {
                if (e.RowIndex < 0) return;
                if (int.TryParse(grid.Rows[e.RowIndex].Cells["DealId"]?.Value?.ToString(), out int dealId))
                {
                    var deal = _controller.GetById(dealId);
                    if (deal != null)
                    {
                        using var form = new DealDetailForm(deal, _controller);
                        form.ShowDialog(this.FindForm());
                        RefreshGrid();
                    }
                }
            };
        }

        private void SetFilter(string stage)
        {
            _filterStage = stage;
            UpdateFilterPillStyles();
            RefreshGrid(reloadFromDb: false);
        }

        private void UpdateFilterPillStyles()
        {
            var pills = new[]
            {
                (btnFilterAll, "All"),
                (btnFilterOffer, "Offer"),
                (btnFilterContract, "Contract"),
                (btnFilterClosed, "Closed"),
                (btnFilterLost, "Lost")
            };

            foreach (var (btn, name) in pills)
            {
                bool isSelected = string.Equals(_filterStage, name, StringComparison.OrdinalIgnoreCase);
                UiRadiusHelper.StyleFilterPill(btn, isSelected);
            }
        }

        private void RefreshGrid(bool reloadFromDb = true)
        {
            if (reloadFromDb || _allDeals.Count == 0)
            {
                _allDeals = _controller.GetAll();
                if (_allDeals.Any(d => d.Customer == null))
                    _customers = _controller.GetCustomerNames();
                if (_allDeals.Any(d => d.Property == null))
                    _properties = _controller.GetPropertyAddresses();
                if (_allDeals.Any(d => d.Agent == null && d.AgentId.HasValue))
                    _agents = _controller.GetAgentNames();
            }

            int total = _allDeals.Count;
            decimal totalVolume = _allDeals.Sum(d => d.Value);
            lblSubtitle.Text = $"{total} deals · ₱{totalVolume:N2} total volume";

            IEnumerable<Deal> query = _allDeals;

            if (string.Equals(_filterStage, "Offer", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(d => string.Equals(d.Stage, "Offer", StringComparison.OrdinalIgnoreCase) ||
                                         string.Equals(d.Stage, "Reservation", StringComparison.OrdinalIgnoreCase));
            }
            else if (string.Equals(_filterStage, "Contract", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(d => string.Equals(d.Stage, "ContractSigned", StringComparison.OrdinalIgnoreCase) ||
                                         string.Equals(d.Stage, "Contract", StringComparison.OrdinalIgnoreCase));
            }
            else if (string.Equals(_filterStage, "Closed", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(d => string.Equals(d.Stage, "Closed", StringComparison.OrdinalIgnoreCase));
            }
            else if (string.Equals(_filterStage, "Lost", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(d => string.Equals(d.Stage, "Lost", StringComparison.OrdinalIgnoreCase));
            }

            string search = txtSearch.Text.Trim();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d =>
                    ContainsText(d.Stage, search) ||
                    ContainsText(d.Customer?.FullName ?? GetName(_customers, d.CustomerId), search) ||
                    ContainsText(d.Property?.Address ?? GetName(_properties, d.PropertyId), search) ||
                    ContainsText(d.Agent?.FullName ?? GetName(_agents, d.AgentId), search));
            }

            _filteredDeals = query.ToList();
            _pagination.UpdatePagination(_filteredDeals.Count, 1, _pagination.PageSize);
            BindCurrentPage();
        }

        private void BindCurrentPage()
        {
            grid.SuspendLayout();
            try
            {
                grid.Columns.Clear();
                grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

                var pageItems = _filteredDeals
                    .Skip((_pagination.CurrentPage - 1) * _pagination.PageSize)
                    .Take(_pagination.PageSize)
                    .Select(d => new
                    {
                        d.DealId,
                        Customer = d.Customer?.FullName ?? GetName(_customers, d.CustomerId),
                        Property = d.Property?.Address ?? GetName(_properties, d.PropertyId),
                        Agent = d.Agent?.FullName ?? GetName(_agents, d.AgentId),
                        Value = $"₱{d.Value:N2}",
                        Commission = $"{d.CommissionRate:P1}",
                        Stage = string.IsNullOrWhiteSpace(d.Stage) ? "OFFER" : d.Stage.ToUpper(),
                        CloseDate = d.ExpectedCloseDate.HasValue ? d.ExpectedCloseDate.Value.ToString("MMM dd, yyyy") : "-"
                    })
                    .ToList();

                grid.DataSource = pageItems;

            var dealIdCol = grid.Columns["DealId"];
            if (dealIdCol is not null) dealIdCol.Visible = false;

            if (grid.Columns["Customer"] is DataGridViewColumn custCol)
            {
                custCol.HeaderText = "BUYER";
                custCol.FillWeight = 140;
                custCol.MinimumWidth = 140;
            }

            if (grid.Columns["Property"] is DataGridViewColumn propCol)
            {
                propCol.HeaderText = "PROPERTY";
                propCol.FillWeight = 180;
                propCol.MinimumWidth = 160;
            }

            if (grid.Columns["Agent"] is DataGridViewColumn agCol)
            {
                agCol.HeaderText = "AGENT";
                agCol.FillWeight = 110;
                agCol.MinimumWidth = 100;
            }

            if (grid.Columns["Value"] is DataGridViewColumn valCol)
            {
                valCol.HeaderText = "DEAL VALUE";
                valCol.FillWeight = 100;
                valCol.MinimumWidth = 100;
                valCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                valCol.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            if (grid.Columns["Commission"] is DataGridViewColumn comCol)
            {
                comCol.HeaderText = "COMMISSION";
                comCol.FillWeight = 90;
                comCol.MinimumWidth = 90;
                comCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                comCol.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            if (grid.Columns["Stage"] is DataGridViewColumn stgCol)
            {
                stgCol.HeaderText = "STAGE";
                stgCol.FillWeight = 95;
                stgCol.MinimumWidth = 95;
                stgCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                stgCol.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            }

            if (grid.Columns["CloseDate"] is DataGridViewColumn dtCol)
            {
                dtCol.HeaderText = "EXPECTED CLOSE";
                dtCol.FillWeight = 110;
                dtCol.MinimumWidth = 110;
            }

            UiGridHelper.AddActionsColumn(grid, 64);
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _lblEmptyState.Visible = (_filteredDeals.Count == 0);
            }
            finally
            {
                grid.ResumeLayout();
            }
        }

        private void GridCellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (grid.Columns[e.ColumnIndex] is not CRMS_Peguit.winforms.Controls.ActionsColumn &&
                grid.Columns[e.ColumnIndex].Name != "Actions") return;

            grid.Rows[e.RowIndex].Selected = true;
            grid.CurrentCell = grid.Rows[e.RowIndex].Cells[e.ColumnIndex];

            var customerVal = grid.Rows[e.RowIndex].Cells["Customer"]?.Value?.ToString() ?? "Buyer";
            var propVal = grid.Rows[e.RowIndex].Cells["Property"]?.Value?.ToString() ?? "Property";
            var stageVal = grid.Rows[e.RowIndex].Cells["Stage"]?.Value?.ToString() ?? "Stage";
            var valVal = grid.Rows[e.RowIndex].Cells["Value"]?.Value?.ToString() ?? "₱0.00";
            var agentVal = grid.Rows[e.RowIndex].Cells["Agent"]?.Value?.ToString() ?? "Agent";

            if (!int.TryParse(grid.Rows[e.RowIndex].Cells["DealId"]?.Value?.ToString(), out int dealId)) return;

            var menu = new ContextMenuStrip
            {
                BackColor = Theme.Surface,
                ForeColor = Theme.TextPrimary,
                Font = new Font("Segoe UI", 10)
            };

            var deal = _controller.GetById(dealId);
            if (deal == null) return;

            var viewItem = new ToolStripMenuItem("📄 View Details & Terms");
            viewItem.Click += (_, _) =>
            {
                using var form = new DealDetailForm(deal, _controller);
                form.ShowDialog(this.FindForm());
                RefreshGrid();
            };
            menu.Items.Add(viewItem);

            var contractItem = new ToolStripMenuItem("📜 View Contract & Terms");
            contractItem.Click += (_, _) =>
            {
                using var viewer = new ContractTermsViewerDialog(deal, _controller);
                viewer.ShowDialog(this.FindForm());
            };
            menu.Items.Add(contractItem);

            if (RbacService.CanEditRecord(deal.AgentId, deal.CreatedByUserId))
            {
                var editItem = new ToolStripMenuItem("✏️ Edit Deal & Terms");
                editItem.Click += (_, _) =>
                {
                    using var form = new DealInputForm(_controller, deal);
                    if (form.ShowDialog(this.FindForm()) == DialogResult.OK && form.Result != null)
                    {
                        _controller.Update(form.Result);
                        RefreshGrid();
                    }
                };
                menu.Items.Add(editItem);
            }

            if (RbacService.IsManager || RbacService.IsSuperAdmin)
            {
                var deleteItem = new ToolStripMenuItem("🗑️ Remove Deal");
                deleteItem.Click += (_, _) =>
                {
                    if (MessageBox.Show($"Are you sure you want to remove Deal #{deal.DealId}?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        _controller.Delete(deal);
                        RefreshGrid();
                    }
                };
                menu.Items.Add(deleteItem);
            }

            menu.Show(grid, grid.PointToClient(Cursor.Position));
        }

        private void Grid_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.Graphics is null) return;

            // Minimalist Status Indicator (Left-aligned at 12px, Strictly No Badges/Pills)
            if (grid.Columns[e.ColumnIndex].Name == "Stage" && e.Value != null)
            {
                string stage = e.Value.ToString() ?? "";
                UiGridHelper.PaintStatusIndicator(grid, e, stage, center: false);
            }
        }

        private static GraphicsPath GetRoundedRectangle(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;
            var arc = new Rectangle(bounds.Location, new Size(diameter, diameter));

            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static string GetName(Dictionary<int, string> lookup, int? id)
        {
            if (!id.HasValue) return "Unassigned";
            return lookup.TryGetValue(id.Value, out string? name) ? name : $"#{id.Value}";
        }

        private static bool ContainsText(string? value, string search)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   value.Contains(search, StringComparison.OrdinalIgnoreCase);
        }

        private void ExportToCsv()
        {
            using var sfd = new SaveFileDialog
            {
                Filter = "CSV File (*.csv)|*.csv",
                FileName = $"Deals_Export_{DateTime.Now:yyyyMMdd_HHmm}.csv"
            };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var deals = _controller.GetAll();
                var customers = _controller.GetCustomerNames();
                var properties = _controller.GetPropertyAddresses();
                var agents = _controller.GetAgentNames();

                var sb = new System.Text.StringBuilder();
                sb.AppendLine("DealId,Customer,Property,Agent,Value,CommissionRate,Stage,ExpectedCloseDate");
                foreach (var d in deals)
                {
                    sb.AppendLine($"\"{d.DealId}\",\"{GetName(customers, d.CustomerId)}\",\"{GetName(properties, d.PropertyId)}\",\"{GetName(agents, d.AgentId)}\",\"{d.Value}\",\"{d.CommissionRate:P1}\",\"{d.Stage}\",\"{d.ExpectedCloseDate:yyyy-MM-dd}\"");
                }
                System.IO.File.WriteAllText(sfd.FileName, sb.ToString());
                MessageBox.Show("Deals exported successfully.", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void LayoutToolbar()
        {
            if (this.IsDisposed) return;

            int rightPadding = 30;
            int leftMargin = 30;
            int totalWidth = ClientSize.Width;

            // Explicit header positioning with clear separation
            lblTitle.Location = new Point(leftMargin, 20);
            lblSubtitle.Location = new Point(leftMargin + 2, lblTitle.Bottom + 4);
            int y = Math.Max(96, lblSubtitle.Bottom + 16);

            // Position header action buttons
            int rightEdge = totalWidth - rightPadding;
            if (_btnAdd is not null && _btnAdd.Visible)
            {
                _btnAdd.Left = rightEdge - _btnAdd.Width;
                _btnAdd.Top = 24;
                rightEdge = _btnAdd.Left - 10;
            }

            if (_btnExport is not null && _btnExport.Visible)
            {
                _btnExport.Left = rightEdge - _btnExport.Width;
                _btnExport.Top = 24;
            }

            // Layout filter pills
            var pills = new[] { btnFilterLost, btnFilterClosed, btnFilterContract, btnFilterOffer, btnFilterAll };
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

                int cardTop = y + txtSearch.Height + 14;
                pnlCard.Top = cardTop;
                pnlCard.Height = Math.Max(100, ClientSize.Height - cardTop - 24);
            }
            else
            {
                // Two rows: search on row 1, filter pills wrapped to row 2
                txtSearch.Top = y;
                txtSearch.Left = leftMargin;
                txtSearch.Width = Math.Max(180, totalWidth - leftMargin - rightPadding);

                int filterX = leftMargin;
                int pillY = y + txtSearch.Height + 10;
                var forwardPills = new[] { btnFilterAll, btnFilterOffer, btnFilterContract, btnFilterClosed, btnFilterLost };
                foreach (var p in forwardPills)
                {
                    p.Top = pillY;
                    p.Left = filterX;
                    filterX += p.Width + 6;
                }

                int cardTop = pillY + 34;
                pnlCard.Top = cardTop;
                pnlCard.Height = Math.Max(100, ClientSize.Height - cardTop - 20);
            }

            pnlCard.Left = leftMargin;
            pnlCard.Width = Math.Max(100, totalWidth - leftMargin - rightPadding);
        }
    }
}
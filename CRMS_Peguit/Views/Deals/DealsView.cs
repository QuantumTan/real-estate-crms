using System.Drawing.Drawing2D;
using CRMS_Peguit.domain.entities;
using CRMS_Peguit.winforms.Auth;
using CRMS_Peguit.winforms.Controllers;
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

        public DealsView()
        {
            InitializeComponent();
            _controller = new DealController();

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
            txtSearch.TextChanged += (_, _) => RefreshGrid();

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
            RefreshGrid();
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

            var deals = _controller.GetAll();
            var customers = _controller.GetCustomerNames();
            var properties = _controller.GetPropertyAddresses();
            var agents = _controller.GetAgentNames();

            int total = deals.Count;
            decimal totalVolume = deals.Sum(d => d.Value);
            lblSubtitle.Text = $"{total} deals · ₱{totalVolume:N0} total volume";

            IEnumerable<Deal> query = deals;

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
                    ContainsText(GetName(customers, d.CustomerId), search) ||
                    ContainsText(GetName(properties, d.PropertyId), search) ||
                    ContainsText(GetName(agents, d.AgentId), search));
            }

            grid.DataSource = query
                .Select(d => new
                {
                    d.DealId,
                    Customer = GetName(customers, d.CustomerId),
                    Property = GetName(properties, d.PropertyId),
                    Agent = GetName(agents, d.AgentId),
                    Value = $"₱{d.Value:N0}",
                    Commission = $"{d.CommissionRate:P1}",
                    Stage = string.IsNullOrWhiteSpace(d.Stage) ? "OFFER" : d.Stage.ToUpper(),
                    CloseDate = d.ExpectedCloseDate.HasValue ? d.ExpectedCloseDate.Value.ToString("MMM dd, yyyy") : "-"
                })
                .ToList();

            grid.ShowCellToolTips = true;

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
            }

            if (grid.Columns["Commission"] is DataGridViewColumn comCol)
            {
                comCol.HeaderText = "COMMISSION";
                comCol.FillWeight = 90;
                comCol.MinimumWidth = 90;
            }

            if (grid.Columns["Stage"] is DataGridViewColumn stgCol)
            {
                stgCol.HeaderText = "STAGE";
                stgCol.FillWeight = 95;
                stgCol.MinimumWidth = 95;
            }

            if (grid.Columns["CloseDate"] is DataGridViewColumn dtCol)
            {
                dtCol.HeaderText = "EXPECTED CLOSE";
                dtCol.FillWeight = 110;
                dtCol.MinimumWidth = 110;
            }

            UiGridHelper.AddActionsColumn(grid, 64);
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _lblEmptyState.Visible = (grid.Rows.Count == 0);
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
            var valVal = grid.Rows[e.RowIndex].Cells["Value"]?.Value?.ToString() ?? "₱0";
            var agentVal = grid.Rows[e.RowIndex].Cells["Agent"]?.Value?.ToString() ?? "Agent";

            if (!int.TryParse(grid.Rows[e.RowIndex].Cells["DealId"]?.Value?.ToString(), out int dealId)) return;

            var menu = new ContextMenuStrip
            {
                BackColor = Theme.Surface,
                ForeColor = Theme.TextPrimary,
                Font = new Font("Segoe UI", 10)
            };

            var viewItem = new ToolStripMenuItem("📄 View Details & Terms");
            viewItem.Click += (_, _) =>
            {
                var deal = _controller.GetById(dealId);
                if (deal != null)
                {
                    using var form = new DealDetailForm(deal, _controller);
                    form.ShowDialog(this.FindForm());
                    RefreshGrid();
                }
            };
            menu.Items.Add(viewItem);

            var contractItem = new ToolStripMenuItem("📜 View Contract & Terms");
            contractItem.Click += (_, _) =>
            {
                var deal = _controller.GetById(dealId);
                if (deal != null)
                {
                    using var viewer = new ContractTermsViewerDialog(deal, _controller);
                    viewer.ShowDialog(this.FindForm());
                }
            };
            menu.Items.Add(contractItem);

            if (RbacService.CanCreateSalesRecord)
            {
                var editItem = new ToolStripMenuItem("✏️ Edit Deal & Terms");
                editItem.Click += (_, _) =>
                {
                    var deal = _controller.GetById(dealId);
                    if (deal != null)
                    {
                        using var form = new DealInputForm(_controller, deal);
                        if (form.ShowDialog(this.FindForm()) == DialogResult.OK && form.Result != null)
                        {
                            _controller.Update(form.Result);
                            RefreshGrid();
                        }
                    }
                };
                menu.Items.Add(editItem);
            }

            if (RbacService.IsManager || RbacService.IsSuperAdmin)
            {
                var deleteItem = new ToolStripMenuItem("🗑️ Remove Deal");
                deleteItem.Click += (_, _) =>
                {
                    var deal = _controller.GetById(dealId);
                    if (deal != null)
                    {
                        if (MessageBox.Show($"Are you sure you want to remove Deal #{deal.DealId}?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                        {
                            _controller.Delete(deal);
                            RefreshGrid();
                        }
                    }
                };
                menu.Items.Add(deleteItem);
            }

            menu.Show(grid, grid.PointToClient(Cursor.Position));
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

                if (stage.Contains("CLOSED", StringComparison.OrdinalIgnoreCase))
                {
                    bgColor = Color.FromArgb(220, 252, 231);
                    textColor = Color.FromArgb(22, 101, 52);
                }
                else if (stage.Contains("CONTRACT", StringComparison.OrdinalIgnoreCase))
                {
                    bgColor = Color.FromArgb(224, 231, 255);
                    textColor = Color.FromArgb(55, 48, 163);
                }
                else if (stage.Contains("RESERVATION", StringComparison.OrdinalIgnoreCase))
                {
                    bgColor = Color.FromArgb(243, 232, 255);
                    textColor = Color.FromArgb(107, 33, 168);
                }
                else if (stage.Contains("LOST", StringComparison.OrdinalIgnoreCase))
                {
                    bgColor = Color.FromArgb(254, 226, 226);
                    textColor = Color.FromArgb(153, 27, 27);
                }
                else
                {
                    bgColor = Color.FromArgb(224, 242, 254);
                    textColor = Color.FromArgb(3, 105, 161);
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
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        e.Graphics.FillPath(brush, path);
                    }

                    TextRenderer.DrawText(e.Graphics, stage, font, pillRect, textColor,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }

                e.Handled = true;
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
            int y = 88;

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
                var forwardPills = new[] { btnFilterAll, btnFilterOffer, btnFilterContract, btnFilterClosed, btnFilterLost };
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
    }
}
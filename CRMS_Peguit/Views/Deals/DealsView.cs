using System.Drawing.Drawing2D;
using CRMS_Peguit.domain.entities;
using CRMS_Peguit.winforms.Controllers;
using CRMS_Peguit.winforms.Models.Services;

namespace CRMS_Peguit.winforms.Views.Deals
{
    public partial class DealsView : UserControl
    {
        private readonly DealController _controller;
        private string _filterStage = "All";

        public DealsView()
        {
            InitializeComponent();
            _controller = new DealController();

            BindEvents();
            UpdateFilterPillStyles();
            RefreshGrid();
        }

        private void BindEvents()
        {
            txtSearch.TextChanged += (_, _) => RefreshGrid();

            btnFilterAll.Click += (_, _) => SetFilter("All");
            btnFilterOffer.Click += (_, _) => SetFilter("Offer");
            btnFilterContract.Click += (_, _) => SetFilter("Contract");
            btnFilterClosed.Click += (_, _) => SetFilter("Closed");
            btnFilterLost.Click += (_, _) => SetFilter("Lost");

            // Modern Grid Styling
            grid.EnableHeadersVisualStyles = false;
            grid.GridColor = Color.FromArgb(241, 245, 249);
            grid.RowTemplate.Height = 52;
            grid.DefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.ForeColor = Color.FromArgb(15, 23, 42);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(241, 245, 249);
            grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f);
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(100, 116, 139);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            grid.ColumnHeadersHeight = 44;

            grid.CellPainting += Grid_CellPainting;
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

            var deals = _controller.GetAll();
            var customers = _controller.GetCustomerNames();
            var properties = _controller.GetPropertyAddresses();
            var agents = _controller.GetAgentNames();

            int total = deals.Count;
            decimal totalVolume = deals.Sum(d => d.Value);
            lblSubtitle.Text = $"{total} deals · ${totalVolume:N0} total volume";

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
                    Value = $"${d.Value:N0}",
                    Commission = $"{d.CommissionRate:P1}",
                    Stage = string.IsNullOrWhiteSpace(d.Stage) ? "OFFER" : d.Stage.ToUpper(),
                    CloseDate = d.ExpectedCloseDate.HasValue ? d.ExpectedCloseDate.Value.ToString("MMM dd, yyyy") : "-"
                })
                .ToList();

            var dealIdCol = grid.Columns["DealId"];
            if (dealIdCol is not null) dealIdCol.Visible = false;

            if (grid.Columns["Customer"] is DataGridViewColumn custCol)
            {
                custCol.HeaderText = "BUYER";
                custCol.FillWeight = 140;
            }

            if (grid.Columns["Property"] is DataGridViewColumn propCol)
            {
                propCol.HeaderText = "PROPERTY";
                propCol.FillWeight = 180;
            }

            if (grid.Columns["Agent"] is DataGridViewColumn agCol)
            {
                agCol.HeaderText = "AGENT";
                agCol.FillWeight = 110;
            }

            if (grid.Columns["Value"] is DataGridViewColumn valCol)
            {
                valCol.HeaderText = "DEAL VALUE";
                valCol.FillWeight = 100;
            }

            if (grid.Columns["Commission"] is DataGridViewColumn comCol)
            {
                comCol.HeaderText = "COMMISSION";
                comCol.FillWeight = 90;
            }

            if (grid.Columns["Stage"] is DataGridViewColumn stgCol)
            {
                stgCol.HeaderText = "STAGE";
                stgCol.FillWeight = 95;
            }

            if (grid.Columns["CloseDate"] is DataGridViewColumn dtCol)
            {
                dtCol.HeaderText = "EXPECTED CLOSE";
                dtCol.FillWeight = 110;
            }
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
    }
}
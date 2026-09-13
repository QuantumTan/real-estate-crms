using System;
using System.Drawing.Drawing2D;
using System.Linq;
using CRMS_Peguit.domain.entities;
using CRMS_Peguit.winforms.Controllers;
using CRMS_Peguit.winforms.Models.Services;
using CRMS_Peguit.winforms.Models.ViewModels;

namespace CRMS_Peguit.winforms.Views.Dashboard
{
    public partial class DashboardView : UserControl
    {
        public event Action<string>? NavigationRequested;

        public DashboardView()
        {
            InitializeComponent();
            SetupStyling();
            LoadData();
        }

        private void SetupStyling()
        {
            // Wire up clickable KPI cards to trigger navigation
            kpiCustomers.Click += (_, _) => RequestNavigation("Customers");
            kpiProperties.Click += (_, _) => RequestNavigation("Properties");
            kpiLeads.Click += (_, _) => RequestNavigation("Leads");
            kpiDeals.Click += (_, _) => RequestNavigation("Deals");

            // Modern rounded cards
            UiRadiusHelper.StyleCard(pnlLeftCard, 12);
            UiRadiusHelper.StyleCard(pnlRightCard, 12);

            // Modern Recent grid styling
            UiGridHelper.ApplyModernGridStyle(gridRecent, 46);
            gridRecent.CellPainting += GridRecent_CellPainting;

            // Restructure right card with modern metric highlight tiles
            SetupHighlightTiles();
        }

        private void SetupHighlightTiles()
        {
            pnlRightCard.Controls.Clear();
            pnlRightCard.Controls.Add(lblRightTitle);
            lblRightTitle.Location = new Point(20, 20);

            var tile1 = CreateHighlightTile("💰", "TOTAL PIPELINE VOLUME", lblStat1Value, Color.FromArgb(224, 242, 254), Color.FromArgb(2, 132, 199));
            var tile2 = CreateHighlightTile("🏢", "AVAILABLE INVENTORY", lblStat2Value, Color.FromArgb(220, 252, 231), Color.FromArgb(22, 163, 74));
            var tile3 = CreateHighlightTile("👥", "ACTIVE SALES AGENTS", lblStat3Value, Color.FromArgb(237, 233, 254), Color.FromArgb(124, 58, 237));

            void PositionTiles()
            {
                int y = 60;
                int tileWidth = Math.Max(200, pnlRightCard.ClientSize.Width - 40);
                foreach (var tile in new[] { tile1, tile2, tile3 })
                {
                    tile.Location = new Point(20, y);
                    tile.Size = new Size(tileWidth, 88);
                    y += 100;
                }
            }

            pnlRightCard.Controls.Add(tile1);
            pnlRightCard.Controls.Add(tile2);
            pnlRightCard.Controls.Add(tile3);

            PositionTiles();
            pnlRightCard.Resize += (_, _) => PositionTiles();
        }

        private Panel CreateHighlightTile(string icon, string title, Label valueLabel, Color iconBg, Color iconColor)
        {
            var panel = new Panel
            {
                BackColor = Color.FromArgb(248, 250, 252),
                Padding = new Padding(14)
            };
            UiRadiusHelper.ApplyRoundedCorners(panel, 10);
            panel.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(226, 232, 240), 1f);
                using var path = UiRadiusHelper.CreateRoundedPath(new Rectangle(0, 0, panel.Width - 1, panel.Height - 1), 10);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawPath(pen, path);
            };

            var iconLabel = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 14f),
                ForeColor = iconColor,
                BackColor = iconBg,
                Size = new Size(42, 42),
                Location = new Point(14, 23),
                TextAlign = ContentAlignment.MiddleCenter
            };
            UiRadiusHelper.MakeCircularAvatar(iconLabel);

            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(68, 18),
                AutoSize = true
            };

            valueLabel.Parent = null;
            valueLabel.Font = new Font("Segoe UI", 16f, FontStyle.Bold);
            valueLabel.Location = new Point(66, 38);
            valueLabel.AutoSize = true;

            panel.Controls.Add(iconLabel);
            panel.Controls.Add(titleLabel);
            panel.Controls.Add(valueLabel);

            return panel;
        }

        private void RequestNavigation(string module)
        {
            if (NavigationRequested is not null)
            {
                NavigationRequested.Invoke(module);
                return;
            }

            if (FindForm() is Form1 form)
            {
                form.NavigateTo(module);
            }
        }

        private void LoadData()
        {
            try
            {
                using var dashboardCtrl = new DashboardController();
                var summary = dashboardCtrl.GetSummary();

                kpiCustomers.SetValue(summary.TotalCustomers);
                kpiProperties.SetValue(summary.ActiveProperties);
                kpiLeads.SetValue(summary.QualifiedLeads);
                kpiDeals.SetValue(summary.TotalDeals);

                lblStat1Value.Text = $"₱{summary.PipelineValue:N0}";
                lblStat2Value.Text = $"{summary.ActiveProperties} listings";
                lblStat3Value.Text = $"{summary.TotalAgents} agents";

                // Populate recent leads
                gridRecent.Columns.Clear();
                gridRecent.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                gridRecent.DataSource = summary.RecentLeads;

                bool hasLeads = summary.RecentLeads.Count > 0;
                lblRecentEmpty.Visible = !hasLeads;
                gridRecent.Visible = hasLeads;

                if (gridRecent.Columns["Lead"] is DataGridViewColumn leadCol)
                {
                    leadCol.HeaderText = "LEAD NAME";
                    leadCol.FillWeight = 140;
                    leadCol.MinimumWidth = 100;
                }
                if (gridRecent.Columns["Email"] is DataGridViewColumn emailCol)
                {
                    emailCol.HeaderText = "EMAIL";
                    emailCol.FillWeight = 140;
                    emailCol.MinimumWidth = 100;
                }
                if (gridRecent.Columns["Source"] is DataGridViewColumn srcCol)
                {
                    srcCol.HeaderText = "SOURCE";
                    srcCol.FillWeight = 90;
                    srcCol.MinimumWidth = 70;
                }
                if (gridRecent.Columns["Value"] is DataGridViewColumn valCol)
                {
                    valCol.HeaderText = "VALUE";
                    valCol.FillWeight = 90;
                    valCol.MinimumWidth = 70;
                }
                if (gridRecent.Columns["Stage"] is DataGridViewColumn stageCol)
                {
                    stageCol.HeaderText = "STAGE";
                    stageCol.FillWeight = 95;
                    stageCol.MinimumWidth = 70;
                }

                gridRecent.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            catch
            {
                lblRecentEmpty.Visible = true;
                gridRecent.Visible = false;
            }
        }

        private void GridRecent_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.Graphics is null) return;

            if (gridRecent.Columns[e.ColumnIndex].Name == "Stage" && e.Value != null)
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
                    bgColor = Color.FromArgb(241, 245, 249);
                    textColor = Color.FromArgb(71, 85, 105);
                }

                using (var font = new Font("Segoe UI", 8f, FontStyle.Bold))
                {
                    var size = TextRenderer.MeasureText(stage, font);
                    int pillWidth = size.Width + 14;
                    int pillHeight = 20;
                    int pillX = e.CellBounds.X + (e.CellBounds.Width - pillWidth) / 2;
                    int pillY = e.CellBounds.Y + (e.CellBounds.Height - pillHeight) / 2;
                    var pillRect = new Rectangle(pillX, pillY, pillWidth, pillHeight);

                    using (var brush = new SolidBrush(bgColor))
                    using (var path = GetRoundedRectangle(pillRect, 6))
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
    }
}
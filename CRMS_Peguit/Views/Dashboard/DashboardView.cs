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
                    tile.Size = new Size(tileWidth, 76);
                    y += 88;
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
                Padding = new Padding(12),
                Height = 76
            };
            UiRadiusHelper.ApplyRoundedCorners(panel, 8);
            panel.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(226, 232, 240), 1f);
                using var path = UiRadiusHelper.CreateRoundedPath(new Rectangle(0, 0, panel.Width - 1, panel.Height - 1), 8);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawPath(pen, path);
            };

            // Left Column: Icon and descriptive title
            var iconLabel = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI Emoji", 13f),
                ForeColor = iconColor,
                BackColor = iconBg,
                Size = new Size(38, 38),
                Location = new Point(14, 19),
                TextAlign = ContentAlignment.MiddleCenter
            };
            UiRadiusHelper.MakeCircularAvatar(iconLabel);

            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 116, 139), // #64748B
                Location = new Point(60, 22),
                AutoSize = true
            };

            // Right Column: Primary metric value cleanly aligned to the far right (#0F172A, Bold)
            valueLabel.Parent = null;
            valueLabel.Font = new Font("Segoe UI", 16f, FontStyle.Bold);
            valueLabel.ForeColor = Color.FromArgb(15, 23, 42); // #0F172A
            valueLabel.AutoSize = true;
            valueLabel.TextAlign = ContentAlignment.MiddleRight;

            void LayoutTile()
            {
                if (panel.Width <= 0) return;
                int valWidth = valueLabel.PreferredWidth;
                valueLabel.Location = new Point(panel.Width - valWidth - 16, (panel.Height - valueLabel.Height) / 2);
                titleLabel.Location = new Point(60, (panel.Height - titleLabel.Height) / 2);
                iconLabel.Location = new Point(14, (panel.Height - iconLabel.Height) / 2);
            }

            panel.Controls.Add(iconLabel);
            panel.Controls.Add(titleLabel);
            panel.Controls.Add(valueLabel);

            panel.SizeChanged += (_, _) => LayoutTile();
            LayoutTile();

            return panel;
        }

        private void RequestNavigation(string module)
        {
            if (NavigationRequested is not null)
            {
                NavigationRequested.Invoke(module);
                return;
            }

            if (FindForm() is MainForm form)
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

                lblStat1Value.Text = $"₱{summary.PipelineValue:N2}";
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
                    valCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    valCol.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                if (gridRecent.Columns["Stage"] is DataGridViewColumn stageCol)
                {
                    stageCol.HeaderText = "STAGE";
                    stageCol.FillWeight = 95;
                    stageCol.MinimumWidth = 70;
                    stageCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    stageCol.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
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

            // Minimalist Status Indicator (Strictly No Badges/Pills)
            if (gridRecent.Columns[e.ColumnIndex].Name == "Stage" && e.Value != null)
            {
                string stage = e.Value.ToString() ?? "";
                UiGridHelper.PaintStatusIndicator(gridRecent, e, stage, center: true);
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
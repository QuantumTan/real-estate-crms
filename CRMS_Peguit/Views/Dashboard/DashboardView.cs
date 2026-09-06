using System;
using System.Drawing.Drawing2D;
using System.Linq;
using CRMS_Peguit.domain.entities;
using CRMS_Peguit.winforms.Controllers;
using CRMS_Peguit.winforms.Models.Services;

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

            // Recent grid styling
            gridRecent.EnableHeadersVisualStyles = false;
            gridRecent.GridColor = Color.FromArgb(241, 245, 249);
            gridRecent.RowTemplate.Height = 46;
            gridRecent.DefaultCellStyle.BackColor = Color.White;
            gridRecent.DefaultCellStyle.ForeColor = Color.FromArgb(15, 23, 42);
            gridRecent.DefaultCellStyle.SelectionBackColor = Color.FromArgb(241, 245, 249);
            gridRecent.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
            gridRecent.DefaultCellStyle.Font = new Font("Segoe UI", 9f);
            gridRecent.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            gridRecent.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(100, 116, 139);
            gridRecent.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            gridRecent.ColumnHeadersHeight = 38;

            gridRecent.CellPainting += GridRecent_CellPainting;
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
                using var customerCtrl = new CustomerController();
                using var propertyCtrl = new PropertyController();
                using var leadCtrl = new LeadController();
                using var dealCtrl = new DealController();

                var customers = customerCtrl.GetAll();
                var properties = propertyCtrl.GetAll();
                var leads = leadCtrl.GetAll();
                var deals = dealCtrl.GetAll();
                var agents = propertyCtrl.GetAgents();

                int totalCustomers = customers.Count;
                int activeProps = properties.Count(p => string.Equals(p.Status, "available", StringComparison.OrdinalIgnoreCase));
                int qualifiedLeads = leads.Count(l => string.Equals(l.Stage, "qualified", StringComparison.OrdinalIgnoreCase));
                int totalDeals = deals.Count;
                decimal pipelineSum = deals.Sum(d => d.Value);

                kpiCustomers.SetValue(totalCustomers);
                kpiProperties.SetValue(activeProps);
                kpiLeads.SetValue(qualifiedLeads);
                kpiDeals.SetValue(totalDeals);

                lblStat1Value.Text = $"${pipelineSum:N0}";
                lblStat2Value.Text = $"{activeProps} listings";
                lblStat3Value.Text = $"{agents.Count} agents";

                // Populate recent leads
                gridRecent.Columns.Clear();
                gridRecent.DataSource = leads
                    .Take(10)
                    .Select(l => new
                    {
                        Lead = l.FullName,
                        Email = string.IsNullOrWhiteSpace(l.Email) ? "-" : l.Email,
                        Source = string.IsNullOrWhiteSpace(l.Source) ? "Website" : l.Source,
                        Value = l.ExpectedValue.HasValue ? $"${l.ExpectedValue.Value:N0}" : "-",
                        Stage = l.Stage.ToUpper()
                    })
                    .ToList();

                if (gridRecent.Columns["Lead"] is DataGridViewColumn leadCol)
                {
                    leadCol.HeaderText = "LEAD NAME";
                    leadCol.FillWeight = 140;
                }
                if (gridRecent.Columns["Email"] is DataGridViewColumn emailCol)
                {
                    emailCol.HeaderText = "EMAIL";
                    emailCol.FillWeight = 140;
                }
                if (gridRecent.Columns["Source"] is DataGridViewColumn srcCol)
                {
                    srcCol.HeaderText = "SOURCE";
                    srcCol.FillWeight = 90;
                }
                if (gridRecent.Columns["Value"] is DataGridViewColumn valCol)
                {
                    valCol.HeaderText = "VALUE";
                    valCol.FillWeight = 90;
                }
                if (gridRecent.Columns["Stage"] is DataGridViewColumn stageCol)
                {
                    stageCol.HeaderText = "STAGE";
                    stageCol.FillWeight = 95;
                }
            }
            catch
            {
                // Fallback graceful load if connection is offline
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
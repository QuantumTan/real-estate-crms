using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CRMS_Peguit.winforms.Auth;
using CRMS_Peguit.winforms.Controllers;
using CRMS_Peguit.winforms.Models.Analytics;
using CRMS_Peguit.winforms.Models.Services;
using Color = System.Drawing.Color;

namespace CRMS_Peguit.winforms.Views.Reports
{
    public partial class ReportsView : UserControl
    {
        private enum ViewDisplayMode { Both, ChartsOnly, TableOnly }

        private readonly ReportsController _controller;
        private object? _currentData;
        private ReportHeader? _currentHeader;
        private ViewDisplayMode _viewMode = ViewDisplayMode.Both;

        public event Action<string>? NavigationRequested;

        public ReportsView()
        {
            InitializeComponent();
            _controller = new ReportsController();

            if (!RbacService.HasFullOversight)
            {
                pnlTop.Visible = false;
                pnlFilters.Visible = false;
                pnlCharts.Visible = false;
                pnlGrid.Visible = false;
                lblAccessDenied.Visible = true;
                lblAccessDenied.BringToFront();
                return;
            }

            SetupUI();
            LoadDropdowns();
            WireEvents();

            // Run initial report on load
            this.Load += (_, _) =>
            {
                LayoutReportControls();
                BtnRunReport_Click(this, EventArgs.Empty);
            };
        }

        private void SetupUI()
        {
            UiGridHelper.ApplyModernGridStyle(gridData, 44);

            UiRadiusHelper.StyleCard(pnlChartCard1, 10);
            UiRadiusHelper.StyleCard(pnlChartCard2, 10);

            btnRunReport.BackColor = Theme.Primary;
            btnRunReport.ForeColor = Theme.Surface;
            btnExportCsv.BackColor = Theme.Surface;
            btnExportCsv.ForeColor = Theme.TextPrimary;
            btnExportPdf.BackColor = Theme.Surface;
            btnExportPdf.ForeColor = Theme.TextPrimary;

            btnExportCsv.Enabled = false;
            btnExportPdf.Enabled = false;

            ConfigurePlot(plotReport1);
            ConfigurePlot(plotReport2);

            UpdateViewModeButtons();
        }

        private void ConfigurePlot(ScottPlot.WinForms.FormsPlot plot)
        {
            if (plot == null) return;
            try
            {
                plot.UserInputProcessor.Disable();
                plot.Plot.FigureBackground.Color = ScottPlot.Color.FromColor(Color.White);
                plot.Plot.DataBackground.Color = ScottPlot.Color.FromColor(Color.White);
                plot.Plot.Axes.Color(ScottPlot.Color.FromColor(Theme.TextSecondary));
                plot.Plot.Axes.Bottom.MinimumSize = 45;
                plot.Plot.Axes.Left.MinimumSize = 40;
            }
            catch { }
        }

        private void LoadDropdowns()
        {
            cboReportType.Items.AddRange(new object[]
            {
                "Sales & Revenue Report",
                "Commission & Payout Settlement Report",
                "Property Inventory & Absorption Report",
                "Lead Pipeline & Conversion Report",
                "Support Ticket Resolution & SLA Report",
                "Agent Productivity & Performance Report"
            });
            cboReportType.SelectedIndex = 0;

            cboDateRange.Items.AddRange(new object[]
            {
                "This Month",
                "This Quarter",
                "This Year",
                "Past 12 Months",
                "All Time",
                "Custom"
            });
            cboDateRange.SelectedIndex = 2; // Default to "This Year" for rich graphical view

            var agents = _controller.GetAgentList();
            cboAgentFilter.Items.Add(new AgentPickerItem(0, "All Agents", ""));
            foreach (var a in agents)
                cboAgentFilter.Items.Add(a);
            cboAgentFilter.SelectedIndex = 0;

            UpdateSecondaryFilter();
        }

        private void WireEvents()
        {
            cboReportType.SelectedIndexChanged += (s, e) =>
            {
                UpdateSecondaryFilter();
                btnExportCsv.Text = "Export CSV";
                btnExportPdf.Text = "Export PDF";
            };
            cboDateRange.SelectedIndexChanged += (s, e) =>
            {
                bool isCustom = cboDateRange.SelectedItem?.ToString() == "Custom";
                dtpStart.Visible = isCustom;
                lblTo.Visible = isCustom;
                dtpEnd.Visible = isCustom;
            };

            btnRunReport.Click += BtnRunReport_Click;
            btnExportCsv.Click += BtnExportCsv_Click;
            btnExportPdf.Click += BtnExportPdf_Click;

            btnViewBoth.Click += (_, _) => SetViewMode(ViewDisplayMode.Both);
            btnViewCharts.Click += (_, _) => SetViewMode(ViewDisplayMode.ChartsOnly);
            btnViewTable.Click += (_, _) => SetViewMode(ViewDisplayMode.TableOnly);

            btnGoToAnalytics.Click += (_, _) =>
            {
                if (ParentForm is MainForm main)
                {
                    main.NavigateTo("Analytics");
                }
                else
                {
                    NavigationRequested?.Invoke("Analytics");
                }
            };

            this.Resize += (_, _) => LayoutReportControls();
            pnlScrollableContent.Resize += (_, _) => LayoutReportControls();
        }

        private void SetViewMode(ViewDisplayMode mode)
        {
            _viewMode = mode;
            UpdateViewModeButtons();
            LayoutReportControls();
        }

        private void UpdateViewModeButtons()
        {
            btnViewBoth.BackColor = _viewMode == ViewDisplayMode.Both ? Theme.PrimaryLight : Color.White;
            btnViewBoth.ForeColor = _viewMode == ViewDisplayMode.Both ? Theme.Primary : Theme.TextSecondary;

            btnViewCharts.BackColor = _viewMode == ViewDisplayMode.ChartsOnly ? Theme.PrimaryLight : Color.White;
            btnViewCharts.ForeColor = _viewMode == ViewDisplayMode.ChartsOnly ? Theme.Primary : Theme.TextSecondary;

            btnViewTable.BackColor = _viewMode == ViewDisplayMode.TableOnly ? Theme.PrimaryLight : Color.White;
            btnViewTable.ForeColor = _viewMode == ViewDisplayMode.TableOnly ? Theme.Primary : Theme.TextSecondary;
        }

        private void LayoutReportControls()
        {
            if (pnlScrollableContent.ClientSize.Width <= 0) return;

            int scrollX = pnlScrollableContent.AutoScrollPosition.X;
            int scrollY = pnlScrollableContent.AutoScrollPosition.Y;
            pnlScrollableContent.AutoScrollPosition = Point.Empty;

            int containerWidth = pnlScrollableContent.ClientSize.Width;
            int containerHeight = pnlScrollableContent.ClientSize.Height;

            int totalContentHeight = 0;

            switch (_viewMode)
            {
                case ViewDisplayMode.Both:
                {
                    pnlCharts.Visible = true;
                    pnlGrid.Visible = true;

                    int chartHeight = 285;
                    pnlCharts.Location = new Point(0, 0);
                    pnlCharts.Size = new Size(containerWidth, chartHeight);

                    int gridHeight = Math.Max(380, containerHeight - chartHeight);
                    pnlGrid.Location = new Point(0, pnlCharts.Bottom);
                    pnlGrid.Size = new Size(containerWidth, gridHeight);

                    totalContentHeight = chartHeight + gridHeight;
                    break;
                }
                case ViewDisplayMode.ChartsOnly:
                {
                    pnlCharts.Visible = true;
                    pnlGrid.Visible = false;

                    // Prevent charts from stretching to abnormal vertical heights when "Chart Only" is active
                    // Cap with reasonable MinHeight (280px) and MaxHeight (480px)
                    int chartHeight = Math.Clamp(containerHeight > 0 ? containerHeight : 380, 280, 480);
                    pnlCharts.Location = new Point(0, 0);
                    pnlCharts.Size = new Size(containerWidth, chartHeight);

                    totalContentHeight = chartHeight;
                    break;
                }
                case ViewDisplayMode.TableOnly:
                {
                    pnlCharts.Visible = false;
                    pnlGrid.Visible = true;

                    int gridHeight = Math.Max(400, containerHeight);
                    pnlGrid.Location = new Point(0, 0);
                    pnlGrid.Size = new Size(containerWidth, gridHeight);

                    totalContentHeight = gridHeight;
                    break;
                }
            }

            // Layout child chart cards inside pnlCharts if visible
            if (pnlCharts.Visible && pnlCharts.ClientSize.Width > 0)
            {
                int pad = 20;
                int totalChartWidth = pnlCharts.ClientSize.Width - (pad * 2);
                int cardWidth = Math.Max(280, (totalChartWidth - 16) / 2);
                int cardHeight = Math.Max(200, pnlCharts.ClientSize.Height - 20);

                pnlChartCard1.Location = new Point(pad, 10);
                pnlChartCard1.Size = new Size(cardWidth, cardHeight);

                pnlChartCard2.Location = new Point(pnlChartCard1.Right + 16, 10);
                pnlChartCard2.Size = new Size(cardWidth, cardHeight);
            }

            pnlScrollableContent.AutoScrollMinSize = new Size(0, totalContentHeight + 10);
            pnlScrollableContent.AutoScrollPosition = new Point(-scrollX, -scrollY);
        }

        private void UpdateSecondaryFilter()
        {
            var rpt = cboReportType.SelectedItem?.ToString() ?? "";
            cboSecondaryFilter.Items.Clear();

            if (rpt.Contains("Sales") || rpt.Contains("Property"))
            {
                lblSecondaryFilter.Visible = true;
                cboSecondaryFilter.Visible = true;
                lblSecondaryFilter.Text = "Property Type";
                cboSecondaryFilter.Items.AddRange(new object[] { "All Types", "House", "Condo", "Townhouse", "Commercial", "Lot" });
                cboSecondaryFilter.SelectedIndex = 0;
            }
            else if (rpt.Contains("Lead"))
            {
                lblSecondaryFilter.Visible = true;
                cboSecondaryFilter.Visible = true;
                lblSecondaryFilter.Text = "Source";
                cboSecondaryFilter.Items.AddRange(new object[] { "All Sources", "Website", "Referral", "Walk-in", "Social Media", "Cold Call" });
                cboSecondaryFilter.SelectedIndex = 0;
            }
            else if (rpt.Contains("Ticket"))
            {
                lblSecondaryFilter.Visible = true;
                cboSecondaryFilter.Visible = true;
                lblSecondaryFilter.Text = "Priority";
                cboSecondaryFilter.Items.AddRange(new object[] { "All Priorities", "Low", "Medium", "High", "Critical" });
                cboSecondaryFilter.SelectedIndex = 0;
            }
            else
            {
                lblSecondaryFilter.Visible = false;
                cboSecondaryFilter.Visible = false;
            }
        }

        private async void BtnRunReport_Click(object? sender, EventArgs e)
        {
            lblLoading.Visible = true;
            btnRunReport.Enabled = false;
            btnExportCsv.Enabled = false;
            btnExportPdf.Enabled = false;
            lblReportHeader.Text = "Generating report and analytical charts...";

            try
            {
                var rpt = cboReportType.SelectedItem?.ToString() ?? "Sales & Revenue Report";
                var dr = GetDateRange();
                int? agentId = (cboAgentFilter.SelectedItem as AgentPickerItem)?.UserId;
                if (agentId == 0) agentId = null;

                string? secFilter = cboSecondaryFilter.SelectedItem?.ToString();
                if (secFilter != null && secFilter.StartsWith("All ")) secFilter = null;

                _currentHeader = new ReportHeader
                {
                    ReportName = rpt,
                    DateRange = dr.Label == "Custom" ? $"{dr.Start:MMM dd, yyyy} - {dr.End:MMM dd, yyyy}" : dr.Label,
                    GeneratedBy = CurrentSession.CurrentUser?.FullName ?? "Admin",
                    GeneratedAt = DateTime.Now
                };

                object? data = null;

                await Task.Run(() =>
                {
                    if (rpt.Contains("Sales"))
                        data = _controller.GetSalesReport(dr, agentId, secFilter);
                    else if (rpt.Contains("Commission"))
                        data = _controller.GetCommissionReport(dr, agentId);
                    else if (rpt.Contains("Property"))
                        data = _controller.GetPropertyInventoryReport(dr, secFilter, null);
                    else if (rpt.Contains("Lead"))
                        data = _controller.GetLeadProgressReport(dr, agentId, secFilter);
                    else if (rpt.Contains("Ticket"))
                        data = _controller.GetTicketResolutionReport(dr, secFilter, null);
                    else if (rpt.Contains("Agent"))
                        data = _controller.GetAgentActivityReport(dr, agentId);
                });

                _currentData = data;
                gridData.DataSource = _currentData;
                FormatGrid(rpt);

                lblReportHeader.Text = $"{_currentHeader.ReportName} · {_currentHeader.DateRange} · Generated by {_currentHeader.GeneratedBy} at {_currentHeader.GeneratedAt:g}";
                
                bool isFinancialReport = rpt.Contains("Commission");
                if (isFinancialReport && !RbacService.CanExportFinancialSettlements)
                {
                    btnExportCsv.Enabled = false;
                    btnExportPdf.Enabled = false;
                    btnExportCsv.Text = "Export (Admin Only)";
                    btnExportPdf.Text = "Export (Admin Only)";
                }
                else
                {
                    btnExportCsv.Enabled = _currentData != null;
                    btnExportPdf.Enabled = _currentData != null;
                    btnExportCsv.Text = "Export CSV";
                    btnExportPdf.Text = "Export PDF";
                }

                // Render accompanying charts
                RenderReportCharts(rpt, _currentData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error running report: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                lblLoading.Visible = false;
                btnRunReport.Enabled = true;
            }
        }

        private void RenderReportCharts(string rptType, object? data)
        {
            plotReport1.Plot.Clear();
            plotReport2.Plot.Clear();

            ConfigurePlot(plotReport1);
            ConfigurePlot(plotReport2);

            if (data == null)
            {
                ShowPlotEmpty(plotReport1, "No data available");
                ShowPlotEmpty(plotReport2, "No data available");
                return;
            }

            if (rptType.Contains("Sales") && data is List<SalesReportRow> sales)
            {
                lblChart1Title.Text = "Sales Volume by Agent (₱ Millions)";
                lblChart2Title.Text = "Transactions by Pipeline Stage";

                if (sales.Count == 0)
                {
                    ShowPlotEmpty(plotReport1, "No sales in selected period");
                    ShowPlotEmpty(plotReport2, "No transactions in selected period");
                    return;
                }

                // Chart 1: Sales volume by agent
                var agentSales = sales
                    .GroupBy(r => string.IsNullOrWhiteSpace(r.AgentName) ? "Unassigned" : r.AgentName)
                    .Select(g => new { Name = g.Key, TotalM = (double)(g.Sum(r => r.DealValue) / 1_000_000m) })
                    .OrderByDescending(x => x.TotalM)
                    .Take(7)
                    .ToList();

                var bars1 = new List<ScottPlot.Bar>();
                var ticks1 = new List<ScottPlot.Tick>();
                for (int i = 0; i < agentSales.Count; i++)
                {
                    bars1.Add(new ScottPlot.Bar
                    {
                        Position = i,
                        Value = agentSales[i].TotalM,
                        FillColor = ScottPlot.Color.FromColor(Theme.Primary),
                        LineWidth = 1
                    });
                    ticks1.Add(new ScottPlot.Tick(i, agentSales[i].Name));
                }
                plotReport1.Plot.Add.Bars(bars1);
                plotReport1.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ticks1.ToArray());
                plotReport1.Plot.Axes.Bottom.TickLabelStyle.Rotation = -25;
                plotReport1.Plot.Axes.Bottom.TickLabelStyle.Alignment = ScottPlot.Alignment.MiddleRight;
                plotReport1.Plot.Axes.Bottom.MinimumSize = 65;
                plotReport1.Plot.Axes.Left.MinimumSize = 45;
                plotReport1.Plot.Axes.Margins(bottom: 0, left: 0.05);
                plotReport1.Refresh();

                // Chart 2: Transactions by stage
                var stageGroups = sales
                    .GroupBy(r => string.IsNullOrWhiteSpace(r.Stage) ? "Unknown" : r.Stage)
                    .Select(g => new { Stage = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                var stageColors = new[] { Theme.StatusSuccess, Theme.Primary, Theme.StatusPending, Theme.PrimaryDark, Theme.StatusAlert, Theme.StatusNeutral };
                var bars2 = new List<ScottPlot.Bar>();
                var ticks2 = new List<ScottPlot.Tick>();
                for (int i = 0; i < stageGroups.Count; i++)
                {
                    bars2.Add(new ScottPlot.Bar
                    {
                        Position = i,
                        Value = stageGroups[i].Count,
                        FillColor = ScottPlot.Color.FromColor(stageColors[i % stageColors.Length]),
                        LineWidth = 1
                    });
                    ticks2.Add(new ScottPlot.Tick(i, stageGroups[i].Stage));
                }
                plotReport2.Plot.Add.Bars(bars2);
                plotReport2.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ticks2.ToArray());
                plotReport2.Plot.Axes.Bottom.TickLabelStyle.Rotation = -25;
                plotReport2.Plot.Axes.Bottom.TickLabelStyle.Alignment = ScottPlot.Alignment.MiddleRight;
                plotReport2.Plot.Axes.Bottom.MinimumSize = 65;
                plotReport2.Plot.Axes.Left.MinimumSize = 45;
                plotReport2.Plot.Axes.Margins(bottom: 0, left: 0.05);
                plotReport2.Refresh();
            }
            else if (rptType.Contains("Commission") && data is List<CommissionReportRow> comms)
            {
                lblChart1Title.Text = "Commissions Earned by Agent (₱k)";
                lblChart2Title.Text = RbacService.CanViewBrokerageMargins
                    ? "Commission Revenue Split (₱k)"
                    : "Agent Commission Share (%)";

                if (comms.Count == 0)
                {
                    ShowPlotEmpty(plotReport1, "No commissions in selected period");
                    ShowPlotEmpty(plotReport2, "No commissions in selected period");
                    return;
                }

                // Chart 1: Commissions per agent
                var agentComms = comms
                    .GroupBy(r => string.IsNullOrWhiteSpace(r.AgentName) ? "Unassigned" : r.AgentName)
                    .Select(g => new { Name = g.Key, TotalK = (double)(g.Sum(r => r.AgentPayoutAmount) / 1_000m) })
                    .OrderByDescending(x => x.TotalK)
                    .Take(7)
                    .ToList();

                var bars1 = new List<ScottPlot.Bar>();
                var ticks1 = new List<ScottPlot.Tick>();
                for (int i = 0; i < agentComms.Count; i++)
                {
                    bars1.Add(new ScottPlot.Bar
                    {
                        Position = i,
                        Value = agentComms[i].TotalK,
                        FillColor = ScottPlot.Color.FromColor(Theme.StatusSuccess),
                        LineWidth = 1
                    });
                    ticks1.Add(new ScottPlot.Tick(i, agentComms[i].Name));
                }
                plotReport1.Plot.Add.Bars(bars1);
                plotReport1.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ticks1.ToArray());
                plotReport1.Plot.Axes.Bottom.TickLabelStyle.Rotation = -25;
                plotReport1.Plot.Axes.Bottom.TickLabelStyle.Alignment = ScottPlot.Alignment.MiddleRight;
                plotReport1.Plot.Axes.Bottom.MinimumSize = 65;
                plotReport1.Plot.Axes.Left.MinimumSize = 45;
                plotReport1.Plot.Axes.Margins(bottom: 0, left: 0.05);
                plotReport1.Refresh();

                // Chart 2: Payout vs Brokerage Split (Admin) OR Agent Commission Share (Manager)
                if (RbacService.CanViewBrokerageMargins)
                {
                    double totalPayoutK = (double)(comms.Sum(c => c.AgentPayoutAmount) / 1_000m);
                    double totalBrokerageK = (double)(comms.Sum(c => c.BrokerageRetainedAmount) / 1_000m);

                    var slices = new List<ScottPlot.PieSlice>
                    {
                        new ScottPlot.PieSlice { Value = Math.Max(0.01, totalPayoutK), FillColor = ScottPlot.Color.FromColor(Theme.StatusSuccess), Label = $"Agent Payouts ({totalPayoutK:N0}k)" },
                        new ScottPlot.PieSlice { Value = Math.Max(0.01, totalBrokerageK), FillColor = ScottPlot.Color.FromColor(Theme.Primary), Label = $"Brokerage Net ({totalBrokerageK:N0}k)" }
                    };
                    var pie = plotReport2.Plot.Add.Pie(slices);
                    pie.DonutFraction = 0.5;
                    pie.SliceLabelDistance = 1.35;
                    plotReport2.Plot.Axes.Frameless();
                    plotReport2.Plot.HideGrid();
                    plotReport2.Plot.Axes.SetLimits(-1.45, 1.45, -1.45, 1.45);
                    plotReport2.Refresh();
                }
                else
                {
                    var palette = new[] { Theme.Primary, Theme.StatusSuccess, Theme.StatusPending, Theme.PrimaryDark, Theme.StatusAlert };
                    var slices = new List<ScottPlot.PieSlice>();
                    for (int i = 0; i < agentComms.Count; i++)
                    {
                        slices.Add(new ScottPlot.PieSlice
                        {
                            Value = Math.Max(0.01, agentComms[i].TotalK),
                            FillColor = ScottPlot.Color.FromColor(palette[i % palette.Length]),
                            Label = $"{agentComms[i].Name} ({agentComms[i].TotalK:N0}k)"
                        });
                    }
                    var pie = plotReport2.Plot.Add.Pie(slices);
                    pie.DonutFraction = 0.5;
                    pie.SliceLabelDistance = 1.35;
                    plotReport2.Plot.Axes.Frameless();
                    plotReport2.Plot.HideGrid();
                    plotReport2.Plot.Axes.SetLimits(-1.45, 1.45, -1.45, 1.45);
                    plotReport2.Refresh();
                }
            }
            else if (rptType.Contains("Property") && data is List<PropertyInventoryReportRow> props)
            {
                lblChart1Title.Text = "Inventory by Property Type";
                lblChart2Title.Text = "Average Days on Market (DOM)";

                if (props.Count == 0)
                {
                    ShowPlotEmpty(plotReport1, "No properties in selected period");
                    ShowPlotEmpty(plotReport2, "No properties in selected period");
                    return;
                }

                // Chart 1: Inventory by Property Type
                var typeGroups = props
                    .GroupBy(p => string.IsNullOrWhiteSpace(p.PropertyType) ? "General" : p.PropertyType)
                    .Select(g => new { Type = g.Key, Count = g.Count(), AvgDom = g.Average(p => p.DaysOnMarket) })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                var bars1 = new List<ScottPlot.Bar>();
                var ticks1 = new List<ScottPlot.Tick>();
                for (int i = 0; i < typeGroups.Count; i++)
                {
                    bars1.Add(new ScottPlot.Bar
                    {
                        Position = i,
                        Value = typeGroups[i].Count,
                        FillColor = ScottPlot.Color.FromColor(Theme.Primary),
                        LineWidth = 1
                    });
                    ticks1.Add(new ScottPlot.Tick(i, typeGroups[i].Type));
                }
                plotReport1.Plot.Add.Bars(bars1);
                plotReport1.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ticks1.ToArray());
                plotReport1.Plot.Axes.Bottom.MinimumSize = 50;
                plotReport1.Plot.Axes.Left.MinimumSize = 45;
                plotReport1.Plot.Axes.Margins(bottom: 0);
                plotReport1.Refresh();

                // Chart 2: Average Days on Market
                var bars2 = new List<ScottPlot.Bar>();
                var ticks2 = new List<ScottPlot.Tick>();
                for (int i = 0; i < typeGroups.Count; i++)
                {
                    bars2.Add(new ScottPlot.Bar
                    {
                        Position = i,
                        Value = Math.Round(typeGroups[i].AvgDom, 1),
                        FillColor = ScottPlot.Color.FromColor(Theme.StatusPending),
                        LineWidth = 1
                    });
                    ticks2.Add(new ScottPlot.Tick(i, typeGroups[i].Type));
                }
                plotReport2.Plot.Add.Bars(bars2);
                plotReport2.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ticks2.ToArray());
                plotReport2.Plot.Axes.Bottom.MinimumSize = 50;
                plotReport2.Plot.Axes.Left.MinimumSize = 45;
                plotReport2.Plot.Axes.Margins(bottom: 0);
                plotReport2.Refresh();
            }
            else if (rptType.Contains("Lead") && data is List<LeadProgressRow> leads)
            {
                lblChart1Title.Text = "Leads by Acquisition Source";
                lblChart2Title.Text = "Pipeline Stage Breakdown";

                if (leads.Count == 0)
                {
                    ShowPlotEmpty(plotReport1, "No leads in selected period");
                    ShowPlotEmpty(plotReport2, "No leads in selected period");
                    return;
                }

                // Chart 1: Source
                var sources = leads
                    .GroupBy(r => string.IsNullOrWhiteSpace(r.Source) ? "Unknown" : r.Source)
                    .Select(g => new { Source = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(7)
                    .ToList();

                var bars1 = new List<ScottPlot.Bar>();
                var ticks1 = new List<ScottPlot.Tick>();
                for (int i = 0; i < sources.Count; i++)
                {
                    bars1.Add(new ScottPlot.Bar
                    {
                        Position = i,
                        Value = sources[i].Count,
                        FillColor = ScottPlot.Color.FromColor(Theme.PrimaryLight),
                        LineColor = ScottPlot.Color.FromColor(Theme.Primary),
                        LineWidth = 1
                    });
                    ticks1.Add(new ScottPlot.Tick(i, sources[i].Source));
                }
                plotReport1.Plot.Add.Bars(bars1);
                plotReport1.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ticks1.ToArray());
                plotReport1.Plot.Axes.Bottom.TickLabelStyle.Rotation = -25;
                plotReport1.Plot.Axes.Bottom.TickLabelStyle.Alignment = ScottPlot.Alignment.MiddleRight;
                plotReport1.Plot.Axes.Bottom.MinimumSize = 65;
                plotReport1.Plot.Axes.Left.MinimumSize = 45;
                plotReport1.Plot.Axes.Margins(bottom: 0, left: 0.05);
                plotReport1.Refresh();

                // Chart 2: Pipeline Stages
                var stages = leads
                    .GroupBy(r => string.IsNullOrWhiteSpace(r.Stage) ? "Unknown" : r.Stage)
                    .Select(g => new { Stage = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                var stageColors = new[] { Theme.StatusNeutral, Theme.StatusPending, Theme.Primary, Theme.StatusSuccess, Theme.StatusAlert };
                var bars2 = new List<ScottPlot.Bar>();
                var ticks2 = new List<ScottPlot.Tick>();
                for (int i = 0; i < stages.Count; i++)
                {
                    bars2.Add(new ScottPlot.Bar
                    {
                        Position = i,
                        Value = stages[i].Count,
                        FillColor = ScottPlot.Color.FromColor(stageColors[i % stageColors.Length]),
                        LineWidth = 1
                    });
                    ticks2.Add(new ScottPlot.Tick(i, stages[i].Stage));
                }
                plotReport2.Plot.Add.Bars(bars2);
                plotReport2.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ticks2.ToArray());
                plotReport2.Plot.Axes.Bottom.MinimumSize = 65;
                plotReport2.Plot.Axes.Left.MinimumSize = 45;
                plotReport2.Plot.Axes.Margins(bottom: 0, left: 0.05);
                plotReport2.Refresh();
            }
            else if (rptType.Contains("Ticket") && data is List<TicketResolutionRow> tickets)
            {
                lblChart1Title.Text = "Tickets by Priority";
                lblChart2Title.Text = "SLA Compliance Performance";

                if (tickets.Count == 0)
                {
                    ShowPlotEmpty(plotReport1, "No tickets in selected period");
                    ShowPlotEmpty(plotReport2, "No tickets in selected period");
                    return;
                }

                // Chart 1: Priority
                var priorities = tickets
                    .GroupBy(r => string.IsNullOrWhiteSpace(r.Priority) ? "Normal" : r.Priority)
                    .Select(g => new { Priority = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                var bars1 = new List<ScottPlot.Bar>();
                var ticks1 = new List<ScottPlot.Tick>();
                for (int i = 0; i < priorities.Count; i++)
                {
                    bars1.Add(new ScottPlot.Bar
                    {
                        Position = i,
                        Value = priorities[i].Count,
                        FillColor = ScottPlot.Color.FromColor(Theme.StatusAlert),
                        LineWidth = 1
                    });
                    ticks1.Add(new ScottPlot.Tick(i, priorities[i].Priority));
                }
                plotReport1.Plot.Add.Bars(bars1);
                plotReport1.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ticks1.ToArray());
                plotReport1.Plot.Axes.Bottom.MinimumSize = 50;
                plotReport1.Plot.Axes.Left.MinimumSize = 45;
                plotReport1.Plot.Axes.Margins(bottom: 0);
                plotReport1.Refresh();

                // Chart 2: SLA Met vs Missed
                int met = tickets.Count(t => t.SlaMet == "Yes");
                int missed = tickets.Count(t => t.SlaMet == "No");
                int pending = tickets.Count(t => t.SlaMet != "Yes" && t.SlaMet != "No");

                var slices = new List<ScottPlot.PieSlice>();
                if (met > 0) slices.Add(new ScottPlot.PieSlice { Value = met, FillColor = ScottPlot.Color.FromColor(Theme.StatusSuccess), Label = $"Met SLA ({met})" });
                if (missed > 0) slices.Add(new ScottPlot.PieSlice { Value = missed, FillColor = ScottPlot.Color.FromColor(Theme.StatusAlert), Label = $"Breached ({missed})" });
                if (pending > 0) slices.Add(new ScottPlot.PieSlice { Value = pending, FillColor = ScottPlot.Color.FromColor(Theme.StatusNeutral), Label = $"In Progress ({pending})" });

                var pie = plotReport2.Plot.Add.Pie(slices);
                pie.DonutFraction = 0.5;
                pie.SliceLabelDistance = 1.35;
                plotReport2.Plot.Axes.Frameless();
                plotReport2.Plot.HideGrid();
                plotReport2.Plot.Axes.SetLimits(-1.45, 1.45, -1.45, 1.45);
                plotReport2.Refresh();
            }
            else if (rptType.Contains("Agent") && data is List<AgentActivityRow> acts)
            {
                lblChart1Title.Text = "Sales Volume by Agent (₱ Millions)";
                lblChart2Title.Text = "Agency Operational Activity Breakdown";

                if (acts.Count == 0)
                {
                    ShowPlotEmpty(plotReport1, "No activities recorded in selected period");
                    ShowPlotEmpty(plotReport2, "No deals recorded in selected period");
                    return;
                }

                // Chart 1: Sales volume by agent
                var topVol = acts.OrderByDescending(a => a.TotalSalesVolume).Take(7).ToList();
                var bars1 = new List<ScottPlot.Bar>();
                var ticks1 = new List<ScottPlot.Tick>();
                for (int i = 0; i < topVol.Count; i++)
                {
                    bars1.Add(new ScottPlot.Bar
                    {
                        Position = i,
                        Value = (double)(topVol[i].TotalSalesVolume / 1_000_000m),
                        FillColor = ScottPlot.Color.FromColor(Theme.Primary),
                        LineWidth = 1
                    });
                    ticks1.Add(new ScottPlot.Tick(i, topVol[i].AgentName));
                }
                plotReport1.Plot.Add.Bars(bars1);
                plotReport1.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ticks1.ToArray());
                plotReport1.Plot.Axes.Bottom.TickLabelStyle.Rotation = -25;
                plotReport1.Plot.Axes.Bottom.TickLabelStyle.Alignment = ScottPlot.Alignment.MiddleRight;
                plotReport1.Plot.Axes.Bottom.MinimumSize = 65;
                plotReport1.Plot.Axes.Left.MinimumSize = 45;
                plotReport1.Plot.Axes.Margins(bottom: 0, left: 0.05);
                plotReport1.Refresh();

                // Chart 2: Operations breakdown
                int totalLeads = acts.Sum(a => a.ActiveLeads);
                int totalDeals = acts.Sum(a => a.DealsClosed);
                int totalFollowUps = acts.Sum(a => a.FollowUpsCompleted);
                int totalTickets = acts.Sum(a => a.TicketsResolved);

                var labels = new[] { "Leads", "Deals", "Follow-Ups", "Tickets" };
                var vals = new[] { totalLeads, totalDeals, totalFollowUps, totalTickets };
                var colors = new[] { Theme.Primary, Theme.StatusSuccess, Theme.StatusPending, Theme.PrimaryDark };

                var bars2 = new List<ScottPlot.Bar>();
                var ticks2 = new List<ScottPlot.Tick>();
                for (int i = 0; i < labels.Length; i++)
                {
                    bars2.Add(new ScottPlot.Bar
                    {
                        Position = i,
                        Value = vals[i],
                        FillColor = ScottPlot.Color.FromColor(colors[i]),
                        LineWidth = 1
                    });
                    ticks2.Add(new ScottPlot.Tick(i, labels[i]));
                }
                plotReport2.Plot.Add.Bars(bars2);
                plotReport2.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ticks2.ToArray());
                plotReport2.Plot.Axes.Margins(bottom: 0);
                plotReport2.Refresh();
            }
        }

        private void ShowPlotEmpty(ScottPlot.WinForms.FormsPlot plot, string message)
        {
            plot.Plot.Clear();
            var txt = plot.Plot.Add.Text(message, 0, 0);
            txt.LabelAlignment = ScottPlot.Alignment.MiddleCenter;
            txt.LabelFontColor = ScottPlot.Color.FromColor(Theme.TextSecondary);
            plot.Plot.Axes.Frameless();
            plot.Plot.HideGrid();
            plot.Refresh();
        }

        private DateRangeFilter GetDateRange()
        {
            string sel = cboDateRange.SelectedItem?.ToString() ?? "This Month";
            if (sel == "This Month") return DateRangeFilter.ThisMonth();
            if (sel == "This Quarter") return DateRangeFilter.ThisQuarter();
            if (sel == "This Year") return DateRangeFilter.ThisYear();
            if (sel == "Past 12 Months") return DateRangeFilter.Past12Months();
            if (sel == "All Time") return DateRangeFilter.AllTime();
            return DateRangeFilter.Custom(dtpStart.Value.Date, dtpEnd.Value.Date.AddDays(1).AddSeconds(-1));
        }

        private void FormatGrid(string rptType)
        {
            if (gridData.Columns.Count == 0) return;

            // Hide backward compatibility alias columns
            string[] hiddenCols = { "Value", "ClosedDate", "DaysInStage", "CommissionRate", "CommissionAmount", "LeadsWorked" };
            foreach (var colName in hiddenCols)
            {
                if (gridData.Columns.Contains(colName) && gridData.Columns[colName] != null)
                    gridData.Columns[colName]!.Visible = false;
            }

            // Hide internal brokerage retained margin from managers
            if (!RbacService.CanViewBrokerageMargins && gridData.Columns.Contains("BrokerageRetainedAmount") && gridData.Columns["BrokerageRetainedAmount"] != null)
            {
                gridData.Columns["BrokerageRetainedAmount"]!.Visible = false;
            }

            foreach (DataGridViewColumn col in gridData.Columns)
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                col.HeaderText = FormatGridHeader(col.Name);

                if (col.ValueType == typeof(decimal))
                {
                    if (col.Name.Contains("Percent") || col.Name.Contains("Rate"))
                    {
                        col.DefaultCellStyle.Format = "0.0'%'";
                    }
                    else
                    {
                        col.DefaultCellStyle.Format = "₱#,##0.00";
                    }
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                else if (col.ValueType == typeof(double))
                {
                    if (col.Name.Contains("Rate") || col.Name.Contains("Percent"))
                    {
                        col.DefaultCellStyle.Format = "0.0'%'";
                    }
                    else
                    {
                        col.DefaultCellStyle.Format = "N1";
                    }
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                else if (col.ValueType == typeof(int))
                {
                    col.DefaultCellStyle.Format = "N0";
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }
        }

        private static string FormatGridHeader(string propName)
        {
            return propName switch
            {
                "DealRef" => "Deal Ref",
                "CustomerName" => "Customer Name",
                "PropertyAddress" => "Property Address",
                "PropertyType" => "Property Type",
                "AgentName" => "Assigned Agent",
                "DealValue" => "Deal Value",
                "Commission" => "Commission",
                "GrossCommission" => "Gross Commission",
                "AgentSplitPercent" => "Agent Split %",
                "AgentPayoutAmount" => "Agent Payout",
                "BrokerageRetainedAmount" => "Brokerage Net",
                "SettlementStatus" => "Status",
                "ExpectedOrClosedDate" => "Closing / Expected Date",
                "PropertyRef" => "Property Ref",
                "ListingPrice" => "Listing Price",
                "ListingAgent" => "Listing Agent",
                "DaysOnMarket" => "Days on Market",
                "AssociatedDeals" => "Active Deals",
                "ListedDate" => "Listed Date",
                "LeadName" => "Lead Name",
                "EstimatedBudget" => "Est. Budget",
                "DaysInPipeline" => "Days in Pipeline",
                "ConvertedToCustomer" => "Converted",
                "CreatedDate" => "Created Date",
                "TicketNumber" => "Ticket #",
                "OpenedDate" => "Opened Date",
                "ResolvedDate" => "Resolved Date",
                "ResolutionTime" => "Resolution Time",
                "SlaMet" => "SLA Met",
                "ActiveLeads" => "Active Leads",
                "LeadsConverted" => "Converted Leads",
                "ConversionRate" => "Conv. Rate %",
                "DealsClosed" => "Deals Closed",
                "TotalSalesVolume" => "Sales Volume",
                "TotalCommissionEarned" => "Total Commission",
                "FollowUpsCompleted" => "Follow-Ups Done",
                "TicketsResolved" => "Tickets Resolved",
                _ => System.Text.RegularExpressions.Regex.Replace(propName, "([A-Z])", " $1").Trim()
            };
        }

        private void BtnExportCsv_Click(object? sender, EventArgs e)
        {
            if (_currentData == null || _currentHeader == null) return;
            var rpt = cboReportType.SelectedItem?.ToString() ?? "";
            if (rpt.Contains("Commission") && !RbacService.CanExportFinancialSettlements)
            {
                MessageBox.Show("Commission and financial settlement exports are restricted to Administrators.", "Access Restricted", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var sfd = new SaveFileDialog { Filter = "CSV Files|*.csv", FileName = $"{_currentHeader.ReportName.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.csv" };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                ExportDynamic(sfd.FileName, true);
                MessageBox.Show("Exported to CSV successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnExportPdf_Click(object? sender, EventArgs e)
        {
            if (_currentData == null || _currentHeader == null) return;
            var rpt = cboReportType.SelectedItem?.ToString() ?? "";
            if (rpt.Contains("Commission") && !RbacService.CanExportFinancialSettlements)
            {
                MessageBox.Show("Commission and financial settlement exports are restricted to Administrators.", "Access Restricted", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var sfd = new SaveFileDialog { Filter = "PDF Files|*.pdf", FileName = $"{_currentHeader.ReportName.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.pdf" };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                ExportDynamic(sfd.FileName, false);
                MessageBox.Show("Exported to PDF successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ExportDynamic(string filePath, bool isCsv)
        {
            if (_currentData == null || _currentHeader == null) return;
            var rpt = cboReportType.SelectedItem?.ToString() ?? "";

            if (rpt.Contains("Sales"))
            {
                if (isCsv) _controller.ExportToCsv((List<SalesReportRow>)_currentData, filePath, _currentHeader);
                else _controller.ExportToPdf((List<SalesReportRow>)_currentData, filePath, _currentHeader);
            }
            else if (rpt.Contains("Commission"))
            {
                if (isCsv) _controller.ExportToCsv((List<CommissionReportRow>)_currentData, filePath, _currentHeader);
                else _controller.ExportToPdf((List<CommissionReportRow>)_currentData, filePath, _currentHeader);
            }
            else if (rpt.Contains("Property"))
            {
                if (isCsv) _controller.ExportToCsv((List<PropertyInventoryReportRow>)_currentData, filePath, _currentHeader);
                else _controller.ExportToPdf((List<PropertyInventoryReportRow>)_currentData, filePath, _currentHeader);
            }
            else if (rpt.Contains("Lead"))
            {
                if (isCsv) _controller.ExportToCsv((List<LeadProgressRow>)_currentData, filePath, _currentHeader);
                else _controller.ExportToPdf((List<LeadProgressRow>)_currentData, filePath, _currentHeader);
            }
            else if (rpt.Contains("Ticket"))
            {
                if (isCsv) _controller.ExportToCsv((List<TicketResolutionRow>)_currentData, filePath, _currentHeader);
                else _controller.ExportToPdf((List<TicketResolutionRow>)_currentData, filePath, _currentHeader);
            }
            else if (rpt.Contains("Agent"))
            {
                if (isCsv) _controller.ExportToCsv((List<AgentActivityRow>)_currentData, filePath, _currentHeader);
                else _controller.ExportToPdf((List<AgentActivityRow>)_currentData, filePath, _currentHeader);
            }
        }
    }
}

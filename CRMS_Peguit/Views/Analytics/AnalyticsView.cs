using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CRMS_Peguit.winforms.Auth;
using CRMS_Peguit.winforms.Controllers;
using CRMS_Peguit.winforms.Controls;
using CRMS_Peguit.winforms.Models.Analytics;
using CRMS_Peguit.winforms.Models.Services;
using Color = System.Drawing.Color;
using Label = System.Windows.Forms.Label;
using FontStyle = System.Drawing.FontStyle;

namespace CRMS_Peguit.winforms.Views.Analytics
{
    public partial class AnalyticsView : UserControl
    {
        private readonly AnalyticsController _controller;
        private AnalyticsSnapshot? _currentSnapshot;

        public event Action<string>? NavigationRequested;

        public AnalyticsView()
        {
            InitializeComponent();
            _controller = new AnalyticsController();

            ApplyStyling();
            BindEvents();
            ApplyRoleBasedRendering();

            // Default to "This Year" or "Past 12 Months" to immediately display populated charts
            cboDateRange.SelectedIndex = 2; // "This Year" triggers ReloadSnapshot
        }

        private void ApplyStyling()
        {
            this.BackColor = Theme.Background;

            UiRadiusHelper.StyleCard(pnlChartDealsClosed, 12);
            UiRadiusHelper.StyleCard(pnlChartPipeline, 12);
            UiRadiusHelper.StyleCard(pnlChartWonVsLost, 12);
            UiRadiusHelper.StyleCard(pnlChartTickets, 12);
            UiRadiusHelper.StyleCard(pnlChartAgents, 12);
            UiRadiusHelper.StyleCard(pnlChartSources, 12);
            UiRadiusHelper.StyleCard(pnlRecentActivity, 12);

            UiRadiusHelper.StyleButton(btnExport, 8);
            btnExport.BackColor = Color.White;
            btnExport.ForeColor = Theme.Primary;
            btnExport.FlatAppearance.BorderColor = Theme.BorderAccessible;
            btnExport.FlatAppearance.BorderSize = 1;

            UiRadiusHelper.StyleButton(btnGoToReports, 8);

            ConfigurePlot(plotDealsClosed);
            ConfigurePlot(plotPipeline);
            ConfigurePlot(plotWonVsLost);
            ConfigurePlot(plotTickets);
            ConfigurePlot(plotAgents);
            ConfigurePlot(plotSources);
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

        private void BindEvents()
        {
            cboDateRange.SelectedIndexChanged += (_, _) => ReloadSnapshot();
            btnExport.Click += (_, _) => ExportCsv();

            btnGoToReports.Click += (_, _) =>
            {
                if (ParentForm is MainForm main)
                {
                    main.NavigateTo("Reports");
                }
                else
                {
                    NavigationRequested?.Invoke("Reports");
                }
            };

            pnlScrollableContent.Resize += (_, _) => AutoLayoutCharts();
            this.Resize += (_, _) => AutoLayoutCharts();
            this.Load += (_, _) => AutoLayoutCharts();
        }

        private void ApplyRoleBasedRendering()
        {
            if (RbacService.IsAgent && !RbacService.HasFullOversight)
            {
                lblTitle.Text = "My Performance Snapshot";
                lblSubtitle.Text = "Personal metrics and pipeline status across your assigned records";
                pnlChartAgents.Visible = false;
                pnlChartSources.Visible = false;
                btnExport.Visible = false;
                btnGoToReports.Visible = false;
            }
            else
            {
                lblTitle.Text = "Team Analytics Performance";
                lblSubtitle.Text = "Live agency-wide business intelligence, deal velocity, and operations";
                pnlChartAgents.Visible = true;
                pnlChartSources.Visible = true;
                btnExport.Visible = RbacService.CanExportData;
                btnGoToReports.Visible = CurrentSession.CanAccess("Reports");
            }

            AutoLayoutCharts();
        }

        private DateRangeFilter GetSelectedDateRange()
        {
            return (cboDateRange.SelectedItem?.ToString() ?? "This Year") switch
            {
                "This Month" => DateRangeFilter.ThisMonth(),
                "This Quarter" => DateRangeFilter.ThisQuarter(),
                "This Year" => DateRangeFilter.ThisYear(),
                "Past 12 Months" => DateRangeFilter.Past12Months(),
                "All Time" => DateRangeFilter.AllTime(),
                _ => DateRangeFilter.ThisYear()
            };
        }

        private void ReloadSnapshot()
        {
            try
            {
                var range = GetSelectedDateRange();
                _currentSnapshot = _controller.GetSnapshot(range);
                UpdateView(_currentSnapshot);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AnalyticsView.ReloadSnapshot] Error: {ex.Message}");
            }
        }

        private void UpdateView(AnalyticsSnapshot? snapshot)
        {
            if (snapshot == null) return;

            // 1. Update KPI cards with dual-metric intelligence
            kpiDealsClosed.SetValue(snapshot.TotalDealsClosed);
            kpiDealsClosed.SetSubtitle(snapshot.TotalSalesVolume > 0 ? $"Vol: ₱{snapshot.TotalSalesVolume:N0}" : "Closed in period");

            kpiCommission.SetValue(snapshot.TotalCommissionEarned.ToString("C0"));
            kpiCommission.SetSubtitle(snapshot.AverageDealSize > 0 ? $"Avg: ₱{snapshot.AverageDealSize:N0}" : "Net earned");

            kpiActiveLeads.SetValue(snapshot.ActiveLeads);
            kpiActiveLeads.SetSubtitle(snapshot.ActivePipelineValue > 0 ? $"Pipe: ₱{snapshot.ActivePipelineValue:N0}" : "In pipeline");

            kpiConversionRate.SetValue($"{snapshot.LeadConversionRate:F1}%");
            kpiConversionRate.SetSubtitle(snapshot.WinRate > 0 ? $"Win Rate: {snapshot.WinRate:F1}%" : "Leads to closed");

            kpiOpenTickets.SetValue(snapshot.OpenSupportTickets);
            kpiOpenTickets.SetSubtitle("Support queue");

            kpiAvgDays.SetValue($"{snapshot.AverageDaysToClose:F1}d");
            kpiAvgDays.SetSubtitle(snapshot.ActivePropertiesCount > 0 ? $"Active Listings: {snapshot.ActivePropertiesCount}" : "Contract lead time");

            // 2. Chart: Deals Closed Over Time
            RenderDealsOverTimeChart(snapshot.DealsOverTime);

            // 3. Chart: Lead Pipeline Funnel
            RenderLeadFunnelChart(snapshot.LeadFunnel);

            // 4. Chart: Deals Won vs. Lost
            RenderDealsWonVsLostChart(snapshot.DealsWonVsLost);

            // 5. Chart: Support Ticket Breakdown
            RenderTicketBreakdownChart(snapshot.TicketBreakdown);

            // 6. Chart: Top-Performing Agents (Manager/Admin only)
            if (pnlChartAgents.Visible && snapshot.TopAgents != null)
            {
                RenderTopAgentsChart(snapshot.TopAgents);
            }

            // 7. Chart: Lead Source Breakdown (Manager/Admin only)
            if (pnlChartSources.Visible && snapshot.LeadSourceBreakdown != null)
            {
                RenderLeadSourcesChart(snapshot.LeadSourceBreakdown);
            }

            // 8. Recent Activity Feed
            RenderRecentActivity(snapshot.RecentActivity);
        }

        private void AutoLayoutCharts()
        {
            if (pnlScrollableContent.ClientSize.Width <= 0) return;

            // Preserve and temporarily reset scroll offset during calculation to prevent coordinates shifting
            int scrollX = pnlScrollableContent.AutoScrollPosition.X;
            int scrollY = pnlScrollableContent.AutoScrollPosition.Y;
            pnlScrollableContent.AutoScrollPosition = Point.Empty;

            int containerWidth = pnlScrollableContent.ClientSize.Width - 40; // padding
            int padding = 16;

            // 1. Layout KPI cards at the top of scrollable host
            pnlKpi.Location = new Point(20, 12);
            pnlKpi.Size = new Size(containerWidth, 114);

            int y = pnlKpi.Bottom + 16;
            int maxY = y;

            bool showActivity = pnlRecentActivity.Visible;
            bool showAgents = pnlChartAgents.Visible;

            if (containerWidth >= 1200 && showActivity)
            {
                // 3-column layout: 2 columns for charts + 1 column for activity feed
                int activityWidth = 360;
                int chartWidth = (containerWidth - activityWidth - (padding * 2)) / 2;
                int chartHeight = 310;

                int x1 = 20;
                int x2 = x1 + chartWidth + padding;
                int x3 = x2 + chartWidth + padding;

                // Row 1
                PositionChart(pnlChartDealsClosed, plotDealsClosed, x1, y, chartWidth, chartHeight);
                PositionChart(pnlChartPipeline, plotPipeline, x2, y, chartWidth, chartHeight);

                pnlRecentActivity.Location = new Point(x3, y);
                pnlRecentActivity.Size = new Size(activityWidth, (chartHeight * 2) + padding);
                pnlActivityFeedList.Size = new Size(activityWidth - 32, pnlRecentActivity.Height - 65);

                // Row 2
                y += chartHeight + padding;
                PositionChart(pnlChartWonVsLost, plotWonVsLost, x1, y, chartWidth, chartHeight);
                PositionChart(pnlChartTickets, plotTickets, x2, y, chartWidth, chartHeight);

                maxY = Math.Max(pnlRecentActivity.Bottom, y + chartHeight);

                // Row 3
                if (showAgents)
                {
                    y += chartHeight + padding;
                    PositionChart(pnlChartAgents, plotAgents, x1, y, chartWidth, chartHeight);
                    PositionChart(pnlChartSources, plotSources, x2, y, chartWidth, chartHeight);
                    maxY = Math.Max(maxY, y + chartHeight);
                }
            }
            else
            {
                // 2-column or 1-column layout
                int cols = containerWidth >= 750 ? 2 : 1;
                int chartWidth = cols == 2 ? (containerWidth - padding) / 2 : containerWidth;
                int chartHeight = 300;

                int x1 = 20;
                int x2 = cols == 2 ? x1 + chartWidth + padding : x1;

                PositionChart(pnlChartDealsClosed, plotDealsClosed, x1, y, chartWidth, chartHeight);
                if (cols == 2)
                {
                    PositionChart(pnlChartPipeline, plotPipeline, x2, y, chartWidth, chartHeight);
                }
                else
                {
                    y += chartHeight + padding;
                    PositionChart(pnlChartPipeline, plotPipeline, x1, y, chartWidth, chartHeight);
                }

                y += chartHeight + padding;
                PositionChart(pnlChartWonVsLost, plotWonVsLost, x1, y, chartWidth, chartHeight);
                if (cols == 2)
                {
                    PositionChart(pnlChartTickets, plotTickets, x2, y, chartWidth, chartHeight);
                }
                else
                {
                    y += chartHeight + padding;
                    PositionChart(pnlChartTickets, plotTickets, x1, y, chartWidth, chartHeight);
                }

                if (showAgents)
                {
                    y += chartHeight + padding;
                    PositionChart(pnlChartAgents, plotAgents, x1, y, chartWidth, chartHeight);
                    if (cols == 2)
                    {
                        PositionChart(pnlChartSources, plotSources, x2, y, chartWidth, chartHeight);
                    }
                    else
                    {
                        y += chartHeight + padding;
                        PositionChart(pnlChartSources, plotSources, x1, y, chartWidth, chartHeight);
                    }
                }

                maxY = y + chartHeight;

                if (showActivity)
                {
                    y += chartHeight + padding;
                    pnlRecentActivity.Location = new Point(x1, y);
                    pnlRecentActivity.Size = new Size(cols == 2 ? containerWidth : chartWidth, 380);
                    pnlActivityFeedList.Size = new Size(pnlRecentActivity.Width - 32, pnlRecentActivity.Height - 65);
                    maxY = pnlRecentActivity.Bottom;
                }
            }

            pnlScrollableContent.AutoScrollMinSize = new Size(0, maxY + 24);
            pnlScrollableContent.AutoScrollPosition = new Point(-scrollX, -scrollY);
        }

        private void PositionChart(Panel panel, ScottPlot.WinForms.FormsPlot plot, int x, int y, int w, int h)
        {
            panel.Location = new Point(x, y);
            panel.Size = new Size(w, h);
            plot.Location = new Point(14, 44);
            plot.Size = new Size(Math.Max(100, w - 28), Math.Max(100, h - 56));
        }

        private void RenderDealsOverTimeChart(List<MonthlyMetric>? metrics)
        {
            plotDealsClosed.Plot.Clear();
            ConfigurePlot(plotDealsClosed);

            if (metrics == null || metrics.Count == 0)
            {
                ShowPlotEmpty(plotDealsClosed, "No closed deals recorded in range");
                return;
            }

            var bars = new List<ScottPlot.Bar>();
            var ticks = new List<ScottPlot.Tick>();

            for (int i = 0; i < metrics.Count; i++)
            {
                bars.Add(new ScottPlot.Bar
                {
                    Position = i,
                    Value = metrics[i].Count,
                    FillColor = ScottPlot.Color.FromColor(Theme.Primary),
                    LineColor = ScottPlot.Color.FromColor(Theme.PrimaryDark),
                    LineWidth = 1
                });
                ticks.Add(new ScottPlot.Tick(i, metrics[i].Month));
            }

            plotDealsClosed.Plot.Add.Bars(bars);
            var tickGen = new ScottPlot.TickGenerators.NumericManual(ticks.ToArray());
            plotDealsClosed.Plot.Axes.Bottom.TickGenerator = tickGen;
            plotDealsClosed.Plot.Axes.Bottom.TickLabelStyle.Rotation = -30;
            plotDealsClosed.Plot.Axes.Bottom.TickLabelStyle.Alignment = ScottPlot.Alignment.MiddleRight;
            plotDealsClosed.Plot.Axes.Bottom.MinimumSize = 60;
            plotDealsClosed.Plot.Axes.Left.MinimumSize = 45;
            plotDealsClosed.Plot.Axes.Margins(bottom: 0, left: 0.05);
            plotDealsClosed.Refresh();
        }

        private void RenderLeadFunnelChart(LeadFunnelData? funnel)
        {
            plotPipeline.Plot.Clear();
            ConfigurePlot(plotPipeline);

            if (funnel == null)
            {
                ShowPlotEmpty(plotPipeline, "No pipeline leads recorded");
                return;
            }

            var stages = new[] { "New", "Contacted", "Qualified", "Converted", "Lost" };
            var counts = new[] { funnel.New, funnel.Contacted, funnel.Qualified, funnel.Converted, funnel.Lost };
            var colors = new[] { Theme.StatusNeutral, Theme.StatusPending, Theme.Primary, Theme.StatusSuccess, Theme.StatusAlert };

            var bars = new List<ScottPlot.Bar>();
            var ticks = new List<ScottPlot.Tick>();

            for (int i = 0; i < stages.Length; i++)
            {
                bars.Add(new ScottPlot.Bar
                {
                    Position = i,
                    Value = counts[i],
                    FillColor = ScottPlot.Color.FromColor(colors[i]),
                    LineWidth = 1
                });
                ticks.Add(new ScottPlot.Tick(i, stages[i]));
            }

            plotPipeline.Plot.Add.Bars(bars);
            var tickGen = new ScottPlot.TickGenerators.NumericManual(ticks.ToArray());
            plotPipeline.Plot.Axes.Bottom.TickGenerator = tickGen;
            plotPipeline.Plot.Axes.Bottom.MinimumSize = 50;
            plotPipeline.Plot.Axes.Left.MinimumSize = 45;
            plotPipeline.Plot.Axes.Margins(bottom: 0);
            plotPipeline.Refresh();
        }

        private void RenderDealsWonVsLostChart(WonLostData? wonLost)
        {
            plotWonVsLost.Plot.Clear();
            ConfigurePlot(plotWonVsLost);

            if (wonLost == null || (wonLost.Won == 0 && wonLost.Lost == 0))
            {
                ShowPlotEmpty(plotWonVsLost, "No closed/lost deals in range");
                return;
            }

            var slices = new List<ScottPlot.PieSlice>
            {
                new ScottPlot.PieSlice
                {
                    Value = Math.Max(0.001, wonLost.Won),
                    FillColor = ScottPlot.Color.FromColor(Theme.StatusSuccess),
                    Label = $"Won ({wonLost.Won})"
                },
                new ScottPlot.PieSlice
                {
                    Value = Math.Max(0.001, wonLost.Lost),
                    FillColor = ScottPlot.Color.FromColor(Theme.StatusAlert),
                    Label = $"Lost ({wonLost.Lost})"
                }
            };

            var pie = plotWonVsLost.Plot.Add.Pie(slices);
            pie.DonutFraction = 0.5;
            pie.SliceLabelDistance = 1.35;
            plotWonVsLost.Plot.Axes.Frameless();
            plotWonVsLost.Plot.HideGrid();
            plotWonVsLost.Plot.Axes.SetLimits(-1.45, 1.45, -1.45, 1.45);
            plotWonVsLost.Refresh();
        }

        private void RenderTicketBreakdownChart(TicketBreakdownData? tickets)
        {
            plotTickets.Plot.Clear();
            ConfigurePlot(plotTickets);

            if (tickets == null)
            {
                ShowPlotEmpty(plotTickets, "No support tickets recorded");
                return;
            }

            var categories = new[] { "Open", "In Progress", "Resolved", "Overdue" };
            var counts = new[] { tickets.Open, tickets.InProgress, tickets.Resolved, tickets.Overdue };
            var colors = new[] { Theme.StatusPending, Theme.Primary, Theme.StatusSuccess, Theme.StatusAlert };

            var bars = new List<ScottPlot.Bar>();
            var ticks = new List<ScottPlot.Tick>();

            for (int i = 0; i < categories.Length; i++)
            {
                bars.Add(new ScottPlot.Bar
                {
                    Position = i,
                    Value = counts[i],
                    FillColor = ScottPlot.Color.FromColor(colors[i]),
                    LineWidth = 1
                });
                ticks.Add(new ScottPlot.Tick(i, categories[i]));
            }

            plotTickets.Plot.Add.Bars(bars);
            var tickGen = new ScottPlot.TickGenerators.NumericManual(ticks.ToArray());
            plotTickets.Plot.Axes.Bottom.TickGenerator = tickGen;
            plotTickets.Plot.Axes.Bottom.MinimumSize = 50;
            plotTickets.Plot.Axes.Left.MinimumSize = 45;
            plotTickets.Plot.Axes.Margins(bottom: 0);
            plotTickets.Refresh();
        }

        private void RenderTopAgentsChart(List<AgentPerformance> topAgents)
        {
            plotAgents.Plot.Clear();
            ConfigurePlot(plotAgents);

            if (topAgents == null || topAgents.Count == 0)
            {
                ShowPlotEmpty(plotAgents, "No agent performance data in range");
                return;
            }

            lblChartAgentsTitle.Text = "Top Agents by Sales Volume (₱ Millions)";

            var bars = new List<ScottPlot.Bar>();
            var ticks = new List<ScottPlot.Tick>();

            for (int i = 0; i < topAgents.Count; i++)
            {
                bars.Add(new ScottPlot.Bar
                {
                    Position = i,
                    Value = (double)(topAgents[i].TotalValue / 1_000_000m),
                    FillColor = ScottPlot.Color.FromColor(Theme.Primary),
                    LineWidth = 1
                });
                ticks.Add(new ScottPlot.Tick(i, topAgents[i].AgentName));
            }

            plotAgents.Plot.Add.Bars(bars);
            var tickGen = new ScottPlot.TickGenerators.NumericManual(ticks.ToArray());
            plotAgents.Plot.Axes.Bottom.TickGenerator = tickGen;
            plotAgents.Plot.Axes.Bottom.TickLabelStyle.Rotation = -30;
            plotAgents.Plot.Axes.Bottom.TickLabelStyle.Alignment = ScottPlot.Alignment.MiddleRight;
            plotAgents.Plot.Axes.Bottom.MinimumSize = 65;
            plotAgents.Plot.Axes.Left.MinimumSize = 45;
            plotAgents.Plot.Axes.Margins(bottom: 0, left: 0.05);
            plotAgents.Refresh();
        }

        private void RenderLeadSourcesChart(List<SourceMetric> sources)
        {
            plotSources.Plot.Clear();
            ConfigurePlot(plotSources);

            if (sources == null || sources.Count == 0)
            {
                ShowPlotEmpty(plotSources, "No lead sources recorded in range");
                return;
            }

            var bars = new List<ScottPlot.Bar>();
            var ticks = new List<ScottPlot.Tick>();

            for (int i = 0; i < sources.Count; i++)
            {
                bars.Add(new ScottPlot.Bar
                {
                    Position = i,
                    Value = sources[i].Count,
                    FillColor = ScottPlot.Color.FromColor(Theme.PrimaryLight),
                    LineColor = ScottPlot.Color.FromColor(Theme.Primary),
                    LineWidth = 1
                });
                ticks.Add(new ScottPlot.Tick(i, sources[i].Source));
            }

            plotSources.Plot.Add.Bars(bars);
            var tickGen = new ScottPlot.TickGenerators.NumericManual(ticks.ToArray());
            plotSources.Plot.Axes.Bottom.TickGenerator = tickGen;
            plotSources.Plot.Axes.Bottom.TickLabelStyle.Rotation = -30;
            plotSources.Plot.Axes.Bottom.TickLabelStyle.Alignment = ScottPlot.Alignment.MiddleRight;
            plotSources.Plot.Axes.Bottom.MinimumSize = 60;
            plotSources.Plot.Axes.Left.MinimumSize = 45;
            plotSources.Plot.Axes.Margins(bottom: 0, left: 0.05);
            plotSources.Refresh();
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

        private void RenderRecentActivity(List<ActivityFeedItem>? feed)
        {
            pnlActivityFeedList.Controls.Clear();
            if (feed == null || feed.Count == 0)
            {
                var lblEmpty = new Label
                {
                    Text = "No recent activity recorded.",
                    Font = new Font("Segoe UI", 9.5f),
                    ForeColor = Theme.TextSecondary,
                    AutoSize = true,
                    Padding = new Padding(10)
                };
                pnlActivityFeedList.Controls.Add(lblEmpty);
                return;
            }

            foreach (var item in feed)
            {
                var rowPanel = new Panel
                {
                    Width = Math.Max(280, pnlActivityFeedList.ClientSize.Width - 10),
                    Height = 48,
                    BackColor = Color.FromArgb(248, 250, 252),
                    Margin = new Padding(0, 0, 0, 6)
                };
                UiRadiusHelper.ApplyRoundedCorners(rowPanel, 8);

                var lblIcon = new Label
                {
                    Text = item.Icon,
                    Font = new Font("Segoe UI", 12f),
                    Size = new Size(32, 32),
                    Location = new Point(8, 8),
                    TextAlign = ContentAlignment.MiddleCenter
                };

                var lblDesc = new Label
                {
                    Text = item.Description,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Theme.TextPrimary,
                    Location = new Point(44, 6),
                    Size = new Size(rowPanel.Width - 52, 18),
                    AutoEllipsis = true
                };

                var lblTime = new Label
                {
                    Text = item.Timestamp.ToString("MMM dd, h:mm tt"),
                    Font = new Font("Segoe UI", 7.5F),
                    ForeColor = Theme.TextSecondary,
                    Location = new Point(44, 26),
                    Size = new Size(rowPanel.Width - 52, 16)
                };

                rowPanel.Controls.Add(lblIcon);
                rowPanel.Controls.Add(lblDesc);
                rowPanel.Controls.Add(lblTime);

                pnlActivityFeedList.Controls.Add(rowPanel);
            }
        }

        private void ExportCsv()
        {
            if (_currentSnapshot == null)
            {
                MessageBox.Show("No analytics data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "CSV File (*.csv)|*.csv",
                FileName = $"Analytics_Summary_{DateTime.Now:yyyyMMdd_HHmm}.csv"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"# NEXA CRM Analytics Summary");
                    sb.AppendLine($"# Date Range: {cboDateRange.SelectedItem}");
                    sb.AppendLine($"# Generated By: {CurrentSession.CurrentUser?.FullName ?? "System"}");
                    sb.AppendLine($"# Generated At: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    sb.AppendLine();
                    sb.AppendLine("Metric,Value");
                    sb.AppendLine($"Total Deals Closed,{_currentSnapshot.TotalDealsClosed}");
                    sb.AppendLine($"Total Sales Volume (₱),{_currentSnapshot.TotalSalesVolume:F2}");
                    sb.AppendLine($"Total Commission Earned (₱),{_currentSnapshot.TotalCommissionEarned:F2}");
                    sb.AppendLine($"Active Pipeline Value (₱),{_currentSnapshot.ActivePipelineValue:F2}");
                    sb.AppendLine($"Average Deal Size (₱),{_currentSnapshot.AverageDealSize:F2}");
                    sb.AppendLine($"Active Leads,{_currentSnapshot.ActiveLeads}");
                    sb.AppendLine($"Lead Conversion Rate (%),{_currentSnapshot.LeadConversionRate:F2}");
                    sb.AppendLine($"Win Rate (%),{_currentSnapshot.WinRate:F2}");
                    sb.AppendLine($"Open Support Tickets,{_currentSnapshot.OpenSupportTickets}");
                    sb.AppendLine($"Average Days to Close,{_currentSnapshot.AverageDaysToClose:F2}");
                    sb.AppendLine($"Active Properties Count,{_currentSnapshot.ActivePropertiesCount}");
                    sb.AppendLine($"Active Inventory Value (₱),{_currentSnapshot.ActiveInventoryValue:F2}");
                    sb.AppendLine();

                    if (_currentSnapshot.DealsOverTime.Count > 0)
                    {
                        sb.AppendLine("Period,Closed Deals,Total Value (₱)");
                        foreach (var m in _currentSnapshot.DealsOverTime)
                        {
                            sb.AppendLine($"\"{m.Month}\",{m.Count},{m.Value:F2}");
                        }
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString());
                    MessageBox.Show("Analytics summary exported successfully.", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to export: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}

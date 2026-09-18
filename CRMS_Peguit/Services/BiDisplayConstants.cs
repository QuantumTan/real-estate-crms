using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ScottPlot;
using ScottPlot.WinForms;
using Color = System.Drawing.Color;

namespace CRMS_Peguit.winforms.Models.Services
{
    /// <summary>
    /// Shared visual constants, number/date formatters, semantic status colors,
    /// and ScottPlot charting configurations for NEXA Business Intelligence surfaces
    /// (DashboardView, AnalyticsView, ReportsView).
    /// </summary>
    public static class BiDisplayConstants
    {
        // =========================================================================
        // 1. SEMANTIC STATUS & OUTCOME PALETTE
        // =========================================================================
        public static readonly Color StatusWon = Theme.StatusSuccess;          // Emerald (#059669) - Closed, Won, Converted, Resolved, Active
        public static readonly Color StatusPending = Theme.StatusPending;      // Amber (#D97706) - Contacted, In Progress, Offer, Contract, Review
        public static readonly Color StatusLost = Theme.StatusAlert;           // Rose (#DC2626) - Lost, Overdue, Breached, High, Urgent, Critical
        public static readonly Color StatusNeutral = Theme.StatusNeutral;      // Slate (#475569) - New, Unassigned, Low, Medium, Prospect
        public static readonly Color PrimaryAccent = Theme.Primary;            // Skyline Blue (#25679C)
        public static readonly Color SecondaryAccent = Theme.PrimaryDark;      // Pressed Azure (#0C4D82)
        public static readonly Color HighlightAccent = Color.FromArgb(139, 92, 246); // Violet / Purple (#8B5CF6)
        public static readonly Color SkyAccent = Color.FromArgb(14, 165, 233);       // Sky (#0EA5E9)

        // Tint backgrounds for status pills/badges (WCAG contrast compliant with dark text)
        public static readonly Color StatusWonBg = Color.FromArgb(220, 252, 231);     // Emerald 100
        public static readonly Color StatusPendingBg = Color.FromArgb(254, 243, 199); // Amber 100
        public static readonly Color StatusLostBg = Color.FromArgb(254, 226, 226);    // Rose 100
        public static readonly Color StatusNeutralBg = Color.FromArgb(241, 245, 249); // Slate 100
        public static readonly Color PrimaryTintBg = Color.FromArgb(224, 242, 254);   // Sky 100

        public static Color GetStatusColor(string? status)
        {
            if (string.IsNullOrWhiteSpace(status)) return StatusNeutral;
            string s = status.Trim().ToLowerInvariant();

            return s switch
            {
                "won" or "closed" or "converted" or "resolved" or "active" or "completed" or "available" or "yes" => StatusWon,
                "in progress" or "contacted" or "pending" or "pending review" or "offer" or "under contract" or "qualified" => StatusPending,
                "lost" or "overdue" or "breached" or "cancelled" or "critical" or "urgent" or "high" or "no" or "inactive" => StatusLost,
                _ => StatusNeutral
            };
        }

        public static Color GetStatusBgColor(string? status)
        {
            if (string.IsNullOrWhiteSpace(status)) return StatusNeutralBg;
            string s = status.Trim().ToLowerInvariant();

            return s switch
            {
                "won" or "closed" or "converted" or "resolved" or "active" or "completed" or "available" or "yes" => StatusWonBg,
                "in progress" or "contacted" or "pending" or "pending review" or "offer" or "under contract" or "qualified" => StatusPendingBg,
                "lost" or "overdue" or "breached" or "cancelled" or "critical" or "urgent" or "high" or "no" or "inactive" => StatusLostBg,
                _ => StatusNeutralBg
            };
        }

        public static Color GetPriorityColor(string? priority)
        {
            if (string.IsNullOrWhiteSpace(priority)) return StatusNeutral;
            string p = priority.Trim().ToLowerInvariant();

            return p switch
            {
                "critical" or "urgent" or "high" => StatusLost,
                "medium" => StatusPending,
                "low" => StatusNeutral,
                _ => PrimaryAccent
            };
        }

        // =========================================================================
        // 2. UNIFIED NUMBER & DATE FORMATTING
        // =========================================================================
        public static string FormatCurrency(decimal amount) => $"₱{amount:N2}";
        public static string FormatCurrency(double amount) => $"₱{amount:N2}";

        public static string FormatCompactCurrency(decimal amount)
        {
            if (Math.Abs(amount) >= 1_000_000m)
                return $"₱{amount / 1_000_000m:N1}M";
            if (Math.Abs(amount) >= 1_000m)
                return $"₱{amount / 1_000m:N0}k";
            return $"₱{amount:N0}";
        }

        public static string FormatPercent(double percent) => $"{percent:F1}%";
        public static string FormatCount(int count) => count.ToString("N0");

        public static string FormatDate(DateTime date) => date.ToString("MMM dd, yyyy");
        public static string FormatDateTime(DateTime date) => date.ToString("MMM dd, yyyy h:mm tt");
        public static string FormatTime(DateTime date) => date.ToString("h:mm tt");

        // =========================================================================
        // 3. SCOTTPLOT 5 STANDARDIZED CHART HELPERS
        // =========================================================================
        public static void ConfigureStandardPlot(FormsPlot? plot)
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
                plot.Plot.Grid.MajorLineColor = ScottPlot.Color.FromHex("#F1F5F9");
            }
            catch { }
        }

        public static void ShowPlotEmpty(FormsPlot plot, string message)
        {
            plot.Plot.Clear();
            ConfigureStandardPlot(plot);
            var txt = plot.Plot.Add.Text($"📊  {message}", 0, 0);
            txt.LabelAlignment = Alignment.MiddleCenter;
            txt.LabelFontSize = 13;
            txt.LabelFontColor = ScottPlot.Color.FromColor(Theme.TextSecondary);
            plot.Plot.Axes.Frameless();
            plot.Plot.HideGrid();
            plot.Plot.Axes.SetLimits(-1, 1, -1, 1);
            plot.Refresh();
        }

        /// <summary>
        /// Renders a trend over time as a proper LINE chart with markers per BI standards.
        /// </summary>
        public static void RenderTrendLinePlot(FormsPlot plot, List<(string label, double value)> data, Color? lineColor = null, Color? markerColor = null)
        {
            plot.Plot.Clear();
            ConfigureStandardPlot(plot);

            if (data == null || data.Count == 0)
            {
                ShowPlotEmpty(plot, "No trend data recorded for selected period");
                return;
            }

            var lineClr = lineColor ?? PrimaryAccent;
            var markClr = markerColor ?? PrimaryAccent;

            double[] xs = data.Select((_, i) => (double)i).ToArray();
            double[] ys = data.Select(d => d.value).ToArray();

            var scatter = plot.Plot.Add.Scatter(xs, ys);
            scatter.Color = ScottPlot.Color.FromColor(lineClr);
            scatter.LineWidth = 2.5f;
            scatter.MarkerSize = 8f;
            scatter.MarkerShape = MarkerShape.FilledCircle;
            scatter.MarkerFillColor = ScottPlot.Color.FromColor(markClr);

            // Shaded area under the line curve
            scatter.FillY = true;
            scatter.FillYColor = ScottPlot.Color.FromColor(Color.FromArgb(35, lineClr.R, lineClr.G, lineClr.B));

            var ticks = data.Select((d, i) => new ScottPlot.Tick(i, d.label)).ToArray();
            var tickGen = new ScottPlot.TickGenerators.NumericManual(ticks);
            plot.Plot.Axes.Bottom.TickGenerator = tickGen;
            plot.Plot.Axes.Bottom.TickLabelStyle.Rotation = -30;
            plot.Plot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleRight;
            plot.Plot.Axes.Bottom.MinimumSize = 55;
            plot.Plot.Axes.Left.MinimumSize = 45;

            double maxVal = ys.Length > 0 ? ys.Max() : 10;
            plot.Plot.Axes.SetLimits(-0.3, xs.Length - 0.7, 0, Math.Max(1.0, maxVal * 1.15));
            plot.Refresh();
        }

        /// <summary>
        /// Renders a part-of-whole Donut chart enforcing a max 5-slice limit with automatic "Other" bucket.
        /// </summary>
        public static void RenderDonutPlot(FormsPlot plot, IEnumerable<(string label, double value, Color color)> rawSlices, int maxSlices = 5)
        {
            plot.Plot.Clear();
            ConfigureStandardPlot(plot);

            var sliceList = rawSlices?.ToList() ?? new List<(string label, double value, Color color)>();
            if (sliceList.Count == 0 || sliceList.All(s => s.value <= 0.0001))
            {
                ShowPlotEmpty(plot, "No segmentation data in selected period");
                return;
            }

            var activeSlices = sliceList.Where(s => s.value > 0.0001).OrderByDescending(s => s.value).ToList();

            var slices = new List<PieSlice>();
            if (activeSlices.Count <= maxSlices)
            {
                foreach (var (lbl, val, col) in activeSlices)
                {
                    slices.Add(new PieSlice
                    {
                        Value = val,
                        FillColor = ScottPlot.Color.FromColor(col),
                        Label = $"{lbl} ({val:N0})"
                    });
                }
            }
            else
            {
                var top = activeSlices.Take(maxSlices - 1).ToList();
                var remaining = activeSlices.Skip(maxSlices - 1).ToList();
                double otherSum = remaining.Sum(r => r.value);

                foreach (var (lbl, val, col) in top)
                {
                    slices.Add(new PieSlice
                    {
                        Value = val,
                        FillColor = ScottPlot.Color.FromColor(col),
                        Label = $"{lbl} ({val:N0})"
                    });
                }

                if (otherSum > 0)
                {
                    slices.Add(new PieSlice
                    {
                        Value = otherSum,
                        FillColor = ScottPlot.Color.FromColor(StatusNeutral),
                        Label = $"Other ({otherSum:N0})"
                    });
                }
            }

            var pie = plot.Plot.Add.Pie(slices);
            pie.DonutFraction = 0.52;
            pie.SliceLabelDistance = 1.32;

            plot.Plot.Axes.Frameless();
            plot.Plot.HideGrid();
            plot.Plot.Axes.SetLimits(-1.45, 1.45, -1.45, 1.45);
            plot.Refresh();
        }

        /// <summary>
        /// Renders a categorical Bar comparison chart with consistent styling.
        /// </summary>
        public static void RenderBarPlot(FormsPlot plot, List<(string label, double value, Color color)> items, double rotation = 0)
        {
            plot.Plot.Clear();
            ConfigureStandardPlot(plot);

            if (items == null || items.Count == 0)
            {
                ShowPlotEmpty(plot, "No comparison data in selected period");
                return;
            }

            var bars = new List<Bar>();
            var ticks = new List<ScottPlot.Tick>();

            for (int i = 0; i < items.Count; i++)
            {
                bars.Add(new Bar
                {
                    Position = i,
                    Value = items[i].value,
                    FillColor = ScottPlot.Color.FromColor(items[i].color),
                    LineWidth = 1
                });
                ticks.Add(new ScottPlot.Tick(i, items[i].label));
            }

            plot.Plot.Add.Bars(bars);
            var tickGen = new ScottPlot.TickGenerators.NumericManual(ticks.ToArray());
            plot.Plot.Axes.Bottom.TickGenerator = tickGen;

            if (Math.Abs(rotation) > 0.01)
            {
                plot.Plot.Axes.Bottom.TickLabelStyle.Rotation = (float)rotation;
                plot.Plot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleRight;
                plot.Plot.Axes.Bottom.MinimumSize = 65;
            }
            else
            {
                plot.Plot.Axes.Bottom.MinimumSize = 48;
            }

            plot.Plot.Axes.Left.MinimumSize = 45;
            plot.Plot.Axes.Margins(bottom: 0, left: 0.05);
            plot.Refresh();
        }

        public static void RenderBarPlot(FormsPlot plot, string[] labels, double[] values, Color[]? barColors = null, double rotation = -25)
        {
            var items = new List<(string label, double value, Color color)>();
            for (int i = 0; i < labels.Length && i < values.Length; i++)
            {
                Color col = barColors != null && i < barColors.Length ? barColors[i] : PrimaryAccent;
                items.Add((labels[i], values[i], col));
            }
            RenderBarPlot(plot, items, rotation);
        }
    }
}

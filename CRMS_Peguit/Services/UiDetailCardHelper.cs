using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CRMS_Peguit.winforms.Models.Services
{
    /// <summary>
    /// Reusable UI builder helper for record detail dialogs (Customer, Lead, Property).
    /// Enforces consistent modern card styling, crisp borders, WCAG-compliant pill badges,
    /// and clean typography.
    /// </summary>
    public static class UiDetailCardHelper
    {
        public static readonly Color CardBg = Color.White;
        public static readonly Color BorderColor = Color.FromArgb(226, 232, 240);       // #E2E8F0
        public static readonly Color SectionTitleColor = Color.FromArgb(30, 41, 59);    // #1E293B
        public static readonly Color LabelMutedColor = Color.FromArgb(100, 116, 139);   // #64748B
        public static readonly Color ValueTextColor = Color.FromArgb(15, 23, 42);       // #0F172A
        public static readonly Color TileBg = Color.FromArgb(248, 250, 252);            // #F8FAFC
        public static readonly Color DividerColor = Color.FromArgb(241, 245, 249);      // #F1F5F9

        /// <summary>
        /// Creates a card panel with anti-aliased rounded corners and subtle border.
        /// </summary>
        public static Panel CreateCard(int radius = 10)
        {
            var panel = new Panel
            {
                BackColor = CardBg,
                Margin = new Padding(0, 0, 0, 16),
                Padding = new Padding(18, 16, 18, 18),
                Width = 660,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            panel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
                using var path = UiRadiusHelper.CreateRoundedPath(rect, radius);
                using var pen = new Pen(BorderColor, 1f);
                e.Graphics.DrawPath(pen, path);
            };

            UiRadiusHelper.ApplyRoundedCorners(panel, radius);
            return panel;
        }

        /// <summary>
        /// Adds a child control to a card panel docked to Top and calls SendToBack()
        /// so that Top-docked controls stack in natural top-to-bottom order.
        /// </summary>
        public static void AddControl(Panel card, Control control)
        {
            if (card is null || control is null) return;
            card.Controls.Add(control);
            control.SendToBack();
        }

        /// <summary>
        /// Creates a card header with section title and optional count badge.
        /// </summary>
        public static Panel CreateCardHeader(string title, string? countText = null)
        {
            var header = new Panel
            {
                Height = 32,
                Dock = DockStyle.Top,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 8)
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = SectionTitleColor,
                AutoSize = true,
                Location = new Point(0, 2)
            };
            header.Controls.Add(lblTitle);

            if (!string.IsNullOrWhiteSpace(countText))
            {
                var lblBadge = CreatePillBadge(countText, Color.FromArgb(241, 245, 249), LabelMutedColor);
                lblBadge.Location = new Point(lblTitle.Right + 10, 2);
                header.Controls.Add(lblBadge);
            }

            return header;
        }

        /// <summary>
        /// Creates a subtle horizontal divider line.
        /// </summary>
        public static Panel CreateDivider()
        {
            return new Panel
            {
                Height = 1,
                Dock = DockStyle.Top,
                BackColor = DividerColor,
                Margin = new Padding(0, 4, 0, 12)
            };
        }

        /// <summary>
        /// Creates a 2-column or 1-column key-value row.
        /// </summary>
        public static Panel CreateKeyValueRow(string label1, string value1, string? label2 = null, string? value2 = null)
        {
            var row = new Panel
            {
                Height = 52,
                Dock = DockStyle.Top,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 4)
            };

            // Left Column
            var lblCaption1 = new Label
            {
                Text = label1.ToUpperInvariant(),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = LabelMutedColor,
                Location = new Point(0, 4),
                AutoSize = true
            };
            var lblVal1 = new Label
            {
                Text = string.IsNullOrWhiteSpace(value1) ? "—" : value1,
                Font = new Font("Segoe UI", 10f, FontStyle.Regular),
                ForeColor = ValueTextColor,
                Location = new Point(0, 24),
                AutoSize = true,
                MaximumSize = new Size(300, 24)
            };
            row.Controls.Add(lblCaption1);
            row.Controls.Add(lblVal1);

            // Right Column (if present)
            if (label2 is not null)
            {
                var lblCaption2 = new Label
                {
                    Text = label2.ToUpperInvariant(),
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    ForeColor = LabelMutedColor,
                    Location = new Point(330, 4),
                    AutoSize = true
                };
                var lblVal2 = new Label
                {
                    Text = string.IsNullOrWhiteSpace(value2) ? "—" : value2,
                    Font = new Font("Segoe UI", 10f, FontStyle.Regular),
                    ForeColor = ValueTextColor,
                    Location = new Point(330, 24),
                    AutoSize = true,
                    MaximumSize = new Size(300, 24)
                };
                row.Controls.Add(lblCaption2);
                row.Controls.Add(lblVal2);
            }

            return row;
        }

        /// <summary>
        /// Creates an accessible, modern rounded pill badge.
        /// </summary>
        public static Label CreatePillBadge(string text, Color bg, Color fg, Color? border = null)
        {
            var badge = new Label
            {
                Text = text.ToUpperInvariant(),
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = fg,
                BackColor = bg,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Height = 24,
                Cursor = Cursors.Default
            };

            // Compute ideal width based on text
            using (var g = badge.CreateGraphics())
            {
                var sz = g.MeasureString(badge.Text, badge.Font);
                badge.Width = Math.Max(54, (int)sz.Width + 18);
            }

            badge.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                var rect = new Rectangle(0, 0, badge.Width - 1, badge.Height - 1);
                int radius = badge.Height / 2;
                using var path = UiRadiusHelper.CreateRoundedPath(rect, radius);
                using var fill = new SolidBrush(bg);
                e.Graphics.FillPath(fill, path);

                var stroke = border ?? Color.FromArgb(40, fg.R, fg.G, fg.B);
                using var pen = new Pen(stroke, 1f);
                e.Graphics.DrawPath(pen, path);

                using var textBrush = new SolidBrush(fg);
                using var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                e.Graphics.DrawString(badge.Text, badge.Font, textBrush, rect, sf);
            };

            return badge;
        }

        /// <summary>
        /// Returns standard background and foreground colors for status badges.
        /// </summary>
        public static (Color bg, Color fg, Color border) GetStatusColors(string? status)
        {
            var s = (status ?? string.Empty).Trim().ToLowerInvariant();
            switch (s)
            {
                case "active":
                case "available":
                case "converted":
                case "approved":
                    return (
                        Color.FromArgb(220, 252, 231), // #DCFCE7
                        Color.FromArgb(22, 101, 52),    // #166534
                        Color.FromArgb(134, 239, 172)   // #86EFAC
                    );

                case "contacted":
                case "qualified":
                case "pending":
                case "pending_review":
                case "in_progress":
                    return (
                        Color.FromArgb(254, 243, 199), // #FEF3C7
                        Color.FromArgb(180, 83, 9),     // #B45309
                        Color.FromArgb(253, 230, 138)   // #FDE68A
                    );

                case "inactive":
                case "lost":
                case "sold":
                case "rejected":
                case "archived":
                    return (
                        Color.FromArgb(254, 226, 226), // #FEE2E2
                        Color.FromArgb(153, 27, 27),    // #991B1B
                        Color.FromArgb(252, 165, 165)   // #FCA5A5
                    );

                default: // "new", neutral, unassigned
                    return (
                        Color.FromArgb(239, 246, 255), // #EFF6FF
                        Color.FromArgb(29, 78, 216),    // #1D4ED8
                        Color.FromArgb(191, 219, 254)   // #BFDBFE
                    );
            }
        }

        /// <summary>
        /// Returns standard colors for Priority badges.
        /// </summary>
        public static (Color bg, Color fg, Color border) GetPriorityColors(string? priority)
        {
            var p = (priority ?? string.Empty).Trim().ToLowerInvariant();
            switch (p)
            {
                case "high":
                case "urgent":
                    return (Color.FromArgb(254, 226, 226), Color.FromArgb(185, 28, 28), Color.FromArgb(252, 165, 165));
                case "medium":
                    return (Color.FromArgb(254, 243, 199), Color.FromArgb(180, 83, 9), Color.FromArgb(253, 230, 138));
                case "low":
                default:
                    return (Color.FromArgb(241, 245, 249), Color.FromArgb(71, 85, 105), Color.FromArgb(203, 213, 225));
            }
        }

        /// <summary>
        /// Extracts clean 1-2 character initials from a full name.
        /// </summary>
        public static string GetInitials(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "?";
            var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
            {
                return parts[0].Length >= 2
                    ? parts[0].Substring(0, 2).ToUpperInvariant()
                    : parts[0].ToUpperInvariant();
            }
            return (parts[0].Substring(0, 1) + parts[^1].Substring(0, 1)).ToUpperInvariant();
        }
    }
}

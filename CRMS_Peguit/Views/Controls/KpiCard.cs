using System.Drawing.Drawing2D;
using CRMS_Peguit.Models;
using CRMS_Peguit.winforms.Models.Services;

namespace CRMS_Peguit.winforms.Controls
{
    // A single clickable KPI tile: title, big number, and a Click event
    // the parent view uses to filter its grid.
    public class KpiCard : Panel
    {
        private readonly Label _lblValue;
        private readonly Label _lblTitle;
        public string FilterKey { get; }
        public bool IsSelected { get; private set; }

        private readonly Color _accentColor;

        public KpiCard() : this("KPI", "all", AzureTints.SkylineBlue)
        {
        }

        public KpiCard(string title, string filterKey, Color accentColor)
        {
            FilterKey = filterKey;
            _accentColor = accentColor;

            TabStop = true;
            Size = new Size(200, 90);
            BackColor = Theme.Surface;
            Cursor = Cursors.Hand;
            UiRadiusHelper.ApplyRoundedCorners(this, 12);

            _lblValue = new Label
            {
                Text = "0",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = accentColor,
                Location = new Point(16, 10),
                AutoSize = true,
                AutoEllipsis = true,
                Cursor = Cursors.Hand
            };

            _lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Theme.TextPrimary,
                Location = new Point(16, 56),
                AutoSize = true,
                AutoEllipsis = true,
                Cursor = Cursors.Hand
            };

            Controls.Add(_lblValue);
            Controls.Add(_lblTitle);

            // Bubble clicks from the child labels up to the card itself
            _lblValue.Click += (_, _) => OnClick(EventArgs.Empty);
            _lblTitle.Click += (_, _) => OnClick(EventArgs.Empty);

            // Hover effects (using AzureTints)
            MouseEnter += (_, _) => BackColor = AzureTints.WhisperTint;
            MouseLeave += (_, _) => BackColor = Theme.Surface;
            _lblValue.MouseEnter += (_, _) => BackColor = AzureTints.WhisperTint;
            _lblValue.MouseLeave += (_, _) => BackColor = Theme.Surface;
            _lblTitle.MouseEnter += (_, _) => BackColor = AzureTints.WhisperTint;
            _lblTitle.MouseLeave += (_, _) => BackColor = Theme.Surface;

            GotFocus += (_, _) => Invalidate();
            LostFocus += (_, _) => Invalidate();
            SizeChanged += (_, _) =>
            {
                _lblTitle.MaximumSize = new Size(Math.Max(50, Width - 64), 36);
            };

            Paint += KpiCard_Paint;
        }

        private static string GetDefaultIcon(string key)
        {
            return (key?.ToLowerInvariant()) switch
            {
                "customers" or "total" => "👥",
                "properties" or "active" => "🏢",
                "leads" => "◎",
                "deals" or "thismonth" => "💼",
                _ => "📊"
            };
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
            {
                OnClick(EventArgs.Empty);
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        public void SetValue(int value)
        {
            _lblValue.Text = value.ToString();
        }

        public void SetSelected(bool selected)
        {
            IsSelected = selected;
            Invalidate();
        }

        private void KpiCard_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Subtle card outer border
            using (var borderPen = new Pen(Color.FromArgb(226, 232, 240), 1f))
            using (var borderPath = UiRadiusHelper.CreateRoundedPath(new Rectangle(0, 0, Width - 1, Height - 1), 12))
            {
                e.Graphics.DrawPath(borderPen, borderPath);
            }

            // Top-right modern icon bubble
            int bubbleSize = 36;
            int bubbleX = Width - bubbleSize - 14;
            int bubbleY = 14;
            if (bubbleX > 80)
            {
                var bubbleRect = new Rectangle(bubbleX, bubbleY, bubbleSize, bubbleSize);

                using (var bubbleBg = new SolidBrush(Color.FromArgb(26, _accentColor.R, _accentColor.G, _accentColor.B)))
                {
                    e.Graphics.FillEllipse(bubbleBg, bubbleRect);
                }

                string icon = GetDefaultIcon(FilterKey);
                using var iconFont = new Font("Segoe UI", 12f);
                using var iconBrush = new SolidBrush(_accentColor);
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                e.Graphics.DrawString(icon, iconFont, iconBrush, bubbleRect, sf);
            }

            // Left accent bar
            int barWidth = IsSelected ? 6 : 4;
            using var brush = new SolidBrush(_accentColor);
            e.Graphics.FillRectangle(brush, 0, 0, barWidth, Height);

            if (IsSelected)
            {
                using var pen = new Pen(_accentColor, 2);
                using var path = UiRadiusHelper.CreateRoundedPath(new Rectangle(1, 1, Width - 2, Height - 2), 12);
                e.Graphics.DrawPath(pen, path);
            }

            if (Focused)
            {
                using var focusPen = new Pen(Theme.FocusBorder, 2);
                using var focusPath = UiRadiusHelper.CreateRoundedPath(new Rectangle(2, 2, Width - 5, Height - 5), 10);
                e.Graphics.DrawPath(focusPen, focusPath);
            }
        }
    }
}
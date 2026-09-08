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
                Cursor = Cursors.Hand
            };

            _lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Theme.TextPrimary,
                Location = new Point(16, 58),
                AutoSize = true,
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

            Paint += KpiCard_Paint;
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

            // Left accent bar, thicker when selected - avoids relying on
            // a full border color change so it stays readable either way.
            int barWidth = IsSelected ? 6 : 4;
            using var brush = new SolidBrush(_accentColor);
            e.Graphics.FillRectangle(brush, 0, 0, barWidth, Height);

            if (IsSelected)
            {
                using var pen = new Pen(_accentColor, 2);
                using var path = UiRadiusHelper.CreateRoundedPath(new Rectangle(1, 1, Width - 2, Height - 2), 12);
                e.Graphics.DrawPath(pen, path);
            }
        }
    }
}
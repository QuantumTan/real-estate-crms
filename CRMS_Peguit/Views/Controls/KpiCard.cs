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
            Size = new Size(200, 84);
            BackColor = Theme.Surface;
            Cursor = Cursors.Hand;
            UiRadiusHelper.ApplyRoundedCorners(this, 8);

            // Left Column: Metric title/label (SemiBold, muted slate #64748B, uppercase tracking)
            _lblTitle = new Label
            {
                Text = title.ToUpperInvariant(),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 116, 139), // #64748B
                AutoSize = false,
                AutoEllipsis = true,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Right Column: Primary metric value (Bold 20pt/28px, dark color #0F172A, HorizontalAlignment="Right")
            _lblValue = new Label
            {
                Text = "0",
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42), // #0F172A
                AutoSize = true,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleRight
            };

            Controls.Add(_lblTitle);
            Controls.Add(_lblValue);

            // Bubble clicks from child labels to the card itself
            _lblValue.Click += (_, _) => OnClick(EventArgs.Empty);
            _lblTitle.Click += (_, _) => OnClick(EventArgs.Empty);

            // Subtle modern hover tint
            MouseEnter += (_, _) => BackColor = Color.FromArgb(248, 250, 252);
            MouseLeave += (_, _) => BackColor = Theme.Surface;
            _lblValue.MouseEnter += (_, _) => BackColor = Color.FromArgb(248, 250, 252);
            _lblValue.MouseLeave += (_, _) => BackColor = Theme.Surface;
            _lblTitle.MouseEnter += (_, _) => BackColor = Color.FromArgb(248, 250, 252);
            _lblTitle.MouseLeave += (_, _) => BackColor = Theme.Surface;

            GotFocus += (_, _) => Invalidate();
            LostFocus += (_, _) => Invalidate();
            SizeChanged += (_, _) => LayoutCard();

            Paint += KpiCard_Paint;
            LayoutCard();
        }

        private void LayoutCard()
        {
            if (Width <= 0 || Height <= 0) return;

            // Measure value width
            int valueWidth = _lblValue.PreferredWidth;
            int rightPadding = 18;
            int leftPadding = 18;

            _lblValue.Location = new Point(Width - valueWidth - rightPadding, (Height - _lblValue.Height) / 2);

            int titleWidth = Math.Max(40, _lblValue.Left - leftPadding - 8);
            _lblTitle.Location = new Point(leftPadding, 12);
            _lblTitle.Size = new Size(titleWidth, Height - 24);
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
            LayoutCard();
        }

        public void SetSelected(bool selected)
        {
            IsSelected = selected;
            Invalidate();
        }

        private void KpiCard_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // All-around subtle border (#E2E8F0, 1px, CornerRadius 8) - No left accent bar!
            Color borderColor = IsSelected ? Color.FromArgb(14, 165, 233) : Color.FromArgb(226, 232, 240);
            float borderWidth = IsSelected ? 1.5f : 1f;

            using (var borderPen = new Pen(borderColor, borderWidth))
            using (var borderPath = UiRadiusHelper.CreateRoundedPath(new Rectangle(0, 0, Width - 1, Height - 1), 8))
            {
                e.Graphics.DrawPath(borderPen, borderPath);
            }

            if (Focused)
            {
                using var focusPen = new Pen(Theme.FocusBorder, 1.5f);
                using var focusPath = UiRadiusHelper.CreateRoundedPath(new Rectangle(2, 2, Width - 5, Height - 5), 6);
                e.Graphics.DrawPath(focusPen, focusPath);
            }
        }
    }
}
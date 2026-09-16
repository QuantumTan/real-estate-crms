using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace CRMS_Peguit.winforms.Models.Services
{
    public enum KpiIconType
    {
        None,
        Users,
        Building,
        Target,
        Briefcase,
        Ticket,
        Clock,
        Refresh,
        AlertTriangle,
        Currency
    }

    /// <summary>
    /// Utility for drawing clean, modern, anti-aliased vector icons (Lucide/Feather style)
    /// directly onto GDI+ graphics contexts without using emojis or font fallbacks.
    /// </summary>
    public static class UiIconHelper
    {
        public static void DrawIcon(Graphics g, KpiIconType icon, Rectangle bounds, Color color)
        {
            if (icon == KpiIconType.None || bounds.Width <= 0 || bounds.Height <= 0) return;

            var oldSmoothing = g.SmoothingMode;
            var oldPixel = g.PixelOffsetMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            float w = bounds.Width;
            float h = bounds.Height;
            float x = bounds.X;
            float y = bounds.Y;

            float stroke = Math.Max(1.5f, w / 12f);
            using var pen = new Pen(color, stroke)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };
            using var brush = new SolidBrush(color);

            switch (icon)
            {
                case KpiIconType.Users:
                    DrawUsersIcon(g, pen, brush, x, y, w, h);
                    break;

                case KpiIconType.Building:
                    DrawBuildingIcon(g, pen, brush, x, y, w, h);
                    break;

                case KpiIconType.Target:
                    DrawTargetIcon(g, pen, brush, x, y, w, h);
                    break;

                case KpiIconType.Briefcase:
                    DrawBriefcaseIcon(g, pen, brush, x, y, w, h);
                    break;

                case KpiIconType.Ticket:
                    DrawTicketIcon(g, pen, brush, x, y, w, h);
                    break;

                case KpiIconType.Clock:
                    DrawClockIcon(g, pen, brush, x, y, w, h);
                    break;

                case KpiIconType.Refresh:
                    DrawRefreshIcon(g, pen, brush, x, y, w, h);
                    break;

                case KpiIconType.AlertTriangle:
                    DrawAlertTriangleIcon(g, pen, brush, x, y, w, h);
                    break;

                case KpiIconType.Currency:
                    DrawCurrencyIcon(g, pen, brush, x, y, w, h);
                    break;
            }

            g.SmoothingMode = oldSmoothing;
            g.PixelOffsetMode = oldPixel;
        }

        private static void DrawUsersIcon(Graphics g, Pen pen, Brush brush, float x, float y, float w, float h)
        {
            // Primary user (centered slightly left)
            float cx = x + w * 0.42f;
            float headR = w * 0.16f;
            float headY = y + h * 0.22f;

            // Head 1
            g.DrawEllipse(pen, cx - headR, headY, headR * 2, headR * 2);

            // Body 1 (shoulder arc)
            using (var bodyPath = new GraphicsPath())
            {
                bodyPath.AddArc(cx - w * 0.28f, y + h * 0.48f, w * 0.56f, h * 0.48f, 195, 150);
                g.DrawPath(pen, bodyPath);
            }

            // Secondary user (offset to the right)
            float cx2 = x + w * 0.74f;
            float headR2 = w * 0.13f;
            float headY2 = y + h * 0.26f;

            // Head 2
            using (var head2Path = new GraphicsPath())
            {
                head2Path.AddArc(cx2 - headR2, headY2, headR2 * 2, headR2 * 2, -60, 200);
                g.DrawPath(pen, head2Path);
            }

            // Body 2
            using (var body2Path = new GraphicsPath())
            {
                body2Path.AddArc(cx2 - w * 0.24f, y + h * 0.52f, w * 0.48f, h * 0.44f, 240, 95);
                g.DrawPath(pen, body2Path);
            }
        }

        private static void DrawBuildingIcon(Graphics g, Pen pen, Brush brush, float x, float y, float w, float h)
        {
            float bx = x + w * 0.18f;
            float by = y + h * 0.14f;
            float bw = w * 0.64f;
            float bh = h * 0.76f;

            // Building outline
            g.DrawRectangle(pen, bx, by, bw, bh);

            // Roof cap
            g.DrawLine(pen, bx - w * 0.06f, by, bx + bw + w * 0.06f, by);

            // 4 Windows
            float winW = bw * 0.22f;
            float winH = bh * 0.16f;
            float col1 = bx + bw * 0.18f;
            float col2 = bx + bw * 0.60f;
            float row1 = by + bh * 0.16f;
            float row2 = by + bh * 0.42f;

            g.FillRectangle(brush, col1, row1, winW, winH);
            g.FillRectangle(brush, col2, row1, winW, winH);
            g.FillRectangle(brush, col1, row2, winW, winH);
            g.FillRectangle(brush, col2, row2, winW, winH);

            // Door
            float doorW = bw * 0.32f;
            float doorH = bh * 0.26f;
            float doorX = bx + (bw - doorW) / 2f;
            float doorY = by + bh - doorH;
            g.DrawRectangle(pen, doorX, doorY, doorW, doorH);
        }

        private static void DrawTargetIcon(Graphics g, Pen pen, Brush brush, float x, float y, float w, float h)
        {
            float cx = x + w * 0.5f;
            float cy = y + h * 0.5f;

            // Outer ring
            float r1 = w * 0.40f;
            g.DrawEllipse(pen, cx - r1, cy - r1, r1 * 2, r1 * 2);

            // Inner ring
            float r2 = w * 0.22f;
            g.DrawEllipse(pen, cx - r2, cy - r2, r2 * 2, r2 * 2);

            // Center dot
            float r3 = w * 0.08f;
            g.FillEllipse(brush, cx - r3, cy - r3, r3 * 2, r3 * 2);

            // Crosshair tick marks
            float tickLen = w * 0.10f;
            g.DrawLine(pen, cx, cy - r1 - tickLen * 0.3f, cx, cy - r1 + tickLen * 0.7f);
            g.DrawLine(pen, cx, cy + r1 - tickLen * 0.7f, cx, cy + r1 + tickLen * 0.3f);
            g.DrawLine(pen, cx - r1 - tickLen * 0.3f, cy, cx - r1 + tickLen * 0.7f, cy);
            g.DrawLine(pen, cx + r1 - tickLen * 0.7f, cy, cx + r1 + tickLen * 0.3f, cy);
        }

        private static void DrawBriefcaseIcon(Graphics g, Pen pen, Brush brush, float x, float y, float w, float h)
        {
            float bx = x + w * 0.14f;
            float by = y + h * 0.32f;
            float bw = w * 0.72f;
            float bh = h * 0.56f;
            float rad = w * 0.08f;

            // Main body
            using (var path = UiRadiusHelper.CreateRoundedPath(new Rectangle((int)bx, (int)by, (int)bw, (int)bh), (int)rad))
            {
                g.DrawPath(pen, path);
            }

            // Top handle
            float hx = x + w * 0.34f;
            float hy = y + h * 0.18f;
            float hw = w * 0.32f;
            float hh = h * 0.16f;
            using (var handlePath = new GraphicsPath())
            {
                handlePath.AddArc(hx, hy, hw * 0.3f, hh * 0.6f, 180, 90);
                handlePath.AddLine(hx + hw * 0.15f, hy, hx + hw - hw * 0.15f, hy);
                handlePath.AddArc(hx + hw - hw * 0.3f, hy, hw * 0.3f, hh * 0.6f, 270, 90);
                handlePath.AddLine(hx + hw, hy + hh * 0.3f, hx + hw, by);
                handlePath.StartFigure();
                handlePath.AddLine(hx, hy + hh * 0.3f, hx, by);
                g.DrawPath(pen, handlePath);
            }

            // Clasp / Middle horizontal seam
            float seamY = by + bh * 0.40f;
            g.DrawLine(pen, bx, seamY, bx + bw, seamY);

            // Center buckle
            float bSize = w * 0.12f;
            g.FillRectangle(brush, bx + (bw - bSize) / 2f, seamY - bSize * 0.4f, bSize, bSize * 0.8f);
        }

        private static void DrawTicketIcon(Graphics g, Pen pen, Brush brush, float x, float y, float w, float h)
        {
            float tx = x + w * 0.12f;
            float ty = y + h * 0.20f;
            float tw = w * 0.76f;
            float th = h * 0.60f;
            float nr = h * 0.12f; // notch radius

            using var path = new GraphicsPath();
            // Top edge
            path.AddLine(tx, ty, tx + tw, ty);
            // Right notch
            path.AddArc(tx + tw - nr, ty + (th / 2f) - nr, nr * 2, nr * 2, 270, -180);
            // Bottom edge
            path.AddLine(tx + tw, ty + th, tx, ty + th);
            // Left notch
            path.AddArc(tx - nr, ty + (th / 2f) - nr, nr * 2, nr * 2, 90, -180);
            path.CloseFigure();

            g.DrawPath(pen, path);

            // Perforated line
            using var dashPen = new Pen(pen.Color, pen.Width * 0.9f)
            {
                DashStyle = DashStyle.Dash,
                DashPattern = new float[] { 2f, 2f }
            };
            float dashX = tx + tw * 0.36f;
            g.DrawLine(dashPen, dashX, ty + nr * 0.5f, dashX, ty + th - nr * 0.5f);
        }

        private static void DrawClockIcon(Graphics g, Pen pen, Brush brush, float x, float y, float w, float h)
        {
            float cx = x + w * 0.5f;
            float cy = y + h * 0.5f;
            float r = w * 0.38f;

            // Outer circle
            g.DrawEllipse(pen, cx - r, cy - r, r * 2, r * 2);

            // Center dot
            float dr = w * 0.06f;
            g.FillEllipse(brush, cx - dr, cy - dr, dr * 2, dr * 2);

            // Hour hand (pointing to 12)
            g.DrawLine(pen, cx, cy, cx, cy - r * 0.55f);

            // Minute hand (pointing to 3)
            g.DrawLine(pen, cx, cy, cx + r * 0.68f, cy);
        }

        private static void DrawRefreshIcon(Graphics g, Pen pen, Brush brush, float x, float y, float w, float h)
        {
            float cx = x + w * 0.5f;
            float cy = y + h * 0.5f;
            float r = w * 0.35f;

            // Circular arc (~300 degrees)
            using (var arcPath = new GraphicsPath())
            {
                arcPath.AddArc(cx - r, cy - r, r * 2, r * 2, 35, 295);
                g.DrawPath(pen, arcPath);
            }

            // Arrow head at the end of the arc
            float arrowEndX = cx + (float)(r * Math.Cos(35 * Math.PI / 180));
            float arrowEndY = cy + (float)(r * Math.Sin(35 * Math.PI / 180));
            float asize = w * 0.16f;

            PointF[] arrow = new[]
            {
                new PointF(arrowEndX - asize * 0.4f, arrowEndY - asize * 0.9f),
                new PointF(arrowEndX + asize * 0.6f, arrowEndY - asize * 0.2f),
                new PointF(arrowEndX - asize * 0.1f, arrowEndY + asize * 0.6f)
            };
            g.FillPolygon(brush, arrow);
        }

        private static void DrawAlertTriangleIcon(Graphics g, Pen pen, Brush brush, float x, float y, float w, float h)
        {
            float topX = x + w * 0.5f;
            float topY = y + h * 0.14f;
            float leftX = x + w * 0.12f;
            float leftY = y + h * 0.86f;
            float rightX = x + w * 0.88f;
            float rightY = y + h * 0.86f;

            // Triangle
            using (var triPath = new GraphicsPath())
            {
                triPath.AddLine(topX, topY, rightX, rightY);
                triPath.AddLine(rightX, rightY, leftX, leftY);
                triPath.CloseFigure();
                g.DrawPath(pen, triPath);
            }

            // Exclamation mark bar
            float barW = pen.Width * 1.1f;
            using var barPen = new Pen(pen.Color, barW) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            g.DrawLine(barPen, topX, topY + h * 0.30f, topX, topY + h * 0.56f);

            // Exclamation dot
            float dotR = pen.Width * 0.75f;
            g.FillEllipse(brush, topX - dotR, topY + h * 0.68f, dotR * 2, dotR * 2);
        }

        private static void DrawCurrencyIcon(Graphics g, Pen pen, Brush brush, float x, float y, float w, float h)
        {
            // Upward trending growth chart with arrow
            float x1 = x + w * 0.14f;
            float y1 = y + h * 0.78f;
            float x2 = x + w * 0.42f;
            float y2 = y + h * 0.54f;
            float x3 = x + w * 0.62f;
            float y3 = y + h * 0.64f;
            float x4 = x + w * 0.86f;
            float y4 = y + h * 0.26f;

            g.DrawLine(pen, x1, y1, x2, y2);
            g.DrawLine(pen, x2, y2, x3, y3);
            g.DrawLine(pen, x3, y3, x4, y4);

            // Arrow head at (x4, y4)
            float asize = w * 0.18f;
            g.DrawLine(pen, x4 - asize, y4, x4, y4);
            g.DrawLine(pen, x4, y4 + asize, x4, y4);

            // Bottom baseline
            using var subtlePen = new Pen(Color.FromArgb(120, pen.Color), pen.Width * 0.8f);
            g.DrawLine(subtlePen, x1, y1 + h * 0.08f, x4, y1 + h * 0.08f);
        }
    }
}

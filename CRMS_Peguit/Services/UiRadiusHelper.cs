using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CRMS_Peguit.winforms.Models.Services
{
    /// <summary>
    /// Utility for applying smooth, modern rounded corner radiuses to WinForms controls
    /// (buttons, cards, panels, and badges) to eliminate harsh, edgy rectangles.
    /// </summary>
    public static class UiRadiusHelper
    {
        public static GraphicsPath CreateRoundedPath(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            int d = radius * 2;
            if (d > bounds.Width) d = bounds.Width;
            if (d > bounds.Height) d = bounds.Height;

            var arc = new Rectangle(bounds.X, bounds.Y, d, d);

            // Top-left
            path.AddArc(arc, 180, 90);

            // Top-right
            arc.X = bounds.Right - d;
            path.AddArc(arc, 270, 90);

            // Bottom-right
            arc.Y = bounds.Bottom - d;
            path.AddArc(arc, 0, 90);

            // Bottom-left
            arc.X = bounds.X;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        public static void ApplyRoundedCorners(Control control, int radius)
        {
            if (control is null) return;

            void UpdateRegion()
            {
                if (control.Width <= 0 || control.Height <= 0) return;
                using var path = CreateRoundedPath(new Rectangle(0, 0, control.Width, control.Height), radius);
                control.Region = new Region(path);
            }

            UpdateRegion();
            control.SizeChanged += (_, _) => UpdateRegion();
        }

        public static void ApplyPillShape(Control control)
        {
            if (control is null) return;

            void UpdateRegion()
            {
                if (control.Width <= 0 || control.Height <= 0) return;
                int radius = control.Height / 2;
                using var path = CreateRoundedPath(new Rectangle(0, 0, control.Width, control.Height), radius);
                control.Region = new Region(path);
            }

            UpdateRegion();
            control.SizeChanged += (_, _) => UpdateRegion();
        }

        public static void StyleButton(Button button, int radius = 8)
        {
            if (button is null) return;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            ApplyRoundedCorners(button, radius);
        }

        public static void StyleCard(Panel panel, int radius = 12)
        {
            if (panel is null) return;
            ApplyRoundedCorners(panel, radius);
        }
    }
}

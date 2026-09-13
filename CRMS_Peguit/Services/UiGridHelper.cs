using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CRMS_Peguit.winforms.Models.Services
{
    /// <summary>
    /// Centralized modern DataGridView styling helper for NEXA CRM.
    /// Provides consistent zebra striping, smooth row hover tracking,
    /// soft modern selection tints, and clean circular action button rendering.
    /// </summary>
    public static class UiGridHelper
    {
        public static readonly Color RowNormal = Color.White;
        public static readonly Color RowAlternate = Color.FromArgb(249, 250, 251); // #F9FAFB
        public static readonly Color RowHover = Color.FromArgb(241, 245, 249);     // #F1F5F9
        public static readonly Color SelectionBg = Color.FromArgb(238, 242, 255);  // #EEF2FF
        public static readonly Color TextDark = Color.FromArgb(15, 23, 42);        // #0F172A
        public static readonly Color HeaderBg = Color.FromArgb(248, 250, 252);     // #F8FAFC
        public static readonly Color HeaderText = Color.FromArgb(100, 116, 139);   // #64748B
        public static readonly Color GridBorder = Color.FromArgb(241, 245, 249);   // #F1F5F9

        public static void ApplyModernGridStyle(DataGridView grid, int rowHeight = 50)
        {
            if (grid is null) return;

            grid.EnableHeadersVisualStyles = false;
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.GridColor = GridBorder;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.ShowCellToolTips = true;
            grid.RowTemplate.Height = rowHeight;
            grid.ColumnHeadersHeight = 48;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            // Column Header Styling
            grid.ColumnHeadersDefaultCellStyle.BackColor = HeaderBg;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = HeaderText;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(12, 0, 12, 0);

            // Default Row Styling
            grid.DefaultCellStyle.BackColor = RowNormal;
            grid.DefaultCellStyle.ForeColor = TextDark;
            grid.DefaultCellStyle.SelectionBackColor = SelectionBg;
            grid.DefaultCellStyle.SelectionForeColor = TextDark;
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f);
            grid.DefaultCellStyle.Padding = new Padding(12, 0, 12, 0);

            // Alternating Row Styling (Zebra Striping)
            grid.AlternatingRowsDefaultCellStyle.BackColor = RowAlternate;
            grid.AlternatingRowsDefaultCellStyle.ForeColor = TextDark;
            grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = SelectionBg;
            grid.AlternatingRowsDefaultCellStyle.SelectionForeColor = TextDark;
            grid.AlternatingRowsDefaultCellStyle.Font = new Font("Segoe UI", 9.5f);
            grid.AlternatingRowsDefaultCellStyle.Padding = new Padding(12, 0, 12, 0);

            // Smooth Row Hover Tracking
            int hoverRow = -1;
            int hoverCol = -1;

            grid.CellMouseEnter += (s, e) =>
            {
                hoverCol = e.ColumnIndex;
                if (e.RowIndex >= 0 && e.RowIndex < grid.RowCount && e.RowIndex != hoverRow)
                {
                    int old = hoverRow;
                    hoverRow = e.RowIndex;
                    if (old >= 0 && old < grid.RowCount && !grid.Rows[old].Selected)
                        grid.InvalidateRow(old);
                    if (hoverRow < grid.RowCount && !grid.Rows[hoverRow].Selected)
                        grid.InvalidateRow(hoverRow);
                }
            };

            grid.CellMouseLeave += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.RowIndex == hoverRow)
                {
                    int old = hoverRow;
                    hoverRow = -1;
                    hoverCol = -1;
                    if (old < grid.RowCount && !grid.Rows[old].Selected)
                        grid.InvalidateRow(old);
                }
            };

            grid.RowPrePaint += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.RowIndex < grid.RowCount && !grid.Rows[e.RowIndex].Selected)
                {
                    if (e.RowIndex == hoverRow)
                    {
                        using var brush = new SolidBrush(RowHover);
                        e.Graphics.FillRectangle(brush, e.RowBounds);
                        e.PaintParts &= ~DataGridViewPaintParts.Background;
                    }
                }
            };

            // Custom header bottom divider line
            grid.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(226, 232, 240), 1f);
                e.Graphics.DrawLine(pen, 0, grid.ColumnHeadersHeight - 1, grid.Width, grid.ColumnHeadersHeight - 1);
            };

            // Intercept Actions column painting for modern circular button
            grid.CellPainting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.Graphics == null) return;
                var col = grid.Columns[e.ColumnIndex];
                if (col.Name == "Actions" || col is Controls.ActionsColumn)
                {
                    PaintActionCell(e, e.RowIndex == hoverRow && e.ColumnIndex == hoverCol);
                }
            };
        }

        public static void PaintActionCell(DataGridViewCellPaintingEventArgs e, bool isHovered)
        {
            if (e.Graphics == null) return;
            e.PaintBackground(e.CellBounds, true);

            int btnSize = 28;
            int x = e.CellBounds.X + (e.CellBounds.Width - btnSize) / 2;
            int y = e.CellBounds.Y + (e.CellBounds.Height - btnSize) / 2;
            var rect = new Rectangle(x, y, btnSize, btnSize);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Soft circular pill background
            Color bg = isHovered ? Color.FromArgb(226, 232, 240) : Color.FromArgb(241, 245, 249);
            using (var brush = new SolidBrush(bg))
            {
                e.Graphics.FillEllipse(brush, rect);
            }

            using (var borderPen = new Pen(Color.FromArgb(226, 232, 240), 1f))
            {
                e.Graphics.DrawEllipse(borderPen, rect);
            }

            // Draw clean centered 3 vertical dots
            using (var font = new Font("Segoe UI", 12f, FontStyle.Bold))
            using (var textBrush = new SolidBrush(Color.FromArgb(71, 85, 105)))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                e.Graphics.DrawString("⋮", font, textBrush, new RectangleF(x, y - 1, btnSize, btnSize), sf);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Adds a fixed-width Actions column that does NOT stretch with AutoSizeColumnsMode.Fill.
        /// Call this after setting all other column FillWeights.
        /// </summary>
        public static void AddActionsColumn(DataGridView grid, int fixedWidth = 64)
        {
            if (grid is null) return;

            // Remove any existing Actions column to avoid duplicates
            var existing = grid.Columns["Actions"];
            if (existing != null) grid.Columns.Remove(existing);

            var col = new Controls.ActionsColumn
            {
                Name = "Actions",
                HeaderText = "",
                Width = fixedWidth,
                MinimumWidth = fixedWidth,
                // Setting FillWeight to a very small value combined with Width freezes it visually
                FillWeight = 1,
                Resizable = DataGridViewTriState.False,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                ReadOnly = true
            };

            // Freeze the column width after Fill mode is applied
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;

            grid.Columns.Add(col);
        }

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

            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}

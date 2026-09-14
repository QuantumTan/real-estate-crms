using System;
using System.Drawing;
using System.Windows.Forms;
using CRMS_Peguit.winforms.Models.Services;

namespace CRMS_Peguit.winforms.Views.Shared
{
    public partial class PlaceholderView : UserControl
    {
        private readonly Panel _pnlCard;
        private readonly Button _btnPrimaryAction;
        private readonly Label _lblTitle;
        private readonly Label _lblSubtitle;

        public PlaceholderView() : this("Module", "Module workflows and system activities are actively integrated with your CRM records.")
        {
        }

        public PlaceholderView(string title, string message)
        {
            InitializeComponent();

            BackColor = Color.FromArgb(244, 247, 251);
            AutoScroll = true;
            Padding = new Padding(30);

            _lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(30, 20),
                AutoSize = true
            };

            _lblSubtitle = new Label
            {
                Text = $"Workspace & Automated Operations Hub — {title}",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(32, 56),
                AutoSize = true
            };

            Controls.Add(_lblTitle);
            Controls.Add(_lblSubtitle);

            // Centered modern white card
            _pnlCard = new Panel
            {
                BackColor = Color.White,
                Location = new Point(30, 92),
                Size = new Size(Math.Max(500, ClientSize.Width - 60), 480)
            };
            UiRadiusHelper.StyleCard(_pnlCard, 14);
            _pnlCard.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(226, 232, 240), 1f);
                using var path = UiRadiusHelper.CreateRoundedPath(new Rectangle(0, 0, _pnlCard.Width - 1, _pnlCard.Height - 1), 14);
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.DrawPath(pen, path);
            };

            // Module icon & badge
            string icon = GetModuleIcon(title);
            var lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI Emoji", 26f),
                ForeColor = Color.FromArgb(15, 91, 158),
                BackColor = Color.FromArgb(238, 242, 255),
                Size = new Size(68, 68),
                Location = new Point((_pnlCard.Width - 68) / 2, 28),
                TextAlign = ContentAlignment.MiddleCenter
            };
            UiRadiusHelper.MakeCircularAvatar(lblIcon);

            var lblBadge = new Label
            {
                Text = "⚡ CONNECTED CRM SERVICE",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(22, 101, 52),
                BackColor = Color.FromArgb(220, 252, 231),
                Size = new Size(200, 26),
                Location = new Point((_pnlCard.Width - 200) / 2, 106),
                TextAlign = ContentAlignment.MiddleCenter
            };
            UiRadiusHelper.ApplyPillShape(lblBadge);

            var lblCardHeading = new Label
            {
                Text = $"{title} Hub & Workflow Engine",
                Font = new Font("Segoe UI", 15f, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Size = new Size(600, 28),
                Location = new Point((_pnlCard.Width - 600) / 2, 140),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblCardMessage = new Label
            {
                Text = $"{message}\r\nData recorded here automatically synchronizes with active Leads, Customers, and Deals.",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(100, 116, 139),
                Size = new Size(680, 42),
                Location = new Point((_pnlCard.Width - 680) / 2, 172),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Feature preview container
            var featureTable = new TableLayoutPanel
            {
                ColumnCount = 3,
                RowCount = 1,
                Location = new Point(24, 230),
                Size = new Size(_pnlCard.Width - 48, 140)
            };
            featureTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            featureTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            featureTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));

            var features = GetModuleFeatures(title);
            foreach (var feat in features)
            {
                featureTable.Controls.Add(CreateFeatureCard(feat.Icon, feat.Title, feat.Desc));
            }

            // Quick action button
            _btnPrimaryAction = new Button
            {
                Text = "  ◎  Jump to Leads Pipeline",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(15, 91, 158),
                Size = new Size(220, 42),
                Location = new Point((_pnlCard.Width - 220) / 2, 400),
                Cursor = Cursors.Hand
            };
            UiRadiusHelper.StyleButton(_btnPrimaryAction, 8);
            _btnPrimaryAction.Click += (_, _) =>
            {
                if (FindForm() is MainForm form)
                {
                    form.NavigateTo("leads");
                }
            };

            _pnlCard.Controls.Add(lblIcon);
            _pnlCard.Controls.Add(lblBadge);
            _pnlCard.Controls.Add(lblCardHeading);
            _pnlCard.Controls.Add(lblCardMessage);
            _pnlCard.Controls.Add(featureTable);
            _pnlCard.Controls.Add(_btnPrimaryAction);

            Controls.Add(_pnlCard);

            Resize += (_, _) =>
            {
                _pnlCard.Width = Math.Max(500, ClientSize.Width - 60);
                _pnlCard.Height = Math.Max(460, ClientSize.Height - 120);

                int cardW = _pnlCard.Width;
                lblIcon.Left = (cardW - lblIcon.Width) / 2;
                lblBadge.Left = (cardW - lblBadge.Width) / 2;
                lblCardHeading.Left = (cardW - lblCardHeading.Width) / 2;
                lblCardMessage.Left = (cardW - lblCardMessage.Width) / 2;
                _btnPrimaryAction.Left = (cardW - _btnPrimaryAction.Width) / 2;
                _btnPrimaryAction.Top = _pnlCard.Height - 64;

                featureTable.Width = cardW - 48;
                featureTable.Height = Math.Max(120, _btnPrimaryAction.Top - featureTable.Top - 18);
            };
        }

        private static string GetModuleIcon(string title)
        {
            return (title.ToLowerInvariant()) switch
            {
                var t when t.Contains("activ") => "📈",
                var t when t.Contains("follow") => "⏱",
                var t when t.Contains("report") => "📊",
                var t when t.Contains("ticket") || t.Contains("support") => "🎟",
                _ => "⚡"
            };
        }

        private static (string Icon, string Title, string Desc)[] GetModuleFeatures(string title)
        {
            string t = title.ToLowerInvariant();
            if (t.Contains("activ"))
            {
                return new[]
                {
                    ("📧", "Communication Logs", "Every customer email, message, and touchpoint is automatically cataloged."),
                    ("🗓", "Lifecycle Timeline", "Complete chronological interaction history across all deals and inquiries."),
                    ("⚡", "Pipeline Synchronization", "Agent actions update lead statuses and milestones in real-time.")
                };
            }
            if (t.Contains("follow"))
            {
                return new[]
                {
                    ("⏰", "Due Date Tracking", "High-priority scheduled reminders for timely client reach-outs."),
                    ("🎯", "Guided Progression", "Step-by-step pipeline milestones for higher conversion rates."),
                    ("👥", "Ownership Routing", "Tasks automatically align to assigned sales staff and managers.")
                };
            }
            if (t.Contains("report"))
            {
                return new[]
                {
                    ("💼", "Volume & Projections", "Comprehensive breakdown of gross sales, commission, and pipeline."),
                    ("📊", "Stage Funnel Analysis", "Real-time visibility into conversion drop-offs and stage velocity."),
                    ("🏆", "Team Performance", "Comparative deal closures and metrics across all sales representatives.")
                };
            }
            // Support Tickets / Default
            return new[]
            {
                ("🎫", "Inquiry Routing", "Organize client support inquiries and service requests into queues."),
                ("⚡", "SLA Monitoring", "Track resolution speed and response timers to ensure client satisfaction."),
                ("💬", "Audit Trail", "Full historic record of resolutions, agent notes, and customer feedback.")
            };
        }

        private static Panel CreateFeatureCard(string icon, string title, string desc)
        {
            var pnl = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 250, 252),
                Margin = new Padding(6),
                Padding = new Padding(12)
            };
            UiRadiusHelper.ApplyRoundedCorners(pnl, 10);
            pnl.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(226, 232, 240), 1f);
                using var path = UiRadiusHelper.CreateRoundedPath(new Rectangle(0, 0, pnl.Width - 1, pnl.Height - 1), 10);
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.DrawPath(pen, path);
            };

            var lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI Emoji", 13f),
                Location = new Point(12, 10),
                Size = new Size(28, 28),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(44, 12),
                AutoSize = true
            };

            var lblDesc = new Label
            {
                Text = desc,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(12, 44),
                Size = new Size(200, 60),
                AutoEllipsis = true
            };

            pnl.Controls.Add(lblIcon);
            pnl.Controls.Add(lblTitle);
            pnl.Controls.Add(lblDesc);

            pnl.Resize += (_, _) =>
            {
                lblDesc.Width = Math.Max(80, pnl.Width - 24);
                lblDesc.Height = Math.Max(40, pnl.Height - 50);
            };

            return pnl;
        }
    }
}

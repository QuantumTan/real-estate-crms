using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using CRMS_Peguit.domain.entities;
using CRMS_Peguit.winforms.Auth;
using CRMS_Peguit.winforms.Controllers;
using CRMS_Peguit.winforms.Models.Services;
using CRMS_Peguit.winforms.Views.Shared;

using Lead = CRMS_Peguit.domain.entities.Lead;

namespace CRMS_Peguit.winforms.Views.Leads
{
    public partial class LeadDetailForm : Form
    {
        private static readonly string[] PipelineStages =
            { "new", "contacted", "qualified", "converted" };

        private static readonly string[] StageDisplayNames =
            { "New", "Contacted", "Qualified", "Converted" };

        private Lead? _lead;
        private readonly LeadController? _controller;

        private Panel? _pnlHeader;
        private Panel? _pnlFooter;
        private Panel? _pnlContent;
        private Button? _btnEdit;
        private Button? _btnMessage;
        private Button? _btnClose;

        public LeadDetailForm()
        {
            InitializeComponent();
        }

        public LeadDetailForm(Lead lead, LeadController controller)
        {
            _lead = lead;
            _controller = controller;
            InitializeComponent();
            SetupFormProperties();
            BuildUi();
        }

        private void SetupFormProperties()
        {
            Text = _lead is not null ? $"Lead Details - {_lead.FullName}" : "Lead Details";
            Size = new Size(760, 700);
            MinimumSize = new Size(620, 520);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(244, 247, 251);
            DoubleBuffered = true;
        }

        private void BuildUi()
        {
            Controls.Clear();
            if (_lead == null || _controller == null) return;

            // 1. Fixed Bottom Action Footer
            BuildFooter();

            // 2. Hero Header
            BuildHeader();

            // 3. Scrollable Content Canvas
            BuildContent();

            Resize += (_, _) => LayoutResponsiveComponents();
            LayoutResponsiveComponents();
        }

        private void BuildHeader()
        {
            _pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 88,
                BackColor = Color.White,
                Padding = new Padding(24, 16, 24, 16)
            };

            _pnlHeader.Paint += (s, e) =>
            {
                using var pen = new Pen(UiDetailCardHelper.BorderColor, 1f);
                e.Graphics.DrawLine(pen, 0, _pnlHeader.Height - 1, _pnlHeader.Width, _pnlHeader.Height - 1);
            };

            // Avatar circle
            var lblAvatar = new Label
            {
                Text = UiDetailCardHelper.GetInitials(_lead!.FullName),
                Font = new Font("Segoe UI", 15f, FontStyle.Bold),
                ForeColor = Color.FromArgb(180, 83, 9),       // #B45309 Amber
                BackColor = Color.FromArgb(254, 243, 199),     // #FEF3C7
                Size = new Size(52, 52),
                Location = new Point(24, 18),
                TextAlign = ContentAlignment.MiddleCenter
            };
            UiRadiusHelper.MakeCircularAvatar(lblAvatar);
            _pnlHeader.Controls.Add(lblAvatar);

            // Title & Subtitle block
            var lblName = new Label
            {
                Text = _lead.FullName,
                Font = new Font("Segoe UI", 15.5f, FontStyle.Bold),
                ForeColor = Theme.TextPrimary,
                Location = new Point(88, 18),
                AutoSize = true
            };
            _pnlHeader.Controls.Add(lblName);

            var regDate = _lead.CreatedAt != default ? _lead.CreatedAt.ToString("MMM d, yyyy") : "N/A";
            var lblMeta = new Label
            {
                Text = $"Lead #{_lead.LeadId}   •   Registered {regDate}",
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = UiDetailCardHelper.LabelMutedColor,
                Location = new Point(88, 48),
                AutoSize = true
            };
            _pnlHeader.Controls.Add(lblMeta);

            // Badges in Header
            var (sBg, sFg, sStroke) = UiDetailCardHelper.GetStatusColors(_lead.Stage);
            var stageBadge = UiDetailCardHelper.CreatePillBadge(_lead.Stage, sBg, sFg, sStroke);
            stageBadge.Name = "headerStageBadge";
            _pnlHeader.Controls.Add(stageBadge);

            var (pBg, pFg, pStroke) = UiDetailCardHelper.GetPriorityColors(_lead.Priority);
            var priorityBadge = UiDetailCardHelper.CreatePillBadge(
                string.IsNullOrWhiteSpace(_lead.Priority) ? "Normal" : _lead.Priority,
                pBg, pFg, pStroke);
            priorityBadge.Name = "headerPriorityBadge";
            _pnlHeader.Controls.Add(priorityBadge);

            Controls.Add(_pnlHeader);
        }

        private void BuildFooter()
        {
            _pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 62,
                BackColor = Color.White,
                Padding = new Padding(24, 12, 24, 12)
            };

            _pnlFooter.Paint += (s, e) =>
            {
                using var pen = new Pen(UiDetailCardHelper.BorderColor, 1f);
                e.Graphics.DrawLine(pen, 0, 0, _pnlFooter.Width, 0);
            };

            // Close Button
            _btnClose = new Button
            {
                Text = "Close",
                Size = new Size(88, 36),
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = Color.FromArgb(30, 41, 59),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                DialogResult = DialogResult.OK
            };
            UiRadiusHelper.StyleButton(_btnClose, 8);
            UiRadiusHelper.AttachHoverFeedback(_btnClose, Color.FromArgb(241, 245, 249), Color.FromArgb(226, 232, 240));
            _pnlFooter.Controls.Add(_btnClose);

            // Edit Button (if permitted)
            bool canEdit = _lead is not null && RbacService.CanEditRecord(_lead.AssignedAgentId, _lead.CreatedByUserId);
            if (canEdit)
            {
                _btnEdit = new Button
                {
                    Text = "✏️ Edit Lead",
                    Size = new Size(118, 36),
                    BackColor = Theme.Primary,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
                };
                UiRadiusHelper.StyleButton(_btnEdit, 8);
                UiRadiusHelper.AttachHoverFeedback(_btnEdit, Theme.Primary, Theme.PrimaryDark);
                _btnEdit.Click += BtnEditClick;
                _pnlFooter.Controls.Add(_btnEdit);
            }

            // Send Email Button
            bool hasEmail = !string.IsNullOrWhiteSpace(_lead?.Email);
            _btnMessage = new Button
            {
                Text = "✉️ Send Email",
                Size = new Size(118, 36),
                BackColor = Color.White,
                ForeColor = hasEmail ? Theme.Primary : Color.FromArgb(148, 163, 184),
                Enabled = hasEmail,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            UiRadiusHelper.StyleButton(_btnMessage, 8);
            _btnMessage.Paint += (s, e) =>
            {
                using var p = new Pen(UiDetailCardHelper.BorderColor, 1f);
                using var path = UiRadiusHelper.CreateRoundedPath(new Rectangle(0, 0, _btnMessage.Width - 1, _btnMessage.Height - 1), 8);
                e.Graphics.DrawPath(p, path);
            };
            if (hasEmail)
            {
                UiRadiusHelper.AttachHoverFeedback(_btnMessage, Color.White, Color.FromArgb(241, 245, 249));
                _btnMessage.Click += BtnMessageClick;
            }
            _pnlFooter.Controls.Add(_btnMessage);

            AcceptButton = _btnClose;
            CancelButton = _btnClose;
            Controls.Add(_pnlFooter);
        }

        private void BuildContent()
        {
            _pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(244, 247, 251),
                Padding = new Padding(24, 18, 24, 18)
            };

            int currentY = 16;

            // --- Card 1: Pipeline Progression Stepper ---
            var cardPipeline = CreateCardPanel(ref currentY);
            cardPipeline.Controls.Add(UiDetailCardHelper.CreateCardHeader("📊  Pipeline Progression"));
            cardPipeline.Controls.Add(UiDetailCardHelper.CreateDivider());

            bool isLost = string.Equals(_lead!.Stage, "lost", StringComparison.OrdinalIgnoreCase);
            if (isLost)
            {
                var lostBanner = new Panel
                {
                    Height = 56,
                    Dock = DockStyle.Top,
                    BackColor = Color.FromArgb(254, 242, 242), // #FEF2F2
                    Padding = new Padding(14, 10, 14, 10)
                };
                lostBanner.Paint += (s, e) =>
                {
                    using var p = new Pen(Color.FromArgb(252, 165, 165), 1f);
                    e.Graphics.DrawRectangle(p, 0, 0, lostBanner.Width - 1, lostBanner.Height - 1);
                };
                var lblLost = new Label
                {
                    Text = "⚠️  This lead was marked LOST and is no longer actively progressing through the sales pipeline.",
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(153, 27, 27),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                lostBanner.Controls.Add(lblLost);
                cardPipeline.Controls.Add(lostBanner);
            }
            else
            {
                var stepper = CreatePipelineStepper();
                cardPipeline.Controls.Add(stepper);
            }
            FinalizeCardHeight(cardPipeline, ref currentY);
            _pnlContent.Controls.Add(cardPipeline);

            // --- Card 2: Lead Information ---
            var cardLeadInfo = CreateCardPanel(ref currentY);
            cardLeadInfo.Controls.Add(UiDetailCardHelper.CreateCardHeader("👤  Lead Information"));
            cardLeadInfo.Controls.Add(UiDetailCardHelper.CreateDivider());

            // Expected Value Highlight Tile (if present)
            if (_lead.ExpectedValue.HasValue && _lead.ExpectedValue.Value > 0)
            {
                var valBanner = new Panel
                {
                    Height = 48,
                    Dock = DockStyle.Top,
                    BackColor = Color.FromArgb(240, 253, 244),
                    Margin = new Padding(0, 0, 0, 10),
                    Padding = new Padding(14, 8, 14, 8)
                };
                valBanner.Paint += (s, e) =>
                {
                    using var pen = new Pen(Color.FromArgb(187, 247, 208), 1f);
                    using var path = UiRadiusHelper.CreateRoundedPath(new Rectangle(0, 0, valBanner.Width - 1, valBanner.Height - 1), 6);
                    e.Graphics.DrawPath(pen, path);
                };
                UiRadiusHelper.ApplyRoundedCorners(valBanner, 6);

                var lblValCap = new Label
                {
                    Text = "EXPECTED VALUE",
                    Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(22, 101, 52),
                    Location = new Point(12, 14),
                    AutoSize = true
                };
                var lblValAmt = new Label
                {
                    Text = $"₱{_lead.ExpectedValue.Value:N2}",
                    Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(21, 128, 61),
                    Location = new Point(130, 10),
                    AutoSize = true
                };
                valBanner.Controls.Add(lblValCap);
                valBanner.Controls.Add(lblValAmt);
                cardLeadInfo.Controls.Add(valBanner);
            }

            cardLeadInfo.Controls.Add(UiDetailCardHelper.CreateKeyValueRow(
                "Email Address", string.IsNullOrWhiteSpace(_lead.Email) ? "Not Provided" : _lead.Email,
                "Phone Number", string.IsNullOrWhiteSpace(_lead.Phone) ? "Not Provided" : _lead.Phone));
            cardLeadInfo.Controls.Add(UiDetailCardHelper.CreateKeyValueRow(
                "Lead Source", string.IsNullOrWhiteSpace(_lead.Source) ? "Direct / Other" : _lead.Source,
                "Priority Level", string.IsNullOrWhiteSpace(_lead.Priority) ? "Normal" : _lead.Priority));
            FinalizeCardHeight(cardLeadInfo, ref currentY);
            _pnlContent.Controls.Add(cardLeadInfo);

            // --- Card 3: Ownership & Assignment ---
            var cardOwner = CreateCardPanel(ref currentY);
            cardOwner.Controls.Add(UiDetailCardHelper.CreateCardHeader("👥  Assignment & Review"));
            cardOwner.Controls.Add(UiDetailCardHelper.CreateDivider());
            var agentName = _controller!.GetAssignedAgentName(_lead.AssignedAgentId);
            cardOwner.Controls.Add(UiDetailCardHelper.CreateKeyValueRow(
                "Assigned Agent", agentName ?? "Unassigned",
                "Assignment Review", _lead.AssignmentStatus));
            if (!string.IsNullOrWhiteSpace(_lead.AssignmentReviewNotes))
            {
                cardOwner.Controls.Add(UiDetailCardHelper.CreateKeyValueRow(
                    "Review Notes", _lead.AssignmentReviewNotes));
            }
            FinalizeCardHeight(cardOwner, ref currentY);
            _pnlContent.Controls.Add(cardOwner);

            // --- Card 4: Notes ---
            var cardNotes = CreateCardPanel(ref currentY);
            cardNotes.Controls.Add(UiDetailCardHelper.CreateCardHeader("📝  Notes & Remarks"));
            cardNotes.Controls.Add(UiDetailCardHelper.CreateDivider());

            var pnlNoteBox = new Panel
            {
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(248, 250, 252),
                Padding = new Padding(14, 10, 14, 10),
                AutoSize = true
            };
            pnlNoteBox.Paint += (s, e) =>
            {
                // Left accent line
                using var brush = new SolidBrush(Color.FromArgb(59, 130, 246));
                e.Graphics.FillRectangle(brush, 0, 0, 4, pnlNoteBox.Height);

                using var pen = new Pen(UiDetailCardHelper.BorderColor, 1f);
                e.Graphics.DrawRectangle(pen, 0, 0, pnlNoteBox.Width - 1, pnlNoteBox.Height - 1);
            };
            var lblNoteText = new Label
            {
                Text = string.IsNullOrWhiteSpace(_lead.Notes) ? "No notes recorded for this lead." : _lead.Notes,
                Font = new Font("Segoe UI", 9.5f, string.IsNullOrWhiteSpace(_lead.Notes) ? FontStyle.Italic : FontStyle.Regular),
                ForeColor = string.IsNullOrWhiteSpace(_lead.Notes) ? UiDetailCardHelper.LabelMutedColor : UiDetailCardHelper.ValueTextColor,
                Dock = DockStyle.Top,
                AutoSize = true,
                MaximumSize = new Size(580, 0)
            };
            pnlNoteBox.Controls.Add(lblNoteText);
            cardNotes.Controls.Add(pnlNoteBox);

            FinalizeCardHeight(cardNotes, ref currentY);
            _pnlContent.Controls.Add(cardNotes);

            // --- Card 5: Recent Activities ---
            var activities = _controller.GetActivityHistory(_lead.LeadId);
            var cardActivities = CreateCardPanel(ref currentY);
            cardActivities.Controls.Add(UiDetailCardHelper.CreateCardHeader("⏱  Recent Activities", activities.Count.ToString()));
            cardActivities.Controls.Add(UiDetailCardHelper.CreateDivider());

            if (activities.Count == 0)
            {
                var lblEmpty = new Label
                {
                    Text = "No activities logged yet for this lead.",
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Italic),
                    ForeColor = UiDetailCardHelper.LabelMutedColor,
                    Dock = DockStyle.Top,
                    Height = 36,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                cardActivities.Controls.Add(lblEmpty);
            }
            else
            {
                foreach (var a in activities.Take(15))
                {
                    var item = CreateTimelineItem(a);
                    cardActivities.Controls.Add(item);
                }
            }
            FinalizeCardHeight(cardActivities, ref currentY);
            _pnlContent.Controls.Add(cardActivities);

            Controls.Add(_pnlContent);
            _pnlContent.BringToFront();
        }

        private Panel CreatePipelineStepper()
        {
            int currentIndex = Array.FindIndex(PipelineStages,
                s => string.Equals(s, _lead?.Stage, StringComparison.OrdinalIgnoreCase));
            if (currentIndex < 0) currentIndex = 0;

            var panel = new Panel
            {
                Height = 74,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(248, 250, 252),
                Margin = new Padding(0, 4, 0, 8),
                Padding = new Padding(16, 10, 16, 10)
            };

            panel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                // Outer border
                var borderRect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
                using var pPath = UiRadiusHelper.CreateRoundedPath(borderRect, 8);
                using var bPen = new Pen(UiDetailCardHelper.BorderColor, 1f);
                e.Graphics.DrawPath(bPen, pPath);

                int count = PipelineStages.Length;
                int startX = 60;
                int endX = panel.Width - 60;
                int lineY = 26;

                // Step positions
                var xs = new int[count];
                for (int i = 0; i < count; i++)
                {
                    xs[i] = startX + (int)((long)(endX - startX) * i / (count - 1));
                }

                // Draw connecting track lines first
                for (int i = 0; i < count - 1; i++)
                {
                    bool segDone = i < currentIndex;
                    using var linePen = new Pen(segDone ? Color.FromArgb(34, 197, 94) : Color.FromArgb(226, 232, 240), 3f);
                    e.Graphics.DrawLine(linePen, xs[i] + 14, lineY, xs[i + 1] - 14, lineY);
                }

                // Draw each node circle & stage label
                int circleSize = 28;
                for (int i = 0; i < count; i++)
                {
                    int cx = xs[i] - circleSize / 2;
                    int cy = lineY - circleSize / 2;
                    var circleRect = new Rectangle(cx, cy, circleSize, circleSize);

                    bool isPassed = i < currentIndex;
                    bool isCurrent = i == currentIndex;

                    if (isPassed)
                    {
                        // Passed: Emerald circle with checkmark
                        using var brush = new SolidBrush(Color.FromArgb(34, 197, 94));
                        e.Graphics.FillEllipse(brush, circleRect);

                        using var font = new Font("Segoe UI", 9f, FontStyle.Bold);
                        using var tBrush = new SolidBrush(Color.White);
                        using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        e.Graphics.DrawString("✓", font, tBrush, circleRect, sf);
                    }
                    else if (isCurrent)
                    {
                        // Current: Primary blue circle with step number and outer ring
                        using var ringPen = new Pen(Color.FromArgb(191, 219, 254), 3f);
                        e.Graphics.DrawEllipse(ringPen, cx - 2, cy - 2, circleSize + 4, circleSize + 4);

                        using var brush = new SolidBrush(Theme.Primary);
                        e.Graphics.FillEllipse(brush, circleRect);

                        using var font = new Font("Segoe UI", 9f, FontStyle.Bold);
                        using var tBrush = new SolidBrush(Color.White);
                        using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        e.Graphics.DrawString((i + 1).ToString(), font, tBrush, circleRect, sf);
                    }
                    else
                    {
                        // Future: Gray outlined circle
                        using var brush = new SolidBrush(Color.White);
                        e.Graphics.FillEllipse(brush, circleRect);

                        using var pen = new Pen(Color.FromArgb(203, 213, 225), 1.5f);
                        e.Graphics.DrawEllipse(pen, circleRect);

                        using var font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                        using var tBrush = new SolidBrush(Color.FromArgb(148, 163, 184));
                        using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        e.Graphics.DrawString((i + 1).ToString(), font, tBrush, circleRect, sf);
                    }

                    // Stage Name Label below
                    Color labelColor = isCurrent ? Theme.Primary : isPassed ? Color.FromArgb(22, 101, 52) : Color.FromArgb(100, 116, 139);
                    var labelFont = new Font("Segoe UI", 8.5f, isCurrent ? FontStyle.Bold : FontStyle.Regular);
                    using (var lBrush = new SolidBrush(labelColor))
                    using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near })
                    {
                        var textRect = new RectangleF(xs[i] - 50, lineY + 18, 100, 20);
                        e.Graphics.DrawString(StageDisplayNames[i], labelFont, lBrush, textRect, sf);
                    }
                }
            };

            UiRadiusHelper.ApplyRoundedCorners(panel, 8);
            return panel;
        }

        private Panel CreateCardPanel(ref int currentY)
        {
            var card = new Panel
            {
                Location = new Point(24, currentY),
                Width = Math.Max(500, ClientSize.Width - 48 - SystemInformation.VerticalScrollBarWidth),
                BackColor = UiDetailCardHelper.CardBg,
                Padding = new Padding(18, 14, 18, 14)
            };

            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                using var path = UiRadiusHelper.CreateRoundedPath(rect, 10);
                using var pen = new Pen(UiDetailCardHelper.BorderColor, 1f);
                e.Graphics.DrawPath(pen, path);
            };

            UiRadiusHelper.ApplyRoundedCorners(card, 10);
            return card;
        }

        private void FinalizeCardHeight(Panel card, ref int currentY)
        {
            int totalHeight = card.Padding.Top;
            foreach (Control c in card.Controls)
            {
                totalHeight += c.Height + c.Margin.Bottom;
            }
            totalHeight += card.Padding.Bottom + 4;
            card.Height = totalHeight;

            currentY += totalHeight + 14;
        }

        private Panel CreateTimelineItem(Activity a)
        {
            var item = new Panel
            {
                Height = 44,
                Dock = DockStyle.Top,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 6)
            };

            var dateBadge = UiDetailCardHelper.CreatePillBadge(
                a.ActivityDate.ToLocalTime().ToString("MMM d, yyyy"),
                Color.FromArgb(241, 245, 249),
                Color.FromArgb(71, 85, 105),
                Color.FromArgb(226, 232, 240));
            dateBadge.Location = new Point(0, 8);
            item.Controls.Add(dateBadge);

            var typeBadge = UiDetailCardHelper.CreatePillBadge(
                a.Type,
                Color.FromArgb(254, 243, 199),
                Color.FromArgb(180, 83, 9),
                Color.FromArgb(253, 230, 138));
            typeBadge.Location = new Point(dateBadge.Right + 8, 8);
            item.Controls.Add(typeBadge);

            var lblNotes = new Label
            {
                Text = a.Notes ?? string.Empty,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = UiDetailCardHelper.ValueTextColor,
                Location = new Point(typeBadge.Right + 12, 10),
                AutoSize = true,
                MaximumSize = new Size(380, 24)
            };
            item.Controls.Add(lblNotes);

            return item;
        }

        private void LayoutResponsiveComponents()
        {
            if (_pnlHeader != null)
            {
                var stageBadge = _pnlHeader.Controls["headerStageBadge"];
                var priorityBadge = _pnlHeader.Controls["headerPriorityBadge"];

                int right = _pnlHeader.ClientSize.Width - 24;
                if (stageBadge != null)
                {
                    stageBadge.Location = new Point(right - stageBadge.Width, 22);
                    right -= (stageBadge.Width + 8);
                }
                if (priorityBadge != null)
                {
                    priorityBadge.Location = new Point(right - priorityBadge.Width, 22);
                }
            }

            if (_pnlFooter != null)
            {
                int right = _pnlFooter.ClientSize.Width - 24;
                if (_btnClose != null)
                {
                    _btnClose.Location = new Point(right - _btnClose.Width, 13);
                    right -= (_btnClose.Width + 10);
                }
                if (_btnEdit != null && _btnEdit.Visible)
                {
                    _btnEdit.Location = new Point(right - _btnEdit.Width, 13);
                }
                if (_btnMessage != null)
                {
                    _btnMessage.Location = new Point(24, 13);
                }
            }

            if (_pnlContent != null)
            {
                int targetWidth = Math.Max(400, _pnlContent.ClientSize.Width - 48);
                foreach (Control c in _pnlContent.Controls)
                {
                    if (c is Panel card)
                    {
                        card.Width = targetWidth;
                    }
                }
            }
        }

        private void BtnEditClick(object? sender, EventArgs e)
        {
            if (_lead == null || _controller == null) return;

            using var editForm = new LeadInputForm(_lead);
            if (editForm.ShowDialog(this) == DialogResult.OK && editForm.Result is not null)
            {
                _controller.Update(editForm.Result);
                _lead = _controller.GetById(_lead.LeadId) ?? editForm.Result;
                BuildUi();
            }
        }

        private void BtnMessageClick(object? sender, EventArgs e)
        {
            if (_lead == null || string.IsNullOrWhiteSpace(_lead.Email)) return;

            using var emailForm = new EmailMessageForm(_lead.FullName, _lead.Email);
            emailForm.ShowDialog(this);
        }
    }
}

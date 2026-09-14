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

namespace CRMS_Peguit.winforms.Views.Customers
{
    public partial class CustomerDetailForm : Form
    {
        private Customer? _customer;
        private readonly CustomerController? _controller;

        private Panel? _pnlHeader;
        private Panel? _pnlFooter;
        private Panel? _pnlContent;
        private Button? _btnEdit;
        private Button? _btnMessage;
        private Button? _btnClose;

        public CustomerDetailForm()
        {
            InitializeComponent();
        }

        public CustomerDetailForm(Customer customer, CustomerController controller)
        {
            _customer = customer;
            _controller = controller;
            InitializeComponent();
            SetupFormProperties();
            BuildUi();
        }

        private void SetupFormProperties()
        {
            Text = _customer is not null ? $"Customer Details - {_customer.FullName}" : "Customer Details";
            Size = new Size(760, 680);
            MinimumSize = new Size(620, 500);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(244, 247, 251);
            DoubleBuffered = true;
        }

        private void BuildUi()
        {
            Controls.Clear();
            if (_customer == null || _controller == null) return;

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
                Text = UiDetailCardHelper.GetInitials(_customer!.FullName),
                Font = new Font("Segoe UI", 15f, FontStyle.Bold),
                ForeColor = Color.FromArgb(29, 78, 216),      // #1D4ED8
                BackColor = Color.FromArgb(239, 246, 255),    // #EFF6FF
                Size = new Size(52, 52),
                Location = new Point(24, 18),
                TextAlign = ContentAlignment.MiddleCenter
            };
            UiRadiusHelper.MakeCircularAvatar(lblAvatar);
            _pnlHeader.Controls.Add(lblAvatar);

            // Title & Subtitle block
            var lblName = new Label
            {
                Text = _customer.FullName,
                Font = new Font("Segoe UI", 15.5f, FontStyle.Bold),
                ForeColor = Theme.TextPrimary,
                Location = new Point(88, 18),
                AutoSize = true
            };
            _pnlHeader.Controls.Add(lblName);

            var regDate = _customer.CreatedAt != default ? _customer.CreatedAt.ToString("MMM d, yyyy") : "N/A";
            var lblMeta = new Label
            {
                Text = $"Customer #{_customer.CustomerId}   •   Customer Since {regDate}",
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = UiDetailCardHelper.LabelMutedColor,
                Location = new Point(88, 48),
                AutoSize = true
            };
            _pnlHeader.Controls.Add(lblMeta);

            // Badges in Header
            var (sBg, sFg, sStroke) = UiDetailCardHelper.GetStatusColors(_customer.Status);
            var statusBadge = UiDetailCardHelper.CreatePillBadge(_customer.Status, sBg, sFg, sStroke);
            statusBadge.Name = "headerStatusBadge";
            _pnlHeader.Controls.Add(statusBadge);

            var typeBadge = UiDetailCardHelper.CreatePillBadge(
                _customer.Type,
                Color.FromArgb(241, 245, 249),
                Color.FromArgb(51, 65, 85),
                Color.FromArgb(203, 213, 225));
            typeBadge.Name = "headerTypeBadge";
            _pnlHeader.Controls.Add(typeBadge);

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
            bool canEdit = _customer is not null && RbacService.CanEditRecord(_customer.AssignedAgentId, _customer.CreatedByUserId);
            if (canEdit)
            {
                _btnEdit = new Button
                {
                    Text = "✏️ Edit Customer",
                    Size = new Size(130, 36),
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
            bool hasEmail = !string.IsNullOrWhiteSpace(_customer?.Email);
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

            // --- Card 1: Contact Information ---
            var cardContact = CreateCardPanel(ref currentY);
            UiDetailCardHelper.AddControl(cardContact, UiDetailCardHelper.CreateCardHeader("👤  Contact Information"));
            UiDetailCardHelper.AddControl(cardContact, UiDetailCardHelper.CreateDivider());
            UiDetailCardHelper.AddControl(cardContact, UiDetailCardHelper.CreateKeyValueRow(
                "Email Address", string.IsNullOrWhiteSpace(_customer!.Email) ? "Not Provided" : _customer.Email,
                "Phone Number", string.IsNullOrWhiteSpace(_customer.Phone) ? "Not Provided" : _customer.Phone));
            UiDetailCardHelper.AddControl(cardContact, UiDetailCardHelper.CreateKeyValueRow(
                "Customer Type", _customer.Type,
                "Account Status", _customer.Status));
            FinalizeCardHeight(cardContact, ref currentY);
            _pnlContent.Controls.Add(cardContact);

            // --- Card 2: Ownership & Assignment ---
            var cardOwner = CreateCardPanel(ref currentY);
            UiDetailCardHelper.AddControl(cardOwner, UiDetailCardHelper.CreateCardHeader("👥  Ownership & Assignment"));
            UiDetailCardHelper.AddControl(cardOwner, UiDetailCardHelper.CreateDivider());
            var agentName = _controller!.GetAssignedAgentName(_customer.AssignedAgentId);
            UiDetailCardHelper.AddControl(cardOwner, UiDetailCardHelper.CreateKeyValueRow(
                "Assigned Agent", agentName ?? "Unassigned",
                "Assignment Review", _customer.AssignmentStatus));
            if (!string.IsNullOrWhiteSpace(_customer.AssignmentReviewNotes))
            {
                UiDetailCardHelper.AddControl(cardOwner, UiDetailCardHelper.CreateKeyValueRow(
                    "Review Notes", _customer.AssignmentReviewNotes));
            }
            FinalizeCardHeight(cardOwner, ref currentY);
            _pnlContent.Controls.Add(cardOwner);

            // --- Card 3: Owned Properties (For Sellers/Both) ---
            bool isSeller = string.Equals(_customer.Type, "seller", StringComparison.OrdinalIgnoreCase)
                || string.Equals(_customer.Type, "both", StringComparison.OrdinalIgnoreCase);

            if (isSeller)
            {
                var properties = _controller.GetOwnedProperties(_customer.CustomerId);
                var cardProps = CreateCardPanel(ref currentY);
                UiDetailCardHelper.AddControl(cardProps, UiDetailCardHelper.CreateCardHeader("🏢  Owned Properties", properties.Count.ToString()));
                UiDetailCardHelper.AddControl(cardProps, UiDetailCardHelper.CreateDivider());

                if (properties.Count == 0)
                {
                    var lblEmpty = new Label
                    {
                        Text = "No property listings currently linked to this customer.",
                        Font = new Font("Segoe UI", 9.5f, FontStyle.Italic),
                        ForeColor = UiDetailCardHelper.LabelMutedColor,
                        Dock = DockStyle.Top,
                        Height = 36,
                        TextAlign = ContentAlignment.MiddleLeft
                    };
                    UiDetailCardHelper.AddControl(cardProps, lblEmpty);
                }
                else
                {
                    foreach (var p in properties)
                    {
                        var propTile = CreatePropertyTile(p);
                        UiDetailCardHelper.AddControl(cardProps, propTile);
                    }
                }
                FinalizeCardHeight(cardProps, ref currentY);
                _pnlContent.Controls.Add(cardProps);
            }

            // --- Card 4: Recent Activities ---
            var activities = _controller.GetActivityHistory(_customer.CustomerId);
            var cardActivities = CreateCardPanel(ref currentY);
            UiDetailCardHelper.AddControl(cardActivities, UiDetailCardHelper.CreateCardHeader("⏱  Recent Activities", activities.Count.ToString()));
            UiDetailCardHelper.AddControl(cardActivities, UiDetailCardHelper.CreateDivider());

            if (activities.Count == 0)
            {
                var lblEmpty = new Label
                {
                    Text = "No activities logged yet for this customer.",
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Italic),
                    ForeColor = UiDetailCardHelper.LabelMutedColor,
                    Dock = DockStyle.Top,
                    Height = 36,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                UiDetailCardHelper.AddControl(cardActivities, lblEmpty);
            }
            else
            {
                foreach (var a in activities.Take(15))
                {
                    var item = CreateTimelineItem(a);
                    UiDetailCardHelper.AddControl(cardActivities, item);
                }
            }
            FinalizeCardHeight(cardActivities, ref currentY);
            _pnlContent.Controls.Add(cardActivities);

            Controls.Add(_pnlContent);
            _pnlContent.BringToFront();
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
            // Calculate height from stacked controls
            int totalHeight = card.Padding.Top;
            foreach (Control c in card.Controls)
            {
                totalHeight += c.Height + c.Margin.Bottom;
            }
            totalHeight += card.Padding.Bottom + 4;
            card.Height = totalHeight;

            currentY += totalHeight + 14;
        }

        private Panel CreatePropertyTile(Property p)
        {
            var tile = new Panel
            {
                Height = 54,
                Dock = DockStyle.Top,
                BackColor = UiDetailCardHelper.TileBg,
                Margin = new Padding(0, 0, 0, 8),
                Padding = new Padding(12, 8, 12, 8)
            };

            tile.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, tile.Width - 1, tile.Height - 1);
                using var path = UiRadiusHelper.CreateRoundedPath(rect, 6);
                using var pen = new Pen(UiDetailCardHelper.BorderColor, 1f);
                e.Graphics.DrawPath(pen, path);
            };
            UiRadiusHelper.ApplyRoundedCorners(tile, 6);

            var lblIcon = new Label
            {
                Text = "🏢",
                Font = new Font("Segoe UI Emoji", 14f),
                Location = new Point(10, 10),
                Size = new Size(28, 28),
                TextAlign = ContentAlignment.MiddleCenter
            };
            tile.Controls.Add(lblIcon);

            var lblAddr = new Label
            {
                Text = p.Address,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = UiDetailCardHelper.ValueTextColor,
                Location = new Point(44, 8),
                AutoSize = true
            };
            tile.Controls.Add(lblAddr);

            var lblDetails = new Label
            {
                Text = $"{p.PropertyType}  •  ₱{p.Price:N2}",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = UiDetailCardHelper.LabelMutedColor,
                Location = new Point(44, 28),
                AutoSize = true
            };
            tile.Controls.Add(lblDetails);

            var (pBg, pFg, pStroke) = UiDetailCardHelper.GetStatusColors(p.Status);
            var badge = UiDetailCardHelper.CreatePillBadge(p.Status, pBg, pFg, pStroke);
            badge.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            badge.Location = new Point(tile.Width - badge.Width - 14, 14);
            tile.Controls.Add(badge);

            return tile;
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

            // Date chip
            var dateBadge = UiDetailCardHelper.CreatePillBadge(
                a.ActivityDate.ToLocalTime().ToString("MMM d, yyyy"),
                Color.FromArgb(241, 245, 249),
                Color.FromArgb(71, 85, 105),
                Color.FromArgb(226, 232, 240));
            dateBadge.Location = new Point(0, 8);
            item.Controls.Add(dateBadge);

            // Type badge
            var typeBadge = UiDetailCardHelper.CreatePillBadge(
                a.Type,
                Color.FromArgb(239, 246, 255),
                Color.FromArgb(29, 78, 216),
                Color.FromArgb(191, 219, 254));
            typeBadge.Location = new Point(dateBadge.Right + 8, 8);
            item.Controls.Add(typeBadge);

            // Notes
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
                var statusBadge = _pnlHeader.Controls["headerStatusBadge"];
                var typeBadge = _pnlHeader.Controls["headerTypeBadge"];

                int right = _pnlHeader.ClientSize.Width - 24;
                if (statusBadge != null)
                {
                    statusBadge.Location = new Point(right - statusBadge.Width, 22);
                    right -= (statusBadge.Width + 8);
                }
                if (typeBadge != null)
                {
                    typeBadge.Location = new Point(right - typeBadge.Width, 22);
                }
            }

            if (_pnlFooter != null)
            {
                // Left-aligned contextual action
                if (_btnMessage != null)
                {
                    _btnMessage.Location = new Point(24, 13);
                }

                // Right-aligned dialog actions
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
            if (_customer == null || _controller == null) return;

            using var editForm = new CustomerInputForm(_customer);
            if (editForm.ShowDialog(this) == DialogResult.OK && editForm.Result is not null)
            {
                _controller.Update(editForm.Result);
                _customer = _controller.GetById(_customer.CustomerId) ?? editForm.Result;
                BuildUi();
            }
        }

        private void BtnMessageClick(object? sender, EventArgs e)
        {
            if (_customer == null || string.IsNullOrWhiteSpace(_customer.Email)) return;

            using var emailForm = new EmailMessageForm(_customer.FullName, _customer.Email);
            emailForm.ShowDialog(this);
        }
    }
}

using CRMS_Peguit.winforms.Models.Services;

namespace CRMS_Peguit.winforms.Views.Shared
{
    public class EmailMessageForm : Form
    {
        private readonly string _recipientName;

        private TextBox txtRecipient = null!;
        private TextBox txtSubject = null!;
        private TextBox txtBody = null!;
        private Button btnSend = null!;
        private Button btnCancel = null!;

        public EmailMessageForm(string recipientName, string? recipientEmail)
        {
            _recipientName = recipientName;
            InitializeComponent();
            txtRecipient.Text = recipientEmail ?? string.Empty;
        }

        private void InitializeComponent()
        {
            Width = 560;
            Height = 520;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            Text = $"Message {_recipientName}";
            BackColor = Theme.Background;
            ForeColor = Theme.TextPrimary;
            Font = new Font("Segoe UI", 10);

            var lblRecipient = CreateLabel("Recipient Email *", 20, 20);
            txtRecipient = CreateTextBox(20, 45, 500);

            var lblSubject = CreateLabel("Subject *", 20, 95);
            txtSubject = CreateTextBox(20, 120, 500);
            txtSubject.Text = $"Follow up from NEXA";

            var lblBody = CreateLabel("Message *", 20, 170);
            txtBody = CreateTextBox(20, 195, 500);
            txtBody.Height = 185;
            txtBody.Multiline = true;
            txtBody.ScrollBars = ScrollBars.Vertical;

            btnSend = new Button
            {
                Text = "Send",
                Location = new Point(330, 410),
                Width = 90,
                Height = 38,
                BackColor = Theme.Primary,
                ForeColor = Theme.Surface,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSend.FlatAppearance.BorderSize = 0;
            btnSend.Click += BtnSendClick;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(430, 410),
                Width = 90,
                Height = 38,
                BackColor = Theme.Surface,
                ForeColor = Theme.TextPrimary,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };

            Controls.AddRange(new Control[]
            {
                lblRecipient,
                txtRecipient,
                lblSubject,
                txtSubject,
                lblBody,
                txtBody,
                btnSend,
                btnCancel
            });

            AcceptButton = btnSend;
            CancelButton = btnCancel;
        }

        private static Label CreateLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                ForeColor = Theme.TextPrimary
            };
        }

        private static TextBox CreateTextBox(int x, int y, int width)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Width = width,
                BackColor = Theme.Surface,
                ForeColor = Theme.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private async void BtnSendClick(object? sender, EventArgs e)
        {
            btnSend.Enabled = false;
            btnSend.Text = "Sending...";

            try
            {
                var result = await ContactEmailService.SendAsync(
                    txtRecipient.Text.Trim(),
                    txtSubject.Text.Trim(),
                    txtBody.Text.Trim());

                MessageBox.Show(
                    result.Message,
                    result.Success ? "Email Sent" : "Email Error",
                    MessageBoxButtons.OK,
                    result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

                if (result.Success)
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
            finally
            {
                btnSend.Enabled = true;
                btnSend.Text = "Send";
            }
        }
    }
}

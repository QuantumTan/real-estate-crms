using CRMS_Peguit.winforms.Models.Services;

namespace CRMS_Peguit.winforms.Views.Shared
{
    public partial class EmailMessageForm : Form
    {
        private readonly string _recipientName;

        public string SentSubject { get; private set; } = string.Empty;

        public EmailMessageForm() : this("Recipient", string.Empty)
        {
        }

        public EmailMessageForm(string recipientName, string? recipientEmail, string? defaultSubject = null)
        {
            _recipientName = recipientName;
            InitializeComponent();
            UiRadiusHelper.StyleButton(btnSend, 8);
            UiRadiusHelper.StyleButton(btnCancel, 8);
            UiRadiusHelper.AttachHoverFeedback(btnCancel, Color.White, Color.FromArgb(241, 245, 249));
            UiRadiusHelper.AttachHoverFeedback(btnSend, Theme.Primary, Theme.PrimaryDark);
            this.Text = $"Message {_recipientName}";
            btnSend.Click += BtnSendClick;
            txtRecipient.Text = recipientEmail ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(defaultSubject))
            {
                txtSubject.Text = defaultSubject.Trim();
            }
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
                    SentSubject = txtSubject.Text.Trim();
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


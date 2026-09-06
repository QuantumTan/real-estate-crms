using System.Net.Mail;

namespace CRMS_Peguit.winforms.Models.Services
{
    public static class ContactEmailService
    {
        public static bool IsValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            try
            {
                var address = new MailAddress(email.Trim());
                return string.Equals(address.Address, email.Trim(), StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public static EmailSendResult SendPlaceholder(string recipientEmail, string subject, string body)
        {
            if (!IsValidEmail(recipientEmail))
            {
                return EmailSendResult.Failed("A valid recipient email is required.");
            }

            if (string.IsNullOrWhiteSpace(subject))
            {
                return EmailSendResult.Failed("Subject is required.");
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                return EmailSendResult.Failed("Message is required.");
            }

            return EmailSendResult.Succeeded(
                "SMTP is not configured yet. This message is ready to send once SMTP settings are connected.");
        }
    }

    public sealed class EmailSendResult
    {
        public bool Success { get; }
        public string Message { get; }

        private EmailSendResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public static EmailSendResult Succeeded(string message) => new(true, message);
        public static EmailSendResult Failed(string message) => new(false, message);
    }
}

using CRMS_Peguit.winforms.Models.Services;

namespace CRMS_Peguit.winforms.Views.Shared
{
    public partial class PlaceholderView : UserControl
    {
        public PlaceholderView(string title, string message)
        {
            BackColor = Theme.Background;
            Padding = new Padding(30);

            Controls.Add(new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Theme.TextPrimary,
                Location = new Point(30, 25),
                AutoSize = true
            });

            Controls.Add(new Label
            {
                Text = message,
                Font = new Font("Segoe UI", 11),
                ForeColor = Theme.TextSecondary,
                Location = new Point(30, 85),
                Size = new Size(720, 70)
            });
        }
    }
}

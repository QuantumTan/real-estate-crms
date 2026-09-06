using CRMS_Peguit.winforms.Models.Services;

namespace CRMS_Peguit.winforms.Views.Shared
{
    public partial class PlaceholderView : UserControl
    {
        public PlaceholderView() : this("Module", "Content coming soon.")
        {
        }

        public PlaceholderView(string title, string message)
        {
            InitializeComponent();

            BackColor = Color.FromArgb(244, 247, 251);
            Padding = new Padding(30);

            Controls.Add(new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(30, 25),
                AutoSize = true
            });

            Controls.Add(new Label
            {
                Text = message,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(32, 65),
                Size = new Size(720, 70)
            });
        }
    }
}

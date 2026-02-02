using System;
using System.Drawing;
using System.Windows.Forms;

namespace ClassPointAddIn.Views
{
    public partial class QuickPollOverlay : Form
    {
        public event EventHandler QuickPollClicked;

        public QuickPollOverlay()
        {
            InitializeComponent();
            InitializeOverlay();
        }

        private void InitializeOverlay()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.BackColor = Color.White;
            this.Opacity = 0.85;
            this.ShowInTaskbar = false;
            this.Size = new Size(70, 70);
            this.StartPosition = FormStartPosition.Manual;

            Button pollButton = new Button();
            pollButton.Text = "📊";
            pollButton.Font = new Font("Segoe UI Emoji", 24);
            pollButton.Size = new Size(60, 60);
            pollButton.Location = new Point(5, 5);
            pollButton.FlatStyle = FlatStyle.Flat;
            pollButton.FlatAppearance.BorderSize = 0;
            pollButton.BackColor = Color.FromArgb(66, 133, 244);
            pollButton.ForeColor = Color.White;
            pollButton.Cursor = Cursors.Hand;
            pollButton.Click += (s, e) => QuickPollClicked?.Invoke(this, EventArgs.Empty);

            this.Controls.Add(pollButton);

            Screen screen = Screen.PrimaryScreen;
            int x = screen.WorkingArea.Right - this.Width - 20;
            int y = screen.WorkingArea.Bottom - this.Height - 200;
            this.Location = new Point(x, y);
        }
    }
}

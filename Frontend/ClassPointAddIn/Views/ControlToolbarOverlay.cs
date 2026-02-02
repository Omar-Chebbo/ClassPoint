using System;
using System.Drawing;
using System.Windows.Forms;

namespace ClassPointAddIn.Views
{
    public partial class ControlToolbarOverlay : Form
    {
        public event EventHandler PickClicked;
        public event EventHandler PollClicked;

        public ControlToolbarOverlay()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.BackColor = Color.FromArgb(245, 245, 245);
            this.Opacity = 0.95;
            this.Width = 250;
            this.Height = 90;

            // Rounded corners
            this.Region = new Region(new Rectangle(0, 0, this.Width, this.Height));

            var layout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(10, 10, 10, 10),
                BackColor = Color.Transparent
            };

            var btnPick = CreateToolbarButton("🎲", Color.Orange);
            var btnPoll = CreateToolbarButton("📊", Color.FromArgb(66, 133, 244));

            btnPick.Click += (s, e) => PickClicked?.Invoke(this, EventArgs.Empty);
            btnPoll.Click += (s, e) => PollClicked?.Invoke(this, EventArgs.Empty);

            layout.Controls.Add(btnPick);
            layout.Controls.Add(btnPoll);

            this.Controls.Add(layout);

            // Position: bottom center
            int x = (Screen.PrimaryScreen.Bounds.Width / 2) - (this.Width / 2);
            int y = Screen.PrimaryScreen.Bounds.Height - this.Height - 20;
            this.Location = new Point(x, y);
        }

        private Button CreateToolbarButton(string icon, Color color)
        {
            var btn = new Button
            {
                Text = icon,
                Font = new Font("Segoe UI Emoji", 22, FontStyle.Bold),
                Width = 65,
                Height = 65,
                FlatStyle = FlatStyle.Flat,
                BackColor = color,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };

            btn.FlatAppearance.BorderSize = 0;

            // make button round
            btn.Paint += (s, e) =>
            {
                var gp = new System.Drawing.Drawing2D.GraphicsPath();
                gp.AddEllipse(0, 0, btn.Width, btn.Height);
                btn.Region = new Region(gp);
            };

            return btn;
        }
    }
}

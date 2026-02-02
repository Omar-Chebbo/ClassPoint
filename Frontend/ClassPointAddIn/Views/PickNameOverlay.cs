using System;
using System.Drawing;
using System.Windows.Forms;

namespace ClassPointAddIn.Views
{
    public partial class PickNameOverlay : Form
    {
        private Button btnPick;

        public PickNameOverlay()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.TopMost = true;
            this.BackColor = Color.White;
            this.TransparencyKey = Color.White; // makes background transparent
            this.Width = 100;
            this.Height = 100;
            this.ShowInTaskbar = false;

            btnPick = new Button
            {
                Text = "🎲",
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                BackColor = Color.Orange,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Width = 70,
                Height = 70
            };

            btnPick.FlatAppearance.BorderSize = 0;
            btnPick.Cursor = Cursors.Hand;
            btnPick.Location = new Point(10, 10);
            btnPick.Region = new Region(new System.Drawing.Drawing2D.GraphicsPath(
                new[] {
                    new Point(0, 35),
                    new Point(35, 0),
                    new Point(70, 35),
                    new Point(35, 70)
                },
                new byte[] {
                    (byte)System.Drawing.Drawing2D.PathPointType.Start,
                    (byte)System.Drawing.Drawing2D.PathPointType.Bezier,
                    (byte)System.Drawing.Drawing2D.PathPointType.Bezier,
                    (byte)System.Drawing.Drawing2D.PathPointType.Bezier
                }));

            // Make the button round
            btnPick.Paint += (s, e) =>
            {
                var gp = new System.Drawing.Drawing2D.GraphicsPath();
                gp.AddEllipse(0, 0, btnPick.Width, btnPick.Height);
                btnPick.Region = new Region(gp);
            };

            btnPick.Click += (s, e) =>
            {
                var pickForm = new PickNameForm(ThisAddIn.StudentsCache);
                pickForm.ShowDialog();
            };

            this.Controls.Add(btnPick);

            // Position bottom-left corner of screen
            this.Location = new Point(20, Screen.PrimaryScreen.Bounds.Height - 120);
        }
    }
}

using ClassPointAddIn.API.Service;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClassPointAddIn.Views
{
    // ==== Custom bar drawing panel ====
    public sealed class BarPanel : Panel
    {
        public double Ratio { get; set; } = 0.0; // 0..1
        public Color BarColor { get; set; } = Color.SteelBlue;
        private const int MinVisiblePx = 3;

        public BarPanel()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint, true);
            BackColor = Color.FromArgb(240, 240, 240);
        }

        protected override void OnPaint(PaintEventArgs e)
        {

            using (var back = new SolidBrush(BackColor))
                e.Graphics.FillRectangle(back, ClientRectangle);

            double r = Math.Max(0.0, Math.Min(1.0, Ratio));
            if (r <= 0.0) return;

            int filled = (int)Math.Round(ClientSize.Height * r);
            if (filled < MinVisiblePx) filled = MinVisiblePx;

            if (filled > 0)
            {
                var rect = new Rectangle(0, ClientSize.Height - filled, ClientSize.Width, filled);
                using (var b = new SolidBrush(BarColor))
                    e.Graphics.FillRectangle(b, rect);
            }
        }
    }

    // ==== Main QuickPoll Results Form ====
    public partial class QuickPollResultForm : Form
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private readonly string pollCode;
        private readonly QuickPollApiClient api;
        private Timer updateTimer;
        private TableLayoutPanel chartPanel;
        private Label lblParticipants;
        private Label lblTimer;
        private Button btnClose;
        private Label lblCode;
        private DateTime startTime;

        public QuickPollResultForm(string pollCode)
        {
            this.pollCode = pollCode;
            this.api = new QuickPollApiClient();

            InitializeComponent();
            SetupUI();
            StartUpdating();
        }

        private void SetupUI()
        {
            // ===== Window =====
            this.Text = "Quick Poll";
            this.Size = new Size(1200, 650);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.Padding = new Padding(12);
            this.Opacity = 0.99;

            // ===== Header =====
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(30, 30, 30)
            };
            header.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, 0xA1, 0x2, 0);
                }
            };

            var lblTitle = new Label
            {
                Text = "Quick Poll Results",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 25)
            };

            lblCode = new Label
            {
                Text = $"Go to classpoint.app and use Code {pollCode}",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 215, 0),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            header.Controls.Add(lblTitle);
            header.Controls.Add(lblCode);
            Controls.Add(header);

            // ===== Chart area =====
            chartPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(30, 100, 30, 20),
                ColumnCount = 1,
                RowCount = 1,
                AutoScroll = false
            };
            Controls.Add(chartPanel);

            // ===== Footer =====
            var footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 100,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            lblParticipants = new Label
            {
                Text = "👥 0",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(25, 38),
                ForeColor = Color.Black
            };

            lblTimer = new Label
            {
                Text = "⏱ 00:00",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(110, 38),
                ForeColor = Color.Black
            };

            btnClose = new Button
            {
                Text = "Close Poll",
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Width = 220,
                Height = 50,
                Location = new Point(380, 25),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += async (s, e) => await ClosePollAsync();

            footer.Controls.Add(lblParticipants);
            footer.Controls.Add(lblTimer);
            footer.Controls.Add(btnClose);
            Controls.Add(footer);

            startTime = DateTime.Now;
        }

        // ===== Timer update =====
        private void StartUpdating()
        {
            updateTimer = new Timer { Interval = 4000 };
            updateTimer.Tick += async (s, e) =>
            {
                await UpdateResultsAsync();

                var elapsed = DateTime.Now - startTime;
                string formatted = $"⏱ {elapsed.Minutes:D2}:{elapsed.Seconds:D2}";

                if (lblTimer.InvokeRequired)
                    lblTimer.Invoke((Action)(() => lblTimer.Text = formatted));
                else
                    lblTimer.Text = formatted;
            };
            updateTimer.Start();
        }

        // ===== Fetch results =====
        private async Task UpdateResultsAsync()
        {
            try
            {
                string json = await api.GetResultsAsync(pollCode);
                var data = JObject.Parse(json);

                // adapt if backend returns different structure
                var options = data["results"] ?? data["options"];

                if (options == null)
                {
                    MessageBox.Show("No results found for this poll.", "Quick Poll", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (options == null)
                    throw new Exception("Invalid results format");

                int totalVotes = 0;

                foreach (var opt in options)
                {
                    int count = 0;

                    if (opt["count"] != null && int.TryParse(opt["count"].ToString(), out int c1))
                        count = c1;
                    else if (opt["vote_count"] != null && int.TryParse(opt["vote_count"].ToString(), out int c2))
                        count = c2;

                    totalVotes += count;
                }


                if (InvokeRequired)
                    Invoke((Action)(() => RenderResults(options, totalVotes)));
                else
                    RenderResults(options, totalVotes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating poll: {ex.Message}");
            }
        }

        private void RenderResults(JToken options, int total)
        {
            lblParticipants.Text = $"👥 {total}";
            var dict = new Dictionary<string, int>();

            foreach (var opt in options)
            {
                string text =
                    opt["text"]?.ToString() ??
                    opt["option"]?.ToString() ??
                    "Option";

                int count =
                    (int?)opt["count"] ??
                    (int?)opt["vote_count"] ??
                    0;

                dict[text] = count;
            }

            DisplayChart(dict, total);
        }

        // ===== Chart display =====
        private void DisplayChart(Dictionary<string, int> options, int totalVotes)
        {
            if (InvokeRequired)
            {
                Invoke((Action)(() => DisplayChart(options, totalVotes)));
                return;
            }

            int optionCount = options.Count;
            if (optionCount == 0) return;

            // === FIRST-TIME CHART BUILD ===
            if (chartPanel.Controls.Count == 0)
            {
                chartPanel.Controls.Clear();
                chartPanel.SuspendLayout();

                var rowPanel = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.White,
                    ColumnCount = optionCount,
                    RowCount = 1,
                    Padding = new Padding(20, 80, 20, 40)
                };

                for (int i = 0; i < optionCount; i++)
                    rowPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / optionCount));

                Color[] palette =
                {
            Color.FromArgb(52, 152, 219),
            Color.FromArgb(46, 204, 113),
            Color.FromArgb(241, 196, 15),
            Color.FromArgb(231, 76, 60),
            Color.FromArgb(155, 89, 182),
            Color.FromArgb(26, 188, 156)
        };

                int index = 0;
                foreach (var kv in options)
                {
                    // Container per option
                    var container = new Panel
                    {
                        Dock = DockStyle.Fill,
                        BackColor = Color.White
                    };

                    // Option label (bottom)
                    var lblOption = new Label
                    {
                        Text = kv.Key,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Dock = DockStyle.Bottom,
                        Font = new Font("Segoe UI", 13, FontStyle.Bold),
                        Height = 45,
                        Name = $"lblOption_{kv.Key}"
                    };

                    // Value label (top)
                    var lblValue = new Label
                    {
                        Text = $"{kv.Value} (0%)",
                        TextAlign = ContentAlignment.MiddleCenter,
                        Dock = DockStyle.Top,
                        Font = new Font("Segoe UI Semibold", 12),
                        Height = 35,
                        Name = $"lblValue_{kv.Key}"
                    };

                    // ---- FIX: bar must be added LAST to stay visible ----
                    var barPanel = new BarPanel
                    {
                        Dock = DockStyle.Fill,
                        Margin = new Padding(20, 10, 20, 10),
                        Name = $"bar_{kv.Key}",
                        BarColor = palette[index++ % palette.Length],
                        Ratio = 0.0
                    };

                    // Correct order → bar is visible
                    container.Controls.Add(lblOption);    // bottom
                    container.Controls.Add(lblValue);     // top
                    container.Controls.Add(barPanel);     // fill (top-most)

                    rowPanel.Controls.Add(container);
                }

                chartPanel.Controls.Add(rowPanel);
                chartPanel.ResumeLayout();
            }

            // ==== UPDATE BARS ====
            int safeTotal = Math.Max(1, totalVotes);

            foreach (var kv in options)
            {
                var lblValue = chartPanel.Controls.Find($"lblValue_{kv.Key}", true).FirstOrDefault() as Label;
                var barPanel = chartPanel.Controls.Find($"bar_{kv.Key}", true).FirstOrDefault() as BarPanel;
                if (lblValue == null || barPanel == null) continue;

                double percent = totalVotes == 0 ? 0 : (kv.Value * 100.0 / safeTotal);
                lblValue.Text = $"{kv.Value} ({percent:0}%)";

                // Smooth animation (optional)
                double target = kv.Value / (double)safeTotal;
                barPanel.Ratio = target;
                barPanel.Refresh(); // forces redraw
            }
        }


        // ===== Close poll =====
        private async Task ClosePollAsync()
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke((Action)(async () => await ClosePollAsync()));
                    return;
                }

                btnClose.Enabled = false;
                btnClose.BackColor = Color.Gray;

                await api.ClosePollAsync(pollCode);
                updateTimer?.Stop();

                this.BackColor = Color.FromArgb(220, 220, 220);
                await Task.Delay(800);

                for (double i = 0.99; i >= 0; i -= 0.05)
                {
                    if (InvokeRequired)
                    {
                        Invoke((Action)(() => this.Opacity = i));
                    }
                    else
                    {
                        this.Opacity = i;
                    }
                    await Task.Delay(30);
                }

                if (InvokeRequired)
                {
                    Invoke((Action)(() => this.Close()));
                }
                else
                {
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                if (InvokeRequired)
                {
                    Invoke((Action)(() =>
                        MessageBox.Show($"Error closing poll: {ex.Message}",
                        "Quick Poll Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    ));
                }
                else
                {
                    MessageBox.Show($"Error closing poll: {ex.Message}",
                        "Quick Poll Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

    }
}

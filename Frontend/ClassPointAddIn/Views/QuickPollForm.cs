using ClassPointAddIn.API.Service;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClassPointAddIn.Views
{
    public class QuickPollForm : Form
    {
        private readonly QuickPollApiClient _api;
        public event EventHandler<string> PollTypeSelected;
        private TextBox txtPollName;  // ✅ NEW


        public QuickPollForm()
        {
            _api = new QuickPollApiClient();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // ====== WINDOW SETTINGS ======
            this.Text = "Quick Poll";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new Size(520, 400);
            this.TopMost = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);

            // ====== MAIN CONTAINER ======
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                ColumnCount = 1,
                RowCount = 5
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));  // Title
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // Top buttons
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));   // Label
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // Numeric buttons
            this.Controls.Add(layout);

            // ====== TITLE ======
            var lblTitle = new Label
            {
                Text = "Select your poll type:",
                Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            layout.Controls.Add(lblTitle, 0, 0);
            // ====== POLL NAME INPUT ======
            var lblName = new Label
            {
                Text = "Enter a name for this poll:",
                Font = new Font("Segoe UI", 11F, FontStyle.Regular),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            layout.Controls.Add(lblName, 0, 1);

            txtPollName = new TextBox
            {
                Text = "e.g., Math Quiz 1",
                ForeColor = Color.Gray,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12F),
                TextAlign = HorizontalAlignment.Center
            };

            // ====== Placeholder Behavior ======
            txtPollName.GotFocus += (s, e) =>
            {
                if (txtPollName.Text == "e.g., Math Quiz 1")
                {
                    txtPollName.Text = "";
                    txtPollName.ForeColor = Color.Black;
                }
            };

            txtPollName.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtPollName.Text))
                {
                    txtPollName.Text = "e.g., Math Quiz 1";
                    txtPollName.ForeColor = Color.Gray;
                }
            };

            layout.Controls.Add(txtPollName, 0, 2);



            // ====== TOP BUTTONS (True/False, Yes/No/Unsure) ======
            var topButtonsPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = false,
                Padding = new Padding(0),
                Margin = new Padding(0, 10, 0, 10)
            };

            var btnTrueFalse = CreatePollButton("True / False");
            var btnYesNoUnsure = CreatePollButton("Yes / No / Unsure");

            topButtonsPanel.Controls.Add(btnTrueFalse);
            topButtonsPanel.Controls.Add(btnYesNoUnsure);
            layout.Controls.Add(topButtonsPanel, 0, 3);

            // ====== LABEL FOR NUMERIC OPTIONS ======
            var lblNumeric = new Label
            {
                Text = "Or select the number of options for your poll:",
                Font = new Font("Segoe UI", 11F, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            layout.Controls.Add(lblNumeric, 0, 4);

            // ====== NUMERIC BUTTONS (2–6) ======
            var numButtonsPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = false,
                Padding = new Padding(0, 5, 0, 5)
            };

            for (int i = 2; i <= 6; i++)
            {
                numButtonsPanel.Controls.Add(CreatePollButton(i.ToString(), 80));
            }

            layout.Controls.Add(numButtonsPanel, 0, 5);

            // ====== CENTER ELEMENTS ON LOAD ======
            this.Load += (s, e) =>
            {
                CenterPanel(topButtonsPanel);
                CenterPanel(numButtonsPanel);
            };
        }

        private Button CreatePollButton(string text, int width = 150)
        {
            var btn = new Button
            {
                Text = text,
                Width = width,
                Height = 60,
                Font = new Font("Segoe UI", 12F, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.WhiteSmoke,
                Margin = new Padding(10),
                Cursor = Cursors.Hand
            };

            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(230, 240, 250);

            btn.Click += async (s, e) =>
            {
                string pollType = text;
                await CreatePollAsync(pollType);
            };

            return btn;
        }

        private void CenterPanel(FlowLayoutPanel panel)
        {
            if (panel.Parent != null)
            {
                panel.Left = Math.Max(0, (panel.Parent.ClientSize.Width - panel.PreferredSize.Width) / 2);
                panel.Anchor = AnchorStyles.None;
            }
        }

        // ====== CREATE POLL ======
        private async Task CreatePollAsync(string pollType)
        {
            try
            {
                // Determine question type
                string questionType;

                if (pollType == "True / False")
                    questionType = "true_false";
                else if (pollType == "Yes / No / Unsure")
                    questionType = "yes_no_unsure";
                else
                    questionType = "custom";


                // Handle numeric options
                int optionCount = int.TryParse(pollType, out int numeric) ? numeric : 2;

                // Use shared API client
                string pollName = txtPollName.Text.Trim();
                if (string.IsNullOrWhiteSpace(pollName))
                {
                    MessageBox.Show("Please enter a poll name before creating.", "Missing Poll Name",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string responseJson = await _api.CreatePollAsync(questionType, optionCount, pollName);




                dynamic result = JsonConvert.DeserializeObject(responseJson);
                string pollCode = result?.code;

                if (string.IsNullOrWhiteSpace(pollCode))
                    throw new Exception("Invalid poll response: missing code");

                // ✅ Hide setup form
                this.Hide();

                // ✅ Show poll results window
                var resultForm = new QuickPollResultForm(pollCode);
                var pptWindowHandle = Process.GetCurrentProcess().MainWindowHandle;

                NativeWindow owner = new NativeWindow();
                owner.AssignHandle(pptWindowHandle);
                resultForm.Show(owner);

                await Task.Delay(300);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating poll:\n{ex.Message}",
                    "Quick Poll Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}

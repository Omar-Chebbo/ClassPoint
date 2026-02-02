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
    public partial class StudentVoteForm : Form
    {
        private readonly QuickPollApiClient _api = new QuickPollApiClient();

        private TextBox txtPollCode;
        private Button btnJoin;
        private FlowLayoutPanel optionsPanel;
        private Button btnSubmit;
        private Label lblStatus;

        private string currentPollCode;
        private int selectedOption = -1;
        private List<string> options = new List<string>();

        public StudentVoteForm()
        {
            InitializeComponent();

            // ✅ Attach the logged-in student's token to every API call
            if (!string.IsNullOrEmpty(ThisAddIn.StudentToken))
            {
                _api.SetBearer(ThisAddIn.StudentToken);
            }
            else
            {
                MessageBox.Show("No student token found. Please log in first.", "Authorization",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        private void InitializeComponent()
        {
            this.Text = "Join a Poll";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new Size(450, 500);
            this.TopMost = true;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                ColumnCount = 1,
                RowCount = 5
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // Code input
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60)); // Join button
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // Options
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60)); // Submit
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // Status
            this.Controls.Add(layout);

            // ====== Poll Code Input ======
            txtPollCode = new TextBox
            {
                Text = "Enter Poll Code",
                ForeColor = Color.Gray,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12F),
                TextAlign = HorizontalAlignment.Center
            };

            // Add placeholder behavior manually
            txtPollCode.GotFocus += (s, e) =>
            {
                if (txtPollCode.Text == "Enter Poll Code")
                {
                    txtPollCode.Text = "";
                    txtPollCode.ForeColor = Color.Black;
                }
            };

            txtPollCode.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtPollCode.Text))
                {
                    txtPollCode.Text = "Enter Poll Code";
                    txtPollCode.ForeColor = Color.Gray;
                }
            };

            layout.Controls.Add(txtPollCode, 0, 0);

            // ====== Join Button ======
            btnJoin = new Button
            {
                Text = "Join Poll",
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Height = 45,
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat
            };
            btnJoin.FlatAppearance.BorderSize = 0;
            btnJoin.Click += async (s, e) => await LoadPollAsync();
            layout.Controls.Add(btnJoin, 0, 1);

            // ====== Options Panel ======
            optionsPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = false,
                Padding = new Padding(10),
                Visible = false
            };
            layout.Controls.Add(optionsPanel, 0, 2);

            // ====== Submit Button ======
            btnSubmit = new Button
            {
                Text = "Submit Vote",
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Height = 45,
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            btnSubmit.FlatAppearance.BorderSize = 0;
            btnSubmit.Click += async (s, e) => await SubmitVoteAsync();
            layout.Controls.Add(btnSubmit, 0, 3);

            // ====== Status Label ======
            lblStatus = new Label
            {
                Text = "",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Black
            };
            layout.Controls.Add(lblStatus, 0, 4);
        }

        // ====== Load Poll ======
        // ====== Load Poll ======
        private async Task LoadPollAsync()
        {
            try
            {
                string code = txtPollCode.Text.Trim();
                if (string.IsNullOrEmpty(code))
                {
                    MessageBox.Show("Please enter a poll code.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                currentPollCode = txtPollCode.Text.Trim();

                string json = await _api.GetResultsAsync(code);
                var data = JObject.Parse(json);

                var newOptions = new List<string>();

                SafeUI(() =>
                {
                    optionsPanel.Controls.Clear();
                    foreach (var opt in data["results"])
                    {
                        string text = opt["option"].ToString();
                        int id = (int)opt["id"];
                        Button btn = new Button
                        {
                            Text = text,
                            Tag = id,
                            Height = 40,
                            Width = 300,
                            BackColor = Color.White
                        };
                        btn.Click += (s, e) =>
                        {
                            selectedOption = (int)((Button)s).Tag;
                            foreach (Button b in optionsPanel.Controls) b.BackColor = Color.White;
                            ((Button)s).BackColor = Color.LightBlue;
                            btnSubmit.Enabled = true;
                        };
                        optionsPanel.Controls.Add(btn);
                    }

                    optionsPanel.Visible = true;
                    lblStatus.Text = "Poll joined successfully!";
                    lblStatus.ForeColor = Color.Green;
                });
            }
            catch (Exception ex)
            {
                SafeUI(() =>
                {
                    lblStatus.Text = $"Error: {ex.Message}";
                    lblStatus.ForeColor = Color.Red;
                });
            }
        }




        // ====== Submit Vote ======
        private async Task SubmitVoteAsync()
        {
            try
            {
                // === Step 1: Ask for student identity FIRST ===
                string name = Prompt.Input("Enter your full name:", "Identify Student", this);
                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("Name is required to vote.", "Missing Name",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string email = Prompt.Input("Enter your email (for tracking one vote only):", "Identify Student", this);
                if (string.IsNullOrWhiteSpace(email))
                {
                    MessageBox.Show("Email is required to vote once per poll.", "Missing Email",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // === Step 2: Validate option selection ===
                if (selectedOption == -1)
                {
                    MessageBox.Show("Please select an option before submitting.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // === Step 3: Submit the vote ===
                SafeUI(() =>
                {
                    lblStatus.Text = "Submitting your vote...";
                    lblStatus.ForeColor = Color.DimGray;
                    btnSubmit.Enabled = false;
                });

                var response = await _api.VoteAsync(currentPollCode, selectedOption, email, name);

                // === Step 4: Handle responses ===
                if (response.IsSuccessStatusCode)
                {
                    SafeUI(() =>
                    {
                        lblStatus.Text = "✅ Vote submitted successfully!";
                        lblStatus.ForeColor = Color.Green;
                        foreach (Button b in optionsPanel.Controls) b.Enabled = false;
                    });

                    await Task.Delay(1200);
                    SafeUI(() => this.Close());
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    SafeUI(() =>
                    {
                        lblStatus.Text = "⚠️ You have already voted in this poll.";
                        lblStatus.ForeColor = Color.Orange;
                        foreach (Button b in optionsPanel.Controls) b.Enabled = false;
                    });
                }
                else
                {
                    var body = await response.Content.ReadAsStringAsync();

                    // Try to extract a clean, readable message from backend JSON
                    string cleanMessage = body;
                    try
                    {
                        dynamic err = Newtonsoft.Json.JsonConvert.DeserializeObject(body);
                        cleanMessage =
                            err.detail != null ? (string)err.detail :
                            err.error != null ? (string)err.error :
                            err.non_field_errors != null ? string.Join(", ", err.non_field_errors.ToObject<string[]>()) :
                            body;
                    }
                    catch
                    {
                        // Fallback to plain text if JSON parsing fails
                        cleanMessage = body;
                    }

                    SafeUI(() =>
                    {
                        lblStatus.Text = $"⚠️ {cleanMessage}";
                        lblStatus.ForeColor = Color.Red;
                        btnSubmit.Enabled = true;
                    });

                    Console.WriteLine($"Backend Error: {cleanMessage}");
                }

            }
            catch (Exception ex)
            {
                SafeUI(() =>
                {
                    lblStatus.Text = $"Error: {ex.Message}";
                    lblStatus.ForeColor = Color.Red;
                    btnSubmit.Enabled = true;
                });
            }
        }





        private void SafeUI(Action action)
        {
            if (this.InvokeRequired)
                this.Invoke(action);
            else
                action();
        }


    } 
}

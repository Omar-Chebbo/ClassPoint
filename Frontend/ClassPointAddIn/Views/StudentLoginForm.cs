using ClassPointAddIn.API.Service;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClassPointAddIn.Views
{
    public partial class StudentLoginForm : Form
    {
        private readonly QuickPollApiClient _api = new QuickPollApiClient();

        private TextBox txtEmail;
        private TextBox txtPassword;
        private Button btnLogin;
        private Label lblStatus;

        public StudentLoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Student Login";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new System.Drawing.Size(350, 250);
            this.Font = new System.Drawing.Font("Segoe UI", 11F);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                ColumnCount = 1,
                RowCount = 4
            };
            this.Controls.Add(layout);

            txtEmail = new TextBox
            {
                Text = "Email",
                ForeColor = System.Drawing.Color.Gray,
                Dock = DockStyle.Fill
            };
            txtEmail.GotFocus += (s, e) =>
            {
                if (txtEmail.Text == "Email")
                {
                    txtEmail.Text = "";
                    txtEmail.ForeColor = System.Drawing.Color.Black;
                }
            };
            txtEmail.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtEmail.Text))
                {
                    txtEmail.Text = "Email";
                    txtEmail.ForeColor = System.Drawing.Color.Gray;
                }
            };

            

            btnLogin = new Button
            {
                Text = "Login",
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.FromArgb(52, 152, 219),
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold)
            };
            lblStatus = new Label { Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleCenter };

            btnLogin.Click += async (s, e) => await LoginAsync();

            layout.Controls.Add(txtEmail, 0, 0);
            
            layout.Controls.Add(btnLogin, 0, 2);
            layout.Controls.Add(lblStatus, 0, 3);
        }

        private async Task LoginAsync()
        {
            try
            {
                lblStatus.Text = "Logging in...";

                // Create HttpClient with old-style using block (valid for C# 7.3)
                using (var client = new HttpClient())
                {
                    string baseUrl = Properties.Settings.Default.QuickPollBase;
                    string url = baseUrl.Replace("quickpolls/", "token/"); // adjust if your backend uses a different path

                    var payloadObj = new
                    {
                        email = txtEmail.Text.Trim(),
                        password = txtPassword.Text.Trim()
                    };

                    string jsonData = JsonConvert.SerializeObject(payloadObj);
                    var httpContent = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    var httpResponse = await client.PostAsync(url, httpContent);
                    string responseBody = await httpResponse.Content.ReadAsStringAsync();

                    if (httpResponse.IsSuccessStatusCode)
                    {
                        dynamic data = JsonConvert.DeserializeObject(responseBody);
                        string token = data.access ?? data.token;

                        ThisAddIn.StudentToken = token;
                        MessageBox.Show("✅ Login successful!", "Login", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        lblStatus.Text = $"Login failed: {responseBody}";
                    }
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Error: {ex.Message}";
            }
        }

    }
}

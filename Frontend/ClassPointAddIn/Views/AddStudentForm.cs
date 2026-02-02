using System;
using System.Windows.Forms;
using ClassPointAddIn.Api.Service;

namespace ClassPointAddIn.Views
{
    public partial class AddStudentForm : Form
    {
        private readonly string _token;

        public AddStudentForm(string token)
        {
            _token = token;
            InitializeComponent();
        }

        private async void btnSubmit_Click(object sender, EventArgs e)
        {
            var fullName = txtFullName.Text.Trim();
            var email = txtEmail.Text.Trim();
          


            if (string.IsNullOrWhiteSpace(fullName))
            {
                MessageBox.Show("Please enter the student's full name.", "Missing Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var svc = new StudentService(_token);
                var result = await svc.AddStudentAsync(fullName, email);
                if (result == "OK")
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    MessageBox.Show(result, "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Request Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}

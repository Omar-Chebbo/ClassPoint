using ClassPointAddIn.API.Service;
using Microsoft.Office.Tools.Ribbon;
using System;
using System.Linq;
using System.Windows.Forms;
using ClassPointAddIn.Views;
using ClassPointAddIn.Users.Auth;
using ClassPointAddIn.Api.Service;

namespace ClassPointAddIn.Views
{
    public partial class ConnectRibbon
    {
        private string _token; // ✅ Store token globally after login

        private void ConnectRibbon_Load(object sender, RibbonUIEventArgs e)
        {
            // Hide some buttons until user logs in
            PickNameButton.Visible = false;
            btnAddStudent.Visible = false;

            // Wire up handlers
            this.PickNameButton.Click += PickNameButton_Click;
            this.btnAddStudent.Click += BtnAddStudent_Click;

            // ✅ Wire QuickPoll buttons
            this.btnQuickPoll.Click += btnQuickPoll_Click;
            this.btnJoinPoll.Click += btnJoinPoll_Click;
            this.btnShowResults.Click += btnShowResults_Click;
        }

        // ================== AUTHENTICATION ==================
        private async void Connect_Click(object sender, RibbonControlEventArgs e)
        {
            var userApiClient = new UserApiClient();
            var quickPollApiClient = new QuickPollApiClient();
            var authService = new AuthenticationService(userApiClient, quickPollApiClient);


            using (var loginForm = new LoginForm(authService))
            {
                var result = loginForm.ShowDialog();
                if (result == DialogResult.OK && loginForm.LoggedInUser != null)
                {
                    //  ✅ Save token from logged-in user
                    _token = loginForm.LoggedInUser.Token.AccessToken;

                    //  ✅ Show other buttons now
                    PickNameButton.Visible = true;
                    btnAddStudent.Visible = true;

                    //  ✅ Fetch students
                    var studentService = new StudentService(_token);
                    var students = await studentService.GetAllStudentsAsync();

                    if (students == null || !students.Any())
                    {
                        MessageBox.Show("No students found in the database.");
                    }
                    else
                    {
                        ThisAddIn.StudentsCache = students;
                    }
                }
            }
        }

        // ================== STUDENT PICK NAME ==================
        private async void PickNameButton_Click(object sender, RibbonControlEventArgs e)
        {
            if (string.IsNullOrEmpty(_token))
            {
                MessageBox.Show("You must log in first.");
                return;
            }

            var studentService = new StudentService(_token);
            var students = await studentService.GetAllStudentsAsync();

            if (students == null || !students.Any())
            {
                MessageBox.Show("No students found.");
                return;
            }

            var pickNameForm = new PickNameForm(students)
            {
                StartPosition = FormStartPosition.CenterScreen,
                TopMost = true
            };

            // Run form in a separate STA thread
            var uiThread = new System.Threading.Thread(() =>
            {
                Application.Run(pickNameForm);
            });
            uiThread.SetApartmentState(System.Threading.ApartmentState.STA);
            uiThread.Start();
        }

        // ================== ADD STUDENT ==================
        private async void BtnAddStudent_Click(object sender, RibbonControlEventArgs e)
        {
            if (string.IsNullOrEmpty(_token))
            {
                MessageBox.Show("You must log in first.");
                return;
            }

            using (var form = new AddStudentForm(_token))
            {
                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    // Refresh student list
                    var studentService = new StudentService(_token);
                    var students = await studentService.GetAllStudentsAsync();
                    ThisAddIn.StudentsCache = students ?? ThisAddIn.StudentsCache;

                    MessageBox.Show("Student added and list refreshed.", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        // ================== QUICK POLL BUTTONS ==================
        private void btnQuickPoll_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                var form = new QuickPollForm();
                form.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Quick Poll: {ex.Message}");
            }
        }

        private void btnJoinPoll_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                // If we already have any open form (like PowerPoint main window), use its handle
                var mainForm = System.Windows.Forms.Application.OpenForms.Cast<System.Windows.Forms.Form>().FirstOrDefault();

                if (mainForm != null && mainForm.InvokeRequired)
                {
                    mainForm.Invoke((MethodInvoker)(() => ShowLoginAndVoteForms()));
                }
                else
                {
                    // If no form is open, we’re already on the UI thread
                    ShowLoginAndVoteForms();
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error opening Join Poll: {ex.Message}");
            }
        }

        private void ShowLoginAndVoteForms()
        {
            try
            {
                if (string.IsNullOrEmpty(ThisAddIn.StudentToken))
                {
                    using (var loginForm = new StudentLoginForm())
                    {
                        var result = loginForm.ShowDialog();
                        if (result != System.Windows.Forms.DialogResult.OK)
                            return;
                    }
                }

                var voteForm = new StudentVoteForm
                {
                    StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen,
                    TopMost = true
                };
                voteForm.Show();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error showing poll: {ex.Message}");
            }
        }



        private void btnShowResults_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                // Ask for poll NAME instead of code
                string name = Microsoft.VisualBasic.Interaction.InputBox(
                    "Enter the Poll Name:", "Show Polls by Name", "");

                if (!string.IsNullOrWhiteSpace(name))
                {
                    var form = new QuickPollResultsByNameForm(name);
                    form.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing results: {ex.Message}");
            }
        }

    }
}

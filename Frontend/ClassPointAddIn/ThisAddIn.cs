using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ClassPointAddIn.Api.Service;
using ClassPointAddIn.Views;
using Office = Microsoft.Office.Core;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;

namespace ClassPointAddIn
{
    public partial class ThisAddIn
    {
        private Microsoft.Office.Interop.PowerPoint.Application app;
        public static List<Student> StudentsCache { get; set; } = new List<Student>();

        private ControlToolbarOverlay toolbarOverlay;
        public static string StudentToken { get; set; }


        private void ThisAddIn_Startup(object sender, EventArgs e)
        {
            app = Globals.ThisAddIn.Application;
            app.SlideShowBegin += App_SlideShowBegin;
            app.SlideShowEnd += App_SlideShowEnd;
        }

        private void App_SlideShowBegin(Microsoft.Office.Interop.PowerPoint.SlideShowWindow Wn)
        {
            toolbarOverlay = new ControlToolbarOverlay();

            // When Pick is clicked
            toolbarOverlay.PickClicked += (s, e) =>
            {
                var pickForm = new PickNameForm(ThisAddIn.StudentsCache);
                pickForm.ShowDialog();
            };

            // When Poll is clicked
            toolbarOverlay.PollClicked += (s, e) =>
            {
                var pollForm = new QuickPollForm();

                pollForm.PollTypeSelected += (sender, pollType) =>
                {
                    MessageBox.Show($"Poll started: {pollType}", "Quick Poll Started",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // TODO: Next step → connect to backend + display live results
                };
                pollForm.ShowDialog();
            };



            toolbarOverlay.Show();
        }

        private void App_SlideShowEnd(Microsoft.Office.Interop.PowerPoint.Presentation Pres)
        {
            if (toolbarOverlay != null && !toolbarOverlay.IsDisposed)
                toolbarOverlay.Close();
        }

        private void ThisAddIn_Shutdown(object sender, EventArgs e)
        {
        }

        #region VSTO generated code
        private void InternalStartup()
        {
            this.Startup += new EventHandler(ThisAddIn_Startup);
            this.Shutdown += new EventHandler(ThisAddIn_Shutdown);
        }

        public static string GetCurrentSlideTitle()
        {
            try
            {
                var slide = Globals.ThisAddIn.Application.ActiveWindow.View.Slide;
                return slide.Shapes.Title?.TextFrame.TextRange.Text ?? "Untitled Slide";
            }
            catch
            {
                return "Unknown Slide";
            }
        }

        #endregion
    }
}

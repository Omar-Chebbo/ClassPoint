using Microsoft.Office.Tools.Ribbon;

namespace ClassPointAddIn.Views
{
    partial class ConnectRibbon : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public ConnectRibbon()
            : base(Globals.Factory.GetRibbonFactory())
        {
            InitializeComponent();
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConnectRibbon));
            this.ConnectTab = this.Factory.CreateRibbonTab();
            this.ConnectGroup = this.Factory.CreateRibbonGroup();
            this.Connect = this.Factory.CreateRibbonButton();
            this.PickNameButton = this.Factory.CreateRibbonButton();
            this.btnAddStudent = this.Factory.CreateRibbonButton();
            this.btnQuickPoll = this.Factory.CreateRibbonButton();
            this.btnJoinPoll = this.Factory.CreateRibbonButton();
            this.btnShowResults = this.Factory.CreateRibbonButton();
            this.tab1 = this.Factory.CreateRibbonTab();
            this.ConnectTab.SuspendLayout();
            this.ConnectGroup.SuspendLayout();
            this.tab1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ConnectTab
            // 
            this.ConnectTab.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
            this.ConnectTab.Groups.Add(this.ConnectGroup);
            this.ConnectTab.Label = "ClassPoint";
            this.ConnectTab.Name = "ConnectTab";
            // 
            // ConnectGroup
            // 
            this.ConnectGroup.Items.Add(this.Connect);
            this.ConnectGroup.Items.Add(this.PickNameButton);
            this.ConnectGroup.Items.Add(this.btnAddStudent);
            this.ConnectGroup.Items.Add(this.btnQuickPoll);
            this.ConnectGroup.Items.Add(this.btnJoinPoll);
            this.ConnectGroup.Items.Add(this.btnShowResults);
            this.ConnectGroup.Label = "Teacher Tools";
            this.ConnectGroup.Name = "ConnectGroup";
            // 
            // Connect
            // 
            this.Connect.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.Connect.Description = "Login or Register to ClassPoint API";
            this.Connect.Image = ((System.Drawing.Image)(resources.GetObject("Connect.Image")));
            this.Connect.Label = "Connect";
            this.Connect.Name = "Connect";
            this.Connect.ShowImage = true;
            this.Connect.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.Connect_Click);
            // 
            // PickNameButton
            // 
            this.PickNameButton.Label = "Pick Name";
            this.PickNameButton.Name = "PickNameButton";
            this.PickNameButton.Visible = false;
            // 
            // btnAddStudent
            // 
            this.btnAddStudent.Label = "Add Student";
            this.btnAddStudent.Name = "btnAddStudent";
            this.btnAddStudent.Visible = false;
            // 
            // btnQuickPoll
            // 
            this.btnQuickPoll.Label = "Create Poll";
            this.btnQuickPoll.Name = "btnQuickPoll";
            this.btnQuickPoll.ScreenTip = "Create a new Quick Poll";
            this.btnQuickPoll.ShowImage = true;
            this.btnQuickPoll.Visible = false;
            // 
            // btnJoinPoll
            // 
            this.btnJoinPoll.Label = "Join Poll";
            this.btnJoinPoll.Name = "btnJoinPoll";
            this.btnJoinPoll.ScreenTip = "Join an active poll to vote";
            this.btnJoinPoll.ShowImage = true;
            // 
            // btnShowResults
            // 
            this.btnShowResults.Label = "Show Results";
            this.btnShowResults.Name = "btnShowResults";
            this.btnShowResults.ScreenTip = "View poll results by code";
            this.btnShowResults.ShowImage = true;
            // 
            // tab1
            // 
            this.tab1.Label = "tab1";
            this.tab1.Name = "tab1";
            // 
            // ConnectRibbon
            // 
            this.Name = "ConnectRibbon";
            this.RibbonType = "Microsoft.PowerPoint.Presentation";
            this.Tabs.Add(this.ConnectTab);
            this.Tabs.Add(this.tab1);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.ConnectRibbon_Load);
            this.ConnectTab.ResumeLayout(false);
            this.ConnectTab.PerformLayout();
            this.ConnectGroup.ResumeLayout(false);
            this.ConnectGroup.PerformLayout();
            this.tab1.ResumeLayout(false);
            this.tab1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab ConnectTab;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup ConnectGroup;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton Connect;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton PickNameButton;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnAddStudent;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnQuickPoll;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnJoinPoll;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnShowResults;
        private RibbonTab tab1;
    }

    partial class ThisRibbonCollection
    {
        internal ConnectRibbon LoginRibbon
        {
            get { return this.GetRibbon<ConnectRibbon>(); }
        }

        private T GetRibbon<T>() where T : RibbonBase
        {
            return Globals.Ribbons.GetRibbon<T>();
        }
    }
}

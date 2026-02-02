using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ClassPointAddIn.Api.Service;
using Microsoft.Office.Interop.PowerPoint;

namespace ClassPointAddIn.Views
{
    public partial class PickNameForm : Form
    {
        private List<Student> _students;
        private Random _rand = new Random();

        // This is the constructor your error says is missing
        public PickNameForm(List<Student> students)
        {
            InitializeComponent();
            _students = students.OrderBy(s => _rand.Next()).ToList();
            LoadStudentCards();
        }

        private void LoadStudentCards()
        {
            flowLayoutPanelStudents.Controls.Clear();

            foreach (var student in _students)
            {
                var displayName = GetDisplayName(student);
                var card = new Button
                {
                    Text = "❓",
                    Tag = displayName,
                    Width = 250,
                    Height = 120,
                    BackColor = GetRandomColor(),
                    Font = new System.Drawing.Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    UseVisualStyleBackColor = false
                };
                card.FlatAppearance.BorderSize = 2;
                card.FlatAppearance.BorderColor = Color.Black;
                card.Click += Card_Click;
                flowLayoutPanelStudents.Controls.Add(card);
            }
        }

        private void Card_Click(object sender, EventArgs e)
        {
            var card = (Button)sender;
            var resolvedName = (string)card.Tag;             //  get resolved name

            if (card.Text == "❓")
            {
                card.Text = resolvedName;                    // show name
                card.BackColor = this.BackColor; ;           // optional highlight
                card.ForeColor = Color.Black;                // better contrast
               
            }
            else
            {
                card.Text = "❓";                             //  hide again
                card.BackColor = GetRandomColor();
                card.ForeColor = Color.White;
            }
        }
        private string GetDisplayName(Student s)
        {
            if (!string.IsNullOrWhiteSpace(s.Name)) return s.Name;
       
            if (!string.IsNullOrWhiteSpace(s.Email)) return s.Email.Split('@')[0];
            return "Unknown";
        }


        private Color GetRandomColor()
        {
            var colors = new[]
            {
                Color.Coral, Color.Teal, Color.MediumPurple,
                Color.OrangeRed, Color.DeepSkyBlue, Color.Salmon,
                Color.MediumSeaGreen, Color.SlateBlue, Color.Tomato
            };
            return colors[_rand.Next(colors.Length)];
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _students = _students.OrderBy(s => _rand.Next()).ToList();
            LoadStudentCards();
        }
    }
}

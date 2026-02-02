using ClassPointAddIn.API.Service;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;

namespace ClassPointAddIn.Views
{
    public class QuickPollResultsByNameForm : Form
    {
        private readonly QuickPollApiClient _api = new QuickPollApiClient();
        private readonly string _pollName;

        private TableLayoutPanel _root;
        private Label _lblHeader;
        private Panel _content;
        private Button _btnRefresh;
        private Button _btnExport;

        private JToken[] _currentPolls = new JToken[0];
        private readonly HashSet<string> _selectedPollCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public QuickPollResultsByNameForm(string pollName)
        {
            _pollName = pollName;

            this.Text = "Poll Results – " + pollName;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 10.5F);
            this.BackColor = Color.White;
            this.Size = new Size(900, 650);

            BuildUI();
            Task.Run(async () => await LoadDataAsync());
        }

        private void BuildUI()
        {
            _root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(16)
            };
            _root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            _root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            _root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            this.Controls.Add(_root);

            _lblHeader = new Label
            {
                Text = "Results for: " + _pollName,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold)
            };
            _root.Controls.Add(_lblHeader, 0, 0);

            var buttonBar = new Panel
            {
                Dock = DockStyle.Fill
            };

            _btnRefresh = new Button
            {
                Text = "Refresh",
                Width = 120,
                Height = 28,
                Left = 0,
                Top = 6
            };
            _btnRefresh.Click += async (s, e) => await LoadDataAsync();
            buttonBar.Controls.Add(_btnRefresh);

            _btnExport = new Button
            {
                Text = "Export Selected to Excel",
                Width = 200,
                Height = 28,
                Left = _btnRefresh.Right + 12,
                Top = 6
            };
            _btnExport.Click += async (s, e) => await ExportSelectedAsync();
            buttonBar.Controls.Add(_btnExport);

            _root.Controls.Add(buttonBar, 0, 1);

            _content = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };
            _root.Controls.Add(_content, 0, 2);
        }

        private async Task LoadDataAsync()
        {
            try
            {
                string json = await _api.GetResultsByNameAsync(_pollName);
                JObject doc = JObject.Parse(json);

                JToken pollsToken = doc["polls"];
                JToken[] polls = pollsToken != null ? pollsToken.ToArray() : new JToken[0];

                _currentPolls = polls;

                this.Invoke(new Action(delegate
                {
                    _content.Controls.Clear();
                    _selectedPollCodes.Clear();

                    int y = 10;
                    foreach (JToken poll in polls)
                    {
                        string code = (string)poll["poll_code"];
                        string createdAt = (string)poll["created_at"];
                        string name = (string)poll["poll_name"];

                        Panel card = new Panel
                        {
                            Left = 10,
                            Top = y,
                            Width = _content.ClientSize.Width - 40,
                            Height = 250,
                            BorderStyle = BorderStyle.FixedSingle,
                            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                        };
                        _content.Controls.Add(card);
                        y += card.Height + 10;

                        CheckBox chk = new CheckBox
                        {
                            Left = 8,
                            Top = 6,
                            Width = 18,
                            Height = 18,
                            Tag = code
                        };
                        chk.CheckedChanged += PollCheckbox_CheckedChanged;
                        card.Controls.Add(chk);

                        Label lblTitle = new Label
                        {
                            Text = name + "   •   Code: " + code + "   •   Created: " + createdAt,
                            Left = chk.Right + 6,
                            Top = 0,
                            Height = 28,
                            Width = card.Width - chk.Right - 12,
                            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                            Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold)
                        };
                        card.Controls.Add(lblTitle);

                        ListView list = new ListView
                        {
                            Dock = DockStyle.Bottom,
                            Height = card.Height - lblTitle.Bottom - 8,
                            View = View.Details,
                            FullRowSelect = true
                        };
                        list.Columns.Add("Option", 240);
                        list.Columns.Add("Votes", 80);
                        list.Columns.Add("Voters (names)", 520);
                        card.Controls.Add(list);

                        JToken results = poll["results"];
                        if (results != null)
                        {
                            foreach (JToken r in results)
                            {
                                string optionText = (string)r["option"];
                                int count = (int)r["vote_count"];
                                string voters = string.Join(", ", r["voters"].Select(v => (string)v));

                                ListViewItem item = new ListViewItem(optionText);
                                item.SubItems.Add(count.ToString());
                                item.SubItems.Add(voters);
                                list.Items.Add(item);
                            }
                        }
                    }

                    if (polls.Length == 0)
                    {
                        Label empty = new Label
                        {
                            Text = "No polls found with this name.",
                            Dock = DockStyle.Top,
                            ForeColor = Color.DimGray
                        };
                        _content.Controls.Add(empty);
                    }
                }));
            }
            catch (Exception ex)
            {
                this.Invoke(new Action(delegate
                {
                    _content.Controls.Clear();
                    Label err = new Label
                    {
                        Text = "Error: " + ex.Message,
                        Dock = DockStyle.Top,
                        ForeColor = Color.Red
                    };
                    _content.Controls.Add(err);
                }));
            }
        }

        private void PollCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (chk == null) return;

            string code = chk.Tag as string;
            if (string.IsNullOrWhiteSpace(code)) return;

            if (chk.Checked)
                _selectedPollCodes.Add(code);
            else
                _selectedPollCodes.Remove(code);
        }

        private async Task ExportSelectedAsync()
        {
            if (_selectedPollCodes.Count == 0)
            {
                MessageBox.Show("Please select at least one poll to export.",
                    "No Poll Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            List<JToken> selectedPolls =
                _currentPolls.Where(p => _selectedPollCodes.Contains((string)p["poll_code"])).ToList();

            if (selectedPolls.Count == 0)
            {
                MessageBox.Show("Selected polls are not in the current list.",
                    "Export", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            selectedPolls.Sort(delegate (JToken a, JToken b)
            {
                DateTime da = ParseDate((string)a["created_at"]);
                DateTime db = ParseDate((string)b["created_at"]);
                return da.CompareTo(db);
            });

            DialogResult choice = MessageBox.Show(
                "Do you want to create a NEW Excel file?\n\n" +
                "Yes = Create new file\nNo = Append to existing file\nCancel = Stop",
                "Export Polls", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (choice == DialogResult.Cancel)
                return;

            string filePath = null;
            bool createNew = (choice == DialogResult.Yes);

            if (createNew)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Excel Files|*.xlsx";
                sfd.FileName = "PollResults_" + _pollName + ".xlsx";
                if (sfd.ShowDialog(this) != DialogResult.OK)
                    return;
                filePath = sfd.FileName;
            }
            else
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "Excel Files|*.xlsx";
                if (ofd.ShowDialog(this) != DialogResult.OK)
                    return;
                filePath = ofd.FileName;
            }

            string sheetName = PromptForText("Sheet Name",
                "Enter sheet name (new or existing):", "All Polls");

            if (string.IsNullOrWhiteSpace(sheetName))
                return;

            int exported = 0;
            int skipped = 0;

            await Task.Run(delegate
            {
                XLWorkbook wb;

                if (!createNew && System.IO.File.Exists(filePath))
                    wb = new XLWorkbook(filePath);
                else
                    wb = new XLWorkbook();

                IXLWorksheet ws = null;

                foreach (IXLWorksheet s in wb.Worksheets)
                {
                    if (string.Equals(s.Name, sheetName, StringComparison.OrdinalIgnoreCase))
                    {
                        ws = s;
                        break;
                    }
                }

                if (ws == null)
                    ws = wb.AddWorksheet(sheetName);

                if (ws.Cell(1, 1).IsEmpty())
                {
                    ws.Cell(1, 1).Value = "Poll Code";
                    ws.Cell(1, 2).Value = "Poll Name";
                    ws.Cell(1, 3).Value = "Created At";
                    ws.Cell(1, 4).Value = "Question Type";
                    ws.Cell(1, 5).Value = "Options Summary";

                    ws.Row(1).Style.Font.Bold = true;
                }

                HashSet<string> existingCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                IXLRange used = ws.RangeUsed();
                int lastRow = 1;
                if (used != null)
                {
                    lastRow = used.LastRow().RowNumber();

                    for (int r = 2; r <= lastRow; r++)
                    {
                        string code = ws.Cell(r, 1).GetString();
                        if (!string.IsNullOrEmpty(code))
                            existingCodes.Add(code);
                    }
                }

                int row = lastRow + 1;

                foreach (JToken poll in selectedPolls)
                {
                    string code = (string)poll["poll_code"];
                    if (existingCodes.Contains(code))
                    {
                        skipped++;
                        continue;
                    }

                    string name = (string)poll["poll_name"];
                    string createdRaw = (string)poll["created_at"];
                    string questionType = Convert.ToString(poll["question_type"]);
                    DateTime createdAt = ParseDate(createdRaw);

                    string summary = BuildOptionsSummary(poll["results"]);

                    ws.Cell(row, 1).Value = code;
                    ws.Cell(row, 2).Value = name;
                    ws.Cell(row, 3).Value = createdAt;
                    ws.Cell(row, 3).Style.DateFormat.Format = "yyyy-MM-dd HH:mm:ss";
                    ws.Cell(row, 4).Value = questionType;
                    ws.Cell(row, 5).Value = summary;

                    existingCodes.Add(code);
                    exported++;
                    row++;
                }

                wb.SaveAs(filePath);
            });

            MessageBox.Show(
                "Export completed.\n\n" +
                exported + " poll(s) exported.\n" +
                skipped + " skipped (already exist).",
                "Export Polls",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private static DateTime ParseDate(string raw)
        {
            DateTime dt;
            if (!string.IsNullOrWhiteSpace(raw) && DateTime.TryParse(raw, out dt))
                return dt;
            return DateTime.MinValue;
        }

        private static string BuildOptionsSummary(JToken resultsToken)
        {
            if (resultsToken == null)
                return string.Empty;

            List<string> parts = new List<string>();

            foreach (JToken r in resultsToken)
            {
                string option = (string)r["option"];
                int count = (int)r["vote_count"];
                List<string> voters = r["voters"]
                    .Select(v => (string)v)
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .ToList();

                if (count > 0 && voters.Count > 0)
                    parts.Add(option + " (" + count + " votes: " + string.Join(", ", voters) + ")");
                else
                    parts.Add(option + " (" + count + ")");
            }

            return string.Join(", ", parts);
        }

        private string PromptForText(string title, string labelText, string defaultValue)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textbox = new TextBox();
            Button ok = new Button();
            Button cancel = new Button();

            form.Text = title;
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.ClientSize = new Size(400, 130);
            form.Font = this.Font;

            label.Text = labelText;
            label.SetBounds(12, 12, 370, 20);

            textbox.Text = defaultValue;
            textbox.SetBounds(12, 36, 370, 24);

            ok.Text = "OK";
            ok.DialogResult = DialogResult.OK;
            ok.SetBounds(214, 76, 80, 28);

            cancel.Text = "Cancel";
            cancel.DialogResult = DialogResult.Cancel;
            cancel.SetBounds(302, 76, 80, 28);

            form.Controls.AddRange(new Control[] { label, textbox, ok, cancel });
            form.AcceptButton = ok;
            form.CancelButton = cancel;

            return form.ShowDialog(this) == DialogResult.OK ? textbox.Text : null;
        }
    }
}

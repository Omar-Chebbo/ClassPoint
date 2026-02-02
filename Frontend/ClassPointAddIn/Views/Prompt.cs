using System.Windows.Forms;

public static class Prompt
{
    public static string Input(string text, string caption, IWin32Window owner = null)
    {
        Form prompt = new Form()
        {
            Width = 400,
            Height = 180,
            Text = caption,
            StartPosition = FormStartPosition.CenterParent,
            TopMost = true
        };

        Label lblText = new Label() { Left = 20, Top = 20, Text = text, AutoSize = true };
        TextBox txtInput = new TextBox() { Left = 20, Top = 50, Width = 340 };
        Button btnOk = new Button() { Text = "OK", Left = 280, Width = 80, Top = 90, DialogResult = DialogResult.OK };

        prompt.Controls.Add(lblText);
        prompt.Controls.Add(txtInput);
        prompt.Controls.Add(btnOk);
        prompt.AcceptButton = btnOk;

        // ensure shown in front of parent
        var result = owner != null ? prompt.ShowDialog(owner) : prompt.ShowDialog();
        return result == DialogResult.OK ? txtInput.Text : "";
    }
}

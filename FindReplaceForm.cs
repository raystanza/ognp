using System;
using System.Drawing;
using System.Windows.Forms;

namespace ognp
{
    public sealed class FindReplaceForm : Form
    {
        private readonly TextBox tbFind = new TextBox();
        private readonly TextBox tbReplace = new TextBox();
        private readonly CheckBox cbMatchCase = new CheckBox { Text = "Match case" };
        private readonly RadioButton rbDown = new RadioButton { Text = "Down", Checked = true };
        private readonly RadioButton rbUp = new RadioButton { Text = "Up" };

        private readonly Button btnFindNext = new Button { Text = "Find Next", DialogResult = DialogResult.None };
        private readonly Button btnReplace = new Button { Text = "Replace", DialogResult = DialogResult.None };
        private readonly Button btnReplaceAll = new Button { Text = "Replace All", DialogResult = DialogResult.None };
        private readonly Button btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };

        private bool replaceMode;

        public event Action<string, bool, bool>? FindNextRequested;
        public event Action<string, string, bool, bool>? ReplaceRequested;
        public event Action<string, string, bool>? ReplaceAllRequested;

        public FindReplaceForm(bool replaceMode)
        {
            this.replaceMode = replaceMode;
            Text = replaceMode ? "Replace" : "Find";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            Width = 420;
            Height = replaceMode ? 240 : 190;
            StartPosition = FormStartPosition.CenterParent;

            // Layout
            var lblFind = new Label { Text = "Find what:", AutoSize = true, Left = 12, Top = 15 };
            tbFind.Left = 90; tbFind.Top = 12; tbFind.Width = 200;

            var lblReplace = new Label { Text = "Replace with:", AutoSize = true, Left = 12, Top = 50 };
            tbReplace.Left = 90; tbReplace.Top = 47; tbReplace.Width = 200;

            cbMatchCase.Left = 12; cbMatchCase.Top = replaceMode ? 80 : 60;

            var grp = new GroupBox { Text = "Direction", Left = 150, Width = 140, Height = 55, Top = replaceMode ? 75 : 55 };
            rbDown.Left = 15; rbDown.Top = 20;
            rbUp.Left = 75; rbUp.Top = 20;
            grp.Controls.Add(rbDown);
            grp.Controls.Add(rbUp);

            btnFindNext.Left = 305; btnFindNext.Top = 10; btnFindNext.Width = 90;
            btnReplace.Left = 305; btnReplace.Top = 45; btnReplace.Width = 90;
            btnReplaceAll.Left = 305; btnReplaceAll.Top = 80; btnReplaceAll.Width = 90;
            btnCancel.Left = 305; btnCancel.Top = replaceMode ? 115 : 80; btnCancel.Width = 90;

            Controls.Add(lblFind);
            Controls.Add(tbFind);
            Controls.Add(cbMatchCase);
            Controls.Add(grp);
            Controls.Add(btnFindNext);
            Controls.Add(btnCancel);

            if (replaceMode)
            {
                Controls.Add(lblReplace);
                Controls.Add(tbReplace);
                Controls.Add(btnReplace);
                Controls.Add(btnReplaceAll);
            }

            AcceptButton = btnFindNext;
            CancelButton = btnCancel;

            btnFindNext.Click += (_, __) => OnFindNext();
            btnReplace.Click += (_, __) => OnReplace();
            btnReplaceAll.Click += (_, __) => OnReplaceAll();
        }

        public void SwitchMode(bool replace)
        {
            if (replace == replaceMode) return;
            replaceMode = replace;
            Close(); // simplest: reopen in the desired mode
        }

        private void OnFindNext()
        {
            if (string.IsNullOrEmpty(tbFind.Text)) return;
            FindNextRequested?.Invoke(tbFind.Text, cbMatchCase.Checked, rbDown.Checked);
        }

        private void OnReplace()
        {
            if (string.IsNullOrEmpty(tbFind.Text)) return;
            ReplaceRequested?.Invoke(tbFind.Text, tbReplace.Text, cbMatchCase.Checked, rbDown.Checked);
        }

        private void OnReplaceAll()
        {
            if (string.IsNullOrEmpty(tbFind.Text)) return;
            ReplaceAllRequested?.Invoke(tbFind.Text, tbReplace.Text, cbMatchCase.Checked);
        }
    }
}

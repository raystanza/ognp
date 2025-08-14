using System;
using System.Windows.Forms;

namespace ognp
{
    public sealed class GoToForm : Form
    {
        private readonly NumericUpDown num = new NumericUpDown();
        private readonly Button ok = new Button { Text = "Go To" };
        private readonly Button cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };

        public int LineNumber => (int)num.Value;

        public GoToForm(int maxLine)
        {
            Text = "Go To Line";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            Width = 280; Height = 140;
            StartPosition = FormStartPosition.CenterParent;

            var lbl = new Label { Text = "Line number:", AutoSize = true, Left = 12, Top = 18 };

            num.Left = 100; num.Top = 15; num.Width = 150;
            num.Minimum = 1; num.Maximum = Math.Max(1, maxLine);

            ok.Left = 100; ok.Top = 50; ok.Width = 70;
            cancel.Left = 180; cancel.Top = 50; cancel.Width = 70;

            AcceptButton = ok;
            CancelButton = cancel;

            ok.Click += (_, __) => DialogResult = DialogResult.OK;

            Controls.Add(lbl);
            Controls.Add(num);
            Controls.Add(ok);
            Controls.Add(cancel);
        }
    }
}

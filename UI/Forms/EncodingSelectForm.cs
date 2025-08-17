/*
 * ognp (OG Notepad)
 * A faithful Notepad clone written in C#.
 *
 * © 2025 Jim Sines (raystanza).
 *
 * Licensed under GPL-3.0-or-later.

 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Text;
using System.Windows.Forms;

namespace ognp;

public sealed class EncodingSelectForm : Form
{
    private readonly ComboBox combo = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Button ok = new() { Text = "OK", DialogResult = DialogResult.OK };
    private readonly Button cancel = new() { Text = "Cancel", DialogResult = DialogResult.Cancel };

    public Encoding SelectedEncoding { get; private set; }

    public EncodingSelectForm(Encoding? current)
    {
        // Sensible default if user cancels or null passed
        SelectedEncoding = current ?? Encoding.Default;

        AutoScaleMode = AutoScaleMode.Font;
        Text = "Choose Encoding";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Width = 360;
        Height = 140;

        var lbl = new Label { Text = "Encoding:", AutoSize = true, Left = 12, Top = 18 };

        combo.Left = 80; combo.Top = 15; combo.Width = 250;
        combo.DropDownWidth = 280; // a bit wider than the box for long names

        // Populate options
        combo.Items.Add(new EncItem("ANSI", Encoding.Default));
        combo.Items.Add(new EncItem("UTF-8", new UTF8Encoding(false)));
        combo.Items.Add(new EncItem("UTF-8 (BOM)", new UTF8Encoding(true)));
        combo.Items.Add(new EncItem("UTF-16 LE", Encoding.Unicode));
        combo.Items.Add(new EncItem("UTF-16 BE", Encoding.BigEndianUnicode));
        combo.Items.Add(new EncItem("UTF-32 LE", new UTF32Encoding(false, true)));
        combo.Items.Add(new EncItem("UTF-32 BE", new UTF32Encoding(true, true)));

        // Preselect the current
        for (int i = 0; i < combo.Items.Count; i++)
        {
            var it = (EncItem)combo.Items[i]!;
            if (EncodingEquals(it.Encoding, SelectedEncoding)) { combo.SelectedIndex = i; break; }
        }
        if (combo.SelectedIndex < 0) combo.SelectedIndex = 0;

        ok.Left = 170; ok.Top = 50; ok.Width = 75; ok.TabIndex = 1;
        cancel.Left = 255; cancel.Top = 50; cancel.Width = 75; cancel.TabIndex = 2;
        combo.TabIndex = 0;

        AcceptButton = ok;
        CancelButton = cancel;

        // When OK is pressed, persist the chosen encoding
        ok.Click += (_, __) =>
        {
            SelectedEncoding = ((EncItem)combo.SelectedItem!).Encoding;
            DialogResult = DialogResult.OK;
        };

        SuspendLayout();
        Controls.Add(lbl);
        Controls.Add(combo);
        Controls.Add(ok);
        Controls.Add(cancel);
        ResumeLayout(performLayout: false);
    }

    private static bool EncodingEquals(Encoding a, Encoding b)
    {
        // Compare by CodePage and BOM presence for UTF-8
        if (a is UTF8Encoding u8a && b is UTF8Encoding u8b)
            return u8a.GetPreamble().Length == u8b.GetPreamble().Length;
        return a.CodePage == b.CodePage;
    }

    private sealed class EncItem
    {
        public string Name { get; }
        public Encoding Encoding { get; }
        public EncItem(string name, Encoding enc) { Name = name; Encoding = enc; }
        public override string ToString() => Name;
    }
}

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
using System.Drawing;
using System.Windows.Forms;

namespace ognp;

public sealed class FindReplaceForm : Form
{
    private readonly TextBox tbFind = new();
    private readonly TextBox tbReplace = new();
    private readonly CheckBox cbMatchCase = new() { Text = "Match case" };
    private readonly RadioButton rbDown = new() { Text = "Down", Checked = true };
    private readonly RadioButton rbUp = new() { Text = "Up" };

    private readonly Button btnFindNext = new() { Text = "Find Next", DialogResult = DialogResult.None };
    private readonly Button btnReplace = new() { Text = "Replace", DialogResult = DialogResult.None };
    private readonly Button btnReplaceAll = new() { Text = "Replace All", DialogResult = DialogResult.None };
    private readonly Button btnCancel = new() { Text = "Cancel", DialogResult = DialogResult.Cancel };

    private bool replaceMode;

    // allow the owner to see which mode this dialog is in
    public bool IsReplaceMode => replaceMode;

    public event Action<string, bool, bool>? FindNextRequested;
    public event Action<string, string, bool, bool>? ReplaceRequested;
    public event Action<string, string, bool>? ReplaceAllRequested;

    public FindReplaceForm(bool replaceMode)
    {
        this.replaceMode = replaceMode;

        AutoScaleMode = AutoScaleMode.Font;
        Text = replaceMode ? "Replace" : "Find";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Width = 420;
        Height = replaceMode ? 240 : 190;
        MinimumSize = new Size(420, replaceMode ? 240 : 190);

        // Layout
        var lblFind = new Label { Text = "Find what:", AutoSize = true, Left = 12, Top = 15 };
        tbFind.Left = 90; tbFind.Top = 12; tbFind.Width = 200;

        var lblReplace = new Label { Text = "Replace with:", AutoSize = true, Left = 12, Top = 50 };
        tbReplace.Left = 90; tbReplace.Top = 47; tbReplace.Width = 200;

        cbMatchCase.Left = 12; cbMatchCase.Top = replaceMode ? 80 : 60;

        // Right-side buttons column
        btnFindNext.Left = 305; btnFindNext.Top = 10; btnFindNext.Width = 90;
        btnReplace.Left = 305; btnReplace.Top = 45; btnReplace.Width = 90;
        btnReplaceAll.Left = 305; btnReplaceAll.Top = 80; btnReplaceAll.Width = 90;
        btnCancel.Left = 305; btnCancel.Top = replaceMode ? 115 : 80; btnCancel.Width = 90;

        // Direction group — ensure radios size to text and keep group left of the right button column
        rbDown.AutoSize = true;
        rbUp.AutoSize = true;

        const int grpLeft = 150;
        int grpTop = replaceMode ? 75 : 55;
        const int innerMargin = 12;
        const int gap = 12;
        int buttonsLeft = btnFindNext.Left;          // 305
        int groupMaxRight = buttonsLeft - 6;         // leave a small gutter
        int availableWidth = Math.Max(60, groupMaxRight - grpLeft);

        // Compute how much width is ideal for a horizontal layout
        int neededWidth = innerMargin + rbDown.PreferredSize.Width + gap + rbUp.PreferredSize.Width + innerMargin;

        var grp = new GroupBox
        {
            Text = "Direction",
            Left = grpLeft,
            Top = grpTop
        };

        // Place "Down"
        rbDown.Left = innerMargin;
        rbDown.Top = 22;

        // If enough room, place "Up" on the same line; otherwise stack vertically
        if (neededWidth <= availableWidth)
        {
            rbUp.Left = innerMargin + rbDown.PreferredSize.Width + gap;
            rbUp.Top = rbDown.Top;

            grp.Width = Math.Min(neededWidth, availableWidth);
            grp.Height = 55;
        }
        else
        {
            rbUp.Left = innerMargin;
            rbUp.Top = rbDown.Top + rbDown.PreferredSize.Height + 6;

            grp.Width = availableWidth;
            grp.Height = rbUp.Bottom + innerMargin;
        }

        grp.Controls.Add(rbDown);
        grp.Controls.Add(rbUp);

        // Tab order
        tbFind.TabIndex = 0;
        tbReplace.TabIndex = 1;
        cbMatchCase.TabIndex = 2;
        grp.TabIndex = 3;
        rbDown.TabIndex = 0;
        rbUp.TabIndex = 1;
        btnFindNext.TabIndex = 4;
        btnReplace.TabIndex = 5;
        btnReplaceAll.TabIndex = 6;
        btnCancel.TabIndex = 7;

        AcceptButton = btnFindNext;
        CancelButton = btnCancel;

        btnFindNext.Click += (_, __) => OnFindNext();
        btnReplace.Click += (_, __) => OnReplace();
        btnReplaceAll.Click += (_, __) => OnReplaceAll();
        btnCancel.Click += (_, __) => Close();

        // Nice UX: focus the Find box and select text when shown
        Shown += (_, __) =>
        {
            tbFind.Focus();
            try { tbFind.SelectAll(); } catch { /* ignore */ }
        };

        SuspendLayout();
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
        ResumeLayout(performLayout: false);
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
        if (!IsReplaceMode || string.IsNullOrEmpty(tbFind.Text)) return;
        ReplaceRequested?.Invoke(tbFind.Text, tbReplace.Text, cbMatchCase.Checked, rbDown.Checked);
    }

    private void OnReplaceAll()
    {
        if (!IsReplaceMode || string.IsNullOrEmpty(tbFind.Text)) return;
        ReplaceAllRequested?.Invoke(tbFind.Text, tbReplace.Text, cbMatchCase.Checked);
    }

    public void SetFindText(string text)
    {
        tbFind.Text = text ?? string.Empty;
        try { tbFind.SelectAll(); } catch { /* ignore */ }
    }

    public void SetReplaceText(string text)
    {
        tbReplace.Text = text ?? string.Empty;
        try { tbReplace.SelectAll(); } catch { /* ignore */ }
    }

    public void SetMatchCase(bool matchCase)
    {
        cbMatchCase.Checked = matchCase;
    }

    public void SetSearchDirection(bool searchDown)
    {
        rbDown.Checked = searchDown;
        rbUp.Checked = !searchDown;
    }

    public void Clear()
    {
        tbFind.Clear();
        tbReplace.Clear();
        cbMatchCase.Checked = false;
        rbDown.Checked = true;
    }
}

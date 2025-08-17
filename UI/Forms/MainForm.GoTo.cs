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
using System.Windows.Forms;

namespace ognp;

public sealed partial class MainForm : Form
{
    // ---------- GO TO ----------

    private void ShowGoTo()
    {
        int totalLines = editor.GetLineFromCharIndex(editor.TextLength) + 1;
        using var dlg = new GoToForm(totalLines);
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            int line = dlg.LineNumber - 1;
            line = Math.Max(0, Math.Min(line, totalLines - 1));
            int index = editor.GetFirstCharIndexFromLine(line);
            if (index >= 0)
            {
                editor.SelectionStart = index;
                editor.SelectionLength = 0;
                editor.ScrollToCaret();
                UpdateStatus();
            }
        }
    }

    private void JumpToLine(int oneBasedLine)
    {
        int totalLines = editor.GetLineFromCharIndex(editor.TextLength) + 1;
        int line = Math.Max(1, Math.Min(oneBasedLine, totalLines)) - 1;
        int idx = editor.GetFirstCharIndexFromLine(line);
        if (idx >= 0)
        {
            editor.SelectionStart = idx;
            editor.SelectionLength = 0;
            editor.ScrollToCaret();
            UpdateStatus();
        }
    }
}

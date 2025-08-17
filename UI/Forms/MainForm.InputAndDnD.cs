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
using System.IO;
using System.Windows.Forms;

namespace ognp;

public sealed partial class MainForm : Form
{
    // ---------- INPUT & DRAG/DROP ----------

    private void Editor_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Insert)
        {
            _overwriteMode = !_overwriteMode;
            UpdateStatus();
            e.Handled = true;
            return;
        }

        if (e.KeyCode == Keys.F5)
        {
            InsertTimeDate();
            e.Handled = true;
        }
        else if (e.KeyCode == Keys.F3 && !e.Shift)
        {
            DoFindNext(false);
            e.Handled = true;
        }
        else if (e.KeyCode == Keys.F3 && e.Shift)
        {
            DoFindNext(true);
            e.Handled = true;
        }
    }

    private void Editor_KeyPress(object? sender, KeyPressEventArgs e)
    {
        if (!_overwriteMode) return;
        if (char.IsControl(e.KeyChar)) return;
        if (editor.SelectionLength > 0) return;

        int caret = editor.SelectionStart;
        if (caret < editor.TextLength)
        {
            char c = editor.Text[caret];
            if (c != '\r' && c != '\n')
            {
                editor.SelectionLength = 1;
            }
        }
    }

    private void Editor_DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true ||
            e.Data?.GetDataPresent(DataFormats.UnicodeText) == true ||
            e.Data?.GetDataPresent(DataFormats.Text) == true)
        {
            e.Effect = DragDropEffects.Copy;
        }
    }

    private void Editor_DragDrop(object? sender, DragEventArgs e)
    {
        if (e.Data == null) return;

        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            if (!ConfirmDiscardChanges()) return;

            var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
            if (files.Length > 0 && File.Exists(files[0]))
            {
                LoadFile(files[0]);
            }
            return;
        }

        string? text = e.Data.GetData(DataFormats.UnicodeText) as string
                    ?? e.Data.GetData(DataFormats.Text) as string;
        if (!string.IsNullOrEmpty(text))
        {
            editor.SelectedText = text;
        }
    }
}

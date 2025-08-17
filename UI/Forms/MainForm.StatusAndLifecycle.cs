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
using System.Text;
using System.Windows.Forms;

namespace ognp;

public sealed partial class MainForm : Form
{
    // ---------- STATUS & LIFECYCLE ----------

    private void OnEditorTextChanged()
    {
        if (suppressTextChanged) return;
        _doc.IsModified = true;
        UpdateTitle();
        UpdateStatus();
    }

    private void UpdateTitle()
    {
        Text = $"{_doc.DisplayName} - OGNP";
    }

    private void UpdateStatus()
    {
        int caret = editor.SelectionStart;
        int line = editor.GetLineFromCharIndex(caret);
        int col = caret - editor.GetFirstCharIndexFromLine(line);

        sbPos.Text = $"Ln {line + 1}, Col {col + 1}";
        sbEol.Text = _doc.EolName;
        sbIns.Text = _overwriteMode ? "OVR" : "INS";
        sbEnc.Text = EncodingDisplayName(_doc.Encoding);
    }

    private static string EncodingDisplayName(Encoding enc)
    {
        if (enc is UTF8Encoding u8)
            return u8.GetPreamble().Length > 0 ? "UTF-8 (BOM)" : "UTF-8";

        return enc.CodePage switch
        {
            1200 => "UTF-16 LE",
            1201 => "UTF-16 BE",
            12000 => "UTF-32 LE",
            12001 => "UTF-32 BE",
            _ when enc.Equals(Encoding.Default) => "ANSI",
            _ => enc.EncodingName
        };
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (!ConfirmDiscardChanges())
        {
            e.Cancel = true;
        }
    }
}

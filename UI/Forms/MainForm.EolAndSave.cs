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
    // ---------- EOL & SAVE HELPERS (delegating to _doc) ----------

    private static string NormalizeToEditor(string text) => text;

    // Compatibility wrapper: original instance method now delegates to DocumentState.
    private void DetectAndSetEol(string text)
    {
        _doc.DetectAndSetEol(text);
        UpdateStatus();
    }

    // Compatibility wrapper: original instance method now delegates to DocumentState.
    private string PrepareForSave(string text) => _doc.PrepareForSave(text);

    // Compatibility wrapper: original took (eol, name). We ignore 'name' since _doc computes EolName.
    private void SetEol(string eol, string name)
    {
        _doc.SetEol(eol);
        UpdateStatus();
    }

    // Possible future support for menu to switch EOLs directly (though not in the original):
    private void SetEolToWindowsCrlf() { _doc.SetEol("\r\n"); UpdateStatus(); }
    private void SetEolToUnixLf()      { _doc.SetEol("\n");   UpdateStatus(); }
    private void SetEolToMacCr()       { _doc.SetEol("\r");   UpdateStatus(); }
}

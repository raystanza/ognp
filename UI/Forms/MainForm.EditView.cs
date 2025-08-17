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
using System.Globalization;
using System.Windows.Forms;
using System.Drawing;

namespace ognp;

public sealed partial class MainForm : Form
{
    // ---------- EDIT/FORMAT/VIEW ----------

    private void InsertTimeDate()
    {
        var stamp = DateTime.Now.ToString(CultureInfo.CurrentCulture);
        editor.SelectedText = stamp;
    }

    private void ToggleWordWrap()
    {
        editor.WordWrap = formatWordWrapItem.Checked;
        editor.ScrollBars = editor.WordWrap ? ScrollBars.Vertical : ScrollBars.Both;
        UpdateViewWordWrapDependencies();
        UpdateStatus();
    }

    private void UpdateViewWordWrapDependencies()
    {
        viewStatusBarItem.Enabled = !editor.WordWrap;
        status.Visible = !editor.WordWrap && viewStatusBarItem.Checked;

        // "Go To..." disabled when Word Wrap is ON
        editGoToItem.Enabled = !editor.WordWrap;
    }

    private void ToggleStatusBar()
    {
        if (!editor.WordWrap)
        {
            status.Visible = viewStatusBarItem.Checked;
        }
        else
        {
            viewStatusBarItem.Checked = false;
            status.Visible = false;
        }
    }

    private void ChooseFont()
    {
        using var dlg = new FontDialog()
        {
            Font = editor.Font,
            ShowEffects = false,
            AllowVerticalFonts = false
        };
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            editor.Font = dlg.Font;
            UpdateStatus();
        }
    }
}

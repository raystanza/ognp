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

public sealed partial class MainForm : Form
{
    private void ShowFindReplace(bool replaceMode)
    {
        var f = findForm;

        // If an existing dialog is around…
        if (f != null)
        {
            if (f.Visible && !f.IsDisposed && f.IsReplaceMode == replaceMode)
            {
                // Refresh with current state and focus it
                if (!string.IsNullOrEmpty(lastFind)) f.SetFindText(lastFind);
                f.SetMatchCase(lastMatchCase);
                f.SetSearchDirection(lastSearchDown);
                try { f.Activate(); f.BringToFront(); } catch { /* ignore */ }
                return;
            }

            // Wrong mode or disposed — close and recreate
            try { f.Close(); } catch { /* ignore */ }
            f = null;
        }

        // Create a fresh dialog in the requested mode
        f = new FindReplaceForm(replaceMode)
        {
            StartPosition = FormStartPosition.CenterParent
        };
        findForm = f;

        // Prefill from saved state, or fall back to current selection the first time
        var initialFind = !string.IsNullOrEmpty(lastFind) ? lastFind : editor.SelectedText;
        if (!string.IsNullOrEmpty(initialFind)) f.SetFindText(initialFind);
        f.SetMatchCase(lastMatchCase);
        f.SetSearchDirection(lastSearchDown);

        f.FindNextRequested += (pattern, matchCase, searchDown) =>
        {
            lastFind = pattern;
            lastMatchCase = matchCase;
            lastSearchDown = searchDown;
            DoFindNext(false);
        };

        f.ReplaceRequested += (pattern, replacement, matchCase, searchDown) =>
        {
            lastFind = pattern;
            lastMatchCase = matchCase;
            lastSearchDown = searchDown;
            DoReplaceOnce(replacement);
        };

        f.ReplaceAllRequested += (pattern, replacement, matchCase) =>
        {
            lastFind = pattern;
            lastMatchCase = matchCase;
            DoReplaceAll(replacement);
        };

        f.FormClosed += (_, __) =>
        {
            findForm = null;
            try { Activate(); editor.Focus(); } catch { /* ignore */ }
        };

        f.Show(this);
    }

    private void DoFindNext(bool reverse)
    {
        if (string.IsNullOrEmpty(lastFind)) { ShowFindReplace(false); return; }

        string text = editor.Text;
        string needle = lastFind;

        var comparison = lastMatchCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;

        int start = editor.SelectionStart;
        int searchFrom = lastSearchDown && !reverse
            ? start + editor.SelectionLength
            : Math.Max(0, start - 1);

        int found = -1;

        if (lastSearchDown && !reverse)
        {
            found = text.IndexOf(needle, searchFrom, comparison);
            if (found < 0) found = text.IndexOf(needle, 0, comparison); // wrap
        }
        else
        {
            if (searchFrom >= 0)
                found = text.LastIndexOf(needle, searchFrom, comparison);
            if (found < 0)
                found = text.LastIndexOf(needle, text.Length - 1, comparison); // wrap
        }

        if (found >= 0)
        {
            editor.SelectionStart = found;
            editor.SelectionLength = needle.Length;
            editor.ScrollToCaret();

            // Make the match obvious: bring focus back to the editor
            if (!editor.Focused)
            {
                try
                {
                    Activate();       // activate main form
                    editor.Focus();   // focus the editor so selection is “active”
                }
                catch { /* ignore */ }
            }

            UpdateStatus();
        }
        else
        {
            MessageBox.Show($"Cannot find \"{needle}\"", "ognp",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void DoReplaceOnce(string replacement)
    {
        if (editor.SelectionLength > 0)
        {
            var selected = editor.SelectedText;
            if (string.Equals(selected, lastFind,
                lastMatchCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase))
            {
                editor.SelectedText = replacement;
            }
        }
        DoFindNext(false);

        // keep focus on the editor after the jump
        if (!editor.Focused)
        {
            try { Activate(); editor.Focus(); } catch { /* ignore */ }
        }
    }

    private void DoReplaceAll(string replacement)
    {
        if (string.IsNullOrEmpty(lastFind)) return;

        string text = editor.Text;
        string pattern = lastFind;

        if (lastMatchCase)
        {
            text = text.Replace(pattern, replacement);
        }
        else
        {
            // Case-insensitive replace
            int i = 0;
            var sb = new StringBuilder(text.Length);
            while (i < text.Length)
            {
                int idx = text.IndexOf(pattern, i, StringComparison.CurrentCultureIgnoreCase);
                if (idx < 0)
                {
                    sb.Append(text, i, text.Length - i);
                    break;
                }
                sb.Append(text, i, idx - i);
                sb.Append(replacement);
                i = idx + pattern.Length;
            }
            text = sb.ToString();
        }

        int caret = editor.SelectionStart;
        suppressTextChanged = true;
        editor.Text = text;
        suppressTextChanged = false;
        editor.SelectionStart = Math.Min(caret, editor.TextLength);
        editor.SelectionLength = 0;
        _doc.IsModified = true;
        UpdateStatus();
        UpdateTitle();
        // bring focus back to the editor
        if (!editor.Focused)
        {
            try { Activate(); editor.Focus(); } catch { /* ignore */ }
        }
    }

}

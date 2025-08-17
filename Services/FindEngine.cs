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

namespace ognp.Services;

/// <summary>
/// Pure search/replace logic used by the Find/Replace UI. No WinForms dependencies.
/// Matches OGNP's current behavior: wrap-around search, case toggle,
/// forward/backward direction, and a simple case-insensitive ReplaceAll.
/// </summary>
internal sealed class FindEngine
{
    /// <summary>
    /// Find the next occurrence of <paramref name="needle"/> in <paramref name="text"/>.
    /// Replicates OGNP's current logic:
    /// - if (searchDown && !reverse): search forward starting just after the current selection
    /// - else (reverse/backward): search backward starting just before the caret
    /// - wrap-around if not found in the initial scan.
    /// Returns the 0-based index of the match or -1 if not found.
    /// </summary>
    public int FindNext(
        string text,
        string needle,
        int selectionStart,
        int selectionLength,
        bool searchDown,
        bool matchCase,
        bool reverse = false)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(needle))
            return -1;

        var comparison = matchCase
            ? StringComparison.CurrentCulture
            : StringComparison.CurrentCultureIgnoreCase;

        int searchFrom = (searchDown && !reverse)
            ? selectionStart + selectionLength
            : Math.Max(0, selectionStart - 1);

        int found = -1;

        if (searchDown && !reverse)
        {
            found = text.IndexOf(needle, searchFrom, comparison);
            if (found < 0) // wrap
                found = text.IndexOf(needle, 0, comparison);
        }
        else
        {
            if (searchFrom >= 0)
                found = text.LastIndexOf(needle, searchFrom, comparison);
            if (found < 0) // wrap
                found = text.LastIndexOf(needle, text.Length - 1, comparison);
        }

        return found;
    }

    /// <summary>
    /// Returns true if the current selection equals the needle, respecting <paramref name="matchCase"/>.
    /// Useful for "Replace" (replace once) logic before advancing to the next match.
    /// </summary>
    public bool SelectionMatches(
        string text,
        int selectionStart,
        int selectionLength,
        string needle,
        bool matchCase)
    {
        if (string.IsNullOrEmpty(needle) || selectionLength != needle.Length)
            return false;

        if (selectionStart < 0 || selectionStart + selectionLength > text.Length)
            return false;

        var selected = text.Substring(selectionStart, selectionLength);

        return matchCase
            ? string.Equals(selected, needle, StringComparison.CurrentCulture)
            : string.Equals(selected, needle, StringComparison.CurrentCultureIgnoreCase);
    }

    /// <summary>
    /// Replace all occurrences of <paramref name="pattern"/> with <paramref name="replacement"/>.
    /// Mirrors OGNP's current behavior:
    /// - Case-sensitive: uses string.Replace (fast).
    /// - Case-insensitive: manual scan using IndexOf with CurrentCultureIgnoreCase.
    /// </summary>
    public string ReplaceAll(
        string text,
        string pattern,
        string replacement,
        bool matchCase)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(pattern))
            return text ?? string.Empty;

        if (matchCase)
        {
            return text.Replace(pattern, replacement);
        }

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
        return sb.ToString();
    }
}

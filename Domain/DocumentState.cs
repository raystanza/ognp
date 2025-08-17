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

using System.Text;

namespace ognp.Domain;

/// <summary>
/// Holds the current document's path, encoding, EOL preference, and modified flag,
/// plus small helpers to keep load/save logic consistent and testable.
/// </summary>
internal sealed class DocumentState
{
    /// <summary>Absolute path to the file on disk (null for a new/untitled document).</summary>
    public string? Path { get; set; }

    /// <summary>Text encoding in use (updated on load; chosen by user on save when applicable).</summary>
    public Encoding Encoding { get; set; } = Encoding.Default;

    /// <summary>The end-of-line sequence to use when saving. Defaults to CRLF.</summary>
    public string Eol { get; private set; } = "\r\n";

    /// <summary>Human-friendly EOL label derived from <see cref="Eol"/>.</summary>
    public string EolName => Eol switch
    {
        "\r\n" => "Windows (CRLF)",
        "\n" => "Unix (LF)",
        "\r" => "Mac (CR)",
        _ => "Custom"
    };

    /// <summary>Set by the UI when the editor text changes or after save.</summary>
    public bool IsModified { get; set; }

    /// <summary>Convenience file name for UI.</summary>
    public string DisplayName => string.IsNullOrEmpty(Path)
        ? "Untitled"
        : System.IO.Path.GetFileName(Path);

    /// <summary>Reset state to an untitled, ANSI, CRLF document.</summary>
    public void ResetToUntitled()
    {
        Path = null;
        Encoding = Encoding.Default;
        SetEol("\r\n");
        IsModified = false;
    }

    /// <summary>
    /// Inspect the given text and set <see cref="Eol"/> to the dominant newline style.
    /// Prefers CRLF if present, else LF, else CR; falls back to CRLF if none detected.
    /// </summary>
    public void DetectAndSetEol(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            Eol = "\r\n";
            return;
        }

        if (text.Contains("\r\n"))
        {
            Eol = "\r\n"; // Windows (CRLF)
        }
        else if (text.Contains('\n'))
        {
            Eol = "\n";   // Unix (LF)
        }
        else if (text.Contains('\r'))
        {
            Eol = "\r";   // Classic Mac (CR)
        }
        else
        {
            Eol = "\r\n"; // default
        }
    }

    /// <summary>
    /// Normalize text to a single internal representation (LF) first, then
    /// rewrite to the active <see cref="Eol"/> for saving. This prevents mixed newlines.
    /// </summary>
    public string PrepareForSave(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // Normalize everything to LF…
        var lfOnly = text.Replace("\r\n", "\n").Replace("\r", "\n");
        // …then rewrite to the chosen EOL.
        return lfOnly.Replace("\n", Eol);
    }

    /// <summary>Change the target EOL style with basic validation.</summary>
    public void SetEol(string eol)
    {
        if (eol is "\r\n" or "\n" or "\r")
        {
            Eol = eol;
        }
    }

    /// <summary>Mark the document as saved.</summary>
    public void MarkSaved() => IsModified = false;

    /// <summary>Update state right after a successful load (path/encoding/EOL, and clear modified flag).</summary>
    public void ApplyLoadedMetadata(string path, Encoding encoding, string loadedText)
    {
        Path = path;
        Encoding = encoding;
        DetectAndSetEol(loadedText);
        IsModified = false;
    }
}

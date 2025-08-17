/*
 * ognp (OG Notepad)
 * A faithful Notepad clone written in C#.
 *
 * © 2025 Jim Sines (raystanza).
 *
 * Licensed under GPL-3.0-or-later.
 *
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

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        string? initialPath = null;
        int? initialLine = null;

        // Accept either: ognp.exe +123 file.txt  OR  ognp.exe file.txt +123
        foreach (var a in args)
        {
            if (a.StartsWith("+", StringComparison.Ordinal) &&
                int.TryParse(a.AsSpan(1), out var n) && n > 0)
            {
                initialLine = n;
            }
            else if (string.IsNullOrWhiteSpace(initialPath))
            {
                initialPath = a;
            }
        }

        // Normalize the path, but don't let bad inputs crash the app.
        if (!string.IsNullOrWhiteSpace(initialPath))
        {
            try
            {
                initialPath = Path.GetFullPath(initialPath!);
            }
            catch (ArgumentException)           { initialPath = null; }
            catch (NotSupportedException)       { initialPath = null; }
            catch (PathTooLongException)        { initialPath = null; }
            catch (System.Security.SecurityException) { initialPath = null; }
        }

        // Optional: only pass an existing file; otherwise start Untitled
        if (!string.IsNullOrEmpty(initialPath) && !File.Exists(initialPath))
        {
            initialPath = null;
        }

        Application.Run(new MainForm(initialPath, initialLine));
    }
}

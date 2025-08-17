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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Runtime.InteropServices; // ExternalException
using System.Windows.Forms;

namespace ognp.Services;

internal interface IPrintService
{
    /// <summary>
    /// Creates a PrintDocument wired to paginate and render the supplied text/font.
    /// The delegates are evaluated on each page render so changes to the editor are reflected.
    /// </summary>
    PrintDocument Create(Func<string> getText, Func<Font> getFont);

    /// <summary>Shows a page setup dialog for the given document. Returns true if the dialog closed normally.</summary>
    bool ShowPageSetup(IWin32Window owner, PrintDocument doc);

    /// <summary>
    /// Shows a print dialog and, if confirmed, prints the document.
    /// Returns true on success. If false and <paramref name="error"/> is non-null, the caller can display it.
    /// </summary>
    bool ShowPrintDialog(IWin32Window owner, PrintDocument doc, out Exception? error);
}

internal sealed class PrintService : IPrintService
{
    public PrintDocument Create(Func<string> getText, Func<Font> getFont)
    {
        if (getText is null) throw new ArgumentNullException(nameof(getText));
        if (getFont is null) throw new ArgumentNullException(nameof(getFont));

        var doc = new PrintDocument();

        // Private pagination state per PrintDocument instance
        int charIndex = 0;

        doc.BeginPrint += (_, __) =>
        {
            // Start from the beginning on each print run
            charIndex = 0;
        };

        doc.PrintPage += (_, e) =>
        {
            var g = e.Graphics;
            if (g is null)
            {
                e.Cancel = true;
                e.HasMorePages = false;
                return;
            }

            string text = getText() ?? string.Empty;
            var font = getFont() ?? SystemFonts.DefaultFont;
            var rect = e.MarginBounds;

            if (charIndex >= text.Length)
            {
                e.HasMorePages = false;
                return;
            }

            using var fmt = new StringFormat(StringFormatFlags.LineLimit);

            // Measure how many characters fit on this page starting from charIndex
            g.MeasureString(
                text.Substring(charIndex),
                font,
                rect.Size,
                fmt,
                out int charsFitted,
                out int linesFitted);

            if (charsFitted <= 0)
            {
                e.HasMorePages = false;
                return;
            }

            // Draw that slice, then advance
            g.DrawString(text.Substring(charIndex, charsFitted), font, Brushes.Black, rect, fmt);
            charIndex += charsFitted;

            e.HasMorePages = charIndex < text.Length;
        };

        return doc;
    }

    public bool ShowPageSetup(IWin32Window owner, PrintDocument doc)
    {
        using var dlg = new PageSetupDialog { Document = doc };
        try
        {
            // PageSetupDialog doesn’t have an OK/Cancel result that matters here;
            // we simply show it and let the printer settings update on the document.
            dlg.ShowDialog(owner);
            return true;
        }
        catch (InvalidPrinterException)
        {
            // No printers / invalid printer installed
            return false;
        }
        catch (Win32Exception)
        {
            // System dialog error
            return false;
        }
    }

    public bool ShowPrintDialog(IWin32Window owner, PrintDocument doc, out Exception? error)
    {
        error = null;
        using var dlg = new PrintDialog { Document = doc, UseEXDialog = true };
        try
        {
            if (dlg.ShowDialog(owner) == DialogResult.OK)
            {
                doc.Print();
                return true;
            }
            return false; // user cancelled
        }
        catch (InvalidPrinterException ex) { error = ex; return false; }
        catch (Win32Exception ex)         { error = ex; return false; }
        catch (ExternalException ex)      { error = ex; return false; } // GDI+ / graphics
    }
}

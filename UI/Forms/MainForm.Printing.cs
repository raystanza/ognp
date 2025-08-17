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
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ognp;

public sealed partial class MainForm : Form
{
    // ---------- Printing ----------

    private void FilePageSetup()
    {
        using var dlg = new PageSetupDialog { Document = _printDoc };
        try
        {
            dlg.ShowDialog(this);
        }
        catch (InvalidPrinterException)
        {
            // No printers / invalid printer installed
        }
        catch (Win32Exception)
        {
            // System dialog error
        }
    }

    private void FilePrint()
    {
        using var dlg = new PrintDialog { Document = _printDoc, UseEXDialog = true };
        try
        {
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                _printDoc.Print();
            }
        }
        catch (InvalidPrinterException ex)
        {
            MessageBox.Show($"Printing failed (invalid printer).\n\n{ex.Message}", "OGNP",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Win32Exception ex)
        {
            MessageBox.Show($"Printing failed (system error).\n\n{ex.Message}", "OGNP",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (ExternalException ex)
        {
            MessageBox.Show($"Printing failed (graphics error).\n\n{ex.Message}", "OGNP",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void PrintDoc_BeginPrint(object? sender, PrintEventArgs e)
    {
        _printCharIndex = 0;
    }

    private void PrintDoc_PrintPage(object? sender, PrintPageEventArgs e)
    {
        var g = e.Graphics;
        if (g is null)
        {
            e.Cancel = true;
            e.HasMorePages = false;
            return;
        }

        string text = editor.Text ?? string.Empty;
        var font = editor.Font ?? SystemFonts.DefaultFont;
        var rect = e.MarginBounds;

        if (_printCharIndex >= text.Length)
        {
            e.HasMorePages = false;
            return;
        }

        using var fmt = new StringFormat(StringFormatFlags.LineLimit);

        g.MeasureString(
            text.AsSpan(_printCharIndex).ToString(),
            font,
            rect.Size,
            fmt,
            out int charsFitted,
            out _);

        if (charsFitted <= 0)
        {
            e.HasMorePages = false;
            return;
        }

        g.DrawString(text.Substring(_printCharIndex, charsFitted), font, Brushes.Black, rect, fmt);
        _printCharIndex += charsFitted;

        e.HasMorePages = _printCharIndex < text.Length;
    }
}

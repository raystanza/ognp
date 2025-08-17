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
    // ---------- FILE OPS ----------

    private bool ConfirmDiscardChanges()
    {
        if (!_doc.IsModified) return true;

        var name = _doc.DisplayName;
        var res = MessageBox.Show(
            $"Do you want to save changes to {name}?",
            "OGNP",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Exclamation);

        if (res == DialogResult.Cancel) return false;
        if (res == DialogResult.Yes) return FileSave();
        return true;
    }

    private void FileNew()
    {
        if (!ConfirmDiscardChanges()) return;

        suppressTextChanged = true;
        editor.Clear();
        suppressTextChanged = false;

        _doc.ResetToUntitled();          // path=null, ANSI, CRLF, not modified
        UpdateTitle();
        UpdateStatus();
    }

    private void FileOpen()
    {
        if (!ConfirmDiscardChanges()) return;

        using var dlg = new OpenFileDialog
        {
            Title = "Open",
            Filter = "Text Documents (*.txt)|*.txt|All Files (*.*)|*.*",
            CheckFileExists = true
        };
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            LoadFile(dlg.FileName);
        }
    }

    // Streamed load + BOM detection + large-file soft warning
    private void LoadFile(string path)
    {
        try
        {
            var fi = new FileInfo(path);
            bool warn = fi.Exists && fi.Length >= LargeFileWarnBytes;

            if (warn)
            {
                var res = MessageBox.Show(
                    $"The file is {fi.Length / (1024 * 1024)} MB. Opening may be slow. Open anyway?",
                    "OGNP",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                if (res != DialogResult.Yes) return;
            }

            UseWaitCursor = true;
            Cursor.Current = Cursors.WaitCursor;
            Application.DoEvents();

            string text;
            Encoding enc;

            // Streamed read; default to ANSI if no BOM is present
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 16))
            using (var sr = new StreamReader(fs, Encoding.Default, detectEncodingFromByteOrderMarks: true, bufferSize: 1 << 16))
            {
                var sb = new StringBuilder((int)Math.Min(Math.Max(1024, fi.Length), 50_000_000)); // heuristic capacity
                char[] buf = new char[1 << 14]; // 16K chars
                int n;
                while ((n = sr.Read(buf, 0, buf.Length)) > 0)
                    sb.Append(buf, 0, n);

                text = sb.ToString();
                enc = sr.CurrentEncoding;
            }

            // Update state based on the loaded content
            _doc.ApplyLoadedMetadata(path, enc, text);

            suppressTextChanged = true;
            editor.SuspendLayout();
            editor.Text = NormalizeToEditor(text);
            editor.SelectionStart = 0;
            editor.SelectionLength = 0;
            editor.ResumeLayout();
            suppressTextChanged = false;

            UpdateTitle();
            UpdateStatus();
        }
        catch (IOException ex)
        {
            MessageBox.Show($"Could not open file (I/O error).\n\n{ex.Message}", "OGNP",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (UnauthorizedAccessException ex)
        {
            MessageBox.Show($"Could not open file (access denied).\n\n{ex.Message}", "OGNP",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (NotSupportedException ex)
        {
            MessageBox.Show($"Could not open file (unsupported path).\n\n{ex.Message}", "OGNP",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (System.Security.SecurityException ex)
        {
            MessageBox.Show($"Could not open file (security error).\n\n{ex.Message}", "OGNP",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            UseWaitCursor = false;
            Cursor.Current = Cursors.Default;
        }
    }

    private bool FileSave()
    {
        if (string.IsNullOrEmpty(_doc.Path))
            return FileSaveAs();

        try
        {
            var text = _doc.PrepareForSave(editor.Text);
            File.WriteAllText(_doc.Path!, text, _doc.Encoding);
            _doc.MarkSaved();
            UpdateTitle();
            UpdateStatus();
            return true;
        }
        catch (IOException ex)
        {
            MessageBox.Show($"Could not save file (I/O error).\n\n{ex.Message}", "OGNP",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            MessageBox.Show($"Could not save file (access denied).\n\n{ex.Message}", "OGNP",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        catch (NotSupportedException ex)
        {
            MessageBox.Show($"Could not save file (unsupported path).\n\n{ex.Message}", "OGNP",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        catch (System.Security.SecurityException ex)
        {
            MessageBox.Show($"Could not save file (security error).\n\n{ex.Message}", "OGNP",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    private bool FileSaveAs()
    {
        using var sfd = new SaveFileDialog
        {
            Title = "Save As",
            Filter = "Text Documents (*.txt)|*.txt|All Files (*.*)|*.*",
            OverwritePrompt = true,
            AddExtension = true,
            DefaultExt = "txt",
            FileName = string.IsNullOrEmpty(_doc.Path) ? "Untitled.txt" : Path.GetFileName(_doc.Path)
        };
        if (sfd.ShowDialog(this) != DialogResult.OK) return false;

        using var encDlg = new EncodingSelectForm(_doc.Encoding);
        if (encDlg.ShowDialog(this) != DialogResult.OK) return false;

        var chosenEnc = encDlg.SelectedEncoding;

        try
        {
            var text = _doc.PrepareForSave(editor.Text);
            File.WriteAllText(sfd.FileName, text, chosenEnc);
            _doc.Path = sfd.FileName;
            _doc.Encoding = chosenEnc;
            _doc.MarkSaved();
            UpdateTitle();
            UpdateStatus();
            return true;
        }
        catch (IOException ex)
        {
            MessageBox.Show($"Could not save file (I/O error).\n\n{ex.Message}", "OGNP",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            MessageBox.Show($"Could not save file (access denied).\n\n{ex.Message}", "OGNP",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        catch (NotSupportedException ex)
        {
            MessageBox.Show($"Could not save file (unsupported path).\n\n{ex.Message}", "OGNP",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        catch (System.Security.SecurityException ex)
        {
            MessageBox.Show($"Could not save file (security error).\n\n{ex.Message}", "OGNP",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }
}

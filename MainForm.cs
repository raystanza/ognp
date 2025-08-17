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
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Printing;

namespace ognp
{
    public sealed class MainForm : Form
    {
        // Core editor and UI
        private readonly TextBox editor = new TextBox();
        private readonly MenuStrip menu = new MenuStrip();
        private readonly StatusStrip status = new StatusStrip();

        // Status bar labels (all disposables -> fields)
        private readonly ToolStripStatusLabel sbPos = new ToolStripStatusLabel();
        private readonly ToolStripStatusLabel sbEol = new ToolStripStatusLabel();
        private readonly ToolStripStatusLabel sbIns = new ToolStripStatusLabel();
        private readonly ToolStripStatusLabel sbEnc = new ToolStripStatusLabel();
        private readonly ToolStripStatusLabel sbSpring = new ToolStripStatusLabel() { Spring = true };
        private readonly ToolStripStatusLabel sbSpacer1 = new ToolStripStatusLabel() { Text = "  " };
        private readonly ToolStripStatusLabel sbSpacer2 = new ToolStripStatusLabel() { Text = "  " };

        // State
        private string? currentPath;
        private Encoding currentEncoding = Encoding.Default;        // ANSI mindset
        private string currentEol = "\r\n";                         // Windows CRLF by default
        private string currentEolName = "Windows (CRLF)";
        private bool suppressTextChanged = false;                   // avoid "modified" flicker during loads
        private bool isModified = false;

        // Find/Replace state
        private FindReplaceForm? findForm;
        private string lastFind = string.Empty;
        private bool lastMatchCase = false;
        private bool lastSearchDown = true;

        // Menus (all disposables -> fields)
        private readonly ToolStripMenuItem mFile = new ToolStripMenuItem("&File");
        private readonly ToolStripMenuItem mEdit = new ToolStripMenuItem("&Edit");
        private readonly ToolStripMenuItem mFormat = new ToolStripMenuItem("F&ormat");
        private readonly ToolStripMenuItem miFormatFont = new ToolStripMenuItem("&Font...");
        private readonly ToolStripMenuItem mView = new ToolStripMenuItem("&View");
        private readonly ToolStripMenuItem mHelp = new ToolStripMenuItem("&Help");

        // File menu items
        private readonly ToolStripMenuItem miFileNew = new ToolStripMenuItem("&New");
        private readonly ToolStripMenuItem miFileOpen = new ToolStripMenuItem("&Open...");
        private readonly ToolStripMenuItem miFileSave = new ToolStripMenuItem("&Save");
        private readonly ToolStripMenuItem miFileSaveAs = new ToolStripMenuItem("Save &As...");
        private readonly ToolStripMenuItem miFilePageSetup = new ToolStripMenuItem("Page Set&up...");
        private readonly ToolStripMenuItem miFilePrint = new ToolStripMenuItem("&Print...");
        private readonly ToolStripMenuItem miFileExit = new ToolStripMenuItem("E&xit");
        private readonly ToolStripSeparator sepFile1 = new ToolStripSeparator();
        private readonly ToolStripSeparator sepFile2 = new ToolStripSeparator();
        private readonly ToolStripSeparator sepFile3 = new ToolStripSeparator();

        // Edit menu items
        private readonly ToolStripMenuItem miEditUndo = new ToolStripMenuItem("&Undo");
        private readonly ToolStripMenuItem miEditCut = new ToolStripMenuItem("Cu&t");
        private readonly ToolStripMenuItem miEditCopy = new ToolStripMenuItem("&Copy");
        private readonly ToolStripMenuItem miEditPaste = new ToolStripMenuItem("&Paste");
        private readonly ToolStripMenuItem miEditDelete = new ToolStripMenuItem("De&lete");
        private readonly ToolStripMenuItem miEditFind = new ToolStripMenuItem("&Find...");
        private readonly ToolStripMenuItem miEditFindNext = new ToolStripMenuItem("Find &Next");
        private readonly ToolStripMenuItem miEditReplace = new ToolStripMenuItem("&Replace...");
        private readonly ToolStripMenuItem miEditSelectAll = new ToolStripMenuItem("Select &All");
        private readonly ToolStripMenuItem miEditTimeDate = new ToolStripMenuItem("Time/&Date");
        private readonly ToolStripSeparator sepEdit1 = new ToolStripSeparator();
        private readonly ToolStripSeparator sepEdit2 = new ToolStripSeparator();
        private readonly ToolStripSeparator sepEdit3 = new ToolStripSeparator();

        // Menu items we need to toggle (make them real fields, not null!)
        private readonly ToolStripMenuItem viewStatusBarItem = new ToolStripMenuItem("&Status Bar");
        private readonly ToolStripMenuItem formatWordWrapItem = new ToolStripMenuItem("&Word Wrap");
        private readonly ToolStripMenuItem editGoToItem = new ToolStripMenuItem("&Go To...");

        // Help
        private readonly ToolStripMenuItem miHelpAbout = new ToolStripMenuItem("&About OGNP");

        // Printing
        private readonly PrintDocument _printDoc = new PrintDocument();
        private int _printCharIndex = 0;

        // Status bar insert/overtype
        private bool _overwriteMode = false;

        // Large-file friendliness
        private const long LargeFileWarnBytes = 10L * 1024 * 1024; // 10 MB soft warning

        public MainForm(string? initialPath, int? initialLine = null)
        {
            Text = "Untitled - OGNP";
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;
            StartPosition = FormStartPosition.CenterScreen;
            Width = 900;
            Height = 650;
            KeyPreview = true;

            // Editor config (classic)
            editor.Multiline = true;
            editor.AcceptsReturn = true;
            editor.AcceptsTab = true;
            editor.ScrollBars = ScrollBars.Both;
            editor.WordWrap = false;
            editor.Font = new Font("Consolas", 11);  // reasonable default; user can change via Format->Font...
            editor.Dock = DockStyle.Fill;

            editor.TextChanged += (_, __) => OnEditorTextChanged();
            editor.KeyUp += (_, __) => UpdateStatus();
            editor.MouseUp += (_, __) => UpdateStatus();
            editor.KeyDown += Editor_KeyDown;

            // Overtype actual behavior
            editor.KeyPress += Editor_KeyPress;

            // Drag & Drop
            editor.AllowDrop = true;
            editor.DragEnter += Editor_DragEnter;
            editor.DragDrop += Editor_DragDrop;

            // Printing
            _printDoc.BeginPrint += PrintDoc_BeginPrint;
            _printDoc.PrintPage += PrintDoc_PrintPage;

            BuildMenus();
            BuildStatusBar();

            Controls.Add(editor);
            Controls.Add(status);
            Controls.Add(menu);
            MainMenuStrip = menu;

            UpdateViewWordWrapDependencies();

            if (!string.IsNullOrWhiteSpace(initialPath) && File.Exists(initialPath))
            {
                LoadFile(initialPath);
                if (initialLine.HasValue) JumpToLine(initialLine.Value);
            }
            else
            {
                UpdateTitle();
                UpdateStatus();
            }

            FormClosing += MainForm_FormClosing;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Items inside Controls/ToolStrips are disposed by base.Dispose via the control tree.
                // Dispose things not parented to the Form's Controls collection.
                _printDoc?.Dispose();
                findForm?.Dispose();
            }
            base.Dispose(disposing);
        }

        // ---------- UI BUILDERS ----------

        private void BuildMenus()
        {
            // File
            miFileNew.ShortcutKeys = Keys.Control | Keys.N;
            miFileNew.Click += (_, __) => FileNew();

            miFileOpen.ShortcutKeys = Keys.Control | Keys.O;
            miFileOpen.Click += (_, __) => FileOpen();

            miFileSave.ShortcutKeys = Keys.Control | Keys.S;
            miFileSave.Click += (_, __) => FileSave();

            miFileSaveAs.ShortcutKeys = Keys.F12;
            miFileSaveAs.Click += (_, __) => FileSaveAs();

            miFilePageSetup.Click += (_, __) => FilePageSetup();

            miFilePrint.ShortcutKeys = Keys.Control | Keys.P;
            miFilePrint.Click += (_, __) => FilePrint();

            miFileExit.Click += (_, __) => Close();

            mFile.DropDownItems.AddRange(new ToolStripItem[]
            {
                miFileNew, miFileOpen, sepFile1,
                miFileSave, miFileSaveAs, sepFile2,
                miFilePageSetup, miFilePrint, sepFile3,
                miFileExit
            });

            // Edit
            miEditUndo.ShortcutKeys = Keys.Control | Keys.Z;
            miEditUndo.Click += (_, __) => { if (editor.CanUndo) editor.Undo(); };

            miEditCut.ShortcutKeys = Keys.Control | Keys.X;
            miEditCut.Click += (_, __) => editor.Cut();

            miEditCopy.ShortcutKeys = Keys.Control | Keys.C;
            miEditCopy.Click += (_, __) => editor.Copy();

            miEditPaste.ShortcutKeys = Keys.Control | Keys.V;
            miEditPaste.Click += (_, __) => editor.Paste();

            miEditDelete.ShortcutKeys = Keys.Delete;
            miEditDelete.Click += (_, __) => { if (editor.SelectionLength > 0) editor.SelectedText = string.Empty; };

            miEditFind.ShortcutKeys = Keys.Control | Keys.F;
            miEditFind.Click += (_, __) => ShowFindReplace(false);

            miEditFindNext.ShortcutKeys = Keys.F3;
            miEditFindNext.Click += (_, __) => DoFindNext(false);

            miEditReplace.ShortcutKeys = Keys.Control | Keys.H;
            miEditReplace.Click += (_, __) => ShowFindReplace(true);

            editGoToItem.ShortcutKeys = Keys.Control | Keys.G;
            editGoToItem.Click += (_, __) => ShowGoTo();

            miEditSelectAll.ShortcutKeys = Keys.Control | Keys.A;
            miEditSelectAll.Click += (_, __) => editor.SelectAll();

            miEditTimeDate.ShortcutKeys = Keys.F5;
            miEditTimeDate.Click += (_, __) => InsertTimeDate();

            mEdit.DropDownItems.AddRange(new ToolStripItem[]
            {
                miEditUndo, sepEdit1,
                miEditCut, miEditCopy, miEditPaste, miEditDelete, sepEdit2,
                miEditFind, miEditFindNext, miEditReplace, editGoToItem, sepEdit3,
                miEditSelectAll, miEditTimeDate
            });

            // Format
            formatWordWrapItem.CheckOnClick = true;
            formatWordWrapItem.Checked = false;
            formatWordWrapItem.Click += (_, __) => ToggleWordWrap();

            miFormatFont.Click += (_, __) => ChooseFont();

            mFormat.DropDownItems.AddRange(new ToolStripItem[]
            {
                formatWordWrapItem,
                miFormatFont
            });

            // View
            viewStatusBarItem.CheckOnClick = true;
            viewStatusBarItem.Checked = true;
            viewStatusBarItem.Click += (_, __) => ToggleStatusBar();
            mView.DropDownItems.Add(viewStatusBarItem);

            // Help
            miHelpAbout.Click += (_, __) =>
            {
                MessageBox.Show(
                    "ognp (OG Notepad)\nA faithful Notepad clone written in C#.\n\n© 2025 Jim Sines (raystanza).\n\nLicensed under GPL-3.0-or-later",
                    "About ognp",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            mHelp.DropDownItems.Add(miHelpAbout);

            menu.Items.AddRange(new ToolStripItem[] { mFile, mEdit, mFormat, mView, mHelp });
        }

        private void BuildStatusBar()
        {
            status.SizingGrip = true;
            status.Items.Add(sbPos);
            status.Items.Add(sbSpring);
            status.Items.Add(sbEol);
            status.Items.Add(sbSpacer1);
            status.Items.Add(sbIns);
            status.Items.Add(sbSpacer2);
            status.Items.Add(sbEnc);
            UpdateStatus();
        }

        // ---------- FILE OPS ----------

        private bool ConfirmDiscardChanges()
        {
            if (!isModified) return true;

            var name = string.IsNullOrEmpty(currentPath) ? "Untitled" : Path.GetFileName(currentPath);
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

            currentPath = null;
            currentEncoding = Encoding.Default;
            SetEol("\r\n", "Windows (CRLF)");
            isModified = false;
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

                // Detect line endings for status + preserve-on-save
                DetectAndSetEol(text);

                suppressTextChanged = true;
                editor.SuspendLayout();
                editor.Text = NormalizeToEditor(text);
                editor.SelectionStart = 0;
                editor.SelectionLength = 0;
                editor.ResumeLayout();
                suppressTextChanged = false;

                currentPath = path;
                currentEncoding = enc;
                isModified = false;
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
            if (string.IsNullOrEmpty(currentPath))
                return FileSaveAs();

            try
            {
                var text = PrepareForSave(editor.Text);
                File.WriteAllText(currentPath, text, currentEncoding);
                isModified = false;
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
                FileName = string.IsNullOrEmpty(currentPath) ? "Untitled.txt" : Path.GetFileName(currentPath)
            };
            if (sfd.ShowDialog(this) != DialogResult.OK) return false;

            using var encDlg = new EncodingSelectForm(currentEncoding);
            if (encDlg.ShowDialog(this) != DialogResult.OK) return false;

            var chosenEnc = encDlg.SelectedEncoding;

            try
            {
                var text = PrepareForSave(editor.Text);
                File.WriteAllText(sfd.FileName, text, chosenEnc);
                currentPath = sfd.FileName;
                currentEncoding = chosenEnc;
                isModified = false;
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

        private static void DetectAndSetEol(string text, out string eol, out string name)
        {
            if (text.Contains("\r\n"))
            { eol = "\r\n"; name = "Windows (CRLF)"; }
            else if (text.Contains('\n'))
            { eol = "\n"; name = "Unix (LF)"; }
            else if (text.Contains('\r'))
            { eol = "\r"; name = "Mac (CR)"; }
            else
            { eol = "\r\n"; name = "Windows (CRLF)"; }
        }
        private void DetectAndSetEol(string text)
        {
            DetectAndSetEol(text, out currentEol, out currentEolName);
        }

        private static string NormalizeToEditor(string text) => text;

        private string PrepareForSave(string text)
        {
            var normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");
            return normalized.Replace("\n", currentEol);
        }

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

        private void ShowFindReplace(bool replaceMode)
        {
            var f = findForm;
            if (f != null)
            {
                f.Focus();
                f.SwitchMode(replaceMode);
                return;
            }

            f = new FindReplaceForm(replaceMode)
            {
                StartPosition = FormStartPosition.CenterParent
            };
            findForm = f;

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

            f.FormClosed += (_, __) => findForm = null;

            f.Show(this);
        }

        private void ShowGoTo()
        {
            int totalLines = editor.GetLineFromCharIndex(editor.TextLength) + 1;
            using var dlg = new GoToForm(totalLines);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                int line = dlg.LineNumber - 1;
                line = Math.Max(0, Math.Min(line, totalLines - 1));
                int index = editor.GetFirstCharIndexFromLine(line);
                if (index >= 0)
                {
                    editor.SelectionStart = index;
                    editor.SelectionLength = 0;
                    editor.ScrollToCaret();
                    UpdateStatus();
                }
            }
        }

        // ---------- FIND/REPLACE ----------

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
            isModified = true;
            UpdateStatus();
        }

        // ---------- EVENTS / HELPERS ----------

        private void Editor_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Insert)
            {
                _overwriteMode = !_overwriteMode;
                UpdateStatus();
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.F5)
            {
                InsertTimeDate();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F3 && e.Shift)
            {
                DoFindNext(true);
                e.Handled = true;
            }
        }

        private void Editor_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (!_overwriteMode) return;
            if (char.IsControl(e.KeyChar)) return;
            if (editor.SelectionLength > 0) return;

            int caret = editor.SelectionStart;
            if (caret < editor.TextLength)
            {
                char c = editor.Text[caret];
                if (c != '\r' && c != '\n')
                {
                    editor.SelectionLength = 1;
                }
            }
        }

        private void Editor_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true ||
                e.Data?.GetDataPresent(DataFormats.UnicodeText) == true ||
                e.Data?.GetDataPresent(DataFormats.Text) == true)
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void Editor_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data == null) return;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                if (!ConfirmDiscardChanges()) return;

                var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
                if (files.Length > 0 && File.Exists(files[0]))
                {
                    LoadFile(files[0]);
                }
                return;
            }

            string? text = e.Data.GetData(DataFormats.UnicodeText) as string
                        ?? e.Data.GetData(DataFormats.Text) as string;
            if (!string.IsNullOrEmpty(text))
            {
                editor.SelectedText = text;
            }
        }

        private void OnEditorTextChanged()
        {
            if (suppressTextChanged) return;
            isModified = true;
            UpdateTitle();
            UpdateStatus();
        }

        private void UpdateTitle()
        {
            string name = string.IsNullOrEmpty(currentPath) ? "Untitled" : Path.GetFileName(currentPath);
            Text = $"{name} - OGNP";
        }

        private void UpdateStatus()
        {
            int caret = editor.SelectionStart;
            int line = editor.GetLineFromCharIndex(caret);
            int col = caret - editor.GetFirstCharIndexFromLine(line);

            sbPos.Text = $"Ln {line + 1}, Col {col + 1}";
            sbEol.Text = currentEolName;
            sbIns.Text = _overwriteMode ? "OVR" : "INS";
            sbEnc.Text = EncodingDisplayName(currentEncoding);
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

        private void JumpToLine(int oneBasedLine)
        {
            int totalLines = editor.GetLineFromCharIndex(editor.TextLength) + 1;
            int line = Math.Max(1, Math.Min(oneBasedLine, totalLines)) - 1;
            int idx = editor.GetFirstCharIndexFromLine(line);
            if (idx >= 0)
            {
                editor.SelectionStart = idx;
                editor.SelectionLength = 0;
                editor.ScrollToCaret();
                UpdateStatus();
            }
        }

        private void SetEol(string eol, string name)
        {
            currentEol = eol;
            currentEolName = name;
        }
    }
}

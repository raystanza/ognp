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
using ognp.Domain;

namespace ognp;

public sealed partial class MainForm : Form
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

    // Shared document state (deduplicates path/encoding/EOL/isModified)
    private readonly DocumentState _doc = new();

    // Other transient UI state
    private bool suppressTextChanged = false; // avoid "modified" flicker during loads

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
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;
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
        editor.Font = new Font("Consolas", 11);
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
            _printDoc?.Dispose();
            findForm?.Dispose();
        }
        base.Dispose(disposing);
    }
}

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
using System.Windows.Forms;

namespace ognp;

public sealed partial class MainForm : Form
{
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
}

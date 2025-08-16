# Contributing to ognp (OG Notepad)

Thanks for considering a contribution! **ognp** is a **faithful, tiny re-creation of classic Windows Notepad**:

- No telemetry, no networking, **no external NuGet packages**
- Plain **WinForms** on **.NET 9 (Windows-only)**
- Minimal features: the original Notepad’s behaviors first

Before starting, please skim this doc so we keep scope tight and UX consistent.

---

## Table of Contents

- [Project Scope](#project-scope)
- [Non-Goals](#non-goals)
- [Getting Set Up](#getting-set-up)
- [How to Contribute](#how-to-contribute)
- [Coding Style](#coding-style)
- [Commit Messages](#commit-messages)
- [Pull Request Checklist](#pull-request-checklist)
- [Bug Reports](#bug-reports)
- [Feature Requests](#feature-requests)
- [Security](#security)
- [License](#license)

---

## Project Scope

**Core behaviors** (present or welcome as small, focused changes):

- **File:** New / Open / Save / Save As… / **Page Setup / Print / Print Preview** / Exit
- **Edit:** Undo, Cut/Copy/Paste/Delete, **Find**, **Find Next (F3)**, **Replace**, **Go To (Ctrl+G)**, **Time/Date (F5)**, Select All
- **Format:** **Word Wrap** (off by default), **Font…**
- **View:** **Status Bar** (hidden when Word Wrap is on)
- **Encoding:** BOM detection (UTF-8/16/32); default to **ANSI** when no BOM; choose encoding on Save As
- **EOL:** detect & preserve **CRLF / LF / CR**; show EOL + encoding in status bar
- **UI construction:** **No designer**; UI is built in code for simplicity and diff-friendliness
- **Quality of life:** Drag-and-drop to open; accurate caret/sel in status bar

---

## Non-Goals

To preserve the Notepad feel, **please do not** propose or add:

- Tabs, split panes, multi-document UI
- Syntax highlighting, language services, plugins, LSP
- Cloud sync, telemetry, auto-update, or any network calls
- Heavy theming/skins or non-WinForms stacks
- Large dependencies (NuGet packages, native libs, P/Invoke shims)

**Classic-friendly** enhancements may be discussed (open an issue first), e.g. MRU/Recent Files, command-line `+<line>`, tiny printing refinements.

---

## Getting Set Up

**Prereqs**:

- Windows 10/11
- [.NET SDK **9.0+**](https://dotnet.microsoft.com/download)
- VS Code or Visual Studio (Community is fine)

**Build & Run**:

```powershell
git clone https://github.com/<you>/ognp.git
cd ognp
dotnet run --project ognp
```

**Optional: portable build (self-contained, single file)**:

```powershell
dotnet publish ognp -c Release -r win-x64 `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:SelfContained=true `
  -p:DebugType=None
```

**Formatting**:

```powershell
dotnet format
```

---

## How to Contribute

1. **Open an issue** to discuss the bug/idea (especially for new features).
2. **Fork** the repo and create a branch:

   - `feat/<short-desc>` for features
   - `fix/<short-desc>` for fixes
   - `docs/<short-desc>` for docs
   - `chore/<short-desc>` for misc

3. Make **small, focused** commits.
4. Ensure it builds/runs: `dotnet build` then `dotnet run`.
5. Open a PR referencing the issue and explaining the change.

---

## Coding Style

- Follow **.editorconfig** (included).
- **Target framework:** `net9.0-windows`
- **Namespaces:** **file-scoped namespaces** (e.g., `namespace Ognp;`)
- **Clarity over cleverness.** Prefer explicit, readable code.
- **UI code:** keep simple; **no designer files** (`*.Designer.cs`) unless discussed.
- **Dependencies:** **no external NuGet packages**.
- **Naming**

  - Private fields: `_camelCase`
  - Types/methods/properties: `PascalCase`
  - Constants: `PascalCase`

- **Usings:** `System*` first; group/separate logically.
- **Braces & layout:** standard K\&R for methods/blocks; consistent with existing files.
- **DPI & accessibility:** respect system fonts, themes, DPI scaling; don’t hardcode colors unless necessary.

---

## Commit Messages

Use **Conventional Commits** where practical:

- `fix: prevent null ref in PrintPage when selection empty`
- `feat: add MRU menu (behind setting)`
- `docs: clarify encoding defaults in README`
- `chore: enable dotnet format in CI`

Keep subjects under \~72 chars; add a short body if helpful.

---

## Pull Request Checklist

**Behavior parity (must-check)**:

- [ ] **Word Wrap ↔ Status Bar:** Status Bar hides when Word Wrap is on; shows when off.
- [ ] **Go To availability:** disabled under Word Wrap (classic behavior).
- [ ] **EOL preservation:** open → edit → save keeps original **CRLF/LF/CR**.
- [ ] **Encoding rules:** BOM honored; no-BOM opens as **ANSI**; Save As can choose encoding; status bar reflects reality.
- [ ] **Find / F3 / Replace:** matches Notepad shortcuts and flow.
- [ ] **Printing:** Page Setup/Print/Preview behave; long lines wrap across pages; no unexpected headers/footers.
- [ ] **Drag & drop:** still opens files correctly.

**Quality & scope**:

- [ ] Change is **within scope** (classic Notepad feel).
- [ ] Builds on Windows with **.NET 9+**.
- [ ] **No new external dependencies.**
- [ ] **Binary size** impact is minimal (avoid bloat).
- [ ] Follows `.editorconfig` (`dotnet format` is clean).
- [ ] Minimal UI changes; consistent menus/shortcuts.
- [ ] Clear before/after explanation; screenshots or a short GIF if UI changed.
- [ ] Tests added **if** behavior is subtle/regression-prone (e.g., EOL/encoding handling).

**Optional but nice**:

- [ ] Commits are **GPG-signed** (not required).
- [ ] PR title uses Conventional Commit style.

---

## Bug Reports

Please include:

- Windows version; `.NET SDK` version (`dotnet --info`)
- Exact steps to reproduce (file path, encoding, EOL if relevant)
- Expected vs. actual behavior
- Any exception text/stack trace (full message)
- A minimal sample file if the issue is encoding/EOL-specific

---

## Feature Requests

Open a GitHub issue first. Keep proposals **tiny and classic-friendly**. Include a short rationale and UX notes (menu item, shortcut, interactions with Word Wrap/Status Bar, etc.). If it risks scope creep, it likely won’t be accepted.

---

## Security

ognp is an offline WinForms app with **no networking**.
If you think you’ve found a vulnerability, please use **GitHub Private Vulnerability Reporting** or email the maintainer. **Do not** open a public issue for security topics.

---

## License

By contributing, you agree your contributions will be licensed under the **GPL-3.0-or-later** license of this repository.

**Trademarks/IP:** Don’t include Microsoft assets or logos. The project is not affiliated with or endorsed by Microsoft.

---

If you want, I can also drop in matching **Issue/PR templates** and a minimal **SECURITY.md** to round out the contributor experience.

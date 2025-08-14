# Contributing to ognp (OG Notepad)

Thanks for considering a contribution! ognp aims to be a **faithful, tiny re-creation of classic Windows Notepad**:

- No telemetry, no networking, **no external NuGet packages**
- Plain **WinForms** on **.NET 8+ (Windows)**
- Minimal features: the original Notepad’s behaviors first

Before starting, please skim this doc so we keep the scope tight and the UX consistent.

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

**Core behaviors** (already present or welcome as small, focused changes):

- File: New / Open / Save / Save As… / Exit
- Edit: Undo, Cut/Copy/Paste/Delete, Find, Find Next (F3), Replace, Go To (Ctrl+G), Time/Date (F5), Select All
- Format: Word Wrap (off by default), Font…
- View: Status Bar (hidden when Word Wrap is on)
- Encoding: BOM detection; default to ANSI when no BOM; choose encoding on Save As
- EOL: detect & preserve CRLF / LF / CR; show in status bar
- **No designer**: UI is built in code for simplicity/diff-friendliness

---

## Non-Goals

To preserve the Notepad feel, **please do not** propose or add:

- Tabs, split panes, multi-document UI
- Syntax highlighting, language services, plugins, LSP
- Cloud sync, telemetry, auto-update, network calls
- Heavy theming/skins or non-WinForms stacks
- Large dependencies (NuGet packages, native libs)

Reasonable, **classic-friendly** enhancements may be discussed (e.g., Printing, MRU/Recent Files, drag-and-drop open, command-line `+<line>`). Open an issue first.

---

## Getting Set Up

**Prereqs**:

- Windows 10/11
- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
- VS Code or Visual Studio (Community is fine)

**Build & Run**:

```powershell
git clone https://github.com/<you>/ognp.git
cd ognp
dotnet run --project ognp
```

**Optional: portable build**:

```powershell
dotnet publish ognp -c Release -r win-x64 `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  --self-contained true
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

3. Make small, focused commits.
4. Ensure it builds: `dotnet build` and run: `dotnet run`.
5. Open a PR that references the issue and explains the change.

---

## Coding Style

- Follow `.editorconfig` (included in the repo).
- **C#/.NET**

  - Target `net8.0-windows`.
  - Prefer **explicit clarity** over cleverness.
  - Keep UI code simple; avoid designer files (`*.Designer.cs`) and auto-generated resources unless discussed.
  - No external NuGet packages.

- **Naming**

  - Private fields: `_camelCase`
  - Types/methods/properties: `PascalCase`
  - Constants: `PascalCase`

- **Usings**

  - `System` namespaces first; separated groups.

- **Braces & layout**

  - Block-scoped namespaces; braces on new lines.

---

## Commit Messages

Use **Conventional Commits** where practical:

- `fix: correct off-by-one in Go To`
- `feat: add print dialog (behind menu)`
- `docs: update README with portable build steps`
- `chore: enable dotnet format in CI`

Keep subject lines under \~72 chars; add a short body if helpful.

---

## Pull Request Checklist

- [ ] Change is **within scope** (classic Notepad feel).
- [ ] Builds on Windows with `.NET 8+`.
- [ ] No new external dependencies.
- [ ] Follows `.editorconfig` (run `dotnet format`).
- [ ] Minimal UI changes; consistent with existing menus/shortcuts.
- [ ] Clear explanation of the before/after behavior.
- [ ] Screenshots if the UI changed.

---

## Bug Reports

Please include:

- Windows version; `.NET SDK` version (`dotnet --info`)
- Steps to reproduce (exact file, encoding, EOL if relevant)
- Expected vs. actual behavior
- Any exception text/stack trace if applicable

---

## Feature Requests

Open a GitHub issue first. Keep proposals **tiny and classic-friendly**. Add a short rationale and UX notes (menu item, shortcut, interactions with Word Wrap/Status Bar, etc.).

---

## Security

ognp is an offline WinForms app with no networking.
If you think you’ve found a vulnerability, please open a **private** advisory on GitHub or email the maintainer. Avoid filing public issues for security topics.

---

## License

By contributing, you agree your contributions will be licensed under the **GPL-3.0-or-later License** of this repository.

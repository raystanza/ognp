# Summary

<!-- What does this PR change, and why? Keep it small and focused. -->

Fixes: #\_\_\_\_ (optional)

---

## Changes

- [ ] User-visible behavior change? If yes, describe it clearly.
- [ ] Internal refactor? If yes, list risk areas.

Screenshots/GIF (if UI changed):

---

## Classic behavior checks (please tick those that apply)

- [ ] **Word Wrap ↔ Status Bar**: Status Bar hides when Word Wrap is **ON**; shows when **OFF**.
- [ ] **Go To**: disabled when Word Wrap is **ON**.
- [ ] **EOL preservation**: open → edit → save keeps original **CRLF/LF/CR**.
- [ ] **Encoding rules**: BOM honored; no-BOM opens as **ANSI**; Save As offers encoding; Status Bar shows true encoding/EOL.
- [ ] **Find / F3 / Replace**: shortcut flow matches classic Notepad.
- [ ] **Printing**: Page Setup / Print / Print Preview work; long lines wrap; no unexpected headers/footers.
- [ ] **Drag & drop**: still opens files correctly.
- [ ] **Caret/selection** in Status Bar remains accurate.

---

## Testing notes

- OS: (e.g., Windows 11 23H2 x64)
- Runtime: (`dotnet --info` key lines)
- Display scaling/DPI: (e.g., 150%)
- Test files used (encodings/EOL): (attach minimal samples if relevant)

---

## Implementation details

- [ ] Targets **`net9.0-windows`**.
- [ ] **No external NuGet packages** or new native deps.
- [ ] **No designer files** (`*.Designer.cs`) introduced unless explicitly discussed.
- [ ] Follows **.editorconfig**; `dotnet format` is clean.
- [ ] Binary size impact is minimal.

If this PR touches **encoding/EOL/printing**, briefly explain key decisions:

---

## Docs

- [ ] README updated (if user-visible change).
- [ ] CONTRIBUTING updated (if process/style changed).

---

## CI

Ensure required status checks are green:

- `Release (Windows .NET 9) / build-release` **or** your configured build job.

---

## Notes for the maintainer

<!-- Any migration concerns, follow-ups, or items to watch for in future PRs. -->

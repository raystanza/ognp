# Security Policy

ognp (OG Notepad) is an **offline**, **dependency-minimal** WinForms text editor for Windows.  
It has no telemetry and makes no network requests. The primary risk surface is **local file handling** (open/save, encodings, line endings, printing, and command-line arguments).

---

## Supported Versions

Security fixes are applied to:

- **Latest release** and the **`main`** branch

Older releases are not maintained for security updates.

---

## Reporting a Vulnerability

**Please report privately. Do not open a public issue.**

### Preferred: GitHub Private Vulnerability Reporting

1. Go to the repo’s **Security** tab → **Advisories** → **Report a vulnerability**.
2. Include details (see checklist below).

### Alternative: Email

- Send details to: <ognp-sec@raystanza.uk>
- Optionally encrypt with PGP (key below).

### What to include

- Clear description of the issue and why it’s a security concern.
- **Repro steps** (minimal file/sample if possible).
- **Expected vs. actual** behavior.
- Version/commit, **Windows version**, and key lines from `dotnet --info`.
- Any crash output, stack traces, or screenshots.
- Impact assessment (data loss, arbitrary write, local DoS, etc.).

### Our process & timelines

- **Acknowledgement:** within **7 business days**.
- **Triage & assessment:** within **14 business days** (often sooner).
- **Fix & release:** coordinated with you; timing depends on complexity.
- We can credit you in release notes (or keep you anonymous—your choice).

---

## Scope

### In scope

Vulnerabilities that cause or enable:

- **Data loss or corruption** (e.g., save/open mishandling, encoding/EOL issues that destroy data without user intent).
- **Arbitrary file overwrite** outside the user’s chosen path.
- **Local code execution** or **unsafe path traversal** via command-line or drag-and-drop (if/when applicable).
- **Reliable local denial of service** (crash/hang) triggered by a crafted text file.

### Out of scope

- Issues in **.NET runtime**, **WinForms**, or **Windows** itself (report to Microsoft).
- Pure **performance limits** on extremely large files (report as a normal bug unless it becomes a reliable DoS).
- Preferences/expectations that contradict documented **encoding/EOL** behavior.
- Social engineering, phishing, or issues requiring privileges you already possess on your device.
- Antivirus false positives or corporate policy blocks.

---

## Coordinated Disclosure

- Please keep details private until a fix is released.
- We’ll coordinate a public advisory and changelog.
- If no fix is planned, we’ll document mitigations/workarounds and disclose responsibly.

---

## Safe Harbor

We support **good-faith security research**:

- Don’t exploit beyond what’s needed to demonstrate the issue.
- Don’t access or modify data that isn’t yours.
- Don’t degrade other users or systems.
- Test only on devices/files you own or have permission to use.

If you act in good faith and follow this policy, we won’t initiate legal action.

---

## Supply Chain & Release Integrity

- **Dependencies:** No external NuGet packages; the app relies on the **.NET Desktop Runtime** only.
- **Repo hardening:** Branch protection, required status checks, **CodeQL code scanning**, **secret scanning + push protection**, and Dependabot alerts/updates are enabled.
- **Releases:** Each release includes `ognp.exe` and a **SHA256** sidecar file (`ognp.exe.sha256`, `sha256sum -b` format).

**Verify downloads (recommended):**

- **Windows (PowerShell):**

  ```powershell
  cd "folder\with\downloads"
  $expected = (Get-Content .\ognp.exe.sha256).Split(" ")[0]
  $actual   = (Get-FileHash -Algorithm SHA256 .\ognp.exe).Hash.ToLower()
  if ($actual -eq $expected) { "OK: SHA256 matches." } else { "MISMATCH!" }
  ```

---

## Contact

- **Preferred:** GitHub Private Vulnerability Reporting (Security → Advisories)
- **Email (security):** [ognp-sec@raystanza.uk](mailto:ognp-sec@raystanza.uk)
- **Email (general):** [ognp@raystanza.uk](mailto:ognp@raystanza.uk)

**Commit-signing PGP fingerprint:**

```text
9516D1AA5BF4241433F3796F26BEB79A8306E33B
```

Thank you for helping keep ognp users safe!

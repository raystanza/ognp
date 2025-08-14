# Security Policy

ognp (OG Notepad) is an **offline**, **dependency-free** WinForms text editor.  
It has no telemetry and makes no network requests. Its primary risk surface is **local file handling** (open/save, encodings, line endings, and command-line arguments).

---

## Supported Versions

Security fixes are applied to:

- **Latest release** and the **`main`** branch

Older releases are not maintained for security updates.

---

## Reporting a Vulnerability

**Please report privately. Do not open a public issue.**

### Preferred: GitHub Security Advisory

1. Go to the repo’s **Security** tab → **Advisories** → **Report a vulnerability**.
2. Include details (see checklist below).

### Alternative: Email

- Send details to: <ognp-sec@raystanza.uk>
- Optionally encrypt with PGP: **_PGP key / fingerprint here_**.

### What to include

- A clear description of the issue and why it’s a security concern.
- Repro steps (minimal file/sample if possible).
- Expected vs. actual behavior.
- Version / commit, OS version, and `.NET SDK/runtime` (`dotnet --info`).
- Any crash output, stack traces, or screenshots.
- Impact assessment (data loss, arbitrary write, local DoS, etc.).

### Our process & timelines

- **Acknowledgement:** within **7 business days**.
- **Triage & assessment:** within **14 business days** (often sooner).
- **Fix & release:** coordinated with you; timing depends on complexity.
- We’ll credit you in release notes unless you prefer to remain anonymous.

---

## Scope

### In scope

- Vulnerabilities that cause or enable:
  - **Data loss or corruption** (e.g., save/open mishandling, encoding/EOL issues that destroy data without user intent).
  - **Arbitrary file overwrite** outside the chosen path.
  - **Local code execution** or **path traversal** via command-line arguments or drag-and-drop (if/when implemented).
  - **Local denial of service** (crash or hang) triggered by a crafted text file that is **reliable and reproducible**.

### Out of scope

- Issues in the **.NET runtime**, **WinForms**, or **Windows OS** itself (please report to their vendors).
- Pure **performance** limitations when opening extremely large files (report as a normal bug unless it crosses into reliable DoS).
- Expected behavior of encodings/EOL preservation (preferences rather than vulnerabilities).
- Social engineering, phishing, or issues requiring privileged access you already possess on your own machine.
- Antivirus false positives or corporate policy restrictions.

---

## Coordinated Disclosure

- Please keep details private until a fix is released.
- We’ll coordinate a public advisory and changelog note.
- If no fix is planned, we’ll document mitigations/workarounds and disclose responsibly.

---

## Safe Harbor

We support **good-faith security research**:

- Don’t exploit beyond what’s required to prove the issue.
- Don’t access data that isn’t yours.
- Don’t degrade other users or systems.
- Follow applicable laws and test only on devices/files you own or have permission to use.

If you act in good faith and follow this policy, we won’t initiate legal action.

---

## Dependencies & Supply Chain

- The project avoids external NuGet packages. It relies on the **.NET SDK/runtime** only.
- For vulnerabilities in .NET itself, see Microsoft’s advisories and updates.
- When release signatures/checksums are provided in the future, verify them before installing.

---

## Contact

- **GitHub Security Advisory (preferred)**
- **Email:** <ognp@raystanza.uk>
- **PGP Public Key:**

```text
-----BEGIN PGP PUBLIC KEY BLOCK-----

mDMEaClG4hYJKwYBBAHaRw8BAQdAbY6JpQ7XbbPPYHB4gOjto062MxAINTqZ6DS4
Amzmwnm0InJheXN0YW56YSA8cmF5c3RhbnphQHJheXN0YW56YS51az6InAQTFgoA
RAIbAwUJBaRXngULCQgHAgIiAgYVCgkICwIEFgIDAQIeBwIXgBYhBP2gbK4mTawN
KbA/UZXt7f3LTdgmBQJoYbphAhkBAAoJEJXt7f3LTdgmjYsA/1RkcbDAGC2TPbiD
z1B8eYi0QJsFNSzzDRYdrDKMwef3AP46MBjpnbTU6/TetKcPnK9+R7rVWq0fw6Ht
yr/Al5aNDLQ3cmF5c3RhbnphIDw1MTgwNTI2OCtyYXlzdGFuemFAdXNlcnMubm9y
ZXBseS5naXRodWIuY29tPoiZBBMWCgBBFiEE/aBsriZNrA0psD9Rle3t/ctN2CYF
AmhhuhUCGwMFCQWkV54FCwkIBwICIgIGFQoJCAsCBBYCAwECHgcCF4AACgkQle3t
/ctN2CYNtgEAzkWtwpxbw/2MiaIo46cYq3mNlME9TrA27yCEySxNnvoBAMQ315hi
Vp3SPUImpR9NeNoRGIzo2LDr1nTD2pj8fw4DuDgEaClG4hIKKwYBBAGXVQEFAQEH
QKfP2rCo+0nxR3BDPafomPFvkAMdW8HjxPuNDS+BFmQzAwEIB4h+BBgWCgAmFiEE
/aBsriZNrA0psD9Rle3t/ctN2CYFAmgpRuICGwwFCQWkV54ACgkQle3t/ctN2CZW
0wD+PBdCWDBIo6IazlvAMMJCqS7IVAAHCI466vG2VzhQA7EA/AjqkzKEQdSekQ+o
YePk8mNFQ00TipnPP5l4V7EgLUIKuDMEaGHBfxYJKwYBBAHaRw8BAQdAhJwloBNE
6gGxEvFGvesMtl3yU4hcIolnOt8Q/OAy526I9QQYFgoAJhYhBP2gbK4mTawNKbA/
UZXt7f3LTdgmBQJoYcF/AhsCBQkFo5qAAIEJEJXt7f3LTdgmdiAEGRYKAB0WIQSV
FtGqW/QkFDPzeW8mvreagwbjOwUCaGHBfwAKCRAmvreagwbjO9dkAP9bBPiFQdg7
48NWaO1pGOSuJkdxRWfbR9PGHrescOacTwD6ApB95qjT5OrMn9lS6oibUn0wAo/m
2xdxFA9uCCuJ5gzdxQD/ZHJZHOsHbWC6KS2i2edglXqinRuR+u9sPkgXpTayTncA
/0jdprEDkFX24RlcuPXsUPRJrSbcqxMafRgl4PLyBv4A
=kvrc
-----END PGP PUBLIC KEY BLOCK-----
```

- **Commit Signing PGP Fingerprint:**

```text
9516D1AA5BF4241433F3796F26BEB79A8306E33B
```

Thank you for helping keep ognp users safe!

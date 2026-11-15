# Contributing to X-Charge

Thank you for considering contributing to **X-Charge**!

## How Can I Contribute?

### Reporting Bugs
- Check the [Issues](../../issues) tab to see if the bug has already been reported.
- If not, open a new issue using the **Bug Report** template.
- Include controller connection details (Wireless Adapter, Bluetooth, or USB).

### Suggesting Enhancements
- Open an issue using the **Feature Request** template.
- Describe the motivation and how the proposed change benefits users.

### Submitting Pull Requests
1. Fork the repository and create your branch from `main`:
   ```bash
   git checkout -b feature/my-new-feature
   ```
2. Build and verify locally:
   ```powershell
   .\build.ps1 -Clean
   ```
3. Commit your changes with clear, semantic commit messages:
   ```bash
   git commit -m "feat: add support for custom hotkeys"
   ```
4. Push to your branch and submit a Pull Request.

## Coding Guidelines
- Follow standard C# and .NET naming conventions.
- Maintain **zero RF interference**: never introduce blocking WinRT/HID operations that disrupt the official Xbox Wireless Adapter.
- Keep GDI handles clean: always invoke `DestroyIcon` after calling `GetHicon()`.
- Add localizations for new user-facing strings across all 7 supported languages in `Strings.cs`.

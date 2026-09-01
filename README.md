# CatX

CatX is a friendly Windows 11 keyboard guard for anyone whose cat believes a keyboard is a heated bed. When the guard is enabled, ordinary keyboard input is ignored until you hold your chosen recovery shortcut. While the keys are guarded, an original animated cat wanders around the desktop.

![Windows 11](https://img.shields.io/badge/Windows-11-0078D4?logo=windows11&logoColor=white)
![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![License](https://img.shields.io/badge/license-MIT-3E8878)
[![Build](https://github.com/digitalrcs/CatX/actions/workflows/build.yml/badge.svg)](https://github.com/digitalrcs/CatX/actions/workflows/build.yml)

## Download and use

1. Open the [latest CatX release](https://github.com/digitalrcs/CatX/releases/latest).
2. Download `CatX-win-x64.zip` and extract it.
3. Run `CatX.exe` on a 64-bit Windows 11 PC.
4. Choose a cat, recovery shortcut, and movement interval.
5. Press **Enable keyboard guard** and immediately test the displayed shortcut.

CatX is currently distributed as an unsigned community application. Windows SmartScreen may show **Windows protected your PC** on first launch. Confirm that the publisher is unknown, select **More info**, and choose **Run anyway** only if the download came from this repository. A future release may be code-signed.

## Features

- Blocks ordinary keyboard input with a Windows low-level keyboard hook.
- Offers three user-selectable recovery shortcuts.
- Includes five original vector cat styles: Marmalade, Midnight, Snowball, Tuxedo, and Calico.
- Includes a **No cat** preference for keyboard-only protection.
- Animates a click-through cat across the entire Windows virtual desktop, including multiple monitors.
- Lets you choose how often the cat changes location.
- Can automatically enable the guard after an optional 30-second, 1-minute, 5-minute, 15-minute, 30-minute, or 1-hour countdown.
- Saves preferences locally in `%LOCALAPPDATA%\CatX\settings.json`.
- Makes no network requests, requires no account, and collects no data.
- Keeps the mouse usable and cannot block Windows' secure `Ctrl + Alt + Delete` screen.

> [!IMPORTANT]
> Test your recovery shortcut before leaving CatX enabled. If needed, use the mouse to select **Disable guard with mouse**, close CatX, or press `Ctrl + Alt + Delete` and use Windows' secure screen.

## Run from source

Requirements: Windows 11 and the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```powershell
git clone https://github.com/digitalrcs/CatX.git
cd CatX
dotnet run --project .\src\CatX\CatX.csproj
```

CatX does not require administrator privileges.

## Build a single Windows executable

```powershell
dotnet publish .\src\CatX\CatX.csproj `
  --configuration Release `
  --runtime win-x64 `
  --self-contained true `
  --output .\publish
```

The self-contained executable will be `publish\CatX.exe`. It includes the .NET runtime, so the destination PC does not need .NET installed.

## How the safety model works

CatX installs its keyboard hook only after you press **Enable keyboard guard** or an enabled auto-lock countdown expires. Because guarded modifier events are themselves suppressed, CatX tracks Ctrl, Alt, and Shift directly from the low-level hook rather than relying on Windows' post-event key state. The selected recovery chord is checked before the final key event is suppressed. When it matches, CatX immediately removes the hook and restores normal input. Closing the application also removes the hook.

Windows handles `Ctrl + Alt + Delete` outside normal user applications, so CatX cannot suppress it. CatX intentionally never blocks the mouse. It does not run as a service, start with Windows, or persist a lock after the process exits.

## Project layout

```text
src/CatX/
  Models/              Preferences and unlock-chord definitions
  Services/            Keyboard guard and local settings storage
  CatOverlayWindow.*   Click-through animated desktop cat
  MainWindow.*         Settings and guard controls
tests/CatX.Tests/      Recovery-chord regression tests
```

## Contributing

Bug reports and pull requests are welcome. Please read [CONTRIBUTING.md](CONTRIBUTING.md) before making a change. For security concerns, follow [SECURITY.md](SECURITY.md).

Additional project information:

- [Architecture](docs/ARCHITECTURE.md)
- [Troubleshooting](docs/TROUBLESHOOTING.md)
- [Privacy](docs/PRIVACY.md)
- [Roadmap](ROADMAP.md)
- [Changelog](CHANGELOG.md)
- [Support](SUPPORT.md)

## Current limitations

- The release is for 64-bit Windows 11 (`win-x64`). ARM64 is not packaged yet.
- CatX protects the current interactive desktop session; it is not a driver or Windows service.
- Cat artwork is vector-based and built into the application. Importing custom animation files is planned, not yet implemented.
- Release executables are not currently code-signed.

## Artwork and naming

All cats in CatX are original vector illustrations created for this project. CatX is not affiliated with Garfield, Paws, Inc., or any other fictional-cat property.

## License

CatX is available under the [MIT License](LICENSE).

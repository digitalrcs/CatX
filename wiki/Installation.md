# Install, update, or remove CatX

## Requirements

- A **64-bit Windows 11** PC.
- No administrator privileges or separate .NET runtime installation are required for the release builds.
- Blender is not required to display the realistic cats.

## Choose a download

Get files from the [official releases page](https://github.com/digitalrcs/CatX/releases/latest).

| Download | Use it for |
| --- | --- |
| `CatX-Windows11-<version>.zip` | The normal installation package. Extract everything and run `Setup.exe`. |
| `CatX-Setup-<version>.exe` | The standalone installer. |
| `CatX-win-x64.zip` | Portable use. Extract everything to a folder and run `CatX.exe`. |
| `SHA256SUMS.txt` | Checking downloaded file hashes. |
| Versioned PDF user guide | An offline, printable reference. |

Follow the installer prompts, optionally create a desktop shortcut, and launch CatX. The installer targets the current Windows user.

CatX release executables are currently unsigned. If SmartScreen appears, verify that the file came from the official release before using **More info → Run anyway**.

## Update

Exit CatX from its tray menu before running the newer installer. For portable use, extract the new release into its own folder and launch that copy after exiting the old one. Preferences are stored separately from the executable.

## Optional download verification

Run PowerShell in your download folder:

```powershell
Get-FileHash .\CatX-Setup-1.2.0.exe -Algorithm SHA256
```

Compare the result with the matching filename in the release's `SHA256SUMS.txt`. Adjust the filename for the version you downloaded.

## Uninstall

Open **Windows Settings → Apps → Installed apps → CatX Keyboard Guard → Uninstall**.

For a portable copy, exit CatX and remove its extracted folder. Preferences may remain in `%LOCALAPPDATA%\CatX\settings.json`; remove that file while CatX is closed if you also want to reset your settings.

**Next:** [Quick start](Quick-Start)

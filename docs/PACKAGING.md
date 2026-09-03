# Packaging CatX for Windows

CatX uses a self-contained .NET 8 publish and an Inno Setup installer. The resulting installer works on 64-bit Windows 11, installs for the current user without requiring administrator privileges, creates an uninstall entry, and can create Start Menu and desktop shortcuts.

## Prerequisites

- Windows 11
- .NET 8 SDK
- [Inno Setup 6](https://jrsoftware.org/isdl.php)

## Build the installer

From the repository root:

```powershell
.\installer\Build-Installer.ps1
```

The script runs the Release tests, publishes a self-contained single-file `win-x64` build to `publish\CatX`, verifies the PDF user guide, compiles `installer\CatX.iss`, and creates the end-user ZIP package.

The finished outputs are:

- `dist\CatX-Setup-1.0.3.exe` - standalone installer.
- `dist\CatX-Windows11-1.0.3.zip` - distribution package containing a root-level `Setup.exe`, user guide, README, and license.

An end user extracts the ZIP and runs `Setup.exe`.

For a nonstandard Inno Setup location:

```powershell
.\installer\Build-Installer.ps1 -InnoCompiler "D:\Tools\Inno Setup 6\ISCC.exe"
```

## Code signing for public distribution

The repository does not contain a signing certificate or private key. For a public release, sign both the published `CatX.exe` and final setup executable with an organization-controlled code-signing certificate. Keep the private key and certificate password outside the repository and CI logs.

Microsoft SignTool is included with the Windows SDK. Use SHA-256 for the file digest and RFC 3161 timestamp digest. Adapt the certificate selection and timestamp URL to the certificate provider:

```powershell
signtool sign /fd SHA256 /td SHA256 /tr "https://your-provider.example/timestamp" /sha1 "CERTIFICATE_THUMBPRINT" ".\publish\CatX\CatX.exe"
signtool sign /fd SHA256 /td SHA256 /tr "https://your-provider.example/timestamp" /sha1 "CERTIFICATE_THUMBPRINT" ".\dist\CatX-Setup-1.0.3.exe"
signtool verify /pa /v ".\dist\CatX-Setup-1.0.3.exe"
```

Official SignTool reference: [Microsoft Learn](https://learn.microsoft.com/windows/win32/seccrypto/signtool)

## Release checklist

- Run `dotnet test .\CatX.sln --configuration Release`.
- Extract the distribution ZIP and run `Setup.exe` on a clean Windows 11 test account.
- Confirm the publisher, icon, version, install location, shortcuts, bundled guide, and uninstall entry.
- Confirm every original cat can be selected.
- Enable the guard and test every recovery shortcut.
- Confirm the cat faces its travel direction and remains fully visible while moving.
- Sign and timestamp both executables before publishing.
- Scan the final installer and publish its SHA-256 checksum with the release.

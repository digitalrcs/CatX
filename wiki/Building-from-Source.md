# Building from source

## Requirements

- Windows 11
- .NET 8 SDK
- Inno Setup 6 for the installer

## Run CatX

```powershell
git clone https://github.com/digitalrcs/CatX.git
cd CatX
dotnet run --project .\src\CatX\CatX.csproj
```

## Test CatX

```powershell
dotnet test .\CatX.sln --configuration Release
```

## Build the Windows installer and ZIP

```powershell
.\installer\Build-Installer.ps1 -Configuration Release
```

The script tests CatX, publishes a self-contained `win-x64` executable, compiles the installer, and creates the Windows 11 distribution ZIP under `dist`.

Read [CONTRIBUTING.md](https://github.com/digitalrcs/CatX/blob/main/CONTRIBUTING.md) before submitting a pull request.

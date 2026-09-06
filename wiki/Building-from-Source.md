# Build and contribute

## Requirements

- Windows 11 and the **.NET 8 SDK**.
- Git to clone the repository.
- **Inno Setup 6** if you want to build the installer.
- Blender only if rebuilding the realistic animation assets.

## Run

```powershell
git clone https://github.com/digitalrcs/CatX.git
cd CatX
dotnet run --project .\src\CatX\CatX.csproj
```

## Test

```powershell
dotnet test .\CatX.sln --configuration Release
```

Tests cover recovery-chord handling and behavior logic, including the cat roster, nap placement, and mouse trips.

## Publish a self-contained application

```powershell
dotnet publish .\src\CatX\CatX.csproj --configuration Release --runtime win-x64 --self-contained true --output .\publish
```

Run `publish\CatX.exe`. The destination PC does not need the .NET runtime installed separately.

For a developer visual review without enabling the keyboard guard:

```powershell
.\publish\CatX.exe --review
```

Review mode leaves saved preferences unchanged. Close the application to end the review.

## Build the installer

```powershell
.\installer\Build-Installer.ps1 -Configuration Release
```

The packaging script tests the project, publishes the application, compiles the installer, and creates distribution files in `dist`.

## Explore the project

| Area | Reference |
| --- | --- |
| Architecture and keyboard hook | [Architecture](https://github.com/digitalrcs/CatX/blob/main/docs/ARCHITECTURE.md) |
| Realistic frames and asset pipeline | [Animated cats](https://github.com/digitalrcs/CatX/blob/main/docs/ANIMATED_CATS.md) |
| Packaging and signing | [Packaging](https://github.com/digitalrcs/CatX/blob/main/docs/PACKAGING.md) |
| Contribution guidelines | [Contributing](https://github.com/digitalrcs/CatX/blob/main/CONTRIBUTING.md) |
| Documentation source | [wiki folder](https://github.com/digitalrcs/CatX/tree/main/wiki) |

The main-window and cat-style images are exported from WPF by `tools/Export-ReadmeImages.ps1`. Cheese-chase review scenes come from `tools/Export-CheeseChaseReview.ps1`.

Source code uses the MIT license. Supplied third-party Blender assets and their rendered frames retain their original licensing; see the repository's artwork notes.

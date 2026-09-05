param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [string]$InnoCompiler
)

$ErrorActionPreference = "Stop"
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$project = Join-Path $repoRoot "src\CatX\CatX.csproj"
$solution = Join-Path $repoRoot "CatX.sln"
$publishDir = Join-Path $repoRoot "publish\CatX"
$guide = Join-Path $repoRoot "output\pdf\CatX-User-Guide-v1.0.3.pdf"
$installerScript = Join-Path $PSScriptRoot "CatX.iss"
$packageReadme = Join-Path $PSScriptRoot "PACKAGE_README.txt"

[xml]$projectXml = Get-Content -LiteralPath $project
$version = [string]$projectXml.Project.PropertyGroup.Version
if ([string]::IsNullOrWhiteSpace($version)) {
    throw "CatX version was not found in $project"
}

& (Join-Path $repoRoot "tools\Test-RealisticAssets.ps1")

dotnet test $solution --configuration $Configuration
if ($LASTEXITCODE -ne 0) { throw "Tests failed." }

dotnet publish $project --configuration $Configuration --runtime win-x64 --self-contained true --output $publishDir
if ($LASTEXITCODE -ne 0) { throw "Publish failed." }

if (-not (Test-Path -LiteralPath $guide)) {
    throw "The bundled user guide is missing: $guide"
}

if ([string]::IsNullOrWhiteSpace($InnoCompiler)) {
    $candidates = @(
        (Join-Path ${env:ProgramFiles(x86)} "Inno Setup 6\ISCC.exe"),
        (Join-Path $env:ProgramFiles "Inno Setup 6\ISCC.exe"),
        (Join-Path $env:LOCALAPPDATA "Programs\Inno Setup 6\ISCC.exe")
    ) | Where-Object { $_ -and (Test-Path -LiteralPath $_) }
    $InnoCompiler = $candidates | Select-Object -First 1
}

if (-not $InnoCompiler -or -not (Test-Path -LiteralPath $InnoCompiler)) {
    throw "Inno Setup 6 was not found. Install it from https://jrsoftware.org/isdl.php or pass -InnoCompiler."
}

& $InnoCompiler "/DMyAppVersion=$version" $installerScript
if ($LASTEXITCODE -ne 0) { throw "Inno Setup compilation failed." }

$setup = Join-Path $repoRoot "dist\CatX-Setup-$version.exe"
if (-not (Test-Path -LiteralPath $setup)) {
    throw "Expected installer was not produced: $setup"
}

$distDir = Join-Path $repoRoot "dist"
$packageName = "CatX-Windows11-$version"
$packageDir = Join-Path $distDir $packageName
$packageZip = Join-Path $distDir "$packageName.zip"
$resolvedDistDir = [System.IO.Path]::GetFullPath($distDir).TrimEnd([System.IO.Path]::DirectorySeparatorChar)
$resolvedPackageDir = [System.IO.Path]::GetFullPath($packageDir)

if (-not $resolvedPackageDir.StartsWith("$resolvedDistDir$([System.IO.Path]::DirectorySeparatorChar)", [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing to replace a package directory outside dist: $resolvedPackageDir"
}

if (Test-Path -LiteralPath $packageDir) {
    Remove-Item -LiteralPath $packageDir -Recurse -Force
}
New-Item -ItemType Directory -Path $packageDir | Out-Null
Copy-Item -LiteralPath $setup -Destination (Join-Path $packageDir "Setup.exe")
Copy-Item -LiteralPath $guide -Destination (Join-Path $packageDir "CatX-User-Guide.pdf")
Copy-Item -LiteralPath $packageReadme -Destination (Join-Path $packageDir "README.txt")
Copy-Item -LiteralPath (Join-Path $repoRoot "LICENSE") -Destination (Join-Path $packageDir "LICENSE.txt")
Copy-Item -LiteralPath (Join-Path $repoRoot "docs\ANIMATED_CATS.md") -Destination (Join-Path $packageDir "ANIMATED_CATS.md")
Copy-Item -LiteralPath (Join-Path $repoRoot "docs\ANIMATION_REVIEW.md") -Destination (Join-Path $packageDir "ANIMATION_REVIEW.md")
Copy-Item -LiteralPath (Join-Path $repoRoot "docs\USER_GUIDE.md") -Destination (Join-Path $packageDir "USER_GUIDE.md")

if (Test-Path -LiteralPath $packageZip) {
    Remove-Item -LiteralPath $packageZip -Force
}
Compress-Archive -Path (Join-Path $packageDir "*") -DestinationPath $packageZip -CompressionLevel Optimal

$setupHash = (Get-FileHash -LiteralPath (Join-Path $packageDir "Setup.exe") -Algorithm SHA256).Hash
$zipHash = (Get-FileHash -LiteralPath $packageZip -Algorithm SHA256).Hash
Write-Host "Created installer: $setup"
Write-Host "Created Windows 11 package: $packageZip"
Write-Host "Setup.exe SHA256: $setupHash"
Write-Host "ZIP SHA256: $zipHash"

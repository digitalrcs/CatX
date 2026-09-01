param(
    [string]$Source = (Join-Path $PSScriptRoot "..\src\CatX\Assets\catx-app-icon.png"),
    [string]$Output = (Join-Path $PSScriptRoot "..\src\CatX\Assets\CatX.ico")
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Drawing

$sourceBitmap = [System.Drawing.Bitmap]::FromFile((Resolve-Path -LiteralPath $Source))
$iconSizes = @(16, 24, 32, 48, 64, 128, 256)
$pngEntries = @()

try {
    foreach ($size in $iconSizes) {
        $bitmap = [System.Drawing.Bitmap]::new($size, $size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
        $stream = [System.IO.MemoryStream]::new()

        try {
            $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
            $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
            $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
            $graphics.DrawImage($sourceBitmap, 0, 0, $size, $size)
            $bitmap.Save($stream, [System.Drawing.Imaging.ImageFormat]::Png)
            $pngEntries += [pscustomobject]@{ Size = $size; Bytes = $stream.ToArray() }
        }
        finally {
            $stream.Dispose()
            $graphics.Dispose()
            $bitmap.Dispose()
        }
    }
}
finally {
    $sourceBitmap.Dispose()
}

$outputPath = [System.IO.Path]::GetFullPath($Output)
$file = [System.IO.File]::Create($outputPath)
$writer = [System.IO.BinaryWriter]::new($file)

try {
    $writer.Write([uint16]0)
    $writer.Write([uint16]1)
    $writer.Write([uint16]$pngEntries.Count)
    $offset = 6 + (16 * $pngEntries.Count)

    foreach ($entry in $pngEntries) {
        $dimension = if ($entry.Size -eq 256) { 0 } else { $entry.Size }
        $writer.Write([byte]$dimension)
        $writer.Write([byte]$dimension)
        $writer.Write([byte]0)
        $writer.Write([byte]0)
        $writer.Write([uint16]1)
        $writer.Write([uint16]32)
        $writer.Write([uint32]$entry.Bytes.Length)
        $writer.Write([uint32]$offset)
        $offset += $entry.Bytes.Length
    }

    foreach ($entry in $pngEntries) {
        $writer.Write($entry.Bytes)
    }
}
finally {
    $writer.Dispose()
    $file.Dispose()
}

Write-Host "Created $outputPath"

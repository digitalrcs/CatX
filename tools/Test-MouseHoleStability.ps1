$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName PresentationFramework,PresentationCore,WindowsBase
$assembly = [Reflection.Assembly]::LoadFrom([IO.Path]::GetFullPath("$PSScriptRoot\..\src\CatX\bin\Release\net8.0-windows\win-x64\CatX.dll"))
$window = [Activator]::CreateInstance($assembly.GetType('CatX.ToyMouseWindow'))
$area = [System.Windows.Rect]::new(-1280,-100,1280,800)
$hole = [System.Windows.Point]::new(-640.37,300.23)
try {
    foreach ($scale in @(1,1.25,1.5,2)) {
        $baseline = $null
        for ($i=0; $i -lt 12; $i++) {
            $center = [System.Windows.Point]::new($hole.X + $(if($i%2){220.17}else{-230.41}) + $i*.31,$hole.Y + $(if($i%3){170.33}else{-180.47}))
            $window.UpdateScene($center,$hole,1,$false,$area)
            if(!$window.IsVisible) { $window.Show() }
            $window.UpdateLayout()
            $bounds = [System.Windows.Rect]::new($window.Left,$window.Top,$window.Width,$window.Height)
            if($bounds -ne $area) { throw 'Toy window bounds changed with mouse movement' }
            $bitmap = [System.Windows.Media.Imaging.RenderTargetBitmap]::new([int]($area.Width*$scale),[int]($area.Height*$scale),96*$scale,96*$scale,[System.Windows.Media.PixelFormats]::Pbgra32)
            $bitmap.Render($window.Content)
            $crop = [System.Windows.Media.Imaging.CroppedBitmap]::new($bitmap,[System.Windows.Int32Rect]::new([int](($hole.X-$area.Left-40)*$scale),[int](($hole.Y-$area.Top-68)*$scale),[int](80*$scale),[int](88*$scale)))
            $pixels = [byte[]]::new($crop.PixelWidth*$crop.PixelHeight*4)
            $crop.CopyPixels($pixels,$crop.PixelWidth*4,0)
            $signature = [Convert]::ToBase64String([Security.Cryptography.SHA256]::HashData($pixels))
            if($null -eq $baseline) { $baseline=$signature }
            elseif($signature -ne $baseline) { throw "Hole pixels changed at scale $scale on frame $i" }
        }
    }
    $newArea=[System.Windows.Rect]::new(0,0,1000,650)
    $window.UpdateScene([System.Windows.Point]::new(500,300),[System.Windows.Point]::new(300,300),1,$true,$newArea)
    if([System.Windows.Rect]::new($window.Left,$window.Top,$window.Width,$window.Height) -ne $newArea) { throw 'Display reconfiguration did not update the surface' }
    'PASS: stationary window and identical hole pixels across 48 mouse positions at 100%, 125%, 150%, and 200% render scaling; monitor change updates bounds.'
} finally { $window.Close() }

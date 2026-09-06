param([string]$Output="$PSScriptRoot\..\tmp\mouse-review-frames")
$ErrorActionPreference='Stop'
Add-Type -AssemblyName PresentationFramework,PresentationCore,WindowsBase
Add-Type -Path ([IO.Path]::GetFullPath("$PSScriptRoot\..\src\CatX\bin\Release\net8.0-windows\win-x64\CatX.dll"))
[IO.Directory]::CreateDirectory([IO.Path]::GetFullPath($Output)) | Out-Null
$app=[CatX.App]::new(); $app.InitializeComponent(); $app.ShutdownMode='OnExplicitShutdown'
$flags=[Reflection.BindingFlags]'Instance,NonPublic'
$settings=[CatX.Models.AppSettings]::new(); $settings.CatStyle='Calico'; $settings.ChaseCursor=$false
$window=[CatX.CatOverlayWindow]::new($settings)
try {
    $window.Show()
    $window.GetType().GetField('_animationTimer',$flags).GetValue($window).Stop()
    $field=$window.GetType().GetField('_behavior',$flags)
    $area=[System.Windows.Rect]::new(0,0,1000,650)
    $behavior=[Activator]::CreateInstance($field.GetValue($window).GetType(),@($area,[int]42,[int]0,[int]1))
    $field.SetValue($window,$behavior)
    $behavior.ChaseCursor=$false
    $toy=$window.GetType().GetField('_mouseBehavior',$flags).GetValue($window)
    $toy.RequestVisit()
    $cats=[Array]::CreateInstance($behavior.GetType(),1)
    $cats[0]=$behavior
    $mouse=$window.GetType().GetField('_mouse',$flags).GetValue($window)
    $brush=[System.Windows.Media.BrushConverter]::new().ConvertFromString('#EEE9DF')
    for($i=0;$i -lt 420;$i++) {
        $toy.Step(.05,$area,$cats,$true)
        $behavior.Step(.05,$area,[System.Windows.Point]::new(500,300),$cats,$toy)
        $window.GetType().GetMethod('RenderPose',$flags).Invoke($window,@([double].05)) | Out-Null
        $window.FindName('DirectionTransform').ScaleX=$behavior.Facing
        $window.UpdateLayout()
        $visual=[System.Windows.Media.DrawingVisual]::new(); $drawing=$visual.RenderOpen()
        $drawing.DrawRectangle($brush,$null,$area)
        $drawing.DrawRectangle([System.Windows.Media.VisualBrush]::new($window.Content),$null,
            [System.Windows.Rect]::new($behavior.Position.X,$behavior.Position.Y,256,224))
        if($toy.Visible) {
            $mouse.UpdateScene($toy.Position,$toy.Hole,$toy.Outward,$toy.AtDoor,$area)
            $mouse.UpdateAdventure($toy)
            $mouse.Content.Measure([System.Windows.Size]::new($mouse.Width,$mouse.Height))
            $mouse.Content.Arrange([System.Windows.Rect]::new(0,0,$mouse.Width,$mouse.Height))
            $mouse.Content.UpdateLayout()
            $drawing.DrawRectangle([System.Windows.Media.VisualBrush]::new($mouse.Content),$null,
                [System.Windows.Rect]::new($mouse.Left,$mouse.Top,$mouse.Width,$mouse.Height))
        }
        $drawing.Close()
        $bitmap=[System.Windows.Media.Imaging.RenderTargetBitmap]::new(1000,650,96,96,[System.Windows.Media.PixelFormats]::Pbgra32)
        $bitmap.Render($visual)
        $encoder=[System.Windows.Media.Imaging.PngBitmapEncoder]::new()
        $encoder.Frames.Add([System.Windows.Media.Imaging.BitmapFrame]::Create($bitmap))
        $stream=[IO.File]::Create((Join-Path $Output ('{0:000}.png' -f $i)))
        try { $encoder.Save($stream) } finally { $stream.Dispose() }
    }
    Write-Output "Exported 21 seconds of actual CatX mouse/cat rendering to $Output"
} finally { $window.Close(); $app.Shutdown() }

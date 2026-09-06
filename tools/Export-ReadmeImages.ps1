param([string]$Output="$PSScriptRoot\..\docs\images")
$ErrorActionPreference='Stop'
Add-Type -AssemblyName PresentationFramework,PresentationCore,WindowsBase
Add-Type -Path "$PSScriptRoot\..\src\CatX\bin\Release\net8.0-windows\win-x64\CatX.dll"
[IO.Directory]::CreateDirectory([IO.Path]::GetFullPath($Output)) | Out-Null
$app=[CatX.App]::new(); $app.InitializeComponent(); $app.ShutdownMode='OnExplicitShutdown'
$flags=[Reflection.BindingFlags]'Instance,NonPublic'
function Save-Drawing($visual,[int]$width,[int]$height,[string]$name) {
    $bitmap=[System.Windows.Media.Imaging.RenderTargetBitmap]::new($width,$height,96,96,[System.Windows.Media.PixelFormats]::Pbgra32)
    $bitmap.Render($visual)
    $encoder=[System.Windows.Media.Imaging.PngBitmapEncoder]::new()
    $encoder.Frames.Add([System.Windows.Media.Imaging.BitmapFrame]::Create($bitmap))
    $stream=[IO.File]::Create((Join-Path $Output $name))
    try { $encoder.Save($stream) } finally { $stream.Dispose() }
}
$main=[CatX.MainWindow]::new()
try {
    # Load demonstration settings without saving or enabling the keyboard guard.
    $main.GetType().GetField('_autoLockTimer',$flags).GetValue($main).Stop()
    $main.GetType().GetField('_loading',$flags).SetValue($main,$true)
    $settings=[CatX.Models.AppSettings]::new()
    $settings.CatStyle='Calico'; $settings.CatCount=3
    $settings.AdditionalCatStyles=[Collections.Generic.List[string]]@('Midnight','Realistic Tabby')
    $main.GetType().GetField('_settings',$flags).SetValue($main,$settings)
    $main.GetType().GetMethod('LoadSettingsIntoControls',$flags).Invoke($main,@()) | Out-Null
    $main.GetType().GetMethod('UpdatePreferenceAvailability',$flags).Invoke($main,@($false)) | Out-Null
    $main.GetType().GetMethod('ScheduleAutoLock',$flags).Invoke($main,@($true)) | Out-Null
    $main.Height=710
    $main.Show(); $main.UpdateLayout()
    $root=$main.Content
    $rect=[System.Windows.Rect]::new(0,0,$root.ActualWidth,$root.ActualHeight)
    $visual=[System.Windows.Media.DrawingVisual]::new(); $drawing=$visual.RenderOpen()
    $drawing.DrawRectangle($main.Background,$null,$rect)
    $mainBrush=[System.Windows.Media.VisualBrush]::new($root)
    $mainBrush.ViewboxUnits='Absolute'; $mainBrush.Viewbox=$rect
    $drawing.DrawRectangle($mainBrush,$null,$rect)
    $drawing.Close()
    Save-Drawing $visual ([int]$root.ActualWidth) ([int]$root.ActualHeight) 'main-window.png'
} finally { $main.Close() }
$styles=@('Marmalade','Midnight','Snowball','Tuxedo','Calico','Realistic Tabby','Realistic Orange','Realistic White','Realistic Grey','Realistic Tuxedo','Realistic Black','Realistic Bicolor')
$visual=[System.Windows.Media.DrawingVisual]::new(); $drawing=$visual.RenderOpen()
$background=[System.Windows.Media.BrushConverter]::new().ConvertFromString('#FFF8E8')
$drawing.DrawRectangle($background,$null,[System.Windows.Rect]::new(0,0,1024,810))
$windows=[Collections.Generic.List[CatX.CatOverlayWindow]]::new()
try {
    for($i=0;$i -lt $styles.Count;$i++) {
        $settings=[CatX.Models.AppSettings]::new(); $settings.CatStyle=$styles[$i]
        $window=[CatX.CatOverlayWindow]::new($settings)
        $windows.Add($window); $window.Show()
        $window.GetType().GetField('_animationTimer',$flags).GetValue($window).Stop()
        $behavior=$window.GetType().GetField('_behavior',$flags).GetValue($window)
        $pose=[Enum]::Parse($behavior.Mood.GetType(),'Walking')
        for($frame=0;$frame -lt 30;$frame++) {
            $behavior.GetType().GetMethod('PreviewPose',$flags).Invoke($behavior,@($pose,[double].05)) | Out-Null
            $window.GetType().GetMethod('RenderPose',$flags).Invoke($window,@([double].05)) | Out-Null
        }
        $window.UpdateLayout()
        $x=($i%4)*256; $y=[Math]::Floor($i/4)*270
        $brush=[System.Windows.Media.VisualBrush]::new($window.Content)
        $brush.ViewboxUnits='Absolute'; $brush.Viewbox=[System.Windows.Rect]::new(0,0,256,224)
        $drawing.DrawRectangle($brush,$null,[System.Windows.Rect]::new($x,$y,256,224))
        $label=[System.Windows.Media.FormattedText]::new($styles[$i],[Globalization.CultureInfo]::InvariantCulture,[System.Windows.FlowDirection]::LeftToRight,[System.Windows.Media.Typeface]::new('Segoe UI'),15,[System.Windows.Media.Brushes]::DarkSlateGray,1)
        $drawing.DrawText($label,[System.Windows.Point]::new($x+(256-$label.Width)/2,$y+238))
    }
    $drawing.Close()
    Save-Drawing $visual 1024 810 'cat-styles.png'
} finally { foreach($window in $windows) { $window.Close() }; $app.Shutdown() }
'Rendered main-window.png and cat-styles.png from current WPF controls and assets.'

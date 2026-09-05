param(
    [string]$Assembly = "$PSScriptRoot\..\src\CatX\bin\Release\net8.0-windows\win-x64\CatX.dll",
    [string]$Output = "$PSScriptRoot\..\tmp\pose-previews",
    [string[]]$Styles = @('Calico','Midnight','Tuxedo','Realistic Tabby'),
    [switch]$MainWindow,
    [switch]$Live
)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName PresentationFramework,PresentationCore,WindowsBase
Add-Type -Path ([IO.Path]::GetFullPath($Assembly))
[IO.Directory]::CreateDirectory([IO.Path]::GetFullPath($Output)) | Out-Null
$app=[CatX.App]::new()
$app.InitializeComponent()
$app.ShutdownMode=[System.Windows.ShutdownMode]::OnExplicitShutdown
$flags=[Reflection.BindingFlags]'Instance,NonPublic'
function Save-Visual($window,$name) {
    $root=[System.Windows.FrameworkElement]$window.Content
    $window.UpdateLayout()
    $bitmap=[System.Windows.Media.Imaging.RenderTargetBitmap]::new([int]$root.ActualWidth,[int]$root.ActualHeight,96,96,[System.Windows.Media.PixelFormats]::Pbgra32)
    if($window -is [CatX.MainWindow]) {
        $visual=[System.Windows.Media.DrawingVisual]::new()
        $drawing=$visual.RenderOpen()
        $rect=[System.Windows.Rect]::new(0,0,$root.ActualWidth,$root.ActualHeight)
        $drawing.DrawRectangle($window.Background,$null,$rect)
        $drawing.DrawRectangle([System.Windows.Media.VisualBrush]::new($root),$null,$rect)
        $drawing.Close()
        $bitmap.Render($visual)
    } else { $bitmap.Render($root) }
    $encoder=[System.Windows.Media.Imaging.PngBitmapEncoder]::new()
    $encoder.Frames.Add([System.Windows.Media.Imaging.BitmapFrame]::Create($bitmap))
    $stream=[IO.File]::Create((Join-Path $Output "$name.png"))
    try { $encoder.Save($stream) } finally { $stream.Dispose() }
}
foreach($style in $Styles) {
    $settings=[CatX.Models.AppSettings]::new()
    $settings.CatStyle=$style
    $window=[CatX.CatOverlayWindow]::new($settings)
    try {
        $window.Show()
        $behavior=$window.GetType().GetField('_behavior',$flags).GetValue($window)
        foreach($moodName in @('Walking','Grooming','Sleeping','Sitting','MouseChase')) {
            $mood=[Enum]::Parse($behavior.Mood.GetType(),$moodName)
            $behavior.GetType().GetProperty('Mood').SetValue($behavior,$mood)
            $behavior.GetType().GetProperty('MoodTime').SetValue($behavior,1.5)
            if($moodName -in @('Walking','MouseChase')) { $behavior.GetType().GetField('_velocity',$flags).SetValue($behavior,[System.Windows.Vector]::new(120,0)) }
            else { $behavior.GetType().GetField('_velocity',$flags).SetValue($behavior,[System.Windows.Vector]::new(0,0)) }
            for($i=0;$i -lt 90;$i++) { $null=$window.GetType().GetMethod('RenderPose',$flags).Invoke($window,@([double](1/60))) }
            Save-Visual $window "$style-$moodName"
        }
        if($Live) {
            # Exercise the real Rendering callback and mouse-window lifecycle, not just pose sampling.
            $settings.ChaseCursor=$false
            $window.ApplyPreferences($settings)
            $behavior.GetType().GetField('_mouseIn',$flags).SetValue($behavior,[double]0.1)
            $window.GetType().GetField('_lastTime',$flags).SetValue($window,[double]0)
            $frame=[System.Windows.Threading.DispatcherFrame]::new()
            $timer=[System.Windows.Threading.DispatcherTimer]::new()
            $timer.Interval=[TimeSpan]::FromSeconds(2)
            $timer.Add_Tick({ $frame.Continue=$false })
            $timer.Start()
            [System.Windows.Threading.Dispatcher]::PushFrame($frame)
            $timer.Stop()
            $mouse=$window.GetType().GetField('_mouse',$flags).GetValue($window)
            if(!$mouse.IsVisible -or $behavior.Speed -le 0) {
                Write-Output "Live diagnostics: mood=$($behavior.Mood) time=$($behavior.MoodTime) speed=$($behavior.Speed) mouse=$($behavior.MouseVisible) area=$($window.GetType().GetField('_area',$flags).GetValue($window))"
                throw "Live movement/toy rendering failed for $style"
            }
            Save-Visual $window "$style-LiveChase"
            Save-Visual $mouse "$style-ToyMouse"
            Write-Output "Live rendering and toy chase verified: $style"
        }
    } finally { $window.Close() }
    if($Live -and $mouse.IsVisible) { throw 'Toy window survived closing the cat.' }
}
if($MainWindow) {
    $main=[CatX.MainWindow]::new()
    try {
        $type=$main.GetType()
        $timer=$type.GetField('_autoLockTimer',$flags).GetValue($main)
        $timer.Stop()
        $settings=$type.GetField('_settings',$flags).GetValue($main)
        $settings.AutoLockAfterSeconds=30
        $settings.CatStyle='Realistic Tabby'
        $type.GetField('_loading',$flags).SetValue($main,$true)
        $combo=$main.FindName('CatStyleCombo')
        $combo.SelectedItem=@($combo.Items | Where-Object Content -EQ 'Realistic Tabby')[0]
        $type.GetField('_loading',$flags).SetValue($main,$false)
        $main.Show()
        Save-Visual $main 'MainWindow'
        $handler=$type.GetMethod('PreviewButton_Click',$flags)
        $null=$handler.Invoke($main,@($null,[System.Windows.RoutedEventArgs]::new()))
        $guard=$type.GetField('_keyboardGuard',$flags).GetValue($main)
        if ($guard.IsActive -or $timer.IsEnabled -or !$type.GetField('_previewing',$flags).GetValue($main)) { throw 'Preview safety check failed.' }
        Save-Visual $main 'MainWindow-Preview'
        $null=$handler.Invoke($main,@($null,[System.Windows.RoutedEventArgs]::new()))
        if (!$timer.IsEnabled -or $guard.IsActive -or $type.GetField('_overlay',$flags).GetValue($main)) { throw 'Preview cleanup / auto-lock restart failed.' }
        $timer.Stop()
        Write-Output 'Preview UI verified: no keyboard hook, auto-lock paused, overlay cleaned up, timer restarted on stop.'
    } finally { $main.Close() }
}
$app.Shutdown()

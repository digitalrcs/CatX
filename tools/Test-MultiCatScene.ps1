$ErrorActionPreference='Stop'
Add-Type -AssemblyName PresentationFramework,PresentationCore,WindowsBase
Add-Type -Path "$PSScriptRoot\..\src\CatX\bin\Release\net8.0-windows\win-x64\CatX.dll"
$app=[CatX.App]::new()
$app.InitializeComponent()
$app.ShutdownMode=[System.Windows.ShutdownMode]::OnExplicitShutdown
$flags=[Reflection.BindingFlags]'Instance,NonPublic'
$window=[CatX.MainWindow]::new()
try {
    $window.GetType().GetField('_reviewOnly',$flags).SetValue($window,$true)
    $window.GetType().GetField('_autoLockTimer',$flags).GetValue($window).Stop()
    $window.Show()
    if($null -ne $window.FindName('PreviewButton')) { throw 'Preview cats button must be absent' }
    $picker=$window.FindName('CatPicker')
    if($null -ne $picker.ItemTemplate) { throw 'Cat dropdown must use plain names' }
    if($picker.SelectionMode -ne 'Extended') { throw 'Multiple selection unavailable' }
    $popup=$window.FindName('CatPickerPopup')
    $popup.IsOpen=$true
    $picker.SelectedItems.Clear()
    foreach($style in @('Calico','Midnight','Realistic Tabby')) { $null=$picker.SelectedItems.Add($style) }
    if(!$popup.IsOpen) { throw 'Dropdown closed during selection' }
    $popup.Child.UpdateLayout()
    $popupBitmap=[System.Windows.Media.Imaging.RenderTargetBitmap]::new([int]$popup.Child.ActualWidth,[int]$popup.Child.ActualHeight,96,96,[System.Windows.Media.PixelFormats]::Pbgra32)
    $popupBitmap.Render($popup.Child)
    $popupEncoder=[System.Windows.Media.Imaging.PngBitmapEncoder]::new()
    $popupEncoder.Frames.Add([System.Windows.Media.Imaging.BitmapFrame]::Create($popupBitmap))
    $popupStream=[IO.File]::Create("$PSScriptRoot\..\tmp\cat-dropdown.png")
    try { $popupEncoder.Save($popupStream) } finally { $popupStream.Dispose() }
    $settings=$window.GetType().GetField('_settings',$flags).GetValue($window)
    if($settings.CatCount -ne 3) { throw 'Picker did not select three cats' }
    $window.GetType().GetMethod('StartReview',$flags).Invoke($window,@())
    if($popup.IsOpen) { throw 'Dropdown must close for preview' }
    $overlays=$window.GetType().GetField('_overlays',$flags).GetValue($window)
    if($overlays.Count -ne 3) { throw 'Preview did not open three overlays' }
    $toyState=$overlays[0].GetType().GetField('_mouseBehavior',$flags).GetValue($overlays[0])
    $toyState.RequestVisit()
    $frame=[System.Windows.Threading.DispatcherFrame]::new()
    $timer=[System.Windows.Threading.DispatcherTimer]::new()
    $timer.Interval=[TimeSpan]::FromSeconds(2)
    $timer.Add_Tick({ $frame.Continue=$false })
    $timer.Start()
    [System.Windows.Threading.Dispatcher]::PushFrame($frame)
    $timer.Stop()
    foreach($overlay in $overlays) {
        if(!$overlay.IsVisible) { throw 'Cat is not visible' }
        if($overlay.GetType().GetField('_companions',$flags).GetValue($overlay).Count -ne 3) { throw 'Companions not connected' }
    }
    $toys=@($app.Windows | Where-Object { $_.GetType().Name -eq 'ToyMouseWindow' })
    if($toys.Count -ne 1 -or !$toys[0].IsVisible) { throw 'Expected exactly one visible mouse for three cats' }
    $shared=$overlays[0].GetType().GetField('_mouseBehavior',$flags).GetValue($overlays[0])
    foreach($overlay in $overlays) {
        if(![Object]::ReferenceEquals($shared,$overlay.GetType().GetField('_mouseBehavior',$flags).GetValue($overlay))) { throw 'Mouse state is not shared' }
    }
    $window.FindName('PlayfulMouseCheck').IsChecked=$false
    $frame.Continue=$true
    $timer.Start()
    [System.Windows.Threading.Dispatcher]::PushFrame($frame)
    $timer.Stop()
    if($toys[0].IsVisible) { throw 'Disabling toy must hide shared mouse' }
    $root=$window.Content
    $window.UpdateLayout()
    $bitmap=[System.Windows.Media.Imaging.RenderTargetBitmap]::new([int]$root.ActualWidth,[int]$root.ActualHeight,96,96,[System.Windows.Media.PixelFormats]::Pbgra32)
    $visual=[System.Windows.Media.DrawingVisual]::new()
    $drawing=$visual.RenderOpen()
    $rect=[System.Windows.Rect]::new(0,0,$root.ActualWidth,$root.ActualHeight)
    $drawing.DrawRectangle($window.Background,$null,$rect)
    $drawing.DrawRectangle([System.Windows.Media.VisualBrush]::new($root),$null,$rect)
    $drawing.Close()
    $bitmap.Render($visual)
    $encoder=[System.Windows.Media.Imaging.PngBitmapEncoder]::new()
    $encoder.Frames.Add([System.Windows.Media.Imaging.BitmapFrame]::Create($bitmap))
    $stream=[IO.File]::Create("$PSScriptRoot\..\tmp\multi-cat-picker.png")
    try { $encoder.Save($stream) } finally { $stream.Dispose() }
    $window.GetType().GetMethod('StopReview',$flags).Invoke($window,@())
    if(@($app.Windows | Where-Object { $_.GetType().Name -eq 'ToyMouseWindow' }).Count -ne 0) { throw 'Mouse window not cleaned up' }
    if($overlays.Count -ne 0) { throw 'Preview cats not cleaned up' }
    $picker.SelectedItems.Clear()
    if($settings.CatStyle -ne 'No cat') { throw 'Empty selection must select no cats' }
    foreach($style in @('Marmalade','Midnight','Snowball','Tuxedo','Calico','Realistic Tabby','Realistic Orange','Realistic White','Realistic Grey')) { $null=$picker.SelectedItems.Add($style) }
    if($settings.CatCount -ne 8 -or $picker.SelectedItems.Count -ne 8) { throw 'Selection limit failed' }
    'PASS: dropdown stays open for multiple choices; three cats share exactly one live mouse; disabling toys hides it; all windows close; empty selection and eight-cat limit work.'
} finally { $window.Close(); $app.Shutdown() }

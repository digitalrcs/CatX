param([string]$Output="$PSScriptRoot\..\tmp\cheese-chase-review")
$ErrorActionPreference='Stop'
Add-Type -AssemblyName PresentationFramework,PresentationCore,WindowsBase
$assembly=[Reflection.Assembly]::LoadFrom([IO.Path]::GetFullPath("$PSScriptRoot\..\src\CatX\bin\Release\net8.0-windows\win-x64\CatX.dll"))
[IO.Directory]::CreateDirectory([IO.Path]::GetFullPath($Output)) | Out-Null
$app=[CatX.App]::new(); $app.InitializeComponent(); $app.ShutdownMode='OnExplicitShutdown'
$flags=[Reflection.BindingFlags]'Instance,NonPublic'
$area=[System.Windows.Rect]::new(0,0,1280,800)
$mouseType=$assembly.GetType('CatX.Services.ToyMouseBehavior')
$catType=$assembly.GetType('CatX.Services.CatBehavior')
$windows=[Collections.Generic.List[CatX.CatOverlayWindow]]::new()
$cats=[Array]::CreateInstance($catType,3)
$toy=[Activator]::CreateInstance($mouseType,@([int]42))
$toy.RequestVisit()
$seen=[Collections.Generic.HashSet[string]]::new()
try {
    for($i=0;$i -lt 3;$i++) {
        $settings=[CatX.Models.AppSettings]::new()
        $settings.CatStyle=@('Calico','Midnight','Realistic Tabby')[$i]
        $settings.ChaseCursor=$false
        $window=[CatX.CatOverlayWindow]::new($settings,$i,3)
        $windows.Add($window)
        $window.Show()
        $window.GetType().GetField('_animationTimer',$flags).GetValue($window).Stop()
        $cats[$i]=[Activator]::CreateInstance($catType,@($area,[int](42+$i),[int]$i,[int]3))
        $cats[$i].ChaseCursor=$false
        $window.GetType().GetField('_behavior',$flags).SetValue($window,$cats[$i])
    }
    $mouse=$windows[0].GetType().GetField('_mouse',$flags).GetValue($windows[0])
    for($frame=0;$frame -lt 600*20;$frame++) {
        $toy.Step(.05,$area,$cats,$true)
        foreach($cat in $cats) { $cat.Step(.05,$area,[System.Windows.Point]::new(600,300),$cats,$toy) }
        $key=if(@($cats | Where-Object { $_.Mood.ToString() -eq 'Sleeping' }).Count -eq 3){'Napping'}elseif($toy.CarryingCheese){'Carrying'}else{$toy.Activity.ToString()}
        if($key -notin @('SeekingCheese','Carrying','Caught','Delivered','Napping') -or $seen.Contains($key)) { continue }
        $null=$seen.Add($key)
        $visual=[System.Windows.Media.DrawingVisual]::new(); $drawing=$visual.RenderOpen()
        $drawing.DrawRectangle([System.Windows.Media.BrushConverter]::new().ConvertFromString('#EEE9DF'),$null,$area)
        for($i=0;$i -lt 3;$i++) {
            $window=$windows[$i]
            $window.GetType().GetMethod('RenderPose',$flags).Invoke($window,@([double].05)) | Out-Null
            $window.FindName('DirectionTransform').ScaleX=$cats[$i].Facing
            $window.UpdateLayout()
            $catBrush=[System.Windows.Media.VisualBrush]::new($window.Content)
            $catBrush.ViewboxUnits='Absolute'
            $catBrush.Viewbox=[System.Windows.Rect]::new(0,0,256,224)
            $drawing.DrawRectangle($catBrush,$null,[System.Windows.Rect]::new($cats[$i].Position.X,$cats[$i].Position.Y,256,224))
        }
        if($toy.Visible) {
            $mouse.UpdateScene($toy.Position,$toy.Hole,$toy.Outward,$toy.AtDoor,$area)
            $mouse.UpdateAdventure($toy)
            $mouse.Content.Measure([System.Windows.Size]::new($area.Width,$area.Height))
            $mouse.Content.Arrange($area)
            $mouse.Content.UpdateLayout()
            $mouseBrush=[System.Windows.Media.VisualBrush]::new($mouse.Content)
            $mouseBrush.ViewboxUnits='Absolute'
            $mouseBrush.Viewbox=$area
            $drawing.DrawRectangle($mouseBrush,$null,$area)
        }
        $drawing.Close()
        $bitmap=[System.Windows.Media.Imaging.RenderTargetBitmap]::new(1280,800,96,96,[System.Windows.Media.PixelFormats]::Pbgra32)
        $bitmap.Render($visual)
        $encoder=[System.Windows.Media.Imaging.PngBitmapEncoder]::new()
        $encoder.Frames.Add([System.Windows.Media.Imaging.BitmapFrame]::Create($bitmap))
        $stream=[IO.File]::Create((Join-Path $Output "$key.png"))
        try { $encoder.Save($stream) } finally { $stream.Dispose() }
        if($seen.Count -eq 5){ break }
    }
    if($seen.Count -ne 5) { throw "Missing render states; observed $($seen -join ', ')" }
    "PASS: rendered seeking, carrying, catch, delivery, and separate naps. Catches=$($toy.Catches), deliveries=$($toy.Deliveries)."
} finally { foreach($window in $windows) { $window.Close() }; $app.Shutdown() }

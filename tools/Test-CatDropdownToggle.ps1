$ErrorActionPreference='Stop'
Add-Type -AssemblyName PresentationFramework,PresentationCore,WindowsBase
Add-Type -Path "$PSScriptRoot\..\src\CatX\bin\Release\net8.0-windows\win-x64\CatX.dll"
$app=[CatX.App]::new()
$app.InitializeComponent()
$app.ShutdownMode='OnExplicitShutdown'
$window=[CatX.MainWindow]::new()
$flags=[Reflection.BindingFlags]'Instance,NonPublic'
function Pump {
    $frame=[System.Windows.Threading.DispatcherFrame]::new()
    $timer=[System.Windows.Threading.DispatcherTimer]::new()
    $timer.Interval=[TimeSpan]::FromMilliseconds(50)
    $timer.Add_Tick({ $frame.Continue=$false })
    $timer.Start()
    [System.Windows.Threading.Dispatcher]::PushFrame($frame)
    $timer.Stop()
}
function Press($element) {
    $event=[System.Windows.Input.MouseButtonEventArgs]::new([System.Windows.Input.Mouse]::PrimaryDevice,[Environment]::TickCount,[System.Windows.Input.MouseButton]::Left)
    $event.RoutedEvent=[System.Windows.UIElement]::PreviewMouseDownEvent
    $element.RaiseEvent($event)
}
function Click-Arrow {
    # Exercise the routed mouse-down before the Button.Click callback, including
    # the window's outside-click handler. The icon is a child of the button.
    Press $button.Content.Children[0]
    $button.RaiseEvent([System.Windows.RoutedEventArgs]::new([System.Windows.Controls.Primitives.ButtonBase]::ClickEvent))
    Pump
}
try {
    $window.GetType().GetField('_reviewOnly',$flags).SetValue($window,$true)
    $window.GetType().GetField('_autoLockTimer',$flags).GetValue($window).Stop()
    $window.Show()
    $window.UpdateLayout()
    Pump
    $button=$window.FindName('CatPickerButton')
    $popup=$window.FindName('CatPickerPopup')
    $picker=$window.FindName('CatPicker')
    $picker.SelectedItems.Clear()
    $null=$picker.SelectedItems.Add('Calico')
    $null=$picker.SelectedItems.Add('Midnight')
    for($i=0;$i -lt 5;$i++) {
        Click-Arrow
        if(!$popup.IsOpen) { throw "Arrow did not open on cycle $i" }
        Press $picker
        if(!$popup.IsOpen) { throw 'Clicking inside list dismissed it' }
        Click-Arrow
        if($popup.IsOpen) { throw "Arrow reopened list on cycle $i" }
    }
    Click-Arrow
    Press $window.FindName('StatusTitle')
    Pump
    if($popup.IsOpen) { throw 'Outside press did not dismiss dropdown' }
    Click-Arrow
    $event=[System.Windows.Input.KeyEventArgs]::new([System.Windows.Input.Keyboard]::PrimaryDevice,[System.Windows.PresentationSource]::FromVisual($window),[Environment]::TickCount,[System.Windows.Input.Key]::Escape)
    $event.RoutedEvent=[System.Windows.UIElement]::PreviewKeyDownEvent
    $picker.RaiseEvent($event)
    Pump
    if($popup.IsOpen) { throw 'Escape did not dismiss dropdown' }
    Click-Arrow
    $window.GetType().GetMethod('MainWindow_Deactivated',$flags).Invoke($window,@($window,[EventArgs]::Empty))
    if($popup.IsOpen) { throw 'Switching apps did not dismiss dropdown' }
    if($picker.SelectedItems.Count -ne 2 -or !$picker.SelectedItems.Contains('Calico') -or !$picker.SelectedItems.Contains('Midnight')) { throw 'Dropdown dismissal changed selections' }
    'PASS: five routed arrow-click cycles stay closed; list clicks stay open; outside click, Escape, and deactivation close; selections persist.'
} finally { $window.Close(); $app.Shutdown() }

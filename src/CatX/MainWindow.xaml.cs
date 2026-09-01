using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using CatX.Models;
using CatX.Services;
using Forms = System.Windows.Forms;

namespace CatX;

public partial class MainWindow : Window
{
    private readonly SettingsService _settingsService = new();
    private readonly KeyboardGuard _keyboardGuard = new();
    private readonly DispatcherTimer _autoLockTimer = new() { Interval = TimeSpan.FromSeconds(1) };
    private AppSettings _settings;
    private CatOverlayWindow? _overlay;
    private Forms.NotifyIcon? _trayIcon;
    private Forms.ToolStripMenuItem? _trayDisableItem;
    private long? _autoLockMonitoringStartedAt;
    private bool _trayTipShown;
    private bool _loading = true;

    public MainWindow()
    {
        InitializeComponent();
        _settings = _settingsService.Load();
        LoadSettingsIntoControls();
        InitializeTrayIcon();
        _keyboardGuard.Unlocked += KeyboardGuard_Unlocked;
        _autoLockTimer.Tick += AutoLockTimer_Tick;
        _loading = false;
        UpdatePreferenceAvailability(false);
        ScheduleAutoLock(true);
    }

    private void LoadSettingsIntoControls()
    {
        SelectByContent(CatStyleCombo, _settings.CatStyle);
        SelectByTag(UnlockCombo, _settings.UnlockChord.ToString());
        SelectByTag(RoamCombo, _settings.RoamEverySeconds.ToString());
        SelectByTag(AutoLockCombo, _settings.AutoLockAfterSeconds.ToString());
    }

    private static void SelectByContent(System.Windows.Controls.ComboBox comboBox, string value) => comboBox.SelectedItem = comboBox.Items.OfType<ComboBoxItem>().FirstOrDefault(item => Equals(item.Content, value)) ?? comboBox.Items[0];
    private static void SelectByTag(System.Windows.Controls.ComboBox comboBox, string value) => comboBox.SelectedItem = comboBox.Items.OfType<ComboBoxItem>().FirstOrDefault(item => Equals(item.Tag, value)) ?? comboBox.Items[0];

    private void InitializeTrayIcon()
    {
        var openItem = new Forms.ToolStripMenuItem("Open CatX");
        _trayDisableItem = new Forms.ToolStripMenuItem("Disable keyboard guard") { Enabled = false };
        var exitItem = new Forms.ToolStripMenuItem("Exit CatX");
        var menu = new Forms.ContextMenuStrip();
        menu.Items.AddRange([openItem, _trayDisableItem, new Forms.ToolStripSeparator(), exitItem]);

        _trayIcon = new Forms.NotifyIcon
        {
            ContextMenuStrip = menu,
            Icon = LoadTrayIcon(),
            Text = "CatX Keyboard Guard",
            Visible = false
        };

        openItem.Click += (_, _) => Dispatcher.BeginInvoke(ShowFromTray);
        _trayDisableItem.Click += (_, _) => Dispatcher.BeginInvoke(DisableGuardFromTray);
        exitItem.Click += (_, _) => Dispatcher.BeginInvoke(Close);
        _trayIcon.DoubleClick += (_, _) => Dispatcher.BeginInvoke(ShowFromTray);
    }

    private static Icon LoadTrayIcon()
    {
        var resource = System.Windows.Application.GetResourceStream(
            new Uri("pack://application:,,,/Assets/CatX.ico", UriKind.Absolute));
        if (resource is null)
            return (Icon)SystemIcons.Application.Clone();

        using var icon = new Icon(resource.Stream);
        return (Icon)icon.Clone();
    }

    private void PreferenceChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading || CatStyleCombo.SelectedItem is not ComboBoxItem cat || UnlockCombo.SelectedItem is not ComboBoxItem unlock || RoamCombo.SelectedItem is not ComboBoxItem roam || AutoLockCombo.SelectedItem is not ComboBoxItem autoLock) return;
        _settings.CatStyle = cat.Content?.ToString() ?? "Marmalade";
        _settings.UnlockChord = Enum.Parse<UnlockChord>(unlock.Tag?.ToString() ?? nameof(UnlockChord.CtrlAltK));
        _settings.RoamEverySeconds = int.Parse(roam.Tag?.ToString() ?? "8");
        _settings.AutoLockAfterSeconds = int.Parse(autoLock.Tag?.ToString() ?? "0");
        _settingsService.Save(_settings);
        _overlay?.ApplyPreferences(_settings);
        UpdatePreferenceAvailability(_keyboardGuard.IsActive);
        ScheduleAutoLock(true);
    }

    private void GuardButton_Click(object sender, RoutedEventArgs e)
    {
        if (_keyboardGuard.IsActive) DisableGuard("Guard disabled with the mouse."); else EnableGuard();
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
            MinimizeToTray();
    }

    private void MinimizeToTray()
    {
        if (_trayIcon is null) return;

        _trayIcon.Visible = true;
        ShowInTaskbar = false;
        Hide();

        if (_trayTipShown) return;
        _trayTipShown = true;
        _trayIcon.ShowBalloonTip(
            3000,
            "CatX is still running",
            "Right-click the cat icon to open CatX or disable the keyboard guard.",
            Forms.ToolTipIcon.Info);
    }

    private void ShowFromTray()
    {
        ShowInTaskbar = true;
        Show();
        WindowState = WindowState.Normal;
        Activate();
        if (_trayIcon is not null) _trayIcon.Visible = false;
    }

    private void DisableGuardFromTray()
    {
        if (!_keyboardGuard.IsActive) return;
        DisableGuard("Guard disabled from the system tray.");
        _trayIcon?.ShowBalloonTip(
            2500,
            "Keyboard guard disabled",
            "Your keyboard is active again.",
            Forms.ToolTipIcon.Info);
    }

    private void EnableGuard()
    {
        try
        {
            _autoLockTimer.Stop();
            _autoLockMonitoringStartedAt = null;
            _keyboardGuard.Enable(_settings.UnlockChord);
            if (_settings.CatStyle != "No cat")
            {
                _overlay = new CatOverlayWindow(_settings);
                _overlay.Show();
            }
            SetLockedVisualState(true, $"Hold {_settings.UnlockChord.ToDisplayName()} to restore typing.");
        }
        catch (Exception ex)
        {
            _keyboardGuard.Disable(); _overlay?.Close(); _overlay = null;
            System.Windows.MessageBox.Show(this, ex.Message, "CatX could not start", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void KeyboardGuard_Unlocked(object? sender, EventArgs e) => Dispatcher.Invoke(() => DisableGuard($"Unlocked with {_settings.UnlockChord.ToDisplayName()}."));

    private void DisableGuard(string detail)
    {
        _keyboardGuard.Disable(); _overlay?.Close(); _overlay = null;
        SetLockedVisualState(false, detail); Activate(); ScheduleAutoLock();
    }

    private void SetLockedVisualState(bool locked, string detail)
    {
        GuardButton.Content = locked ? "Disable guard with mouse" : "Enable keyboard guard";
        GuardButton.Background = Brush(locked ? "#3E8878" : "#F46F55");
        StatusCard.Background = Brush(locked ? "#DDF1E9" : "#FFF0CC");
        StatusDot.Fill = Brush(locked ? "#3E8878" : "#D89B24");
        StatusTitle.Text = locked ? "Keyboard guard is active" : "Guard is ready";
        StatusDetail.Text = detail;
        if (_trayDisableItem is not null) _trayDisableItem.Enabled = locked;
        UpdatePreferenceAvailability(locked);
    }

    private void UpdatePreferenceAvailability(bool locked)
    {
        CatStyleCombo.IsEnabled = UnlockCombo.IsEnabled = AutoLockCombo.IsEnabled = !locked;
        RoamCombo.IsEnabled = !locked && _settings.CatStyle != "No cat";
    }

    private void ScheduleAutoLock(bool updateIdleStatus = false)
    {
        _autoLockTimer.Stop();
        _autoLockMonitoringStartedAt = null;
        if (_keyboardGuard.IsActive) return;
        if (_settings.AutoLockAfterSeconds <= 0)
        {
            if (updateIdleStatus)
            {
                StatusTitle.Text = "Guard is ready";
                StatusDetail.Text = "Automatic locking is off. Your keyboard is active.";
            }
            return;
        }

        _autoLockMonitoringStartedAt = Environment.TickCount64;
        _autoLockTimer.Start();
        UpdateAutoLockStatus(TimeSpan.Zero);
    }

    private void AutoLockTimer_Tick(object? sender, EventArgs e)
    {
        if (_autoLockMonitoringStartedAt is null || _keyboardGuard.IsActive)
        {
            _autoLockTimer.Stop();
            return;
        }

        if (!TryGetEffectiveIdleDuration(out var idleDuration))
        {
            _autoLockTimer.Stop();
            _autoLockMonitoringStartedAt = null;
            StatusTitle.Text = "Auto-lock unavailable";
            StatusDetail.Text = "Windows activity monitoring is unavailable. Enable the keyboard guard manually.";
            return;
        }

        if (idleDuration >= TimeSpan.FromSeconds(_settings.AutoLockAfterSeconds))
        {
            _autoLockTimer.Stop();
            _autoLockMonitoringStartedAt = null;
            EnableGuard();
            return;
        }

        UpdateAutoLockStatus(idleDuration);
    }

    private bool TryGetEffectiveIdleDuration(out TimeSpan idleDuration)
    {
        if (_autoLockMonitoringStartedAt is not long monitoringStartedAt ||
            !UserActivityMonitor.TryGetIdleDuration(out var desktopIdleDuration))
        {
            idleDuration = TimeSpan.Zero;
            return false;
        }

        var monitoringElapsed = Math.Max(0, Environment.TickCount64 - monitoringStartedAt);
        var effectiveIdle = UserActivityMonitor.EffectiveIdleMilliseconds(
            (long)desktopIdleDuration.TotalMilliseconds,
            monitoringElapsed);
        idleDuration = TimeSpan.FromMilliseconds(effectiveIdle);
        return true;
    }

    private void UpdateAutoLockStatus(TimeSpan idleDuration)
    {
        var seconds = Math.Max(1, (int)Math.Ceiling(_settings.AutoLockAfterSeconds - idleDuration.TotalSeconds));
        var remaining = seconds < 60 ? $"{seconds} second{(seconds == 1 ? "" : "s")}" : $"{Math.Ceiling(seconds / 60d):0} minute(s)";
        StatusTitle.Text = "Waiting for inactivity";
        StatusDetail.Text = $"The keyboard guard will enable after {remaining} with no keyboard or mouse activity.";
    }

    private static SolidColorBrush Brush(string color) => new((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color));

    protected override void OnClosing(CancelEventArgs e)
    {
        _autoLockTimer.Stop();
        _keyboardGuard.Dispose();
        _overlay?.Close();
        if (_trayIcon is not null)
        {
            var trayImage = _trayIcon.Icon;
            _trayIcon.Visible = false;
            _trayIcon.ContextMenuStrip?.Dispose();
            _trayIcon.Dispose();
            trayImage?.Dispose();
        }
        base.OnClosing(e);
    }
}

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using CatX.Models;
using CatX.Services;

namespace CatX;

public partial class MainWindow : Window
{
    private readonly SettingsService _settingsService = new();
    private readonly KeyboardGuard _keyboardGuard = new();
    private readonly DispatcherTimer _autoLockTimer = new() { Interval = TimeSpan.FromSeconds(1) };
    private AppSettings _settings;
    private CatOverlayWindow? _overlay;
    private DateTime? _autoLockAt;
    private bool _loading = true;

    public MainWindow()
    {
        InitializeComponent();
        _settings = _settingsService.Load();
        LoadSettingsIntoControls();
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

    private static void SelectByContent(ComboBox comboBox, string value) => comboBox.SelectedItem = comboBox.Items.OfType<ComboBoxItem>().FirstOrDefault(item => Equals(item.Content, value)) ?? comboBox.Items[0];
    private static void SelectByTag(ComboBox comboBox, string value) => comboBox.SelectedItem = comboBox.Items.OfType<ComboBoxItem>().FirstOrDefault(item => Equals(item.Tag, value)) ?? comboBox.Items[0];

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

    private void EnableGuard()
    {
        try
        {
            _autoLockTimer.Stop();
            _autoLockAt = null;
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
            MessageBox.Show(this, ex.Message, "CatX could not start", MessageBoxButton.OK, MessageBoxImage.Error);
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
        _autoLockAt = null;
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

        _autoLockAt = DateTime.Now.AddSeconds(_settings.AutoLockAfterSeconds);
        _autoLockTimer.Start();
        UpdateAutoLockStatus();
    }

    private void AutoLockTimer_Tick(object? sender, EventArgs e)
    {
        if (_autoLockAt is null || _keyboardGuard.IsActive)
        {
            _autoLockTimer.Stop();
            return;
        }

        if (_autoLockAt <= DateTime.Now)
        {
            _autoLockTimer.Stop();
            _autoLockAt = null;
            EnableGuard();
            return;
        }

        UpdateAutoLockStatus();
    }

    private void UpdateAutoLockStatus()
    {
        if (_autoLockAt is null) return;
        var seconds = Math.Max(1, (int)Math.Ceiling((_autoLockAt.Value - DateTime.Now).TotalSeconds));
        var remaining = seconds < 60 ? $"{seconds} second{(seconds == 1 ? "" : "s")}" : $"{Math.Ceiling(seconds / 60d):0} minute(s)";
        StatusTitle.Text = "Auto-lock scheduled";
        StatusDetail.Text = $"The keyboard guard will enable in {remaining}.";
    }

    private static SolidColorBrush Brush(string color) => new((Color)ColorConverter.ConvertFromString(color));

    protected override void OnClosing(CancelEventArgs e)
    {
        _autoLockTimer.Stop(); _keyboardGuard.Dispose(); _overlay?.Close(); base.OnClosing(e);
    }
}

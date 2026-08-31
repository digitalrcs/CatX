using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CatX.Models;
using CatX.Services;

namespace CatX;

public partial class MainWindow : Window
{
    private readonly SettingsService _settingsService = new();
    private readonly KeyboardGuard _keyboardGuard = new();
    private AppSettings _settings;
    private CatOverlayWindow? _overlay;
    private bool _loading = true;

    public MainWindow()
    {
        InitializeComponent();
        _settings = _settingsService.Load();
        LoadSettingsIntoControls();
        _keyboardGuard.Unlocked += KeyboardGuard_Unlocked;
        _loading = false;
    }

    private void LoadSettingsIntoControls()
    {
        SelectByContent(CatStyleCombo, _settings.CatStyle);
        SelectByTag(UnlockCombo, _settings.UnlockChord.ToString());
        SelectByTag(RoamCombo, _settings.RoamEverySeconds.ToString());
    }

    private static void SelectByContent(ComboBox comboBox, string value) => comboBox.SelectedItem = comboBox.Items.OfType<ComboBoxItem>().FirstOrDefault(item => Equals(item.Content, value)) ?? comboBox.Items[0];
    private static void SelectByTag(ComboBox comboBox, string value) => comboBox.SelectedItem = comboBox.Items.OfType<ComboBoxItem>().FirstOrDefault(item => Equals(item.Tag, value)) ?? comboBox.Items[0];

    private void PreferenceChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading || CatStyleCombo.SelectedItem is not ComboBoxItem cat || UnlockCombo.SelectedItem is not ComboBoxItem unlock || RoamCombo.SelectedItem is not ComboBoxItem roam) return;
        _settings.CatStyle = cat.Content?.ToString() ?? "Marmalade";
        _settings.UnlockChord = Enum.Parse<UnlockChord>(unlock.Tag?.ToString() ?? nameof(UnlockChord.CtrlAltK));
        _settings.RoamEverySeconds = int.Parse(roam.Tag?.ToString() ?? "8");
        _settingsService.Save(_settings);
        _overlay?.ApplyPreferences(_settings);
    }

    private void GuardButton_Click(object sender, RoutedEventArgs e)
    {
        if (_keyboardGuard.IsActive) DisableGuard("Guard disabled with the mouse."); else EnableGuard();
    }

    private void EnableGuard()
    {
        try
        {
            _keyboardGuard.Enable(_settings.UnlockChord);
            _overlay = new CatOverlayWindow(_settings);
            _overlay.Show();
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
        SetLockedVisualState(false, detail); Activate();
    }

    private void SetLockedVisualState(bool locked, string detail)
    {
        GuardButton.Content = locked ? "Disable guard with mouse" : "Enable keyboard guard";
        GuardButton.Background = Brush(locked ? "#3E8878" : "#F46F55");
        StatusCard.Background = Brush(locked ? "#DDF1E9" : "#FFF0CC");
        StatusDot.Fill = Brush(locked ? "#3E8878" : "#D89B24");
        StatusTitle.Text = locked ? "Keyboard guard is active" : "Guard is ready";
        StatusDetail.Text = detail;
        CatStyleCombo.IsEnabled = UnlockCombo.IsEnabled = RoamCombo.IsEnabled = !locked;
    }

    private static SolidColorBrush Brush(string color) => new((Color)ColorConverter.ConvertFromString(color));

    protected override void OnClosing(CancelEventArgs e)
    {
        _keyboardGuard.Dispose(); _overlay?.Close(); base.OnClosing(e);
    }
}

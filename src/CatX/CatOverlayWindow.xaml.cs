using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using CatX.Models;

namespace CatX;

public partial class CatOverlayWindow : Window
{
    private const int GwlExStyle = -20;
    private const int WsExTransparent = 0x20;
    private const int WsExNoActivate = 0x08000000;
    private const int WsExToolWindow = 0x80;
    private readonly Random _random = new();
    private readonly DispatcherTimer _roamTimer = new();
    private readonly DispatcherTimer _stepTimer = new() { Interval = TimeSpan.FromMilliseconds(240) };
    private bool _step;

    public CatOverlayWindow(AppSettings settings)
    {
        InitializeComponent();
        ApplyPreferences(settings);
        Loaded += (_, _) =>
        {
            MoveImmediately();
            _roamTimer.Start();
            _stepTimer.Start();
        };
        _roamTimer.Tick += (_, _) => Roam();
        _stepTimer.Tick += (_, _) => AnimateStep();
        Closed += (_, _) => { _roamTimer.Stop(); _stepTimer.Stop(); };
    }

    public void ApplyPreferences(AppSettings settings)
    {
        _roamTimer.Interval = TimeSpan.FromSeconds(settings.RoamEverySeconds);
        var palette = PaletteFor(settings.CatStyle);

        Body.Fill = Head.Fill = FrontLeg.Fill = BackLeg.Fill = LeftEar.Fill = RightEar.Fill = Brush(palette.Base);
        Tail.Stroke = Brush(palette.Base);
        Chest.Fill = Brush(palette.Chest);
        HeadPatch.Fill = Brush(palette.HeadPatch);
        BodyPatch.Fill = Brush(palette.BodyPatch);
        HeadPatch.Opacity = palette.PatchOpacity;
        BodyPatch.Opacity = palette.PatchOpacity;
        HeadPatch.Visibility = palette.ShowHeadPatch ? Visibility.Visible : Visibility.Collapsed;
        BodyPatch.Visibility = palette.ShowBodyPatch ? Visibility.Visible : Visibility.Collapsed;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        var handle = new WindowInteropHelper(this).Handle;
        var style = GetWindowLongPtr(handle, GwlExStyle).ToInt64();
        SetWindowLongPtr(handle, GwlExStyle, new IntPtr(style | WsExTransparent | WsExNoActivate | WsExToolWindow));
    }

    private void MoveImmediately()
    {
        Left = SystemParameters.VirtualScreenLeft + 20;
        Top = SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight - Height - 40;
    }

    private void Roam()
    {
        var minX = SystemParameters.VirtualScreenLeft + 10;
        var minY = SystemParameters.VirtualScreenTop + 10;
        var maxX = SystemParameters.VirtualScreenLeft + Math.Max(10, SystemParameters.VirtualScreenWidth - Width - 10);
        var maxY = SystemParameters.VirtualScreenTop + Math.Max(10, SystemParameters.VirtualScreenHeight - Height - 20);
        var targetX = minX + _random.NextDouble() * Math.Max(1, maxX - minX);
        var targetY = minY + _random.NextDouble() * Math.Max(1, maxY - minY);
        // Every built-in cat is drawn facing left. Mirror it only when travelling right.
        DirectionTransform.ScaleX = ScaleForTravel(Left, targetX);

        var duration = TimeSpan.FromSeconds(Math.Clamp(Math.Abs(targetX - Left) / 260, 1.4, 4.5));
        var horizontalMove = new DoubleAnimation(targetX, duration) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut } };
        BeginAnimation(LeftProperty, horizontalMove);
        BeginAnimation(TopProperty, new DoubleAnimation(targetY, duration) { EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut } });
    }

    private void AnimateStep()
    {
        _step = !_step;
        var duration = TimeSpan.FromMilliseconds(240);
        var easing = new SineEase { EasingMode = EasingMode.EaseInOut };
        CatBody.BeginAnimation(Canvas.TopProperty, new DoubleAnimation(_step ? -4 : 2, duration) { EasingFunction = easing });
        FrontLegRotate.BeginAnimation(System.Windows.Media.RotateTransform.AngleProperty, new DoubleAnimation(_step ? -15 : 15, duration) { EasingFunction = easing });
        BackLegRotate.BeginAnimation(System.Windows.Media.RotateTransform.AngleProperty, new DoubleAnimation(_step ? 15 : -15, duration) { EasingFunction = easing });
    }

    internal static double ScaleForTravel(double currentX, double targetX) => targetX >= currentX ? -1 : 1;

    internal static CatPalette PaletteFor(string catStyle) => catStyle switch
    {
        // Midnight is a solid coat. The old contrasting patches looked like misplaced spots.
        "Midnight" => new CatPalette("#2F3542", "#525B6C", "#2F3542", "#2F3542", false, false),
        "Snowball" => new CatPalette("#F4F1EA", "#D9D5CD", "#E5E0D7", "#C7C2B8", true, true, 0.55),
        // Tuxedo's white chest supplies its clean two-tone pattern without a stray facial or flank spot.
        "Tuxedo" => new CatPalette("#30343B", "#F7F4EC", "#30343B", "#30343B", false, false),
        "Calico" => new CatPalette("#F4E5C8", "#FFF4DD", "#2E333A", "#E8874B", true, true),
        _ => new CatPalette("#F49A4A", "#FFC982", "#D8773B", "#C65B36", true, true, 0.35)
    };

    private static SolidColorBrush Brush(string color) => new((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color));
    internal sealed record CatPalette(
        string Base,
        string Chest,
        string HeadPatch,
        string BodyPatch,
        bool ShowHeadPatch,
        bool ShowBodyPatch,
        double PatchOpacity = 1);
    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern IntPtr GetWindowLongPtr(IntPtr window, int index);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    private static extern IntPtr SetWindowLongPtr(IntPtr window, int index, IntPtr value);
}

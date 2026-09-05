using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Windows.Media;
using CatX.Models;
using CatX.Services;
using System.Diagnostics;
using System.Windows.Threading;
using Point = System.Windows.Point;

namespace CatX;

public partial class CatOverlayWindow : Window
{
    private const int GwlExStyle = -20;
    private const int WsExTransparent = 0x20;
    private const int WsExNoActivate = 0x08000000;
    private const int WsExToolWindow = 0x80;
    private readonly Stopwatch _clock = new();
    // Transparent windows can stop receiving composition callbacks when visually still.
    // Keep the behavior clock alive independently, with elapsed-time motion at ~60 Hz.
    private readonly DispatcherTimer _animationTimer = new(DispatcherPriority.Render) { Interval = TimeSpan.FromMilliseconds(16) };
    private double _lastTime;
    private CatBehavior? _behavior;
    private ToyMouseWindow? _mouse;
    private RealisticCatFrames? _frames;
    private AppSettings _settings = new();
    private string _loadedStyle = "";
    private string _lastClip = "";
    private double _clipTime;
    private double _transitionTime;
    private double _stride;
    private double _restScale = 1;
    private CatMood _previousMood;
    private bool _closed;
    private string _screenName = "";
    private Rect _area;
    private double _displayCheckIn;

    public CatOverlayWindow(AppSettings settings)
    {
        InitializeComponent();
        ApplyPreferences(settings);
        Loaded += (_, _) =>
        {
            var cursor = System.Windows.Forms.Cursor.Position;
            var screen = System.Windows.Forms.Screen.FromPoint(cursor);
            _screenName = screen.DeviceName;
            UpdateArea(screen);
            _behavior = new CatBehavior(_area);
            UpdateBehaviorPreferences();
            Left = _behavior.Position.X;
            Top = _behavior.Position.Y;
            _mouse = new ToyMouseWindow();
            _clock.Start();
            _animationTimer.Tick += RenderTick;
            _animationTimer.Start();
        };
        Closed += (_, _) =>
        {
            _closed = true;
            _animationTimer.Stop();
            _animationTimer.Tick -= RenderTick;
            _clock.Stop();
            _mouse?.Close();
            _frames = null;
            RealisticFrame.Source = RealisticNextFrame.Source = TransitionFrame.Source = null;
        };
    }

    public void ApplyPreferences(AppSettings settings)
    {
        _settings = settings;
        UpdateBehaviorPreferences();
        var realistic = settings.CatStyle.StartsWith("Realistic ", StringComparison.Ordinal);
        if (_loadedStyle != settings.CatStyle)
        {
            _frames = realistic ? new RealisticCatFrames(settings.CatStyle[10..]) : null;
            _loadedStyle = settings.CatStyle;
            _lastClip = "";
            _clipTime = 0;
            RealisticFrame.Source = RealisticNextFrame.Source = TransitionFrame.Source = null;
        }
        CatBody.Visibility = realistic ? Visibility.Collapsed : Visibility.Visible;
        RealisticLayer.Visibility = realistic ? Visibility.Visible : Visibility.Collapsed;
        Canvas.SetTop(GroundShadow, realistic ? 188 : 200);
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

    private void UpdateBehaviorPreferences()
    {
        if (_behavior is null) return;
        _behavior.ChaseCursor = _settings.ChaseCursor;
        _behavior.PlayfulMouse = _settings.PlayfulMouse;
        _behavior.RoamSeconds = _settings.RoamEverySeconds;
    }

    private void UpdateArea(System.Windows.Forms.Screen screen)
    {
        var transform = PresentationSource.FromVisual(this)?.CompositionTarget?.TransformFromDevice ?? Matrix.Identity;
        var bounds = screen.WorkingArea;
        _area = new Rect(transform.Transform(new Point(bounds.Left, bounds.Top)), transform.Transform(new Point(bounds.Right, bounds.Bottom)));
    }

    private void RenderTick(object? sender, EventArgs e)
    {
        if (_closed || _behavior is null) return;
        var now = _clock.Elapsed.TotalSeconds;
        var dt = Math.Clamp(now - _lastTime, 0, .05);
        _lastTime = now;
        if (dt == 0) return;
        _displayCheckIn -= dt;
        if (_displayCheckIn <= 0)
        {
            var screen = System.Windows.Forms.Screen.AllScreens.FirstOrDefault(s => s.DeviceName == _screenName)
                ?? System.Windows.Forms.Screen.PrimaryScreen!;
            UpdateArea(screen);
            _displayCheckIn = 2;
        }
        var pixel = System.Windows.Forms.Cursor.Position;
        var transform = PresentationSource.FromVisual(this)?.CompositionTarget?.TransformFromDevice ?? Matrix.Identity;
        _behavior.Step(dt, _area, transform.Transform(new Point(pixel.X, pixel.Y)));
        Left = _behavior.Position.X;
        Top = _behavior.Position.Y;
        DirectionTransform.ScaleX = _behavior.Facing;
        RenderPose(dt);
        if (_mouse is not null)
        {
            if (_behavior.MouseVisible)
            {
                _mouse.MoveTo(_behavior.MousePosition);
                if (!_mouse.IsVisible) _mouse.Show();
            }
            else if (_mouse.IsVisible) _mouse.Hide();
        }
    }

    private void RenderPose(double dt)
    {
        var cat = _behavior!;
        var excited = cat.Mood is CatMood.CursorChase or CatMood.MouseChase;
        var moving = cat.Speed > 8;
        MoodIndicator.Text = cat.Mood == CatMood.Sleeping ? "z z" : excited ? "!" : "";
        // Keep the status glyph readable when the complete artwork is mirrored.
        MoodIndicator.RenderTransform = new ScaleTransform(cat.Facing, 1, 10, 10);
        if (_frames is not null)
        {
            var clip = cat.Mood switch
            {
                CatMood.Sleeping => "sleep",
                CatMood.LyingDown or CatMood.Waking => "lie",
                CatMood.Grooming => "groom",
                CatMood.Sitting => "sit",
                _ => moving ? excited ? "run" : "walk" : "idle"
            };
            if (_lastClip != clip || (cat.Mood == CatMood.Waking && _previousMood != cat.Mood))
            {
                TransitionFrame.Source = RealisticFrame.Source;
                _transitionTime = .16;
                _clipTime = 0;
                _lastClip = clip;
            }
            else _clipTime += dt * (moving && clip is "walk" or "run" ? Math.Clamp(cat.Speed / (excited ? 285 : 120), .35, 1.4) : 1);
            var sample = _frames.Sample(clip, _clipTime, cat.Mood == CatMood.Waking);
            RealisticFrame.Source = sample.First;
            RealisticNextFrame.Source = sample.Next;
            RealisticNextFrame.Opacity = sample.Blend;
            _transitionTime = Math.Max(0, _transitionTime - dt);
            TransitionFrame.Opacity = _transitionTime / .16;
            _previousMood = cat.Mood;
            return;
        }

        _stride += dt * Math.Clamp(cat.Speed / 120, 0, 2.5) * 10;
        var stride = Math.Sin(_stride);
        var resting = cat.Mood is CatMood.Sleeping or CatMood.LyingDown;
        var sitting = cat.Mood is CatMood.Sitting or CatMood.Grooming;
        var scale = resting ? .50 : sitting ? .82 : 1;
        _restScale += (scale - _restScale) * (1 - Math.Exp(-4 * dt));
        RestTransform.ScaleY = _restScale + (cat.Mood == CatMood.Sleeping ? Math.Sin(cat.MoodTime * 1.8) * .008 : 0);
        OpenEyes.Visibility = resting ? Visibility.Collapsed : Visibility.Visible;
        ClosedEyes.Visibility = resting ? Visibility.Visible : Visibility.Collapsed;
        Canvas.SetTop(CatBody, 44 + (moving ? Math.Abs(stride) * -3 : 0));
        FrontLegRotate.Angle = moving ? stride * 19 : cat.Mood == CatMood.Grooming ? 110 + Math.Sin(cat.MoodTime * 5) * 14 : resting ? 80 : 0;
        BackLegRotate.Angle = moving ? -stride * 19 : resting ? -65 : sitting ? -35 : 0;
        HeadTilt.Angle = cat.Mood == CatMood.Grooming ? 8 + Math.Sin(cat.MoodTime * 5) * 7 : resting ? 12 : 0;
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

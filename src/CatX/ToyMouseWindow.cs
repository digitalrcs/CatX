using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using Brushes = System.Windows.Media.Brushes;

namespace CatX;

/// <summary>A decorative, click-through mouse; never moves or captures the actual Windows cursor.</summary>
internal sealed class ToyMouseWindow : Window
{
    private readonly ScaleTransform _facing = new(1, 1);
    private readonly Canvas _scene = new();
    private readonly Canvas _mouse;
    private readonly Canvas _outside = new();
    private readonly System.Windows.Shapes.Path _hole;
    private double _previousX;
    public ToyMouseWindow()
    {
        Width = 360; Height = 100; WindowStyle = WindowStyle.None; AllowsTransparency = true;
        Background = Brushes.Transparent; ShowInTaskbar = false; ShowActivated = false;
        Topmost = true; ResizeMode = ResizeMode.NoResize; IsHitTestVisible = false; Focusable = false;
        var canvas = new Canvas { Width = 58, Height = 34, RenderTransform = _facing, RenderTransformOrigin = new System.Windows.Point(.5, .5) };
        _mouse = canvas;
        // A little arched cartoon doorway. Mouse artwork is clipped to the outside
        // of its mouth, revealing/hiding the body progressively at the same location.
        _hole = new System.Windows.Shapes.Path
        {
            Data = Geometry.Parse("M 0,66 L 0,32 C 0,-10 64,-10 64,32 L 64,66 Z"),
            Fill = Brushes.Black, Stroke = Brushes.SaddleBrown, StrokeThickness = 5
        };
        _scene.Children.Add(_hole);
        _scene.Children.Add(_outside);
        _outside.Children.Add(canvas);
        var tail = new System.Windows.Shapes.Path { Data = Geometry.Parse("M 33,22 C 54,34 59,6 47,9"), Stroke = Brushes.RosyBrown, StrokeThickness = 2.5 };
        canvas.Children.Add(tail);
        AddEllipse(canvas, 32, 19, 5, 11, Brushes.SlateGray);
        AddEllipse(canvas, 12, 13, 9, 3, Brushes.DarkGray);
        AddEllipse(canvas, 7, 8, 11, 5, Brushes.Pink);
        AddEllipse(canvas, 3, 3, 7, 15, Brushes.Black);
        AddEllipse(canvas, 4, 4, 2, 22, Brushes.LightPink);
        Content = _scene;
    }
    public void UpdateScene(System.Windows.Point center, System.Windows.Point hole, double outward, double travel)
    {
        if (Math.Abs(center.X - _previousX) > .01) _facing.ScaleX = center.X > _previousX ? -1 : 1;
        _previousX = center.X;
        Width = travel + 140;
        Left = outward > 0 ? hole.X - 70 : hole.X - Width + 70;
        Top = hole.Y - 68;
        _scene.Width = Width; _scene.Height = Height;
        var mouth = hole.X - Left;
        Canvas.SetLeft(_hole, mouth - 32); Canvas.SetTop(_hole, 17);
        // The opening's middle is the doorway plane; nothing behind it is drawn.
        _outside.Clip = new RectangleGeometry(outward > 0
            ? new Rect(mouth, 0, Width - mouth, Height)
            : new Rect(0, 0, mouth, Height));
        Canvas.SetLeft(_mouse, center.X - Left - 29);
        Canvas.SetTop(_mouse, center.Y - Top - 17);
    }
    private static void AddEllipse(Canvas canvas, double w, double h, double x, double y, System.Windows.Media.Brush brush)
    {
        var shape = new Ellipse { Width = w, Height = h, Fill = brush };
        Canvas.SetLeft(shape, x); Canvas.SetTop(shape, y); canvas.Children.Add(shape);
    }
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        var handle = new WindowInteropHelper(this).Handle;
        SetWindowLongPtr(handle, -20, new IntPtr(GetWindowLongPtr(handle, -20).ToInt64() | 0x20 | 0x08000000 | 0x80));
    }
    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")] private static extern IntPtr GetWindowLongPtr(IntPtr h, int n);
    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")] private static extern IntPtr SetWindowLongPtr(IntPtr h, int n, IntPtr value);
}

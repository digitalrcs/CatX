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
    private double _previousX;
    public ToyMouseWindow()
    {
        Width = 58; Height = 34; WindowStyle = WindowStyle.None; AllowsTransparency = true;
        Background = Brushes.Transparent; ShowInTaskbar = false; ShowActivated = false;
        Topmost = true; ResizeMode = ResizeMode.NoResize; IsHitTestVisible = false; Focusable = false;
        var canvas = new Canvas { Width = 58, Height = 34, RenderTransform = _facing, RenderTransformOrigin = new System.Windows.Point(.5, .5) };
        var tail = new System.Windows.Shapes.Path { Data = Geometry.Parse("M 33,22 C 54,34 59,6 47,9"), Stroke = Brushes.RosyBrown, StrokeThickness = 2.5 };
        canvas.Children.Add(tail);
        AddEllipse(canvas, 32, 19, 5, 11, Brushes.SlateGray);
        AddEllipse(canvas, 12, 13, 9, 3, Brushes.DarkGray);
        AddEllipse(canvas, 7, 8, 11, 5, Brushes.Pink);
        AddEllipse(canvas, 3, 3, 7, 15, Brushes.Black);
        AddEllipse(canvas, 4, 4, 2, 22, Brushes.LightPink);
        Content = canvas;
    }
    public void MoveTo(System.Windows.Point center)
    {
        if (Math.Abs(center.X - _previousX) > 1) _facing.ScaleX = center.X > _previousX ? -1 : 1;
        _previousX = center.X;
        Left = center.X - Width / 2; Top = center.Y - Height / 2;
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

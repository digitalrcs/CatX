using System.Windows;
using Point = System.Windows.Point;

namespace CatX.Services;

internal enum MouseActivity { Hidden, Emerging, SeekingCheese, Collecting, Returning, Entering, Caught, Delivered }

/// <summary>One shared mouse makes cheese trips. Speed and steering vary per trip,
/// allowing real catches as well as successful escapes, without teleporting.</summary>
internal sealed class ToyMouseBehavior
{
    private readonly Random _random;
    private Rect _area;
    private Point _target;
    private double _wait = 12;
    private double _pause;
    private bool _nimble;
    public const double WalkSpeed = 80;
    public const double EscapeSpeed = 330;
    public const double CatchDistance = 36;
    public Point Position { get; private set; }
    public Point Hole { get; private set; }
    public Point CheesePosition { get; private set; }
    public bool CarryingCheese { get; private set; }
    public int Deliveries { get; private set; }
    public int Catches { get; private set; }
    public double Outward { get; private set; }
    public MouseActivity Activity { get; private set; }
    public bool Visible => Activity != MouseActivity.Hidden;
    public bool AtDoor => Activity is MouseActivity.Emerging or MouseActivity.Entering;
    public bool IsCatchable => Activity is MouseActivity.SeekingCheese or MouseActivity.Collecting or MouseActivity.Returning;

    public ToyMouseBehavior(int? seed = null) => _random = seed.HasValue ? new Random(seed.Value) : new Random();
    public void RequestVisit() => _wait = 0;
    public void Reset() { Activity = MouseActivity.Hidden; _wait = 12; _pause = 0; CarryingCheese = false; }

    public bool TryCatch(CatBehavior cat)
    {
        if (!IsCatchable || !cat.PlayfulMouse || cat.Mood != CatMood.MouseChase || (cat.Center - Position).Length > CatchDistance) return false;
        Activity = MouseActivity.Caught;
        if (CarryingCheese) CheesePosition = Position;
        CarryingCheese = false;
        _pause = 2;
        Catches++;
        cat.CelebrateCatch();
        return true;
    }

    public void Step(double seconds, Rect area, IReadOnlyList<CatBehavior> cats, bool enabled)
    {
        var dt = Math.Clamp(seconds, 0, .05);
        if (!enabled || area.Width < 220 || area.Height < 180) { Reset(); return; }
        if (Visible && area != _area) { Reset(); return; }
        if (dt == 0) return;
        if (!Visible)
        {
            _wait -= dt;
            if (_wait > 0) return;
            _area = area;
            Hole = Enumerable.Range(0, 24).Select(_ => RandomPoint(area, 90)).MaxBy(p => Clearance(p, cats));
            Outward = Hole.X < area.Left + area.Width / 2 ? 1 : -1;
            Position = Hole - new Vector(Outward * 40, 0);
            CheesePosition = Enumerable.Range(0, 24).Select(_ => RandomPoint(area, 70))
                .MaxBy(p => (p - Hole).Length + Math.Abs(p.Y - Hole.Y) * .5);
            _target = Clamp(Hole + new Vector(Outward * 90, 0), area);
            _nimble = _random.NextDouble() < .55;
            CarryingCheese = false;
            Activity = MouseActivity.Emerging;
            return;
        }
        if (Activity is MouseActivity.Caught or MouseActivity.Delivered)
        {
            _pause -= dt;
            if (_pause <= 0) { Reset(); _wait = 8 + _random.NextDouble() * 8; }
            return;
        }
        foreach (var cat in cats) if (TryCatch(cat)) return;
        if (Activity == MouseActivity.Collecting)
        {
            _pause -= dt;
            if (_pause <= 0)
            {
                CarryingCheese = true;
                Activity = MouseActivity.Returning;
                _target = Clamp(Hole + new Vector(Outward * 90, 0), area);
            }
            return;
        }
        var danger = Clearance(Position, cats) < 260;
        var delta = _target - Position;
        var speed = danger && !AtDoor ? (_nimble ? EscapeSpeed : 150) : WalkSpeed;
        var distance = Math.Min(speed * dt, delta.Length);
        if (delta.Length > .01)
        {
            var direction = delta / delta.Length;
            if (!AtDoor)
            {
                var directions = Enumerable.Range(0, 24).Select(i => new Vector(Math.Cos(i * Math.Tau / 24), Math.Sin(i * Math.Tau / 24))).Append(direction);
                direction = directions.MaxBy(v =>
                {
                    var look = Clamp(Position + v * Math.Min(65, delta.Length), area);
                    return -Math.Max(0, (_nimble ? 100 : 55) - Clearance(look, cats)) * 5 - (look - _target).Length;
                });
            }
            Position = Clamp(Position + direction * distance, area);
        }
        foreach (var cat in cats) if (TryCatch(cat)) return;
        if ((Position - _target).Length > 3) return;
        switch (Activity)
        {
            case MouseActivity.Emerging:
                Activity = MouseActivity.SeekingCheese;
                _target = CheesePosition;
                break;
            case MouseActivity.SeekingCheese:
                Activity = MouseActivity.Collecting;
                _pause = .7;
                break;
            case MouseActivity.Returning:
                Activity = MouseActivity.Entering;
                _target = Hole - new Vector(Outward * 40, 0);
                break;
            case MouseActivity.Entering:
                Activity = MouseActivity.Delivered;
                CarryingCheese = false;
                Deliveries++;
                _pause = 1.5;
                break;
        }
    }

    private Point RandomPoint(Rect area, double margin) => new(
        area.Left + margin + _random.NextDouble() * Math.Max(0, area.Width - 2 * margin),
        area.Top + margin + _random.NextDouble() * Math.Max(0, area.Height - 2 * margin));
    private static double Clearance(Point p, IReadOnlyList<CatBehavior> cats) => cats.Count == 0 ? 10000 : cats.Min(cat => (cat.Center - p).Length);
    private static Point Clamp(Point p, Rect area) => new(
        Math.Clamp(p.X, area.Left + 48, area.Right - 48),
        Math.Clamp(p.Y, area.Top + 24, area.Bottom - 24));
}

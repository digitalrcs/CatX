using System.Windows;
using Point = System.Windows.Point;

namespace CatX.Services;

internal enum CatMood { Idle, Walking, Sitting, Grooming, LyingDown, Sleeping, Waking, CursorChase, MouseChase }

/// <summary>Time-based motion and play decisions shared by vector and Blender-rendered cats.</summary>
internal sealed class CatBehavior
{
    private readonly Random _random;
    private readonly CursorExcitement _cursor = new();
    private Point _target;
    private Vector _velocity;
    private double _decisionIn = 1.5;
    private double _chaseFor;
    private double _mouseIn;
    private double _mouseFor;
    private double _mouseAngle;
    private bool _goingToBed;
    private bool _wakeForCursor;
    public const double Width = 256, Height = 224;
    public const double MouseEscapeDistance = 145;
    public Point Position { get; private set; }
    public Point MousePosition { get; private set; }
    public bool MouseVisible => _mouseFor > 0;
    public CatMood Mood { get; private set; } = CatMood.Idle;
    public double MoodTime { get; private set; }
    public double Facing { get; private set; } = 1;
    public double Speed => _velocity.Length;
    public bool ChaseCursor { get; set; } = true;
    public bool PlayfulMouse { get; set; } = true;
    public int RoamSeconds { get; set; } = 8;
    public Point Center => Position + new Vector(Width / 2, Height * .68);

    public CatBehavior(Rect area, int? seed = null)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
        Position = Clamp(new Point(area.Left + 30, area.Bottom - Height - 12), area);
        _target = Position;
        _mouseIn = 12 + _random.NextDouble() * 10;
    }

    public void Step(double seconds, Rect area, Point cursor)
    {
        // A suspended/resumed UI must not leap through minutes of simulated movement.
        var dt = Math.Clamp(seconds, 0, .05);
        if (dt == 0) return;
        MoodTime += dt;
        Position = Clamp(Position, area);
        var excited = _cursor.Observe(cursor, dt);
        _chaseFor = ChaseCursor ? Math.Max(0, _chaseFor - dt) : 0;
        if (ChaseCursor && excited && area.Contains(cursor))
        {
            _chaseFor = 4;
            if (Mood is CatMood.Sleeping or CatMood.LyingDown)
            {
                _wakeForCursor = true;
                Change(CatMood.Waking);
            }
            else if (Mood != CatMood.Waking) Change(CatMood.CursorChase);
            _goingToBed = false;
        }

        if (!PlayfulMouse || _chaseFor > 0)
        {
            _mouseFor = 0;
            _mouseIn = Math.Max(_mouseIn, 10);
        }
        else
        {
            _mouseIn -= dt;
            if (_mouseIn <= 0 && Mood is not (CatMood.Sleeping or CatMood.LyingDown or CatMood.Waking))
            {
                _mouseFor = 8 + _random.NextDouble() * 5;
                _mouseIn = 30 + _random.NextDouble() * 30;
                // Start the toy on the opposite side of the monitor.
                _mouseAngle = Math.Atan2(Center.Y - (area.Top + area.Height / 2), Center.X - (area.Left + area.Width / 2)) + Math.PI;
                _goingToBed = false;
                Change(CatMood.MouseChase);
            }
        }
        if (MouseVisible)
        {
            _mouseFor -= dt;
            _mouseAngle += dt * 390 / Math.Max(140, Math.Min(area.Width, area.Height) * .36);
            MousePosition = new Point(area.Left + area.Width / 2 + Math.Cos(_mouseAngle) * (Math.Max(1, area.Width / 2 - 55)),
                area.Top + area.Height / 2 + Math.Sin(_mouseAngle) * (Math.Max(1, area.Height / 2 - 45)));
            // Escape before contact, including on a tiny monitor or after a display resize.
            if ((MousePosition - Center).Length < MouseEscapeDistance) _mouseFor = 0;
        }

        if (Mood == CatMood.CursorChase)
        {
            if (_chaseFor <= 0) Rest();
            else _target = Clamp(cursor - new Vector(Width / 2, Height * .68), area);
        }
        if (Mood == CatMood.MouseChase)
        {
            if (!MouseVisible) { Change(CatMood.Sitting); _decisionIn = 3; }
            else _target = Clamp(MousePosition - new Vector(Width / 2, Height * .68), area);
        }
        if (Mood == CatMood.LyingDown && MoodTime >= 5) { Change(CatMood.Sleeping); _decisionIn = 16 + _random.NextDouble() * 20; }
        else if (Mood == CatMood.Waking && MoodTime >= 2)
        {
            if (_wakeForCursor && ChaseCursor && _chaseFor > 0) Change(CatMood.CursorChase);
            else Rest();
            _wakeForCursor = false;
        }
        else if (Mood is CatMood.Idle or CatMood.Sitting or CatMood.Grooming or CatMood.Sleeping)
        {
            _decisionIn -= dt;
            if (_decisionIn <= 0)
            {
                if (Mood == CatMood.Sleeping) Change(CatMood.Waking);
                else ChooseActivity(area);
            }
        }

        var moving = Mood is CatMood.Walking or CatMood.CursorChase or CatMood.MouseChase;
        var delta = _target - Position;
        var limit = Mood is CatMood.CursorChase or CatMood.MouseChase ? 285d : 120d;
        var desired = moving && delta.Length > 1 ? delta / delta.Length * Math.Min(limit, delta.Length * 3) : new Vector();
        _velocity += (desired - _velocity) * (1 - Math.Exp(-9 * dt));
        if (!moving) _velocity = new Vector();
        Position = Clamp(Position + _velocity * dt, area);
        if (MouseVisible && (MousePosition - Center).Length < MouseEscapeDistance) _mouseFor = 0;
        if (Math.Abs(_velocity.X) > 2) Facing = _velocity.X > 0 ? -1 : 1;
        if (Mood == CatMood.Walking && delta.Length < 3 && _velocity.Length < 10)
        {
            if (_goingToBed) { Change(CatMood.LyingDown); _goingToBed = false; }
            else Rest();
        }
    }

    private void ChooseActivity(Rect area)
    {
        switch (_random.Next(5))
        {
            case 0: Change(CatMood.Grooming); _decisionIn = 7 + _random.NextDouble() * 6; break;
            case 1: Change(CatMood.Sitting); _decisionIn = 5 + _random.NextDouble() * 6; break;
            default:
                _goingToBed = _random.Next(4) == 0;
                _target = _goingToBed
                    ? new Point(_random.Next(2) == 0 ? area.Left + 8 : area.Right - Width - 8, area.Bottom - Height - 8)
                    : new Point(area.Left + _random.NextDouble() * Math.Max(1, area.Width - Width), area.Top + _random.NextDouble() * Math.Max(1, area.Height - Height));
                _target = Clamp(_target, area);
                Change(CatMood.Walking);
                break;
        }
    }

    private void Rest() { Change(CatMood.Idle); _decisionIn = Math.Clamp(RoamSeconds, 2, 60); }
    private void Change(CatMood mood) { if (Mood == mood) return; Mood = mood; MoodTime = 0; }
    internal static Point Clamp(Point p, Rect area) => new(
        Math.Clamp(p.X, area.Left, Math.Max(area.Left, area.Right - Width)),
        Math.Clamp(p.Y, area.Top, Math.Max(area.Top, area.Bottom - Height)));
}

/// <summary>Recognizes quick changes of direction; a normal straight mouse move is not a play signal.</summary>
internal sealed class CursorExcitement
{
    private Point? _previous;
    private Vector _direction;
    private double _score;
    public bool Observe(Point cursor, double dt)
    {
        _score = Math.Max(0, _score - dt * 2.3);
        if (_previous is not Point previous) { _previous = cursor; return false; }
        var motion = cursor - previous;
        _previous = cursor;
        if (motion.Length > 450) { _score = 0; _direction = new Vector(); return false; }
        if (motion.Length < 3 || motion.Length / Math.Max(.001, dt) < 250) return false;
        motion.Normalize();
        if (_direction.Length > 0 && Vector.Multiply(motion, _direction) < .25) _score += 1;
        _direction = motion;
        return _score >= 2;
    }
}

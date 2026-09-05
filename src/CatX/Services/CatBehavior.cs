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
    private double _mouseElapsed;
    private Rect _mouseArea;
    private double _napIn = 40;
    private bool _goingToBed;
    private bool _wakeForCursor;
    public const double Width = 256, Height = 224;
    public const double MouseEscapeDistance = 145;
    public const double MouseSpeed = 80;
    public Point Position { get; private set; }
    public Point MousePosition { get; private set; }
    public Point MouseHolePosition { get; private set; }
    public double MouseOutwardDirection { get; private set; }
    public double MouseTravel { get; private set; }
    public bool MouseVisible => _mouseFor > 0;
    public CatMood Mood { get; private set; } = CatMood.Idle;
    public double MoodTime { get; private set; }
    public double Facing { get; private set; } = 1;
    public double Speed => _velocity.Length;
    public bool ChaseCursor { get; set; } = true;
    public bool PlayfulMouse { get; set; } = true;
    public int RoamSeconds { get; set; } = 8;
    public Point Center => Position + new Vector(Width / 2, Height * .68);

    public CatBehavior(Rect area, int? seed = null, int slot = 0, int count = 1)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
        Position = Clamp(new Point(area.Left + 30, area.Bottom - Height - 12), area);
        if (count > 1)
            Position = Clamp(new Point(area.Left + (area.Width - Width) * slot / Math.Max(1, count-1),
                area.Bottom - Height - 12 - (slot % 2) * 100), area);
        _napIn += slot * 7;
        _target = Position;
        _mouseIn = 12 + _random.NextDouble() * 10;
    }

    public void Step(double seconds, Rect area, Point cursor)
    {
        // A suspended/resumed UI must not leap through minutes of simulated movement.
        var dt = Math.Clamp(seconds, 0, .05);
        if (dt == 0) return;
        MoodTime += dt;
        if (Mood is not (CatMood.LyingDown or CatMood.Sleeping or CatMood.Waking)) _napIn -= dt;
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
            if (_mouseIn <= 0 && _napIn > 12 && !_goingToBed && !MouseVisible && Mood is not (CatMood.Sleeping or CatMood.LyingDown or CatMood.Waking))
            {
                StartMouseVisit(area);
            }
        }
        if (MouseVisible)
        {
            _mouseElapsed += dt;
            _mouseFor = Math.Max(0, _mouseFor - dt);
            // A short, horizontal excursion: reveal the hole, emerge, sniff, return,
            // then leave the empty hole visible briefly. The toy never teleports away.
            var legTime = (MouseTravel + 40) / MouseSpeed;
            var elapsed = _mouseElapsed - 1;
            var distance = elapsed < 0 ? -40
                : elapsed < legTime ? -40 + elapsed * MouseSpeed
                : elapsed < legTime + 1.5 ? MouseTravel
                : Math.Max(-40, MouseTravel - (elapsed - legTime - 1.5) * MouseSpeed);
            MousePosition = MouseHolePosition + new Vector(MouseOutwardDirection * distance, 0);
            // A display reconfiguration invalidates the old hole coordinates.
            if (area != _mouseArea) _mouseFor = 0;
        }

        if (Mood == CatMood.CursorChase)
        {
            if (_chaseFor <= 0) Rest();
            else _target = Clamp(cursor - new Vector(Width / 2, Height * .68), area);
        }
        if (Mood == CatMood.MouseChase)
        {
            if (!MouseVisible) { Change(CatMood.Sitting); _decisionIn = 3; }
            // Stalk the far end of the mouse's excursion, leaving room for its
            // unhurried return. Do not race through the mouse or into its hole.
            else _target = Clamp(MouseHolePosition + new Vector(MouseOutwardDirection * (MouseTravel + 190), 0)
                - new Vector(Width / 2, Height * .68), area);
        }
        if (_napIn <= 0 && !MouseVisible && _chaseFor <= 0 && !_goingToBed && Mood is not (CatMood.LyingDown or CatMood.Sleeping or CatMood.Waking)) GoToBed(area);
        if (Mood == CatMood.LyingDown && MoodTime >= 5) { Change(CatMood.Sleeping); _decisionIn = 30 + _random.NextDouble() * 15; }
        else if (Mood == CatMood.Waking && MoodTime >= 2)
        {
            if (_wakeForCursor && ChaseCursor && _chaseFor > 0) Change(CatMood.CursorChase);
            else Rest();
            _wakeForCursor = false;
            _napIn = 60 + _random.NextDouble() * 30;
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
        var limit = Mood == CatMood.CursorChase ? 285d : Mood == CatMood.MouseChase ? 70d : 120d;
        var desired = moving && delta.Length > 1 ? delta / delta.Length * Math.Min(limit, delta.Length * 3) : new Vector();
        _velocity += (desired - _velocity) * (1 - Math.Exp(-9 * dt));
        if (!moving) _velocity = new Vector();
        Position = Clamp(Position + _velocity * dt, area);
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
            case 0: Change(CatMood.Grooming); _decisionIn = 8 * _random.Next(1, 3); break;
            case 1: Change(CatMood.Sitting); _decisionIn = 5 + _random.NextDouble() * 6; break;
            default:
                _target = new Point(area.Left + _random.NextDouble() * Math.Max(1, area.Width - Width), area.Top + _random.NextDouble() * Math.Max(1, area.Height - Height));
                _target = Clamp(_target, area);
                Change(CatMood.Walking);
                break;
        }
    }

    private void GoToBed(Rect area)
    {
        _goingToBed = true;
        _target = Clamp(new Point(Center.X < area.Left + area.Width / 2 ? area.Left + 8 : area.Right - Width - 8,
            area.Bottom - Height - 8), area);
        Change(CatMood.Walking);
    }

    internal void PreviewPose(CatMood mood, double dt)
    {
        Change(mood);
        MoodTime += Math.Clamp(dt, 0, .05);
        _mouseFor = _chaseFor = 0;
        _velocity = new Vector(mood == CatMood.CursorChase ? 285 : mood == CatMood.Walking ? 120 : 0, 0);
    }

    internal void EndPosePreview()
    {
        _velocity = new Vector();
        _target = Position;
        _goingToBed = false;
        _napIn = 40;
        Rest();
    }

    internal void RequestMousePreview() { _mouseIn = .1; _napIn = 90; }

    private void StartMouseVisit(Rect area)
    {
        _mouseIn = 35 + _random.NextDouble() * 20;
        if (area.Width < 700 || area.Height < Height) return;
        // Try random desktop locations, reserving space between the excursion and
        // the cat. Holes are decorative overlays, not restricted to screen edges.
        for (var attempt = 0; attempt < 20; attempt++)
        {
            MouseHolePosition = new Point(area.Left + 72 + _random.NextDouble() * (area.Width-144),
                area.Top + 80 + _random.NextDouble() * Math.Max(1, area.Height-140));
            MouseOutwardDirection = Center.X > MouseHolePosition.X ? 1 : -1;
            MouseTravel = Math.Min(420 + _random.NextDouble()*220, Math.Abs(Center.X-MouseHolePosition.X)-220);
            if (MouseTravel >= Math.Min(340, area.Width*.28)) break;
        }
        // Do not spawn a toy where there is no room for both animals to stay apart.
        if (MouseTravel < 200) return;
        _mouseArea = area;
        _mouseElapsed = 0;
        _mouseFor = 1 + 2 * (MouseTravel + 40) / MouseSpeed + 1.5 + 1;
        MousePosition = MouseHolePosition - new Vector(MouseOutwardDirection * 40, 0);
        Change(CatMood.MouseChase);
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

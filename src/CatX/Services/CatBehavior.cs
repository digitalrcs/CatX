using System.Windows;
using Point = System.Windows.Point;

namespace CatX.Services;

internal enum CatMood { Idle, Walking, Sitting, Grooming, LyingDown, Sleeping, Waking, CursorChase, MouseChase, SocialApproach }

/// <summary>Time-based motion and play decisions shared by vector and Blender-rendered cats.</summary>
internal sealed class CatBehavior
{
    private readonly Random _random;
    private readonly CursorExcitement _cursor = new();
    private Point _target;
    private Vector _velocity;
    private double _decisionIn = 1.5;
    private double _chaseFor;
    private double _napIn = 40;
    private bool _goingToBed;
    private bool _wakeForCursor;
    private double _socialIn = 3;
    private CatBehavior? _friend;
    public bool Greeting => Mood == CatMood.Sitting && _friend is not null;

    public const double Width = 256, Height = 224;
    public const double NapSpacing = 170;
    public Point? NapSpot { get; private set; }
    private double _caughtFor;
    public bool CaughtMouse => _caughtFor > 0;
    public Point Position { get; private set; }
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
    }

    public void Step(double seconds, Rect area, Point cursor, IReadOnlyList<CatBehavior>? companions = null, ToyMouseBehavior? mouse = null)
    {
        // A suspended/resumed UI must not leap through minutes of simulated movement.
        var dt = Math.Clamp(seconds, 0, .05);
        if (dt == 0) return;
        MoodTime += dt;
        _caughtFor = Math.Max(0, _caughtFor - dt);
        if (Mood is not (CatMood.LyingDown or CatMood.Sleeping or CatMood.Waking)) _napIn -= dt;
        Position = Clamp(Position, area);
        if (NapSpot is Point reserved && Clamp(reserved, area) != reserved)
        {
            NapSpot = null;
            _goingToBed = false;
            GoToBed(area, companions);
        }
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
            NapSpot = null;
        }

        var followMouse = PlayfulMouse && mouse?.IsCatchable == true && _chaseFor <= 0 && _napIn > 0
            && !_goingToBed && _caughtFor <= 0 && Mood is not (CatMood.Sleeping or CatMood.LyingDown or CatMood.Waking or CatMood.Grooming);
        if (followMouse) Change(CatMood.MouseChase);

        if (Mood == CatMood.CursorChase)
        {
            if (_chaseFor <= 0) Rest();
            else _target = Clamp(cursor - new Vector(Width / 2, Height * .68), area);
        }
        if (Mood == CatMood.MouseChase)
        {
            if (!followMouse) { Change(CatMood.Sitting); _decisionIn = 3; }
            else
            {
                _target = Clamp(mouse!.Position - new Vector(Width / 2, Height * .68), area);
            }
        }
        if (_napIn <= 0 && _chaseFor <= 0 && !_goingToBed && Mood is not (CatMood.LyingDown or CatMood.Sleeping or CatMood.Waking)) GoToBed(area, companions);
        if (Mood == CatMood.LyingDown && MoodTime >= 5) { Change(CatMood.Sleeping); _decisionIn = 30 + _random.NextDouble() * 15; }
        else if (Mood == CatMood.Waking && MoodTime >= 2)
        {
            if (_wakeForCursor && ChaseCursor && _chaseFor > 0) Change(CatMood.CursorChase);
            else Rest();
            _wakeForCursor = false;
            NapSpot = null;
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

        // Social visits yield to naps, cursor play and toys. Follow a live companion,
        // then pause facing them before returning to independent roaming.
        _socialIn -= dt;
        if (Mood is not (CatMood.SocialApproach or CatMood.Sitting)) _friend = null;
        if (_friend is not null && (companions is null || !companions.Contains(_friend)))
        {
            _friend = null;
            if (Mood == CatMood.SocialApproach) Rest();
        }
        if (_socialIn <= 0 && !_goingToBed && _napIn > 12 && Mood is (CatMood.Idle or CatMood.Walking))
        {
            _socialIn = 15 + _random.NextDouble() * 10;
            _friend = companions?.Where(cat => cat != this && cat.Mood is not (CatMood.Sleeping or CatMood.LyingDown or CatMood.Waking))
                .OrderBy(cat => (cat.Position - Position).Length).FirstOrDefault();
            if (_friend is not null) Change(CatMood.SocialApproach);
        }
        if (Mood == CatMood.SocialApproach && _friend is not null)
        {
            if (_friend.Mood is CatMood.Sleeping or CatMood.LyingDown or CatMood.Waking)
            {
                _friend = null;
                Rest();
            }
            else
            {
                var offset = Position - _friend.Position;
                if (offset.Length < 1) offset = new Vector(1, 0);
                offset.Normalize();
                _target = Clamp(_friend.Position + offset * 185, area);
                if ((Position - _friend.Position).Length < 205)
                {
                    Change(CatMood.Sitting);
                    _decisionIn = 2.5;
                }
                else if (MoodTime > 8) { _friend = null; Rest(); }
            }
        }
        if (Greeting) Facing = _friend!.Position.X > Position.X ? -1 : 1;
        var moving = Mood is CatMood.Walking or CatMood.CursorChase or CatMood.MouseChase or CatMood.SocialApproach;
        var delta = _target - Position;
        var limit = Mood == CatMood.CursorChase ? 285d : Mood == CatMood.MouseChase ? 260d : 120d;
        var desired = moving && delta.Length > 1 ? delta / delta.Length * Math.Min(limit, delta.Length * 3) : new Vector();
        _velocity += (desired - _velocity) * (1 - Math.Exp(-9 * dt));
        if (!moving) _velocity = new Vector();
        Position = Clamp(Position + _velocity * dt, area);
        if (Mood == CatMood.MouseChase) mouse?.TryCatch(this);
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

    private void GoToBed(Rect area, IReadOnlyList<CatBehavior>? companions)
    {
        var occupied = companions?.Where(cat => cat != this && cat.NapSpot.HasValue)
            .Select(cat => Clamp(cat.NapSpot!.Value, area)).ToArray() ?? [];
        // Reserve before walking so cats choosing a bed on the next frame cannot
        // choose the same place. Prefer napping right here, then nearby spots.
        var candidates = new List<Point> { Position };
        for (var radius = NapSpacing; radius <= Math.Max(area.Width, area.Height) + NapSpacing; radius += NapSpacing)
            for (var angle = 0; angle < 16; angle++)
                candidates.Add(Clamp(Position + new Vector(Math.Cos(angle * Math.Tau / 16), Math.Sin(angle * Math.Tau / 16)) * radius, area));
        var spot = candidates.Distinct().Where(point => occupied.All(other => (point - other).Length >= NapSpacing - .01))
            .Select(point => (Point?)point).FirstOrDefault();
        if (spot is not Point bed)
        {
            // A tiny display may not have room for all cats to nap at once.
            NapSpot = null;
            _goingToBed = false;
            Rest();
            _napIn = 2;
            return;
        }
        NapSpot = bed;
        _target = bed;
        _goingToBed = true;
        Change(CatMood.Walking);
    }

    internal void CelebrateCatch()
    {
        _caughtFor = 2.5;
        _friend = null;
        _velocity = new Vector();
        Change(CatMood.Sitting);
        _decisionIn = 3;
    }

    internal void PreviewPose(CatMood mood, double dt)
    {
        Change(mood);
        MoodTime += Math.Clamp(dt, 0, .05);
        _chaseFor = 0;
        _velocity = new Vector(mood == CatMood.CursorChase ? 285 : mood == CatMood.Walking ? 120 : 0, 0);
    }

    internal void EndPosePreview()
    {
        _velocity = new Vector();
        _target = Position;
        _goingToBed = false;
        NapSpot = null;
        _friend = null;
        _socialIn = 3;
        _napIn = 40;
        Rest();
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

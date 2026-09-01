namespace CatX.Models;

public sealed class AppSettings
{
    public string CatStyle { get; set; } = "Marmalade";
    public UnlockChord UnlockChord { get; set; } = UnlockChord.CtrlAltK;
    public int RoamEverySeconds { get; set; } = 8;
    public int AutoLockAfterSeconds { get; set; }
}

public enum UnlockChord
{
    CtrlAltK,
    CtrlShiftF12,
    AltShiftPause
}

public static class UnlockChordExtensions
{
    public static string ToDisplayName(this UnlockChord chord) => chord switch
    {
        UnlockChord.CtrlAltK => "Ctrl + Alt + K",
        UnlockChord.CtrlShiftF12 => "Ctrl + Shift + F12",
        UnlockChord.AltShiftPause => "Alt + Shift + Pause",
        _ => throw new ArgumentOutOfRangeException(nameof(chord))
    };
}

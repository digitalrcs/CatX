using System.Runtime.InteropServices;

namespace CatX.Services;

/// <summary>
/// Reads only Windows' last-input timestamp. It does not capture keys, mouse
/// positions, button names, typed text, or any other input content.
/// </summary>
internal static class UserActivityMonitor
{
    public static bool TryGetIdleDuration(out TimeSpan idleDuration)
    {
        var lastInput = new LastInputInfo
        {
            Size = (uint)Marshal.SizeOf<LastInputInfo>()
        };

        if (!GetLastInputInfo(ref lastInput))
        {
            idleDuration = TimeSpan.Zero;
            return false;
        }

        idleDuration = TimeSpan.FromMilliseconds(ElapsedMilliseconds(GetTickCount(), lastInput.Time));
        return true;
    }

    internal static uint ElapsedMilliseconds(uint currentTick, uint previousTick) =>
        unchecked(currentTick - previousTick);

    internal static long EffectiveIdleMilliseconds(long desktopIdleMilliseconds, long monitoringElapsedMilliseconds) =>
        Math.Max(0, Math.Min(desktopIdleMilliseconds, monitoringElapsedMilliseconds));

    [StructLayout(LayoutKind.Sequential)]
    private struct LastInputInfo
    {
        public uint Size;
        public uint Time;
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetLastInputInfo(ref LastInputInfo lastInputInfo);

    [DllImport("kernel32.dll")]
    private static extern uint GetTickCount();
}

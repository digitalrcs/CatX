using System.Diagnostics;
using System.Runtime.InteropServices;
using CatX.Models;

namespace CatX.Services;

/// <summary>
/// Installs a Windows low-level keyboard hook only while the guard is active.
/// The selected recovery chord is evaluated before input is suppressed.
/// Windows' secure Ctrl+Alt+Delete screen cannot be blocked by this hook.
/// </summary>
public sealed class KeyboardGuard : IDisposable
{
    private const int WhKeyboardLl = 13;
    private const int WmKeyDown = 0x0100;
    private const int WmKeyUp = 0x0101;
    private const int WmSysKeyDown = 0x0104;
    private const int WmSysKeyUp = 0x0105;
    private const int VkControl = 0x11;
    private const int VkShift = 0x10;
    private const int VkMenu = 0x12;
    private const int VkK = 0x4B;
    private const int VkF12 = 0x7B;
    private const int VkPause = 0x13;

    private readonly LowLevelKeyboardProc _callback;
    private IntPtr _hook;
    private UnlockChord _unlockChord;

    public KeyboardGuard() => _callback = HookCallback;

    public bool IsActive => _hook != IntPtr.Zero;
    public event EventHandler? Unlocked;

    public void Enable(UnlockChord chord)
    {
        if (IsActive) return;
        _unlockChord = chord;
        using var process = Process.GetCurrentProcess();
        using var module = process.MainModule;
        var moduleHandle = GetModuleHandle(module?.ModuleName);
        _hook = SetWindowsHookEx(WhKeyboardLl, _callback, moduleHandle, 0);
        if (_hook == IntPtr.Zero)
            throw new InvalidOperationException("Windows did not allow CatX to install its keyboard guard.");
    }

    public void Disable()
    {
        if (!IsActive) return;
        UnhookWindowsHookEx(_hook);
        _hook = IntPtr.Zero;
    }

    private IntPtr HookCallback(int code, IntPtr message, IntPtr data)
    {
        if (code < 0 || !IsActive)
            return CallNextHookEx(_hook, code, message, data);

        var messageCode = message.ToInt32();
        var isDown = messageCode is WmKeyDown or WmSysKeyDown;
        var isKeyboardMessage = isDown || messageCode is WmKeyUp or WmSysKeyUp;
        if (!isKeyboardMessage)
            return CallNextHookEx(_hook, code, message, data);

        var key = Marshal.ReadInt32(data);
        if (isDown && MatchesUnlockChord(key))
        {
            // Remove the hook immediately. Raise the UI event asynchronously so the
            // hook returns quickly and never stalls input system-wide.
            Disable();
            ThreadPool.QueueUserWorkItem(_ => Unlocked?.Invoke(this, EventArgs.Empty));
            return new IntPtr(1);
        }

        return new IntPtr(1);
    }

    private bool MatchesUnlockChord(int key)
    {
        var ctrl = IsPressed(VkControl);
        var alt = IsPressed(VkMenu);
        var shift = IsPressed(VkShift);

        return _unlockChord switch
        {
            UnlockChord.CtrlAltK => key == VkK && ctrl && alt,
            UnlockChord.CtrlShiftF12 => key == VkF12 && ctrl && shift,
            UnlockChord.AltShiftPause => key == VkPause && alt && shift,
            _ => false
        };
    }

    private static bool IsPressed(int key) => (GetAsyncKeyState(key) & 0x8000) != 0;

    public void Dispose()
    {
        Disable();
        GC.SuppressFinalize(this);
    }

    private delegate IntPtr LowLevelKeyboardProc(int code, IntPtr message, IntPtr data);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc callback, IntPtr module, uint threadId);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hook);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hook, int code, IntPtr message, IntPtr data);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int key);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string? moduleName);
}

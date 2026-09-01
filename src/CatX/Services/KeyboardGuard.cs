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
    private readonly LowLevelKeyboardProc _callback;
    private readonly UnlockChordState _chordState = new();
    private IntPtr _hook;
    private UnlockChord _unlockChord;

    public KeyboardGuard() => _callback = HookCallback;

    public bool IsActive => _hook != IntPtr.Zero;
    public event EventHandler? Unlocked;

    public void Enable(UnlockChord chord)
    {
        if (IsActive) return;
        _unlockChord = chord;
        _chordState.Reset();
        using var process = Process.GetCurrentProcess();
        using var module = process.MainModule;
        var moduleHandle = GetModuleHandle(module?.ModuleName);
        _hook = SetWindowsHookEx(WhKeyboardLl, _callback, moduleHandle, 0);
        if (_hook == IntPtr.Zero)
            throw new InvalidOperationException("Windows did not allow CatX to install its keyboard guard.");
    }

    public void Disable()
    {
        if (IsActive)
        {
            UnhookWindowsHookEx(_hook);
            _hook = IntPtr.Zero;
        }
        _chordState.Reset();
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
        // Modifier key-down messages are also suppressed by this hook, so querying
        // Windows' asynchronous key state is unreliable here. Track every modifier
        // transition inside the hook and evaluate the final key against that state.
        if (_chordState.Process(key, isDown, _unlockChord))
        {
            // Remove the hook immediately. Raise the UI event asynchronously so the
            // hook returns quickly and never stalls input system-wide.
            Disable();
            ThreadPool.QueueUserWorkItem(_ => Unlocked?.Invoke(this, EventArgs.Empty));
            return new IntPtr(1);
        }

        return new IntPtr(1);
    }

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

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string? moduleName);
}

/// <summary>
/// Maintains modifier state from the same low-level events CatX suppresses. Kept
/// separate from the native hook so recovery behavior can be unit tested.
/// </summary>
internal sealed class UnlockChordState
{
    internal const int VkControl = 0x11;
    internal const int VkShift = 0x10;
    internal const int VkMenu = 0x12;
    internal const int VkLShift = 0xA0;
    internal const int VkRShift = 0xA1;
    internal const int VkLControl = 0xA2;
    internal const int VkRControl = 0xA3;
    internal const int VkLMenu = 0xA4;
    internal const int VkRMenu = 0xA5;
    internal const int VkK = 0x4B;
    internal const int VkF12 = 0x7B;
    internal const int VkPause = 0x13;

    private readonly HashSet<int> _pressedModifiers = [];
    private readonly object _sync = new();

    public bool Process(int key, bool isDown, UnlockChord chord)
    {
        lock (_sync)
        {
            if (IsModifier(key))
            {
                if (isDown) _pressedModifiers.Add(key); else _pressedModifiers.Remove(key);
                return false;
            }

            if (!isDown) return false;

            var ctrl = HasAny(VkControl, VkLControl, VkRControl);
            var alt = HasAny(VkMenu, VkLMenu, VkRMenu);
            var shift = HasAny(VkShift, VkLShift, VkRShift);

            return chord switch
            {
                UnlockChord.CtrlAltK => key == VkK && ctrl && alt,
                UnlockChord.CtrlShiftF12 => key == VkF12 && ctrl && shift,
                UnlockChord.AltShiftPause => key == VkPause && alt && shift,
                _ => false
            };
        }
    }

    public void Reset()
    {
        lock (_sync) _pressedModifiers.Clear();
    }

    private bool HasAny(params int[] keys) => keys.Any(_pressedModifiers.Contains);

    private static bool IsModifier(int key) => key is
        VkControl or VkShift or VkMenu or VkLShift or VkRShift or
        VkLControl or VkRControl or VkLMenu or VkRMenu;
}

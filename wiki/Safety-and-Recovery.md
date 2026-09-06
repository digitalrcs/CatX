# Restore typing and stay in control

CatX temporarily suppresses ordinary keyboard input. It keeps the mouse available and provides several ways to restore typing.

## Choose and test a shortcut

Select one combination before enabling the guard:

| Shortcut | Practical note |
| --- | --- |
| **Ctrl + Alt + K** | Convenient on most keyboards. |
| **Ctrl + Shift + F12** | Some laptop keyboards also require Fn for F12. |
| **Alt + Shift + Pause** | Requires a keyboard with an accessible Pause key. |

Hold the modifier keys, then press the final key. Test your choice immediately after enabling the guard, especially after changing it.

## If you cannot type

1. Use the exact shortcut displayed in CatX.
2. Click **Disable guard with mouse**.
3. If CatX is minimized, right-click its notification-area icon and select **Disable keyboard guard**.
4. Close CatX with the mouse to remove its hook.
5. If the interface is unavailable, press **Ctrl + Alt + Delete**, open Task Manager, and end `CatX.exe`.

Windows handles **Ctrl + Alt + Delete** outside ordinary applications; CatX cannot suppress that secure screen.

## What protection covers

CatX protects the current interactive desktop session from accidental keyboard input. It is not a Windows account lock or a substitute for locking your computer when leaving it unattended.

The hook exists only while the CatX process owns it. CatX does not run as a service or keep a lock after it exits. It never blocks the mouse.

If automatic protection returns after unlocking, set **Auto-lock after no activity** to **Off**.

**Next:** [Troubleshooting](Troubleshooting)

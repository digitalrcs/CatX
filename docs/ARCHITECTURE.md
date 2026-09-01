# CatX architecture

CatX is a single-process WPF desktop application targeting .NET 8 and Windows 11. It does not install a driver, service, browser component, or background updater.

## Runtime components

```text
MainWindow
  ├── SettingsService ──> %LOCALAPPDATA%\CatX\settings.json
  ├── KeyboardGuard ────> Windows WH_KEYBOARD_LL hook
  └── CatOverlayWindow ─> transparent, click-through WPF window
```

### Main window

`MainWindow` owns the application lifecycle and is the only component allowed to enable or disable the guard. It keeps preference controls disabled while guarding so the displayed recovery shortcut always matches the active hook.

Minimizing `MainWindow` hides it from the taskbar and exposes a `NotifyIcon` in the Windows notification area. Its context menu dispatches Open, Disable keyboard guard, and Exit commands back to the WPF dispatcher. The Disable item mirrors `KeyboardGuard.IsActive`, so it is available only when there is an active hook to remove. Closing CatX disposes the tray icon and menu.

### Keyboard guard

`KeyboardGuard` uses `SetWindowsHookEx` with `WH_KEYBOARD_LL`. While active, it returns a nonzero result for ordinary keyboard messages. Because suppressed modifier messages may not appear in Windows' asynchronous key state, `UnlockChordState` records Ctrl, Alt, and Shift transitions from the hook itself. It checks that tracked state and the selected recovery key first. A matching recovery chord removes the hook immediately and then notifies the UI on a worker thread.

The hook belongs to the CatX process. Windows removes it if the process exits. `MainWindow.OnClosing` also disposes it deliberately. CatX does not and cannot intercept the Windows secure-attention sequence, `Ctrl + Alt + Delete`.

### Cat overlay

`CatOverlayWindow` is transparent, topmost, excluded from the taskbar, non-activating, and marked click-through with extended Windows styles. WPF animations move the window within `SystemParameters.VirtualScreen*`, which covers the combined multi-monitor desktop. The overlay mirrors every cat from its left-facing source orientation when moving right, so the cat always faces its travel direction.

The five cat styles recolor built-in vector shapes and use eased WPF property animations. No cat artwork is loaded from executable or remote content.

The application icon and DigitalRCS logo are compiled WPF resources. Installer packaging remains self-contained and does not download artwork or executable content at runtime.

### Settings

`SettingsService` serializes four non-sensitive preferences as JSON: cat style, recovery chord, roaming interval, and optional auto-lock delay. Malformed JSON falls back to safe defaults. Auto-lock is off by default. When enabled, `UserActivityMonitor` reads only Windows' last-input timestamp. Keyboard or mouse activity resets the inactivity countdown, and the guard activates only after the selected period with no input. Idle time from before launch, a preference change, or an unlock is not counted.

## Trust boundaries

- **Keyboard input:** observed and suppressed locally only while the guard is enabled; never stored or transmitted.
- **Mouse input:** never hooked or suppressed. Its Windows last-input timestamp resets the optional inactivity timer.
- **Network:** CatX has no networking code.
- **Files:** CatX writes only its local preferences file during normal operation.
- **Privileges:** standard user privileges are sufficient and expected.

## Release pipeline

The Build workflow restores, compiles, publishes a self-contained `win-x64` executable, and uploads it as a workflow artifact. Pushing a `v*` tag runs the Release workflow, packages `CatX.exe` in a ZIP, and attaches it to a GitHub release. The executable is currently unsigned.

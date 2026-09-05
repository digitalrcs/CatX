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

`CatOverlayWindow` is transparent, topmost, excluded from the taskbar, non-activating, and marked click-through with extended Windows styles. A 16 ms dispatcher timer advances `CatBehavior` using elapsed time, with smoothed velocity and a capped time step after a UI pause. This keeps decisions running even when a transparent window is visually still and composition callbacks pause. The selected monitor's working area and Windows cursor coordinates are converted to WPF coordinates; display changes are rechecked periodically. The overlay mirrors left-facing source artwork based on horizontal velocity. Closing it stops the timer, detaches its callback, and closes its decorative mouse window.

The five original styles use vector geometry with walking, grooming, and resting poses. Seven realistic styles use embedded PNGs rendered from the supplied Blender rig. `RealisticCatFrames` loads only the selected coat, freezes the decoded images, and supplies interpolated clip frames. Clip changes briefly crossfade. Blender is an offline authoring dependency only. See `tools/render_realistic_cats.py` and `docs/ANIMATED_CATS.md`.

`CatBehavior` owns idle, walking, sitting, grooming, lying down, sleeping, waking, and chase states. `CursorExcitement` recognizes fast direction changes while rejecting steady movement and cursor jumps. `ToyMouseWindow` is also non-activating and click-through. Its moving target escapes before contact, and actual cursor play has priority. All positions stay in memory. The preview UI runs the same overlay while leaving the guard disabled and pausing automatic locking.

The application icon and DigitalRCS logo are compiled WPF resources. Installer packaging remains self-contained and does not download artwork or executable content at runtime.

### Settings

`SettingsService` serializes six preferences as JSON: cat style, recovery chord, roaming interval, optional auto-lock delay, cursor play, and toy mouse visits. Missing play preferences default to enabled; auto-lock defaults to off. Malformed JSON falls back to safe defaults. `UserActivityMonitor` reads only Windows' last-input timestamp. Keyboard or mouse activity resets the inactivity countdown. Idle time from before launch, a preference change, an unlock, or the end of preview is not counted.

## Trust boundaries

- **Keyboard input:** observed and suppressed locally only while the guard is enabled; never stored or transmitted.
- **Mouse input:** never hooked, suppressed, moved, or clicked. Its Windows last-input timestamp resets the optional inactivity timer. While an overlay is active, cursor coordinates are sampled in memory for play behavior and never persisted.
- **Network:** CatX has no networking code.
- **Files:** CatX writes only its local preferences file during normal operation.
- **Privileges:** standard user privileges are sufficient and expected.

## Release pipeline

The Build workflow restores, compiles, publishes a self-contained `win-x64` executable, and uploads it as a workflow artifact. Pushing a `v*` tag runs the Release workflow, packages `CatX.exe` in a ZIP, and attaches it to a GitHub release. The executable is currently unsigned.

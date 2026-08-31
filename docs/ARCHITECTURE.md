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

### Keyboard guard

`KeyboardGuard` uses `SetWindowsHookEx` with `WH_KEYBOARD_LL`. While active, it returns a nonzero result for ordinary keyboard messages. It checks modifier state and the selected recovery key first. A matching recovery chord removes the hook immediately and then notifies the UI on a worker thread.

The hook belongs to the CatX process. Windows removes it if the process exits. `MainWindow.OnClosing` also disposes it deliberately. CatX does not and cannot intercept the Windows secure-attention sequence, `Ctrl + Alt + Delete`.

### Cat overlay

`CatOverlayWindow` is transparent, topmost, excluded from the taskbar, non-activating, and marked click-through with extended Windows styles. WPF animations move the window within `SystemParameters.VirtualScreen*`, which covers the combined multi-monitor desktop. Cat styles recolor original built-in vector shapes rather than loading executable or remote content.

### Settings

`SettingsService` serializes three non-sensitive preferences as JSON: cat style, recovery chord, and roaming interval. Malformed JSON falls back to safe defaults. No setting automatically enables the guard.

## Trust boundaries

- **Keyboard input:** observed and suppressed locally only while the guard is enabled; never stored or transmitted.
- **Mouse input:** never hooked or suppressed.
- **Network:** CatX has no networking code.
- **Files:** CatX writes only its local preferences file during normal operation.
- **Privileges:** standard user privileges are sufficient and expected.

## Release pipeline

The Build workflow restores, compiles, publishes a self-contained `win-x64` executable, and uploads it as a workflow artifact. Pushing a `v*` tag runs the Release workflow, packages `CatX.exe` in a ZIP, and attaches it to a GitHub release. The executable is currently unsigned.

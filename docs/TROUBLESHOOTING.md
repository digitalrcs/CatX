# Troubleshooting

## I cannot type after enabling CatX

1. Hold the exact recovery combination displayed in the CatX window.
2. Use the mouse to select **Disable guard with mouse**.
3. Close CatX with the mouse from the taskbar or window controls.
4. If the interface is unavailable, press `Ctrl + Alt + Delete`, open Task Manager, and end `CatX.exe`.

The process-owned hook disappears when CatX exits.

## The recovery shortcut does not work

- Press and hold both modifier keys before pressing the final key.
- Some laptop function rows require `Fn`; try the `Ctrl + Alt + K` option.
- Accessibility tools, keyboard remappers, remote-desktop software, or vendor hotkey utilities may reserve a combination. Choose a different CatX shortcut before enabling the guard again.

Version 1.0.1 fixed modifier tracking while input is suppressed. If you are using 1.0.0, download the latest release before troubleshooting further.

## CatX locks again after I unlock it

Check **Auto-lock after no activity** in the main window. When enabled, every keyboard or mouse action resets the countdown. Protection returns only after the selected period with no input. Select **Off** to require manual activation.

## Windows SmartScreen appears

CatX releases are not currently code-signed. Verify that the ZIP came from `https://github.com/digitalrcs/CatX/releases`, scan it with your security software, and use SmartScreen's **More info** option only if you trust the download. Building from source is also supported.

## The cat does not appear

- Make sure **Desktop cat** is not set to **No cat**.
- Check all connected monitors; CatX uses the complete virtual desktop.
- Wait for the selected movement interval.
- Display-management software that creates unusual virtual-screen bounds may position the overlay unexpectedly. Disable the guard, reconnect displays, and restart CatX.

## Preferences reset

CatX stores preferences in `%LOCALAPPDATA%\CatX\settings.json`. If it is invalid or inaccessible, CatX uses defaults. Closing CatX, deleting that file, and reopening the app resets preferences safely.

## CatX disappeared after minimizing

CatX remains running in the Windows notification area instead of the taskbar. Select the hidden-icons arrow if needed, then find the CatX cat icon. Double-click it to reopen CatX, or right-click it to disable the keyboard guard or exit.

## Reporting a problem

Use the repository's bug-report form and include Windows version, CatX version, shortcut choice, keyboard layout, number of monitors, and exact reproduction steps. Do not include passwords, typed content, or other private data.

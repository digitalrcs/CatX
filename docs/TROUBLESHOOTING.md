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

## Windows SmartScreen appears

CatX releases are not currently code-signed. Verify that the ZIP came from `https://github.com/digitalrcs/CatX/releases`, scan it with your security software, and use SmartScreen's **More info** option only if you trust the download. Building from source is also supported.

## The cat does not appear

- Check all connected monitors; CatX uses the complete virtual desktop.
- Wait for the selected movement interval.
- Display-management software that creates unusual virtual-screen bounds may position the overlay unexpectedly. Disable the guard, reconnect displays, and restart CatX.

## Preferences reset

CatX stores preferences in `%LOCALAPPDATA%\CatX\settings.json`. If it is invalid or inaccessible, CatX uses defaults. Closing CatX, deleting that file, and reopening the app resets preferences safely.

## Reporting a problem

Use the repository's bug-report form and include Windows version, CatX version, shortcut choice, keyboard layout, number of monitors, and exact reproduction steps. Do not include passwords, typed content, or other private data.

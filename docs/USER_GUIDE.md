# CatX Keyboard Guard - User Guide

**Version 1.2.0**
**Dan Roberts - DigitalRCS**

CatX is a friendly Windows 11 keyboard guard for moments when a cat decides the keyboard is the best seat in the house. It temporarily ignores ordinary keyboard input while keeping the mouse available and showing an animated desktop cat.

CatX runs locally, requires no account, makes no network requests, and collects no data.

## Install CatX

1. Download the current `CatX-Setup` installer from the official DigitalRCS CatX release.
2. Double-click the installer.
3. Review the license and installation location.
4. Optionally select **Create a desktop shortcut**.
5. Choose **Install**, then **Launch CatX**.

CatX installs for the current Windows user and does not require administrator privileges. If Windows SmartScreen identifies an unsigned build, verify that the installer came from the official CatX release before choosing **More info** and **Run anyway**. Never bypass a warning for a file from an unknown source.

## Choose your settings

### Desktop cat

- **No cat** - keyboard protection without a desktop animation.
- **Marmalade, Midnight, Snowball, Tuxedo, or Calico** - original animated vector cats.
- **Realistic Tabby, Orange, White, Grey, Tuxedo, Black, or Bicolor** - textured animations rendered from the Blender cat rig. No Blender installation is needed.

Open **Choose your cats** and Ctrl-click up to eight names. Selected names are highlighted; there are no checkboxes inside the list. Click the arrow again, click outside, or press Escape to close it. Use **No cats / clear selection** for keyboard-only protection. Choose cats before enabling the guard.

Cats roam, greet companions, groom, and nap anywhere on the starting monitor. Each reserves a separate sleeping spot, so they can rest beside each other. Naps last 30-45 seconds; rapid back-and-forth cursor movement can wake a cat for play.

One shared mouse finds visible cheese, carries it toward its hole, and tries to evade cats. Cats can run and catch it; some mouse trips succeed and display **+1 cheese**. A catch produces a brief playful celebration. **Chase playful cursor** and **Toy mouse visits** can be changed independently while cats are active. The hole stays stationary during a trip. No animation intercepts clicks.

There is no Preview cats button or preview-action selector. Enabling the guard shows the selected cats using natural behavior. For a developer review with the keyboard guard disabled, launch `CatX.exe --review`; close the app to end the review.

See [Animated cats](ANIMATED_CATS.md) for more detail. The included v1.2.0 PDF covers the current controls and behaviors.

### Unlock combination

Choose one private recovery shortcut and remember it before enabling the guard:

- `Ctrl + Alt + K`
- `Ctrl + Shift + F12`
- `Alt + Shift + Pause`

### Cat moves every

Choose how frequently the desktop cat moves to a new location. This setting is unavailable when **No cat** is selected.

### Auto-lock after no activity

Leave automatic locking **Off**, or select an inactivity period. While the keyboard guard is inactive, every keyboard or mouse action resets the timer. CatX activates the guard only after there has been no input for the full selected period.

## Use the keyboard guard

1. Confirm your recovery shortcut in the CatX window.
2. Select **Enable keyboard guard**.
3. Test the recovery shortcut immediately.
4. Leave CatX open while protection is needed.

While active, CatX ignores ordinary keyboard input. The mouse remains available. Use **Disable guard with mouse** in the CatX window, or hold the selected recovery shortcut, to restore typing.

### Minimize to the system tray

Minimizing CatX hides its taskbar button and leaves a cat icon in the Windows notification area. Right-click the icon to **Open CatX**, **Disable keyboard guard**, or **Exit CatX**. The Disable command is available only while the guard is active. Double-click the cat icon to reopen the window.

## Safety and emergency recovery

- Test the selected recovery shortcut every time you change it.
- CatX never blocks the mouse.
- Windows handles `Ctrl + Alt + Delete` outside CatX, so the secure Windows screen remains available.
- Closing CatX removes its keyboard hook and restores normal input.
- CatX does not run as a service and does not keep the keyboard locked after the process exits.

If typing does not return:

1. Click **Disable guard with mouse**.
2. Close the CatX window with the mouse.
3. Press `Ctrl + Alt + Delete` and use the Windows secure screen if necessary.

## Troubleshooting

### The desktop cat is missing

- Confirm **Desktop cat** is not set to **No cat**.
- Look on the monitor where your cursor was when the guard started; cats stay within that monitor's working area.
- Disable and re-enable the guard after changing the cat selection.

### The recovery shortcut does not respond

- Hold all keys in the selected combination at the same time.
- Try the left and right Ctrl, Alt, or Shift keys.
- Use the mouse fallback and verify the selected shortcut before enabling the guard again.

### CatX does not start

- Confirm the PC runs 64-bit Windows 11.
- Reinstall CatX from the official release.
- If security software quarantined CatX, verify the installer source before restoring or allowing it.

## Privacy and local settings

CatX does not collect typed content, analytics, account information, or personal data. Preferences are stored locally in:

`%LOCALAPPDATA%\CatX\settings.json`

Uninstalling CatX removes the application. The local settings file may remain so preferences can be restored after reinstalling; it can be deleted manually when CatX is closed.

## Uninstall CatX

1. Open Windows **Settings**.
2. Select **Apps**, then **Installed apps**.
3. Find **CatX Keyboard Guard**.
4. Choose **Uninstall** and follow the prompts.

## Support

Project and release information: [github.com/digitalrcs/CatX](https://github.com/digitalrcs/CatX)

When reporting a reproducible problem, include the CatX version, Windows version, CPU architecture, keyboard layout, selected recovery shortcut, and the steps that produced the problem. Never share passwords, authentication codes, private typed content, or security tokens.

---

CatX is original software from **Dan Roberts - DigitalRCS**.

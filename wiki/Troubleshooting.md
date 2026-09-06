# Troubleshooting

## I cannot type

Use **Disable guard with mouse**, the tray menu, or close CatX. Full fallback steps are on [Safety and recovery](Safety-and-Recovery).

If a shortcut is unreliable, hold the modifier keys before the final key. Check whether your laptop requires **Fn**, or choose **Ctrl + Alt + K**. Keyboard remappers and other hotkey utilities may reserve combinations.

## The cats do not appear

- Select at least one name under **Choose your cats**.
- Enable the keyboard guard.
- Check the monitor where the cursor was when protection started.
- To change monitors, disable the guard, move the cursor, and enable it again.

## I can only choose one cat

Hold **Ctrl** while clicking each additional name. An ordinary click replaces the selection. You can choose up to eight cats.

## The cat list stays open

The list deliberately stays open while selecting names. Click its arrow again, click outside it, or press **Escape** to close it. If an older build will not stay closed, update to the [latest release](https://github.com/digitalrcs/CatX/releases/latest).

## A cat is sleeping or ignoring the mouse

Naps and pauses are normal. Cats reserve separate nap spots and sleep for about 30–45 seconds. Quick cursor direction changes can wake a cat when **Chase playful cursor** is enabled.

Check **Toy mouse visits** for mouse activity. Allow time for a new trip; not every cat joins every chase.

## The guard turns itself back on

Set **Auto-lock after no activity** to **Off** for manual activation. With a delay selected, protection returns after that full period without keyboard or mouse activity.

## CatX disappeared when minimized

Find the cat icon in the Windows notification area, including the hidden-icons menu. Double-click it to reopen CatX.

## CatX will not start

Confirm Windows 11 x64, extract the full portable ZIP if using it, or reinstall from the [official release](https://github.com/digitalrcs/CatX/releases/latest). See [Installation](Installation) for unsigned-build and SmartScreen information.

## Preferences are incorrect or keep resetting

Preferences live in `%LOCALAPPDATA%\CatX\settings.json`. An invalid or inaccessible file causes CatX to use defaults.

To reset: exit CatX, rename that file to `settings.backup.json`, and reopen CatX. This preserves a backup while CatX creates fresh settings.

## Report a problem

[Open a GitHub issue](https://github.com/digitalrcs/CatX/issues/new/choose) with the CatX version, Windows version, keyboard layout, chosen shortcut, monitor setup, and exact steps. For visual issues, include a screenshot with private information removed.

See the repository's [security policy](https://github.com/digitalrcs/CatX/blob/main/SECURITY.md) for security reports.

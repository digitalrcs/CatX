# Local, private, offline

CatX requires **no account**, makes **no network requests**, and has **no analytics or telemetry**.

## What CatX uses

| Information | Purpose and handling |
| --- | --- |
| Keyboard events while guarded | Detect the selected recovery chord and suppress ordinary input. Typed content is not recorded. |
| Windows last-input timestamp | Determine when the selected inactivity period has elapsed. |
| Cursor positions during play | Animate reactions to movement. Positions remain transiently in memory. |
| Preferences | Save your cats, shortcut, timing, and play choices locally. |

## Where preferences live

`%LOCALAPPDATA%\CatX\settings.json`

There is no cloud sync. To clear preferences, exit CatX and remove the file. Uninstalling may leave it available for a future installation.

Opening GitHub download, help, or support links in your browser is separate from CatX's offline operation.

Read the repository's [privacy document](https://github.com/digitalrcs/CatX/blob/main/docs/PRIVACY.md) for the project policy.

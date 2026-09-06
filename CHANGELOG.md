# Changelog

All notable CatX changes are documented here. The project follows [Semantic Versioning](https://semver.org/).

## [1.2.0] - 2026-09-06

- Cats can nap anywhere, reserving separate spots while allowing nearby sleepers.
- Added visible cheese fetching, carrying, and delivery by the shared mouse, with cat running/captures and variable mouse evasion.

- Replaced automatic popup/toggle dismissal with explicit button, outside-click, Escape, and deactivation handling so arrow clicks close the cat list without reopening it.

- Fixed mouse-hole jitter by keeping the transparent drawing window stationary while the mouse moves inside it.

- Added a simple Ctrl-click dropdown for selecting up to eight cats, with selected names highlighted.
- Removed the Preview cats button and preview-action selector. The cat dropdown shows plain names with highlighted selections and no checkboxes.
- Replaced per-cat toy mice with one shared mouse that explores the desktop, turns, pauses, avoids nearby cats, and returns to its hole.
- Added companion approaches and heart greetings during natural play, with saved-selection and behavior regression coverage.

## [1.1.1] - 2026-09-05

- Added 1–8 independently styled cats and immediate animation-review controls in keyboard-safe preview mode.
- Authored a settled Blender sleep loop with tail-tip-only motion. Realistic cats use the supplied paw-wash action; the rejected custom ear reach has been removed. Original animated cats keep their approved paw/ear grooming.
- Randomized mouse-hole locations throughout the working area and extended toy excursions while retaining the slower speed.

- Removed overlapping realistic-cat frames that made running cats appear to have extra legs.
- Scheduled regular corner naps, with a first bedtime after about 40 seconds plus walking/lying-down time and 30–45 seconds of sleep. Toy visits no longer interrupt bedtime.
- Replaced the fast orbiting toy with an 80-pixel-per-second excursion out of a cartoon mouse hole and back into the same hole. Cats stalk from a safe distance instead of catching it.

## [1.1.0] - 2026-09-05

- Added seven realistic coats rendered from the supplied Blender cat rig, with walk, run, idle, sit, grooming, lie-down, sleep, and waking animations.
- Shared time-based behavior and smooth movement for all cats, including corner naps and excited cursor chasing.
- Added an occasional escaping toy mouse and independent play preferences.
- Added a keyboard-safe preview that pauses automatic locking until it is stopped.
- Documented in-memory cursor sampling and source-asset provenance.

## [1.0.3] - 2026-09-03

### Fixed

- Removed misplaced face and body spots from the solid Midnight and two-tone Tuxedo cats.
- Repositioned Calico markings into separate crown and flank regions so they no longer cover the eyes or cross onto the wrong body part.
- Added regression coverage for coat-specific marking visibility.

## [1.0.2] - 2026-09-01

### Fixed

- Cats now face the direction they travel and use smoother eased movement transitions.
- Auto-lock now resets on keyboard or mouse activity and activates only after the selected period with no input.

### Changed

- Reduced the main window to a compact 800 by 560 layout while keeping every setting visible.

### Added

- DigitalRCS branding, a cat application icon, a branded user guide, and a per-user Windows installer definition.
- System-tray minimization with Open, Disable keyboard guard, and Exit commands.
- Regression tests for cat direction and inactivity timing.

## [1.0.1] - 2026-08-31

### Fixed

- Recovery shortcuts now track Ctrl, Alt, and Shift directly from suppressed low-level hook events, allowing keyboard unlock to work while the guard is active.
- Cat coat patches now render beneath facial features so eyes and whiskers remain visible.

### Added

- **No cat** preference for users who want keyboard protection without a desktop animation.
- Optional auto-lock countdown with delays from 30 seconds to 1 hour; off remains the default.
- Regression tests for every recovery shortcut, left/right modifier keys, modifier release, and state reset.

## [1.0.0] - 2026-08-31

### Added

- Windows 11 WPF keyboard guard with process-scoped low-level hook.
- Selectable recovery shortcuts: `Ctrl + Alt + K`, `Ctrl + Shift + F12`, and `Alt + Shift + Pause`.
- Five original vector cat themes.
- Animated click-through roaming across the Windows virtual desktop.
- Configurable roaming intervals and local preference persistence.
- Mouse fallback and documented `Ctrl + Alt + Delete` recovery path.
- Self-contained `win-x64` publishing and GitHub Actions build/release workflows.
- User, contributor, architecture, privacy, security, support, and troubleshooting documentation.

[1.0.0]: https://github.com/digitalrcs/CatX/releases/tag/v1.0.0
[1.0.1]: https://github.com/digitalrcs/CatX/releases/tag/v1.0.1
[1.0.2]: https://github.com/digitalrcs/CatX/releases/tag/v1.0.2
[1.0.3]: https://github.com/digitalrcs/CatX/releases/tag/v1.0.3
[1.1.0]: https://github.com/digitalrcs/CatX/releases/tag/v1.1.0
[1.1.1]: https://github.com/digitalrcs/CatX/releases/tag/v1.1.1

[1.2.0]: https://github.com/digitalrcs/CatX/releases/tag/v1.2.0

# Changelog

All notable CatX changes are documented here. The project follows [Semantic Versioning](https://semver.org/).

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

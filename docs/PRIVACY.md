# Privacy

CatX is designed to work entirely on the local Windows computer.

## Data CatX processes

The keyboard hook receives low-level key events while the guard is active solely to detect the configured recovery chord and suppress other input. CatX does not translate those events into text, log them, save them, or transmit them.

While a cat or preview is active, the overlay samples the Windows cursor position in memory to recognize quick direction changes and animate cursor chasing. Positions are not logged, saved, or transmitted. The decorative toy mouse does not control or click the real cursor. Closing the cat stops sampling.

## Data CatX stores

CatX stores only these preferences in `%LOCALAPPDATA%\CatX\settings.json`:

- selected cat style;
- selected recovery shortcut;
- cat movement interval.
- optional auto-lock delay.
- cursor-chasing and toy-mouse preferences.

The file can be deleted at any time to reset preferences.

## Network and analytics

CatX contains no networking, telemetry, advertising, analytics, crash-reporting, account, or update-checking code. GitHub may collect its own standard web and download information when someone visits this repository or downloads a release; that is governed by GitHub's policies, not by the CatX application.

## Permissions

CatX does not request administrator privileges. It does not access contacts, location, camera, microphone, documents, browser data, or clipboard contents.

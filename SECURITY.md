# Security policy

## Supported version

Security fixes are applied to the latest release.

## Reporting a vulnerability

Please do not open a public issue for a vulnerability that could prevent users from recovering keyboard control. Use GitHub's **Security > Report a vulnerability** feature for this repository. Include the CatX version, Windows version, reproduction steps, and the recovery methods you tried.

## Safety guarantees and boundaries

- CatX blocks ordinary keyboard messages only while its process and hook are active.
- The configured recovery chord removes the hook before updating the interface.
- The mouse remains active.
- Windows' secure `Ctrl + Alt + Delete` sequence is not interceptable by CatX.
- CatX does not require elevation, install a driver, run a service, or make network requests.

If CatX exits unexpectedly, Windows removes the process-owned hook and keyboard input returns.

# Safety and recovery

Always test the selected recovery shortcut immediately after enabling CatX.

## Recovery options

1. Hold the complete recovery shortcut shown in CatX.
2. Use the mouse to select **Disable guard with mouse**.
3. If CatX is minimized, right-click its cat icon in the Windows notification area and choose **Disable keyboard guard**.
4. Close CatX; Windows removes the process-owned keyboard hook.
5. Press `Ctrl + Alt + Delete` to use the Windows secure screen if necessary.

## Safety boundaries

- CatX never blocks the mouse.
- CatX cannot block the Windows secure-attention sequence.
- CatX does not install a driver or Windows service.
- CatX does not keep the keyboard guarded after the process exits.
- CatX requires no administrator privileges.

Report a security-sensitive recovery failure privately through [GitHub Security Advisories](https://github.com/digitalrcs/CatX/security/advisories/new), not as a public issue.

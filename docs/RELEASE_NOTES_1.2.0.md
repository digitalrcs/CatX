# CatX 1.2.0

Cats now share the desktop with social greetings, separate nap spots, and a cheese-fetching mouse.

- Ctrl-click up to eight names in a plain multi-select dropdown. The arrow closes the list reliably; selected names remain highlighted.
- Cats can nap anywhere, reserving separate spots while allowing nearby sleepers.
- One shared mouse finds visible cheese, carries it home, and sometimes evades the cats. Cats can run and catch it, with playful catch and delivery feedback.
- The mouse hole stays stationary without jitter as the mouse travels.
- Removed the Preview cats button and preview-action selector. Cats use natural behavior while the guard is active. The optional developer `--review` launcher remains keyboard-safe.
- Updated the bundled PDF and Markdown user guides for these controls and behaviors.

## Downloads

Download **CatX-Windows11-1.2.0.zip**, extract it, and run **Setup.exe**, or use **CatX-Setup-1.2.0.exe** directly.

For a portable copy, download **CatX-win-x64.zip** and run **CatX.exe**. No separate .NET or Blender installation is needed. Close any older CatX instance before starting this version.

The release includes **SHA256SUMS.txt**. This community build remains unsigned. Realistic-cat assets retain their original third-party licensing; see ANIMATED_CATS.md.

Validation: 31 automated tests passed, plus WPF dropdown, shared-mouse lifecycle, stationary-hole pixel checks, and rendered cheese/catch/delivery/nap reviews.

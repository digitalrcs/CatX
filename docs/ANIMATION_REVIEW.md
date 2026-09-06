# CatX animation preview

CatX 1.2.0 includes the reviewed sleep/groom animations, longer mouse excursions from random desktop locations, and multiple independently styled cats.

The main window has no Preview cats button or preview-action selector. In normal use, Ctrl-click names in the cat dropdown and enable the keyboard guard to display those cats.

For keyboard-safe developer review, run `CatX.exe --review` or **Start-Animation-Review.cmd**. This automatically shows three cats using natural behavior, disables the guard and automatic locking, and leaves saved preferences unchanged. Close the application to end the review. No Blender is required.

## Things to review

- Sleep: cats reserve separate spots throughout the screen. The realistic cat keeps its head and body settled; only the very last tail segment twitches intermittently. Original cats also have a separate animated tail tip.
- Groom: realistic cats use the supplied model's simpler paw-washing motion. The custom ear reach and its shoulder/leg bending have been removed. Original animated cats retain their approved paw/tongue/ear-wiping poses unchanged.
- Mouse: exactly one mouse serves the whole group. Watch it find and carry cheese, dodge chasing cats, and return home. Some trips end with a cat catching the mouse.
- Multiple cats: different styles should appear together, start apart, and stop together. Each has independent activity timing. More realistic cats use more memory and rendering resources; reduce the count on smaller PCs.

The application is the best place to check actual on-screen size and performance. The release remains unsigned. Earlier local review bundles also contain looping GIF previews; those GIFs are not required by the application.

# Animated cats in CatX 1.1.1

Select an original cat or one of seven Realistic coats, then use Preview cat or enable the keyboard guard. Preview leaves typing available and pauses auto-lock. Stop preview before changing the selected coat. Click Stop preview, disable the guard, or exit CatX to remove both the cat and any visiting mouse.

Cats walk and look around, sit, wash their faces, and lie down to nap in the lower corners. The first bedtime is scheduled after about 40 seconds, plus time to walk to the nearest lower corner and lie down. Naps last 30–45 seconds; after waking, the next bedtime is scheduled in 60–90 seconds. Toy visits cannot interrupt bedtime. Shake the cursor rapidly back and forth to wake the cat and invite a short playful chase. Straight cursor movements are not a play signal.

A gray toy mouse occasionally emerges from an arched cartoon mouse hole at a random working-area location, strolls farther out (typically 340–640 logical pixels on roomy displays) at 80 logical pixels per second, pauses to sniff, and returns into that same hole. The cat approaches at a slower stalking pace and stays clear of the return path. Playful cursor chasing takes priority over toy visits. Disabling toys, closing the cat, or changing the monitor geometry cancels the visit immediately.

The two checkboxes independently enable cursor play and toy mouse visits. Cursor positions are used briefly in memory and are never recorded or sent anywhere. All overlays are click-through and never move or click the actual pointer.

The cat stays on the monitor where the cursor was when the overlay started, inside its working area above the taskbar. Start a new preview on another monitor to move the companion there. This also avoids crossing empty gaps between monitors. Movement and animation use elapsed time with smooth acceleration; a delayed frame does not cause a large jump.

## Blender rendering

Source supplied by the application owner: `E:\Data\BlenderCats\Cat_full\full\Cat_male_IP_animations.blend`, with the adjacent `textures` folder. The source file is not modified or copied into CatX. It contains a rig and existing in-place actions. The renderer uses the highest-detail mesh, repaired texture connections, normal mapping, studio lighting, a fixed orthographic camera, and transparent 256 by 224 PNGs.

Actions: Walk_forward_IP, Run_forward_IP, Idle_1, Sit_idle_1, and Lie, plus CatX_Groom_PawWash and CatX_Sleep_TailTip. `tools/catx_animation_actions.py` freezes all head/body transforms for sleep and moves only tail_06 (20 fps). Realistic grooming uses the supplied Sit_wash action baked without custom limb offsets or ear-reaching IK targets (15 fps); the rejected ear-wiping motion has been removed. Original vector cats retain their approved paw/ear grooming. Lie contains a complete down-and-up sequence; runtime holds its resting midpoint for lying down and plays the first half backward over two seconds for waking. All seven coat textures share the same actions and framing. Exactly one complete pose is drawn at a time, without transparent frame blending or crossfades that duplicate legs. Screen movement is still smoothly interpolated; this is not live 3D rendering.

Use **Cats on screen** for 1–8 independently styled companions. Use **Preview action** for immediate in-place Sleep/Groom/Walk/Run playback or a Mouse visit. See [Animation review](ANIMATION_REVIEW.md) for safe preview instructions.

From the repository root, render with Blender installed:

```powershell
& 'C:\Program Files\Blender Foundation\Blender 5.2\blender.exe' -b 'E:\Data\BlenderCats\Cat_full\full\Cat_male_IP_animations.blend' --python tools\render_realistic_cats.py -- --output src\CatX\Assets\Realistic
```

Use a fresh output directory when changing camera, lighting, frame counts, or renderer settings. Existing PNGs are skipped to resume an interrupted render. `--preview --coats Tabby` produces selected poses in a separate output directory for initial camera checks; do not use preview output for a release. Build only after the full render finishes and asset validation succeeds.

Runtime loads just the selected coat from embedded resources and releases it when the overlay closes. No Blender runtime, source model, external texture paths, or network connection is needed on the user's computer. The model and textures remain in the original source folder.

## Asset rights

These realistic cats are derived from the owner-supplied third-party model. No asset license or author information was present alongside the supplied files. The CatX MIT license does not grant rights to these assets; consult the original model/texture license before reusing or redistributing the rendered output. Original vector cats and CatX source remain available as before.

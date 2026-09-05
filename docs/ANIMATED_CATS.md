# Animated cats in CatX 1.1.0

Select an original cat or one of seven Realistic coats, then use Preview cat or enable the keyboard guard. Preview leaves typing available and pauses auto-lock. Stop preview before changing the selected coat. Click Stop preview, disable the guard, or exit CatX to remove both the cat and any visiting mouse.

Cats walk and look around, sit, wash their faces, and lie down to nap in the lower corners. Shake the cursor rapidly back and forth to invite a short playful chase. Straight cursor movements are not a play signal. A gray toy mouse occasionally runs around the monitor and disappears before contact. Playful cursor chasing takes priority over chasing the toy. Sleeping cats wake before chasing.

The two checkboxes independently enable cursor play and toy mouse visits. Cursor positions are used briefly in memory and are never recorded or sent anywhere. All overlays are click-through and never move or click the actual pointer.

The cat stays on the monitor where the cursor was when the overlay started, inside its working area above the taskbar. Start a new preview on another monitor to move the companion there. This also avoids crossing empty gaps between monitors. Movement and animation use elapsed time with smooth acceleration; a delayed frame does not cause a large jump.

## Blender rendering

Source supplied by the application owner: `E:\Data\BlenderCats\Cat_full\full\Cat_male_IP_animations.blend`, with the adjacent `textures` folder. The source file is not modified or copied into CatX. It contains a rig and existing in-place actions. The renderer uses the highest-detail mesh, repaired texture connections, normal mapping, studio lighting, a fixed orthographic camera, and transparent 256 by 224 PNGs.

Actions: Walk_forward_IP, Run_forward_IP, Idle_1, Sit_idle_1, Sit_wash, Sleep, and Lie. Lie contains a complete down-and-up sequence; runtime holds its resting midpoint for lying down and plays the first half backward over two seconds for waking. All seven coat textures share the same actions and framing. Frames are blended at display refresh time and poses briefly crossfade; this is not live 3D rendering.

From the repository root, render with Blender installed:

```powershell
& 'C:\Program Files\Blender Foundation\Blender 5.2\blender.exe' -b 'E:\Data\BlenderCats\Cat_full\full\Cat_male_IP_animations.blend' --python tools\render_realistic_cats.py -- --output src\CatX\Assets\Realistic
```

Use a fresh output directory when changing camera, lighting, frame counts, or renderer settings. Existing PNGs are skipped to resume an interrupted render. `--preview --coats Tabby` produces selected poses in a separate output directory for initial camera checks; do not use preview output for a release. Build only after the full render finishes and asset validation succeeds.

Runtime loads just the selected coat from embedded resources and releases it when the overlay closes. No Blender runtime, source model, external texture paths, or network connection is needed on the user's computer. The model and textures remain in the original source folder.

## Asset rights

These realistic cats are derived from the owner-supplied third-party model. No asset license or author information was present alongside the supplied files. The CatX MIT license does not grant rights to these assets; consult the original model/texture license before reusing or redistributing the rendered output. Original vector cats and CatX source remain available as before.

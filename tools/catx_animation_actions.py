"""Author CatX review actions on the supplied rig, without editing its source file."""
import math
import bpy
from mathutils import Matrix


def author_actions(rig, scene):
    for track in rig.animation_data.nla_tracks:
        track.mute = True

    def capture(action, frame):
        rig.animation_data.action = bpy.data.actions[action]
        scene.frame_set(frame)
        bpy.context.view_layer.update()
        return ({b.name: b.matrix_basis.copy() for b in rig.pose.bones},
                {b.name: b.matrix.copy() for b in rig.pose.bones})

    sleep, sleep_world = capture('Sleep', 50)
    # Keep the artist-supplied wash motion intact. The rejected custom near-paw
    # ear reach forced an implausible shoulder/elbow pose; do not recreate it.
    source_wash = [capture('Sit_wash', frame)[0] for frame in range(101)]
    created = {}

    def restore(pose):
        for bone in rig.pose.bones:
            bone.matrix_basis = pose[bone.name]
        bpy.context.view_layer.update()

    def key_pose(frame):
        for bone in rig.pose.bones:
            bone.rotation_mode = 'QUATERNION'
            for prop in ('location', 'rotation_quaternion', 'scale'):
                bone.keyframe_insert(data_path=prop, frame=frame, group=bone.name)

    action = bpy.data.actions.new('CatX_Sleep_TailTip')
    rig.animation_data.action = action
    # Freeze the settled sleep pose, including every parent of the head. Animate
    # only the final tail bone, sideways in the floor plane, with a quiet pause.
    for frame in range(121):
        restore(sleep)
        t = frame / 30
        pulse = math.sin(math.pi * t / 1.8)**2 if t < 1.8 else 0
        angle = math.radians(16) * pulse * math.sin(t * math.tau * 1.6)
        base = sleep_world['tail_06']
        pivot = Matrix.Translation(base.translation)
        rig.pose.bones['tail_06'].matrix = pivot @ Matrix.Rotation(angle, 4, 'Z') @ pivot.inverted() @ base
        key_pose(frame)
    created['sleep'] = action.name

    action = bpy.data.actions.new('CatX_Groom_PawWash')
    rig.animation_data.action = action
    # Bake every channel to prevent an unkeyed control inheriting the sleep pose
    # when clips change. No manual IK targets, mirroring, or limb offsets.
    for frame, pose in enumerate(source_wash):
        restore(pose)
        key_pose(frame)
    created['groom'] = action.name
    return created

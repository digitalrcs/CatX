"""Read-only Blender rig inspection for CatX animation authoring."""
import bpy, json
rig = bpy.data.objects['Arm_Cat']
print('CATX_BONES=' + json.dumps([{'name': b.name, 'parent': b.parent.name if b.parent else None,
    'head': list(b.head_local), 'tail': list(b.tail_local)} for b in rig.data.bones]))
print('CATX_CONSTRAINTS=' + json.dumps([{'name': b.name, 'constraints': [c.type for c in b.constraints]}
    for b in rig.pose.bones if b.constraints]))
print('CATX_ACTIONS=' + json.dumps([{'name': a.name, 'range': list(a.frame_range)} for a in bpy.data.actions]))
for action_name in ['Sleep','Sit_wash']:
    rig.animation_data.action = bpy.data.actions[action_name]
    for frame in [0,25,50,75,100]:
        bpy.context.scene.frame_set(frame)
        print('CATX_POSE=' + json.dumps({'action': action_name, 'frame': frame, 'bones': [
            {'name': b.name, 'location': list(b.location), 'rotation': list(b.rotation_quaternion),
             'head': list(b.head), 'tail': list(b.tail)} for b in rig.pose.bones]}))

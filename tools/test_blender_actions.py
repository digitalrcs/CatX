"""Run in background Blender against the source model; checks authored rig motion."""
import bpy, sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).parent))
from catx_animation_actions import author_actions
rig=bpy.data.objects['Arm_Cat']
scene=bpy.context.scene
rig.animation_data.action=bpy.data.actions['Sit_wash']
source_limbs={}
limbs=['shoulder_blade.L','hip_f.L','leg_f.L','foot_f.L','shoulder_blade.R','hip_f.R','leg_f.R','foot_f.R']
for frame in [0,25,50,75,100]:
    scene.frame_set(frame)
    source_limbs[frame]={name:rig.pose.bones[name].matrix.copy() for name in limbs}
actions=author_actions(rig,scene)
rig.animation_data.action=bpy.data.actions[actions['sleep']]
heads=[]; tips=[]
for frame in range(121):
    scene.frame_set(frame)
    heads.append(rig.pose.bones['head'].matrix.copy())
    tips.append(rig.pose.bones['tail_06'].tail.copy())
assert max(abs(h[i][j]-heads[0][i][j]) for h in heads for i in range(4) for j in range(4)) < 1e-6, 'Sleep head moved'
assert max((t-tips[0]).length for t in tips) > .003, 'Tail tip did not twitch'
assert (tips[-1]-tips[0]).length < 1e-6, 'Sleep loop is not seamless'
rig.animation_data.action=bpy.data.actions[actions['groom']]
for frame, expected in source_limbs.items():
    scene.frame_set(frame)
    for name,matrix in expected.items():
        actual=rig.pose.bones[name].matrix
        assert max(abs(actual[i][j]-matrix[i][j]) for i in range(4) for j in range(4)) < .0001, (frame,name,'Grooming limb differs from supplied animation')
assert actions['groom']=='CatX_Groom_PawWash'
print('PASS: stationary sleep head, moving tail tip, closed sleep loop; grooming shoulders/legs match the supplied wash action without the custom ear reach.')

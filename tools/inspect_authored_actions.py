import bpy, sys, json
from pathlib import Path
sys.path.insert(0, str(Path(__file__).parent))
from catx_animation_actions import author_actions
rig=bpy.data.objects['Arm_Cat']
actions=author_actions(rig,bpy.context.scene)
for key in ['groom','sleep']:
    rig.animation_data.action=bpy.data.actions[actions[key]]
    for f in [0,45,120]:
        bpy.context.scene.frame_set(f)
        print('AUTHORED',key,f,json.dumps({n:{'head':list(rig.pose.bones[n].head),'loc':list(rig.pose.bones[n].location)} for n in ['head','Helper_foot_f.L','foot_f.L','leg_f.L','tail_06']}))
for b in rig.pose.bones:
    for c in b.constraints:
        print('CONSTRAINT',b.name,c.type,c.influence,getattr(c,'subtarget',''),getattr(c,'chain_count',''))

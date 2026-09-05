import bpy, json
from mathutils import Vector

result = {"objects": [], "actions": [], "images": []}
for obj in bpy.data.objects:
    result["objects"].append({"name":obj.name,"type":obj.type,"hidden":obj.hide_render,"dimensions":list(obj.dimensions),"location":list(obj.location),"materials":[m.name for m in obj.data.materials] if obj.type=="MESH" else [], "action":obj.animation_data.action.name if obj.animation_data and obj.animation_data.action else None})
for action in bpy.data.actions:
    result["actions"].append({"name":action.name,"frames":list(action.frame_range)})
for img in bpy.data.images:
    result["images"].append({"name":img.name,"filepath":img.filepath})
print("CATX_INSPECTION="+json.dumps(result))

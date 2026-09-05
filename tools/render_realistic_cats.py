"""Run with Blender -b <Cat_male_IP_animations.blend> --python this_file -- --output <dir>.
The purchased/source model stays outside CatX. Only rendered frames are packaged.
"""
import argparse, json, math, os, sys
from pathlib import Path
import bpy
from mathutils import Vector

parser = argparse.ArgumentParser()
parser.add_argument('--output', required=True)
parser.add_argument('--preview', action='store_true')
parser.add_argument('--coats', default='Tabby,Orange,White,Grey,Tuxedo,Black,Bicolor')
parser.add_argument('--clips', default='walk,run,idle,sit,groom,sleep,lie')
parser.add_argument('--save-blend', help='Optional separate review .blend; never use the source path.')
parser.add_argument('--indices', help='Comma-separated sparse frame indices for pose review only.')
args = parser.parse_args(sys.argv[sys.argv.index('--')+1:])
output = Path(args.output).resolve()
output.mkdir(parents=True, exist_ok=True)
scene = bpy.context.scene
rig = bpy.data.objects['Arm_Cat']
sys.path.insert(0, str(Path(__file__).resolve().parent))
from catx_animation_actions import author_actions
authored = author_actions(rig, scene)
mesh = bpy.data.objects['Cat_LOD0']
for obj in list(bpy.data.objects):
    if obj.type == 'MESH':
        obj.hide_render = obj != mesh
        obj.hide_set(obj != mesh)
if rig.animation_data:
    for track in rig.animation_data.nla_tracks:
        track.mute = True

textures = Path(bpy.data.filepath).parent / 'textures'
material = bpy.data.materials.new('CatX textured fur')
material.use_nodes = True
nodes = material.node_tree.nodes
shader = nodes.get('Principled BSDF')
shader.inputs['Roughness'].default_value = .83
texture = nodes.new('ShaderNodeTexImage')
material.node_tree.links.new(texture.outputs['Color'], shader.inputs['Base Color'])
normal = nodes.new('ShaderNodeTexImage')
normal.image = bpy.data.images.load(str(textures / 'Cat_Normal.tif'), check_existing=True)
normal.image.colorspace_settings.name = 'Non-Color'
normal_map = nodes.new('ShaderNodeNormalMap')
normal_map.inputs['Strength'].default_value = .55
material.node_tree.links.new(normal.outputs['Color'], normal_map.inputs['Color'])
material.node_tree.links.new(normal_map.outputs['Normal'], shader.inputs['Normal'])
mesh.data.materials.clear()
mesh.data.materials.append(material)

scene.render.engine = 'CYCLES'
scene.cycles.samples = 12
scene.cycles.use_denoising = True
try:
    prefs = bpy.context.preferences.addons['cycles'].preferences
    prefs.compute_device_type = 'OPTIX'
    prefs.get_devices()
    for device in prefs.devices: device.use = device.type != 'CPU'
    if any(d.use for d in prefs.devices): scene.cycles.device = 'GPU'
except Exception as error:
    print('GPU selection:', error)
scene.render.resolution_x = 256
scene.render.resolution_y = 224
scene.render.resolution_percentage = 100
scene.render.film_transparent = True
scene.render.image_settings.file_format = 'PNG'
scene.render.image_settings.color_mode = 'RGBA'
scene.render.image_settings.compression = 40
scene.render.fps = 30
scene.render.use_persistent_data = True
scene.view_settings.view_transform = 'AgX'
scene.world.use_nodes = True
scene.world.node_tree.nodes.get('Background').inputs[0].default_value = (.55,.60,.7,1)
scene.world.node_tree.nodes.get('Background').inputs[1].default_value = .7

def aim(obj, target):
    obj.rotation_euler = (Vector(target)-obj.location).to_track_quat('-Z','Y').to_euler()

# Side view with a small frontal angle, all clips use one fixed camera and scale.
bpy.ops.object.camera_add(location=(1.4,-.65,.70))
camera = bpy.context.object
camera.data.type = 'ORTHO'
camera.data.ortho_scale = .90
aim(camera, (0,0,.24))
scene.camera = camera
for location, energy, size in [((1,-1,2),110,2),((-1,.4,1),65,1.5)]:
    bpy.ops.object.light_add(type='AREA', location=location)
    light = bpy.context.object
    light.data.energy = energy
    light.data.shape = 'DISK'
    light.data.size = size
    aim(light, (0,0,.2))

clips = {
    'walk': ('Walk_forward_IP', 0,24,24,True),
    'run': ('Run_forward_IP', 0,14,14,True),
    'idle': ('Idle_1', 0,100,30,True),
    'sit': ('Sit_idle_1', 0,140,42,True),
    'groom': (authored['groom'], 0,100,50,True),
    'sleep': (authored['sleep'], 0,120,80,True),
    'lie': ('Lie', 0,150,45,False),
}
coats = {'Tabby':'tiger','Orange':'orang','White':'white','Grey':'grey','Tuxedo':'Bl_wt','Black':'black','Bicolor':'Bl_wt2'}
manifest = {'width':256,'height':224,'facing':'left','coats':[], 'clips':{}}
for clip,(action,start,end,count,loop) in clips.items():
    manifest['clips'][clip] = {'frames':count,'fps':count/((end-start)/30),'loop':loop}
for coat in args.coats.split(','):
    texture.image = bpy.data.images.load(str(textures / ('Cat_Color_'+coats[coat]+'.tif')), check_existing=True)
    manifest['coats'].append(coat)
    for clip,(action,start,end,count,loop) in clips.items():
        if clip not in args.clips.split(','): continue
        rig.animation_data.action = bpy.data.actions[action]
        folder = output / coat / clip
        folder.mkdir(parents=True, exist_ok=True)
        indices = [0,count//2] if args.preview else range(count)
        if args.indices: indices = [int(i) for i in args.indices.split(',') if int(i) < count]
        for index in indices:
            frame = start + (end-start)*index/(count if loop else count-1)
            scene.frame_set(math.floor(frame), subframe=frame%1)
            path = folder / f'{index:03d}.png'
            if path.exists(): continue
            scene.render.filepath = str(path)
            bpy.ops.render.render(write_still=True)
            print(f'CATX_FRAME {coat}/{clip}/{index}', flush=True)
(output/'manifest.json').write_text(json.dumps(manifest,indent=2))
if args.save_blend:
    save_path = Path(args.save_blend).resolve()
    if save_path == Path(bpy.data.filepath).resolve():
        raise ValueError('Refusing to overwrite the supplied source model.')
    bpy.ops.wm.save_as_mainfile(filepath=str(save_path))

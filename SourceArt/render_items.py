"""Render inventory portraits from the actual game meshes (Blender 4)."""
import bpy,sys
from pathlib import Path
from mathutils import Vector
root=Path(__file__).resolve().parents[1];bpy.ops.wm.open_mainfile(filepath=str(root/'SourceArt/World.blend'))
sc=bpy.context.scene;sc.render.engine='CYCLES';sc.cycles.samples=32;sc.cycles.use_denoising=True;sc.render.resolution_x=sc.render.resolution_y=256;sc.render.resolution_percentage=100;sc.render.film_transparent=True;sc.render.image_settings.file_format='PNG';sc.render.image_settings.color_mode='RGBA';cam=sc.camera;cam.data.type='ORTHO';sc.world.node_tree.nodes['Background'].inputs[1].default_value=.7
out=root/'Assets/Resources/UI/Items';out.mkdir(parents=True,exist_ok=True)
for name in ['Journal','Flashlight','Disc','Handle','Gear0','Gear1','Gear2','Note0']:
 obj=bpy.data.objects['I_'+name];visible={o for o in [obj]+list(obj.children_recursive) if o.type=='MESH'}
 for o in bpy.data.objects:
  if o.type=='MESH':o.hide_render=o not in visible
 points=[o.matrix_world@Vector(c) for o in visible for c in o.bound_box];lo=Vector(tuple(min(p[k] for p in points) for k in range(3)));hi=Vector(tuple(max(p[k] for p in points) for k in range(3)));center=(lo+hi)*.5;size=max(hi-lo);cam.location=center+Vector((.8,-1.3,1.6))*max(size,1);cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.ortho_scale=size*1.55;sc.render.filepath=str(out/(name+'.png'));bpy.ops.render.render(write_still=True)
print('Rendered 8 inventory portraits')

"""Blender 4: build the cohesive art upgrade while preserving all gameplay anchors."""
import sys,os,math,random,json
sys.path.append('/usr/lib/python3/dist-packages')
import bpy,bmesh
from mathutils import Vector
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
exec(compile((ROOT/'SourceArt/build_world.py').read_text().split('# checkpoint first')[0],str(ROOT/'SourceArt/build_world.py'),'exec'),globals())
ROOT=Path(ROOT);random.seed(47)
remove=('C_Masonry','C_Coping','C_Paving','C_Pillar','C_Base','Capital','C_Trunk','Canopy','Branch','C_Rock','LeafBlade','Vine','TentCanvas','C_Crate','C_TableLeg','C_TableTop','C_Lintel','C_FinalLintel','SanctuaryRoof')
for o in list(bpy.data.objects):
 if o.name.startswith(remove):bpy.data.objects.remove(o,do_unlink=True)
for name,col in {'Stone':(.49,.43,.32),'Sand':(.43,.35,.24),'Edge':(.64,.56,.39),'Earth':(.24,.25,.13),'Leaf':(.20,.31,.055),'LeafLight':(.37,.43,.09),'LeafDark':(.09,.19,.045),'Wood':(.16,.078,.031),'WoodLight':(.31,.17,.065),'Gold':(.45,.26,.065),'GoldLight':(.80,.56,.18),'Canvas':(.55,.39,.20),'Dark':(.035,.115,.12)}.items():M[name].diffuse_color=(*col,1);M[name].node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=(*col,1)
CACHE={};COUNTS={};texdir=ROOT/'Assets/Resources/Art/Textures';texdir.mkdir(parents=True,exist_ok=True)
def template(source,name):
 key=source+'/'+name
 if key in CACHE:return CACHE[key]
 p=next(p for p in (ROOT/'SourceArt/ThirdParty'/source).glob(name+'.*') if p.suffix in ['.glb','.obj']);before=set(bpy.data.objects)
 if p.suffix=='.glb':bpy.ops.import_scene.gltf(filepath=str(p))
 else:bpy.ops.wm.obj_import(filepath=str(p),forward_axis='NEGATIVE_Z',up_axis='Y')
 meshes=[o for o in set(bpy.data.objects)-before if o.type=='MESH'];bpy.ops.object.select_all(action='DESELECT')
 for o in meshes:o.select_set(True)
 bpy.context.view_layer.objects.active=meshes[0]
 if len(meshes)>1:bpy.ops.object.join()
 o=bpy.context.object;world=o.matrix_world.copy();o.parent=None;o.matrix_world=world;bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
 vs=[v.co.copy() for v in o.data.vertices];lo=Vector(tuple(min(v[k] for v in vs) for k in range(3)));hi=Vector(tuple(max(v[k] for v in vs) for k in range(3)));size=hi-lo;center=Vector(((lo.x+hi.x)/2,(lo.y+hi.y)/2,lo.z))
 for v in o.data.vertices:v.co-=center
 if source=='KayKit':
  kind='Stone' if name.startswith(('wall','column','floor')) else 'Props';mat=bpy.data.materials.get('KayKit_'+kind)
  if not mat:
   mat=o.data.materials[0].copy();mat.name='KayKit_'+kind;bs=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED');tint=(1,.86,.61,1) if kind=='Stone' else (.90,.89,.78,1);mat.diffuse_color=tint
   for n in list(mat.node_tree.nodes):
    if n.type=='TEX_IMAGE' and n.image:
     n.image.filepath_raw=str(texdir/'KayKit_dungeon_texture.png');n.image.file_format='PNG';n.image.save()
     links=list(bs.inputs['Base Color'].links)
     if links:
      color=links[0].from_socket;mix=mat.node_tree.nodes.new('ShaderNodeMixRGB');mix.blend_type='MULTIPLY';mix.inputs[0].default_value=1;mix.inputs[2].default_value=tint;mat.node_tree.links.new(color,mix.inputs[1]);mat.node_tree.links.new(mix.outputs[0],bs.inputs['Base Color'])
   bs.inputs['Roughness'].default_value=.78
  o.data.materials.clear();o.data.materials.append(mat)
  for f in o.data.polygons:f.material_index=0
 else:
  for i,mat in enumerate(o.data.materials):
   n=mat.name.lower();target='LeafLight' if 'grass' in n else 'Leaf' if 'leaf' in n else 'Wood' if 'bark' in n else 'Sand' if 'dirt' in n else 'Edge';o.data.materials[i]=M[target]
 if source=='KenneyNature' and name.startswith('tree'):
  foliage={i for i,m in enumerate(o.data.materials) if m and m.name.startswith('Leaf')};leafmesh=o.data.copy()
  for mesh,keep_leaf in [(leafmesh,True),(o.data,False)]:
   bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.delete(bm,geom=[f for f in bm.faces if (f.material_index in foliage)!=keep_leaf],context='FACES');loose=[v for v in bm.verts if not v.link_faces]
   if loose:bmesh.ops.delete(bm,geom=loose,context='VERTS')
   bm.to_mesh(mesh);bm.free()
  leaf=bpy.data.objects.new('Soft foliage',leafmesh);bpy.context.collection.objects.link(leaf);bpy.ops.object.select_all(action='DESELECT');leaf.select_set(True);bpy.context.view_layer.objects.active=leaf;sub=leaf.modifiers.new('Curved crowns','SUBSURF');sub.levels=2;bpy.ops.object.modifier_apply(modifier=sub.name)
  for f in leaf.data.polygons:f.use_smooth=True
  o.select_set(True);bpy.context.view_layer.objects.active=o;bpy.ops.object.join()
 if source=='KenneyNature' and name.startswith('rock'):
  bevel=o.modifiers.new('Soft silhouette','BEVEL');bevel.width=.075;bevel.segments=3;bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=bevel.name)
  for f in o.data.polygons:f.use_smooth=True
 data=o.data.copy();CACHE[key]=(data,size)
 for obj in set(bpy.data.objects)-before:bpy.data.objects.remove(obj,do_unlink=True)
 return data,size

def asset(source,name,p,size=None,scale=1,angle=0,solid=False):
 data,dim=template(source,name);key=source+'/'+name;COUNTS[key]=COUNTS.get(key,0)+1;o=bpy.data.objects.new(('C_' if solid else 'D_')+source+'_'+name,data);bpy.context.collection.objects.link(o);o.location=p;o.rotation_euler.z=angle;o.scale=tuple(size[i]/dim[i] for i in range(3)) if size else (scale,scale,scale);return o

def masonry(a,b,h):
 ax,ay=a;bx,by=b;L=math.hypot(bx-ax,by-ay);angle=math.atan2(by-ay,bx-ax);n=max(1,round(L/3.8));w=L/n
 for i in range(n):asset('KayKit','wall_cracked' if i%3==1 else 'wall',(ax+(i+.5)/n*(bx-ax),ay+(i+.5)/n*(by-ay),0),size=(w+.02,.62,h+random.uniform(-.04,.04)),angle=angle,solid=True)
for a,b,h in [((-22,-12),(-22,28),4),((23,-12),(23,28),4),((-22,28),(23,28),4),((-22,-12),(-1.8,-12),4.5),((1.8,-12),(23,-12),4.5),((9,-12),(9,2),3),((9,5.5),(9,28),3),((-19,9),(-9,9),3.8),((-19,0),(-19,9),3.8),((10.5,23),(22.5,23),3.8),((22.5,14),(22.5,23),3.8),((-4.5,15),(-1.7,15),4.6),((1.7,15),(4.5,15),4.6),((-4.5,15),(-4.5,25),4.6),((4.5,15),(4.5,25),4.6),((-4.5,25),(4.5,25),4.6)]:masonry(a,b,h)
for x in range(-8,9,2):
 for y in range(-10,15,2):
  if abs(x)<=2 and y in [2,4]:continue
  n=random.choice(['floor_tile_small','floor_tile_small','floor_tile_small_broken_A','floor_tile_small_weeds_A']);asset('KayKit',n,(x+random.uniform(-.05,.05),y+random.uniform(-.05,.05),-.03),size=(1.96,1.96,.26 if 'weeds' in n else .13),angle=random.choice([0,math.pi/2,math.pi,3*math.pi/2]),solid=True)
for y in range(-24,-12,2):
 for x in [-1,1]:asset('KayKit','floor_tile_small_broken_A',(x+random.uniform(-.1,.1),y,.01),size=(1.7,1.8,.10),angle=random.choice([0,math.pi/2]),solid=True)
def arch(cx,y,width=3.6):
 radius=width/2+.12;spring=2.85
 for x in [cx-radius-.28,cx+radius+.28]:
  asset('KayKit','column',(x,y,0),size=(1.05,1.2,4.9),solid=True)
  for z in [.3,3.4,4.6]:box('C_ArchCollar',(x,y,z),(1.22,1.32,.20),'Edge',.06)
 for i in range(13):
  a=(i+.5)*math.pi/13;ob=box('C_ArchStone',(cx+radius*math.cos(a),y,spring+radius*math.sin(a)),(.48,.95,.66),random.choice(['Stone','Sand','Edge']),.09);ob.rotation_euler.y=math.pi/2-a
 box('C_Keystone',(cx,y-.08,spring+radius+.12),(.55,1.1,.9),'Edge',.1)
 for x in [cx-3.3,cx+3.3]:asset('KayKit','column',(x,y,0),size=(1.25,1.3,5.8),solid=True);box('TowerCrown',(x,y,5.7),(1.5,1.55,.38),'Edge',.08)
arch(0,-12);arch(0,15,3.3)
for i,(w,d,z) in enumerate([(10.5,11.5,4.7),(8.8,9.8,5.12),(6.9,7.9,5.54),(5,6,5.96),(3.2,4.2,6.38)]):box('SanctuaryCornice',(0,20,z),(w,d,.42),'Sand' if i%2 else 'Stone',.11)
g=group('SunCrest',(0,14.5,5.9));cylinder('SunDisc',(0,0,0),.64,.1,'Gold',g,(math.pi/2,0,0));glyph(2,(0,-.07,0),.52,g)
for x in [-3.2,3.2]:
 for y in [18,23]:asset('KayKit','column',(x,y,0),size=(.9,.9,4.4),solid=True)
for p,size in [((-4,-16,0),(2.7,1.2,1.03)),((15,15.8,0),(2.3,1.25,1.03))]:asset('KayKit','table_medium',p,size=size,solid=True)
for x,y,s in [(-7.3,-16.7,.48),(-8,-20,.4),(21,16,.45),(-17,7,.35)]:asset('KayKit','box_stacked',(x,y,0),scale=s,angle=.15,solid=True)
for x,y,a in [(-6,-17,.4),(16.8,16.8,1.7)]:asset('KayKit','chair',(x,y,0),scale=.9,angle=a,solid=True)
for x,y in [(-3.7,-19.5),(20,21),(11.5,20)]:asset('KayKit','chest',(x,y,0),scale=.55,angle=.15,solid=True)
for x,y in [(-7,-14.5),(-17,7.5),(21,14.7),(5.6,14)]:asset('KayKit','barrel_large_decorated',(x,y,0),scale=.42,solid=True)
asset('KayKit','barrel_small_stack',(20,22.2,0),scale=.65,solid=True);asset('KayKit','bed_floor',(-5,-20,0),scale=.65,angle=math.pi/2);asset('KayKit','bottle_A_labeled_green',(-4.9,-16,1.04),scale=.32);asset('KayKit','candle_triple',(15.9,15.8,1.05),scale=.35)
vs=[];N=14
for j in range(N+1):
 for i in range(N+1):
  x=-8.6+i/N*7.2;y=-22.4+j/N*5.5;u=abs((x+5)/3.6);z=3.55-u*1.35-.22*math.sin(math.pi*j/N)*math.sin(math.pi*u);vs.append((x,y,z))
fs=[]
for j in range(N):
 for i in range(N):a=j*(N+1)+i;fs.append((a,a+1,a+N+2,a+N+1))
o=raw('DrapedCanvas',vs,fs,'Canvas');o.modifiers.new('Thickness','SOLIDIFY').thickness=.025
for x in [-8.6,-1.4]:
 for y in [-22.4,-16.9]:cylinder('C_CanopyPole',(x,y,1.1),.055,2.2,'Wood');tube('GuyRope',[(x,y,2.2),(x+(x+5)*.28,y-.5,.08)],.018,'Paper')
for i in range(42):
 side=-1 if i%2 else 1;x=side*random.uniform(24,29);y=random.uniform(-26,32);asset('KenneyNature',random.choice(['tree_detailed','tree_oak','tree_plateau']),(x,y,-.15),size=(random.uniform(3.8,6),random.uniform(3.8,6),random.uniform(6,9)),angle=random.random()*6.28)
for x,y,h in [(-11,-20,8),(7,-19,7.5),(-20,-5,7),(-20,16,8),(7,24,8),(-7,25,6.5)]:asset('KenneyNature','tree_detailed',(x,y,0),size=(5,5,h),angle=.3,solid=True)
for i in range(18):asset('KenneyNature','tree_oak',(-28+i*3.3,random.uniform(29,33),-.15),size=(7,7,random.uniform(9,12)),angle=random.random()*6.28)
for i in range(16):
 a=i/16*math.tau;asset('KenneyNature','rock_tallC',(math.cos(a)*random.uniform(31,40),4+math.sin(a)*random.uniform(34,42),-1),size=(random.uniform(7,14),random.uniform(7,12),random.uniform(7,12)),angle=a)
for x,y,s in [(-9,-14,2.4),(7,-14,2),(-8,-22,1.5),(6,-23,2),(-20,11,2),(-18,-3,1.3),(7,13,1.3),(21,10,1.4)]:asset('KenneyNature','rock_largeA',(x,y,0),size=(s,s*.8,s*.65),angle=random.random()*6,solid=True)
for i in range(240):
 x=random.uniform(-21,22);y=random.uniform(-25,27)
 if abs(x)<3 or (abs(x)<8 and -10<y<14) or (10<x<21 and 2<y<22) or (-18<x<-9 and 0<y<9):continue
 asset('KenneyNature',random.choice(['plant_bushDetailed','grass_large','grass_leafsLarge']),(x,y,.02),scale=random.uniform(1,2.3),angle=random.random()*6.28)
for x,y in [(-3,-13),(3,-13),(-6,14),(6,14),(-9,-9),(8,-8),(-9,11),(-19,1),(21,13)]:
 asset('KenneyNature','plant_bushSmall',(x,y,0),size=(1.4,1.1,.9))
 for k in range(5):asset('KenneyNature','grass_leafsLarge',(x+random.uniform(-1,1),y+random.uniform(-.6,.6),.03),scale=1.9,angle=k)
for x,y,zmax in [(-2.7,-12.6,5),(3.1,-12.6,5),(-17.7,8.6,3.5),(-4.9,14.5,4.5),(4.8,14.5,4.5),(20.7,22.5,3.5)]:
 for strand in range(3):
  pts=[(x+strand*.25+math.sin(k*.65)*.12,y,.15+k*.23) for k in range(int(zmax/.23))];tube('IvyStem',pts,.018,'Wood')
  for k,(xx,yy,zz) in enumerate(pts):
   sign=(-1)**k;raw('IvyLeaf',[(xx,yy,zz),(xx+sign*.20,yy-.06,zz+.20),(xx+sign*.42,yy,zz+.12),(xx+sign*.22,yy-.04,zz-.08),(xx+sign*.20,yy-.12,zz+.07)],[(0,1,4),(1,2,4),(2,3,4),(3,0,4)],random.choice(['Leaf','LeafLight','LeafDark']))
box('SurveyMap',(-4.25,-15.85,1.055),(.65,.65,.012),'Paper',.015)
for k in range(7):tube('MapInk',[(-4.5+k*.07,-16.1,1.065),(-4.45+k*.07,-15.9,1.066),(-4.55+k*.07,-15.65,1.065)],.003,'Dark')
for y in [-.22,.22]:box('BookBinding',(0,y,.09),(.43,.025,.015),'Gold',.005,bpy.data.objects['I_Journal'])
for i in range(3):bpy.data.objects['I_Gear'+str(i)].rotation_euler.x=-math.pi/2
mark=group('DiscSun',parent=bpy.data.objects['I_Disc']);mark.location.z=.06;mark.rotation_euler.x=math.pi/2;glyph(2,(0,0,0),.19,mark)
for i,(x,y,z) in enumerate([(-5,-16,1.5),(-2.8,-12.5,2.2),(2.8,-12.5,2.2),(-16.5,7.5,2.3),(16.5,20.5,2.8),(-3.5,18,2.6),(3.5,23,2.6)]):group('P_Lantern'+str(i),(x,y,z))
group('P_Menu',(3.8,-23,2.1));group('P_MenuLook',(0,-11,2.7))
bs=M['Flame'].node_tree.nodes['Principled BSDF'];bs.inputs['Emission Color'].default_value=(1,.5,.08,1);bs.inputs['Emission Strength'].default_value=2.5
bpy.ops.object.select_all(action='SELECT');bpy.ops.export_scene.fbx(filepath=str(ROOT/'Assets/Resources/Art/World.fbx'),use_selection=True,object_types={'MESH','EMPTY'},apply_unit_scale=True,bake_space_transform=True,add_leaf_bones=False,axis_forward='-Z',axis_up='Y',path_mode='RELATIVE')
(ROOT/'Documentation/geometry.json').write_text(json.dumps({'objects':len(bpy.data.objects),'vertices':sum(len(o.data.vertices) for o in bpy.data.objects if o.type=='MESH'),'anchors':{o.name:list(o.location) for o in bpy.data.objects if o.name.startswith(('I_','A_','P_'))},'source_asset_instances':COUNTS},indent=2))
sc=bpy.context.scene;sc.world.use_nodes=True;sc.world.node_tree.nodes['Background'].inputs[0].default_value=(.58,.72,.86,1);sc.world.node_tree.nodes['Background'].inputs[1].default_value=.65
bpy.ops.object.light_add(type='SUN');sun=bpy.context.object;sun.rotation_euler=(.58,-.38,-.55);sun.data.energy=2;sun.data.angle=.12
for o in list(bpy.data.objects):
 if o.name.startswith('P_Lantern'):
  bpy.ops.object.light_add(type='POINT',location=o.location);l=bpy.context.object;l.data.energy=35;l.data.color=(1,.57,.19);l.data.shadow_soft_size=.3
sc.render.engine='CYCLES';sc.cycles.samples=48;sc.cycles.use_denoising=True;sc.render.resolution_x=1280;sc.render.resolution_y=800;sc.render.resolution_percentage=100;sc.view_settings.view_transform='AgX';sc.view_settings.look='AgX - Medium High Contrast'
bpy.ops.object.camera_add();cam=bpy.context.object;sc.camera=cam;cam.data.lens=27;cam.location=(3,-23,2.2);cam.rotation_euler=(Vector((-1,-12,2.3))-cam.location).to_track_quat('-Z','Y').to_euler();bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'SourceArt/World.blend'))
if os.environ.get('ASTER_SKIP_PREVIEWS')!='1':
 for name,p,t in [('Camp',(3,-23,2.2),(-1,-12,2.3)),('Courtyard',(-4,-8,2.1),(-2,13,3)),('Water',(10,1,2.7),(17,10,1.4)),('Workshop',(12,14,2.3),(17,21,1.8)),('Overview',(33,-35,30),(0,3,1))]:
  cam.location=p;cam.rotation_euler=(Vector(t)-cam.location).to_track_quat('-Z','Y').to_euler();sc.render.filepath=str(ROOT/'Documentation/Previews'/f'{name}.png');bpy.ops.render.render(write_still=True)
print('ART UPGRADE COMPLETE',COUNTS)

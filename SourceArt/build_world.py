import sys
if "/usr/lib/python3/dist-packages" not in sys.path: sys.path.append("/usr/lib/python3/dist-packages")
import bpy,math,random,os,json
from mathutils import Vector
random.seed(71);ROOT=os.path.dirname(os.path.dirname(os.path.abspath(__file__)));OUT=ROOT+'/Assets/Resources/Art';os.makedirs(OUT,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
M={};C={}
for n,c in {'Sand':(.64,.49,.30),'Stone':(.76,.63,.43),'Edge':(.86,.74,.52),'Moss':(.25,.34,.14),'Dark':(.055,.14,.15),'Gold':(.65,.38,.10),'GoldLight':(.90,.64,.23),'Wood':(.28,.15,.07),'WoodLight':(.46,.27,.12),'Canvas':(.76,.52,.25),'Leaf':(.28,.46,.13),'LeafLight':(.45,.59,.19),'LeafDark':(.14,.28,.12),'Earth':(.42,.35,.21),'Water':(.065,.43,.43),'Paper':(.82,.71,.49),'Glass':(.57,.81,.81),'Flame':(1,.65,.17)}.items():
 m=bpy.data.materials.new(n);m.diffuse_color=(*c,1);m.use_nodes=True;b=m.node_tree.nodes.get('Principled BSDF');b.inputs['Base Color'].default_value=(*c,1);b.inputs['Roughness'].default_value=.35 if n.startswith('Gold') else .8;b.inputs['Metallic'].default_value=.5 if n.startswith('Gold') else 0;M[n]=m

def group(n,p=(0,0,0),parent=None):
 o=bpy.data.objects.new(n,None);bpy.context.collection.objects.link(o);o.location=p;o.parent=parent;return o

def raw(n,vs,fs,m,p=(0,0,0),parent=None):
 me=bpy.data.meshes.new(n);me.from_pydata(vs,[],fs);me.update();me.materials.append(M[m]);o=bpy.data.objects.new(n,me);bpy.context.collection.objects.link(o);o.location=p;o.parent=parent;return o

def instance(n,data,p,s=(1,1,1),parent=None):
 o=bpy.data.objects.new(n,data);bpy.context.collection.objects.link(o);o.location=p;o.scale=s;o.parent=parent;return o

def box(n,p,s,m='Stone',b=.06,parent=None):
 key=(tuple(s),m,b)
 if key not in C:
  bpy.ops.mesh.primitive_cube_add(size=1);o=bpy.context.object;o.scale=s;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);mod=o.modifiers.new('Rounded edges','BEVEL');mod.width=b;mod.segments=3;bpy.ops.object.modifier_apply(modifier=mod.name);o.data.materials.append(M[m]);o.data.use_auto_smooth=True;C[key]=o.data.copy();bpy.data.objects.remove(o,do_unlink=True)
 return instance(n,C[key],p,parent=parent)
def cylinder(n,p,r,d,m='Gold',parent=None,rot=None,N=32):
 key=('cyl',r,d,m,N)
 if key not in C:
  vs=[]
  for z,rr in [(-d/2,r*.94),(-d/2+.015,r),(d/2-.015,r),(d/2,r*.94)]:
   for i in range(N):a=i*math.tau/N;vs.append((rr*math.cos(a),rr*math.sin(a),z))
  fs=[tuple(range(N-1,-1,-1)),tuple(range(3*N,4*N))]
  for k in range(3):
   for i in range(N):j=(i+1)%N;fs.append((k*N+i,k*N+j,(k+1)*N+j,(k+1)*N+i))
  o=raw('template',vs,fs,m);C[key]=o.data;bpy.data.objects.remove(o,do_unlink=True)
 o=instance(n,C[key],p,parent=parent)
 if rot:o.rotation_euler=rot
 return o
def blob(n,p,s,m='Leaf',parent=None):
 key=('ico',m)
 if key not in C:
  bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2,radius=1);o=bpy.context.object;o.data.materials.append(M[m]);C[key]=o.data.copy();bpy.data.objects.remove(o,do_unlink=True)
 return instance(n,C[key],p,s,parent)
def tube(n,pts,r,m='GoldLight',parent=None):
 for a,b in zip(pts,pts[1:]):
  a,b=Vector(a),Vector(b);o=cylinder(n,(a+b)/2,r,(b-a).length,m,parent,N=8);o.rotation_euler=(b-a).to_track_quat('Z','Y').to_euler()
def ring(n,p,r,t,m='Gold',parent=None,vertical=True):
 pts=[]
 for i in range(33):a=i*math.tau/32;pts.append((p[0]+r*math.cos(a),p[1]+(0 if vertical else r*math.sin(a)),p[2]+(r*math.sin(a) if vertical else 0)))
 tube(n,pts,t,m,parent)
def glyph(k,p,s=.22,parent=None):
 x,y,z=p
 if k==0:
  pts=[(x+math.sin(i*math.pi/12)*s*.5,y,z+math.cos(i*math.pi/12)*s) for i in range(25)];tube('LeafGlyph',pts,.018,parent=parent);tube('Vein',[(x-s*.2,y,z-s*.7),(x+s*.2,y,z+s*.7)],.016,parent=parent)
 elif k==1:
  for row in [-1,0,1]:tube('Wave',[(x-s+i*s/6,y,z+row*s*.45+math.sin(i*.6)*s*.15) for i in range(13)],.018,parent=parent)
 elif k==2:
  ring('Sun',(x,y,z),s*.45,.018,parent=parent)
  for i in range(8):a=i*math.pi/4;tube('Ray',[(x+math.cos(a)*s*.65,y,z+math.sin(a)*s*.65),(x+math.cos(a)*s,y,z+math.sin(a)*s)],.018,parent=parent)
 else:tube('Mountain',[(x-s,y,z-s*.6),(x,y,z+s),(x+s,y,z-s*.6),(x-s,y,z-s*.6)],.018,parent=parent)
def wall(x1,y1,x2,y2,h=3):
 L=math.hypot(x2-x1,y2-y1);angle=math.atan2(y2-y1,x2-x1);cols=max(1,round(L/1.55));w=L/cols;rows=round(h/.64)
 for row in range(rows):
  for col in range(cols):
   v=(col+.5)*w;o=box('C_Masonry',(x1+math.cos(angle)*v,y1+math.sin(angle)*v,row*.64+.32),(w-.025,.62,.61),random.choice(['Stone','Stone','Sand','Edge']),.06);o.rotation_euler.z=angle
 for col in range(cols):
  v=(col+.5)*w;o=box('C_Coping',(x1+math.cos(angle)*v,y1+math.sin(angle)*v,rows*.64+.12),(w+.04,.80,.24),'Edge',.06);o.rotation_euler.z=angle

def pillar(x,y,h=3.6):
 box('C_Base',(x,y,.16),(1.12,1.12,.32),'Edge',.08)
 for j in range(round((h-.5)/.6)):box('C_Pillar',(x,y,.62+j*.6),(.78,.78,.57),'Stone',.08)
 box('Capital',(x,y,h),(1.15,1.15,.3),'Edge',.07)
def table(x,y,w=2):
 for xx in [-w/2+.1,w/2-.1]:
  for yy in [-.4,.4]:box('C_TableLeg',(x+xx,y+yy,.45),(.14,.14,.9),'Wood',.025)
 for k in range(5):box('C_TableTop',(x,y-.5+(k+.5)*.2,.98),(w,.19,.12),'WoodLight',.025)
def pot(x,y,s=.6):
 prof=[(.0,.28),(.1,.35),(.3,.55),(.7,.5),(.95,.25),(1.05,.28),(1.05,.19),(.9,.18)];vs=[];N=24
 for z,r in prof:
  for i in range(N):a=i*math.tau/N;vs.append((x+s*r*math.cos(a),y+s*r*math.sin(a),s*z))
 fs=[]
 for j in range(len(prof)-1):
  for i in range(N):k=j*N+i;q=j*N+(i+1)%N;fs.append((k,q,q+N,k+N))
 return raw('Pottery',vs,fs,'Canvas')
def lantern(x,y,z):
 cylinder('LanternBase',(x,y,z),.16,.08);cylinder('LanternGlow',(x,y,z+.22),.10,.35,'Flame');cylinder('LanternCap',(x,y,z+.43),.16,.08)
 for i in range(4):a=i*math.pi/2;cylinder('LanternRib',(x+.12*math.cos(a),y+.12*math.sin(a),z+.22),.015,.38)
 ring('Handle',(x,y,z+.55),.09,.016)
def tree(x,y,s=1):
 cylinder('C_Trunk',(x,y,1.5*s),.25*s,3*s,'Wood',N=9)
 for k in range(4):
  a=k*2.4;px=x+math.cos(a)*s;py=y+math.sin(a)*s;tube('Branch',[(x,y,2*s),(px,py,3.5*s)],.09*s,'Wood');blob('Canopy',(px,py,(3.6+(k%2)*.4)*s),(1.5*s,1.35*s,1.2*s),random.choice(['Leaf','LeafLight','LeafDark']))
def wheel(n,p,r=.38):
 g=group(n,p);ring('Rim',(0,0,0),r,.045,parent=g);cylinder('Hub',(0,0,0),.1,.14,'GoldLight',g,(math.pi/2,0,0))
 for i in range(5):a=i*math.tau/5;tube('Spoke',[(0,0,0),(r*math.cos(a),0,r*math.sin(a))],.025,'Gold',g)
 return g
def lever(n,p):
 g=group(n,p);box('Plate',(0,0,0),(.35,.22,.6),'Gold',.04,g);tube('Arm',[(0,-.15,0),(0,-.4,.45)],.045,'GoldLight',g);blob('Grip',(0,-.4,.45),(.08,.08,.12),'Wood',g);return g
def gear(n,p,r):
 g=group(n,p);N=48;vs=[]
 for y in [-.09,.09]:
  for i in range(N):a=i*math.tau/N;rr=r*(1 if i%4 in [1,2] else .82);vs.append((rr*math.cos(a),y,rr*math.sin(a)))
 fs=[tuple(range(N-1,-1,-1)),tuple(range(N,2*N))]
 for i in range(N):j=(i+1)%N;fs.append((i,j,j+N,i+N))
 o=raw('GearBody',vs,fs,'Gold',parent=g);ring('GearRidge',(0,-.1,0),r*.56,.025,'GoldLight',g);cylinder('Axle',(0,-.11,0),r*.17,.15,'Wood',g,(math.pi/2,0,0));return g

def door(n,x,y,width=1.7):
 g=group(n,(x,y,1.6));box('Door',(0,0,0),(width,.28,3.2),'Wood',.08,g)
 for a in [-1,1]:box('Band',(a*(width/2-.12),-.18,0),(.10,.07,3),'Gold',.02,g);box('Band',(0,-.18,a*1.25),(width,.07,.1),'Gold',.02,g)
 glyph(2,(0,-.22,0),.45,g);return g
# terrain and main routes
box('C_Ground',(0,2,-.3),(54,65,.6),'Earth',.1)
for x in range(-8,9,2):
 for y in range(-10,17,2):box('C_Paving',(x+random.uniform(-.04,.04),y,.035),(1.94,1.94,.12),random.choice(['Stone','Sand','Edge']),.06)
wall(-22,-12,-22,28);wall(23,-12,23,28);wall(-22,28,23,28)
wall(-22,-12,-1.8,-12,3.5);wall(1.8,-12,23,-12,3.5)
pillar(-2,-12,3.8);pillar(2,-12,3.8);box('C_Lintel',(0,-12,3.55),(3.7,.85,.7),'Edge',.1)
door('A_GateLeft',-.86,-12);door('A_GateRight',.86,-12)
for i in range(3):
 g=group('I_Ring'+str(i),(3.2+i*.8,-12.55,1.6));cylinder('Dial',(0,0,0),.32,.14,'Sand',g,(math.pi/2,0,0))
 for k in range(4):q=group('Symbol'+str(k),parent=g);glyph(k,(0,-.1,0),.20,q)
lever('I_GateLever',(5.8,-12.55,1.5))
# campsite, soft fabric shelter
vs=[(-8,-22,0),(-5,-22,3),(-2,-22,0),(-8,-18,0),(-5,-18,3),(-2,-18,0)]
raw('TentCanvas',vs,[(0,1,4,3),(1,2,5,4),(0,2,1)],'Canvas');tube('TentRidge',[(-5,-22.4,3),(-5,-17.7,3)],.06,'Wood')
for y in [-22,-18]:cylinder('C_TentPole',(-5,y,1.5),.07,3,'Wood')
for x in [-8,-2]:
 for y in [-22,-18]:tube('Rope',[(x,y,.15),(x+(-1 if x<-5 else 1),y-.6,0)],.018,'Paper')
table(-4,-16,2.7);lantern(-5,-16,1.06)
g=group('I_Journal',(-4,-16,1.07));box('Cover',(0,0,0),(.48,.60,.07),'Wood',.025,g);box('Pages',(0,0,.05),(.43,.55,.03),'Paper',.01,g)
g=group('I_Flashlight',(-3,-16,1.12));cylinder('Body',(0,0,0),.07,.4,'Dark',g,(0,math.pi/2,0));cylinder('Head',(.21,0,0),.11,.1,'Gold',g,(0,math.pi/2,0))
box('C_Pedestal',(-3.7,-13.7,.58),(.85,.85,1.16),'Stone',.1);g=group('I_Disc',(-3.7,-13.7,1.25));cylinder('Disc',(0,0,0),.32,.09,'Sand',g);ring('Edge',(0,0,.05),.28,.014,'Gold',g,False)
# courtyard pool
for x in [-2.7,2.7]:box('C_PoolEdge',(x,3,.2),(.3,3.5,.4),'Edge',.07)
for y in [1.25,4.75]:box('C_PoolEdge',(0,y,.2),(5.4,.3,.4),'Edge',.07)
box('PoolFloor',(0,3,.105),(5.1,3.2,.1),'Dark',.02);box('A_PoolWater',(0,3,.13),(5.05,3.15,.025),'Water',.01)
# mirror room
wall(-19,9,-9,9,3.6);wall(-19,0,-19,9,3.6)
box('C_MirrorFloor',(-14,4.5,.02),(10,9,.1),'Sand',.04)
for x in [-18.6,-9.4]:
 for y in [.4,8.6]:pillar(x,y,3.8)
for side in [-1,1]:box('MirrorRoof',(-14+side*3,4.5,4.35),(4,9.6,.18),'Canvas',.04)
for side in [-1,1]:box('MirrorRoof',(-14,4.5+side*2.9,4.35),(2,3.8,.18),'Canvas',.04)
box('A_Shutter',(-14,4.5,4.49),(2.1,2.1,.12),'Wood',.025)
lever('I_Shutter',(-10,8.5,1.5))
for i,(x,y) in enumerate([(-16,2),(-12,5)]):
 box('C_MirrorBase',(x,y,.58),(.85,.85,1.16),'Stone',.1);g=group('I_Mirror'+str(i),(x,y,1.8));cylinder('Reflector',(0,0,0),.55,.06,'Glass',g,(math.pi/2,0,0));ring('Frame',(0,-.035,0),.59,.045,'Gold',g);cylinder('Stem',(0,0,-.4),.055,.85,'Gold',g)
g=group('P_Receiver',(-15,8.58,2.4));box('Receiver',(0,0,0),(.7,.15,.7),'Gold',.06,g);glyph(2,(0,-.1,0),.24,g)
# divider ensures no bypass
wall(9,-12,9,2,2.7);wall(9,5.5,9,28,2.7);g=group('A_SideGate',(9,3.75,1.3));box('Door',(0,0,0),(.3,3.4,2.6),'Wood',.06,g)
# compact waterworks
box('C_Reservoir',(19,5,.6),(3.5,3.5,1.2),'Stone',.1);box('ReservoirWater',(19,5,1.23),(3.2,3.2,.03),'Water',.01)
for i in range(3):
 x=12+i*3;box('C_ChannelBase',(x,9,.2),(2.8,2.8,.4),'Sand',.07);g=group('I_Channel'+str(i),(x,9,.55));cylinder('ChannelPlate',(0,0,0),1.22,.18,'Stone',g)
 for xx in [-.48,.48]:box('Rim',(xx,0,.19),(.17,2.2,.3),'Edge',.04,g)
 box('ChannelFlow',(0,0,.08),(.76,2.15,.02),'Water',.005,g)
for y in [7.7,10.3]:box('C_Kerb',(15,y,.6),(9,.18,.32),'Edge',.04)
wheel('I_Valve',(18.7,3.1,1),.4)
box('C_HandleShelf',(11.2,4.8,.6),(1.2,.7,.14),'WoodLight',.025);wheel('I_Handle',(11.2,4.8,.75),.3).rotation_euler.x=math.pi/2
# wheel and shaft
w=group('A_Waterwheel',(12,12.4,1.45));ring('Rim',(0,0,0),1.25,.085,'Wood',w)
for i in range(12):
 a=i*math.tau/12;tube('Spoke',[(0,0,0),(1.25*math.cos(a),0,1.25*math.sin(a))],.045,'WoodLight',w);o=box('Paddle',(1.25*math.cos(a),0,1.25*math.sin(a)),(.35,.65,.15),'WoodLight',.025,w);o.rotation_euler.y=-a
cylinder('Shaft',(12,15,1.45),.12,5,'Gold',rot=(math.pi/2,0,0))
# workshop
wall(10.5,23,22.5,23,3.5);wall(22.5,14,22.5,23,3.5)
for x in [10.8,22]:
 for y in [14.8,22.5]:box('C_Post',(x,y,1.8),(.22,.22,3.6),'Wood',.035)
for side in [-1,1]:o=box('WorkshopRoof',(16.4+side*2.9,18.5,4.1),(6,8.6,.16),'Canvas',.035);o.rotation_euler.y=side*.16
box('C_GearBoard',(17,21.3,1.6),(4.3,.35,2.6),'Stone',.12)
for i,(x,r) in enumerate([(15.7,.63),(16.96,.53),(18.04,.42)]):
 g=group('I_Socket'+str(i),(x,21.06,1.75));cylinder('Socket',(0,0,0),r,.05,'Dark',g,(math.pi/2,0,0));glyph(i,(0,-.055,-.85),.16,g);gear('A_Gear'+str(i),(x,20.92,1.75),r)
lever('I_MachineLever',(19.7,21,1.4));table(15,15.8,2.3)
for i,(x,y,z,r) in enumerate([(14.3,15.8,1.13,.55),(15.5,15.8,1.13,.46),(21,18,1.05,.35)]):gear('I_Gear'+str(i),(x,y,z),r).rotation_euler.x=math.pi/2
box('C_Shelf',(21,18,.85),(1.5,.8,.16),'WoodLight',.03)
# sanctuary
wall(-4.5,15,-1.7,15,3.7);wall(1.7,15,4.5,15,3.7);wall(-4.5,15,-4.5,25,3.7);wall(4.5,15,4.5,25,3.7);wall(-4.5,25,4.5,25,3.7)
box('C_FinalLintel',(0,15,3.6),(3.5,.85,.65),'Edge',.1);door('A_FinalLeft',-.82,15,1.62);door('A_FinalRight',.82,15,1.62)
for x in [-4.8,4.8]:pillar(x,15,4);pot(x,14)
box('SanctuaryRoof',(0,20,4.1),(10,11,.22),'Sand',.06);box('C_ReliefBase',(0,20,.65),(3.4,2.6,1.3),'Stone',.14)
for x,y,s in [(-.9,20.2,.65),(.4,20,.8),(1,20.4,.55)]:blob('ReliefMountain',(x,y,1.5),(s,s,s*.6),'Sand')
for i in range(5):box('MiniHome',(-1.2+i*.5,19.3,1.5),(.30,.32,.35),'Edge',.02)
box('C_ValveBase',(2.8,20,.55),(.75,.75,1.1),'Stone',.08);wheel('I_FinalValve',(2.8,20,1.2),.43)
# optional notes and decor
for i,(x,y,z) in enumerate([(-8,-18,.8),(-17.7,7.6,.9),(21,20,1)]):
 box('C_NoteShelf',(x,y,z-.09),(.8,.65,.13),'WoodLight',.02);g=group('I_Note'+str(i),(x,y,z));box('Pages',(0,0,0),(.45,.35,.035),'Paper',.01,g)
 for k in range(5):box('Ink',(0,-.1+k*.04,.025),(.28,.008,.005),'Dark',.001,g)
for x,y in [(-2,-12.5),(2,-12.5),(-18.6,.2),(-9.4,.2),(-4.8,14.5),(4.8,14.5)]:
 g=group('Banner',(x,y,2.5));raw('Fabric',[(-.3,0,.5),(.3,0,.5),(.3,0,-.5),(0,0,-.75),(-.3,0,-.5)],[(0,1,2,3,4)],'Dark',parent=g);glyph(2,(0,-.02,0),.19,g)
for x,y in [(-8,-15),(5,-7),(-17,8),(21,22),(-3,24)]:pot(x,y);lantern(x+.6,y,.25)
for x,y in [(-7,-16),(-7.8,-16.7),(21,16)]:
 box('C_Crate',(x,y,.36),(.72,.72,.72),'Wood',.04)
 for z in [.1,.62]:box('Band',(x,y-.38,z),(.74,.07,.07),'WoodLight',.02)
for i in range(24):tree(random.choice([-1,1])*random.uniform(24,27),random.uniform(-24,29),random.uniform(.8,1.15))
for i in range(8):tree(random.uniform(-20,20),random.uniform(29,32),1)
for x,y in [(-13,-21),(7,-22),(-19,-17),(-20,14),(7,25)]:tree(x,y,.9)
for i in range(70):
 x=random.uniform(-21,22);y=random.uniform(-25,26)
 if abs(x)<9 or (9<x<22 and 1<y<23) or (-19<x<-9 and 0<y<10):continue
 blob('C_Rock',(x,y,.2),(random.uniform(.3,.65),random.uniform(.3,.6),random.uniform(.25,.55)),'Sand')
# Small modeled ground plants and climbing vines: coherent original foliage.
def leaf(p,a,length=.24,width=.085,parent=None):
 x,y,z=p;pts=[]
 for u,v,h in [(0,0,0),(.35,-1,.16),(1,0,0),(.35,1,.16),(.45,0,.28)]:
  xx=u*length;yy=v*width;pts.append((x+xx*math.cos(a)-yy*math.sin(a),y+xx*math.sin(a)+yy*math.cos(a),z+h*length))
 return raw('LeafBlade',pts,[(0,1,4),(1,2,4),(2,3,4),(3,0,4)],random.choice(['Leaf','LeafLight','LeafDark']),parent=parent)
for x,y in [(-7,-7),(7,-5),(-7,10),(7,11),(-6,13),(6,13),(-17,-1),(-11,-1),(-20,6),(21,12),(-5,-13.1),(6,-13.1)]:
 for k in range(16):
  px=x+random.uniform(-.65,.65);py=y+random.uniform(-.5,.5);z=random.uniform(.08,.22)
  for j in range(3):leaf((px,py,z),random.uniform(0,math.tau),random.uniform(.18,.4),.06)
for x,y in [(-18.65,2),(-18.65,7),(-4.15,16),(-4.15,23),(4.15,24),(20,22.65),(-20,-12.4),(7,-12.4)]:
 pts=[(x+math.sin(i*.5)*.13,y, .15+i*.21) for i in range(14)];tube('Vine',pts,.025,'Wood')
 for i,p in enumerate(pts):
  for side in [-1,1]:
   o=leaf((p[0],p[1]-.03,p[2]),0,.30,.13);# orient broad leaf plane vertically against stone
   # vertices are world-space: keep silhouette varied without extra transforms
for x,y in [(-6,13),(6,13),(-7,-4),(7,-4)]:
 pot(x,y,.9)
 for k in range(10):leaf((x,y,.95),k*2.4,.5,.12)
for n,p in {'P_Spawn':(0,-21,.12),'P_Look':(0,-12,1.6),'P_Courtyard':(0,-5,.12),'P_Water':(11,3.6,.12),'P_End':(0,11,.12),'P_Inspect':(0,20,1.8)}.items():group(n,p)
# checkpoint first, then portable mesh export
bpy.ops.wm.save_as_mainfile(filepath=ROOT+'/SourceArt/World.blend')
bpy.ops.object.select_all(action='SELECT');bpy.ops.export_scene.fbx(filepath=OUT+'/World.fbx',use_selection=True,object_types={'MESH','EMPTY'},apply_unit_scale=True,bake_space_transform=True,add_leaf_bones=False,axis_forward='-Z',axis_up='Y')
with open(ROOT+'/Documentation/geometry.json','w') as f:json.dump({'objects':len(bpy.data.objects),'vertices':sum(len(o.data.vertices) for o in bpy.data.objects if o.type=='MESH'),'anchors':{o.name:list(o.location) for o in bpy.data.objects if o.name.startswith(('I_','A_','P_'))}},f,indent=2)
# actual model previews (not Unity screenshots)
world=bpy.context.scene.world;world.use_nodes=True;world.node_tree.nodes['Background'].inputs[0].default_value=(.55,.67,.74,1);world.node_tree.nodes['Background'].inputs[1].default_value=.7
bpy.ops.object.light_add(type='SUN');sun=bpy.context.object;sun.rotation_euler=(.55,-.45,-.6);sun.data.energy=2.0;sun.data.angle=.15
sc=bpy.context.scene;sc.render.engine='CYCLES';sc.cycles.samples=24;sc.cycles.use_denoising=False;sc.render.resolution_x=1280;sc.render.resolution_y=800;sc.render.resolution_percentage=100;sc.view_settings.view_transform='Standard';sc.view_settings.look='Medium High Contrast'
bpy.ops.object.camera_add();cam=bpy.context.object;sc.camera=cam;cam.data.lens=25
os.makedirs(ROOT+'/Documentation/Previews',exist_ok=True)
for name,p,t in [('Courtyard',(-4,-8,2.1),(-4,8,2)),('Camp',(3,-23,2.5),(-3,-15,1.5)),('Water',(10,1,2.8),(17,10,1)),('Workshop',(11,14,2.4),(17,21,1.6)),('Overview',(33,-33,29),(0,3,0))]:
 cam.location=p;cam.rotation_euler=(Vector(t)-cam.location).to_track_quat('-Z','Y').to_euler();sc.render.filepath=ROOT+'/Documentation/Previews/'+name+'.png';bpy.ops.render.render(write_still=True)
bpy.ops.wm.save_as_mainfile(filepath=ROOT+'/SourceArt/World.blend');print('COMPLETE')

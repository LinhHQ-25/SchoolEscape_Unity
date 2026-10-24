using UnityEngine;
using System.Collections.Generic;
namespace Aster {
public class AsterWaterworksPolish:MonoBehaviour {
 Expedition game;Material stone,mud,water,wood;Transform gateHinge;Quaternion gateClosed;GameObject poolSurface;float gateAngle;Transform supplyWheel,finalWheel;float supplyAngle,finalAngle;
 public static void Install(Expedition game){var p=new GameObject("Waterworks composition").AddComponent<AsterWaterworksPolish>();p.game=game;p.Build();}
 Material Mat(Color c,string shader){var m=new Material(Resources.Load<Shader>(shader));m.color=c;return m;}
 Transform Box(string name,Vector3 pos,Vector3 size,Material mat,Transform parent=null,bool collision=true){
  var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=name;o.transform.SetParent(parent?parent:transform,false);o.transform.position=pos;o.transform.rotation=Quaternion.identity;var parentScale=o.transform.parent.lossyScale;o.transform.localScale=new Vector3(size.x/Mathf.Abs(parentScale.x),size.y/Mathf.Abs(parentScale.y),size.z/Mathf.Abs(parentScale.z));o.GetComponent<Renderer>().sharedMaterial=mat;
  if(!collision){o.GetComponent<Collider>().enabled=false;Destroy(o.GetComponent<Collider>());}return o.transform;
 }
 void Hide(Transform t,bool collider=true){if(!t)return;foreach(var r in t.GetComponentsInChildren<Renderer>(true))r.enabled=false;if(collider)foreach(var c in t.GetComponentsInChildren<Collider>(true))c.enabled=false;}
 Bounds BoundsOf(Transform t){
  var bounds=new Bounds(t.position,Vector3.zero);bool first=true;
  var source=Resources.Load<GameObject>("Art/World");
  foreach(var f in t.GetComponentsInChildren<MeshFilter>(true)){
   Mesh mesh=f.sharedMesh;var r=f.GetComponent<Renderer>();
   if(r&&r.isPartOfStaticBatch&&source){foreach(var s in source.GetComponentsInChildren<MeshFilter>(true))if(s.name==f.name){mesh=s.sharedMesh;break;}}
   if(!mesh)continue;var b=mesh.bounds;
   for(int i=0;i<8;i++){var p=f.transform.TransformPoint(b.center+Vector3.Scale(b.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1)));if(first){bounds=new Bounds(p,Vector3.zero);first=false;}else bounds.Encapsulate(p);}
  }return bounds;
 }
 void Build(){
  stone=Mat(new Color(.40f,.39f,.29f),"Art/AgedStone");stone.mainTexture=Resources.Load<Texture2D>("Restoration/StoneGrain");
  mud=Mat(new Color(.20f,.235f,.16f),"Art/WetGround");water=Mat(new Color(.10f,.28f,.24f,.78f),"Art/StillWater");
  wood=new Material(Shader.Find("Standard"));wood.color=new Color(.23f,.17f,.10f);wood.SetFloat("_Glossiness",.12f);
  Pool();Reservoir();Gate();Marsh();Pickups();Valves();
 }
 void Pool(){
  var old=game.Node("A_PoolWater");var b=BoundsOf(old);Vector3 c=b.center;c.y=0;
  Hide(old);foreach(var t in game.world.GetComponentsInChildren<Transform>(true))if(t.name.StartsWith("C_PoolEdge")||t.name=="PoolFloor")Hide(t);
  float hx=Mathf.Max(2.50f,b.size.x*.5f),hz=Mathf.Max(1.55f,b.size.z*.5f);
  Box("Silt bed",new Vector3(c.x,.005f,c.z),new Vector3(hx*2,.015f,hz*2),mud,null,false);
  // Individually laid coping stones with recessed mortar instead of four oversized bars.
  for(int side=-1;side<=1;side+=2){
   int nx=Mathf.CeilToInt(hx*2/.55f),nz=Mathf.CeilToInt(hz*2/.55f);
   for(int i=0;i<nx;i++){if(side==-1&&i==0)continue;Box("Pool coping",new Vector3(c.x-hx+(i+.5f)*hx*2/nx,.17f,c.z+side*(hz+.12f)),new Vector3(hx*2/nx-.012f,.34f,.30f),stone);}
   for(int i=0;i<nz;i++){if(side==-1&&i==0)continue;Box("Pool coping",new Vector3(c.x+side*(hx+.12f),.17f,c.z-hz+(i+.5f)*hz*2/nz),new Vector3(.30f,.34f,hz*2/nz-.012f),stone);}
  }
  poolSurface=FlatWater("Restored courtyard pool",new Vector3(c.x,.028f,c.z),hx*2-.04f,hz*2-.04f);
  // A visible inlet joins the previous tailrace to the inside of this pool.
  Vector3 a=new Vector3(-3,.045f,-5),end=new Vector3(c.x-hx+.15f,.035f,c.z-hz+.15f);
  var channel=new GameObject("Pool inlet");channel.transform.SetParent(transform,false);
  Vector3 d=end-a;float length=new Vector2(d.x,d.z).magnitude;
  var slab=Box("Inlet bed",(a+end)*.5f-Vector3.up*.055f,new Vector3(.48f,.08f,length),stone);
  slab.rotation=Quaternion.LookRotation(new Vector3(d.x,0,d.z));
  var flow=FlatWater("Pool inlet flow",(a+end)*.5f,.28f,length);flow.transform.rotation=slab.rotation;
  flow.transform.SetParent(poolSurface.transform,true);
 }
 void Reservoir(){
  Hide(game.world.Find("C_Reservoir"));Hide(game.world.Find("ReservoirWater"));
  Vector3 c=new Vector3(-19,0,-5);
  Box("Reservoir foundation",c+Vector3.up*.7f,new Vector3(3.65f,1.4f,3.65f),stone);
  Box("Reservoir inner bed",c+Vector3.up*1.47f,new Vector3(3.20f,.14f,3.20f),stone);
  // Water 1.97; lip 2.12: fifteen centimetres of freeboard.
  foreach(int side in new[]{-1,1}){
   Box("Reservoir retaining wall",c+new Vector3(side*1.68f,1.76f,0),new Vector3(.22f,.72f,3.6f),stone);
   if(side==1)Box("Reservoir retaining wall",c+new Vector3(0,1.76f,side*1.68f),new Vector3(3.35f,.72f,.22f),stone);
   else foreach(float x in new[]{-1.06f,1.06f})Box("Outlet wall shoulders",c+new Vector3(x,1.76f,-1.68f),new Vector3(1.23f,.72f,.22f),stone);
   for(int i=0;i<6;i++){
    if(side==1||i<2||i>3)Box("Reservoir rim block",c+new Vector3(-1.5f+i*.6f,2.075f,side*1.68f),new Vector3(.58f,.09f,.30f),stone);
    Box("Reservoir rim block",c+new Vector3(side*1.68f,2.075f,-1.5f+i*.6f),new Vector3(.30f,.09f,.58f),stone);
   }
  }
  FlatWater("Reservoir recessed surface",c+Vector3.up*1.97f,3.10f,3.10f);
  // Short outlet lip encloses the feed without putting the valve inside a pier.
  Box("Outlet lip",new Vector3(-19,1.81f,-6.65f),new Vector3(.74f,.18f,.60f),stone);
 }
 GameObject FlatWater(string name,Vector3 p,float x,float z){
  var o=new GameObject(name);o.transform.SetParent(transform,false);o.transform.position=p;
  var m=new Mesh();m.vertices=new[]{new Vector3(-x/2,0,-z/2),new Vector3(x/2,0,-z/2),new Vector3(x/2,0,z/2),new Vector3(-x/2,0,z/2)};
  m.uv=new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up};m.triangles=new[]{0,2,1,0,3,2};m.RecalculateNormals();m.RecalculateTangents();o.AddComponent<MeshFilter>().sharedMesh=m;o.AddComponent<MeshRenderer>().sharedMaterial=water;return o;
 }
 void Gate(){
  var target=game.Node("A_SideGate");var b=BoundsOf(target);bool alongX=b.size.x>b.size.z;
  Vector3 along=alongX?Vector3.right:Vector3.forward,front=alongX?Vector3.forward:Vector3.right;
  float width=alongX?b.size.x:b.size.z,height=b.size.y;Vector3 center=b.center;
  // A side-hinged leaf is supported even when open.
  gateHinge=new GameObject("Side gate hinge").transform;gateHinge.SetParent(transform,false);gateHinge.position=center-along*width*.5f;
  gateClosed=gateHinge.rotation;target.SetParent(gateHinge,true);
  for(int side=-1;side<=1;side+=2){
   Vector3 p=center+along*side*(width*.5f+.15f);p.y=(b.max.y+.16f)*.5f;
   Box("Gate stone jamb",p,alongX?new Vector3(.32f,b.max.y+.16f,.55f):new Vector3(.55f,b.max.y+.16f,.32f),stone);
   for(int j=0;j<6;j++){var q=p;q.y=.20f+j*(b.max.y/6);Box("Jamb masonry course",q,alongX?new Vector3(.38f,.10f,.60f):new Vector3(.60f,.10f,.38f),stone);}
  }
  Box("Gate lintel",new Vector3(center.x,b.max.y+.22f,center.z),alongX?new Vector3(width+.68f,.30f,.60f):new Vector3(.60f,.30f,width+.68f),stone);
  var leaf=new GameObject("Gate ironwork").transform;leaf.SetParent(target,true);
  for(int j=0;j<3;j++){
   Vector3 p=center+Vector3.up*((j-1)*height*.32f)+front*.18f;
   Box("Bronze gate strap",p,alongX?new Vector3(width*.94f,.10f,.07f):new Vector3(.07f,.10f,width*.94f),AsterGearwork.Metal,leaf,false);
   for(int k=0;k<5;k++){Vector3 q=p+along*((k-2)*width*.18f);Box("Strap rivet",q+front*.04f,Vector3.one*.045f,AsterGearwork.Metal,leaf,false);}
  }
  for(int k=0;k<8;k++){Vector3 p=center+along*((k-3.5f)*width/8)+front*.17f;Box("Plank seam",p,alongX?new Vector3(.012f,height*.94f,.018f):new Vector3(.018f,height*.94f,.012f),wood,leaf,false);}
 }
 void Marsh(){
  for(int i=0;i<24;i++){
   float x=-21.7f+(i%6)*1.8f,z=-1.3f-(i/6)*3.1f;
   if((x+19)*(x+19)+(z+5)*(z+5)<6)continue;
   var o=new GameObject("Shallow wet earth");o.transform.SetParent(transform,false);o.transform.position=new Vector3(x,.012f,z);
   int n=24;var v=new Vector3[n+1];var uv=new Vector2[n+1];var t=new int[n*3];uv[0]=Vector2.one*.5f;
   for(int k=0;k<n;k++){float a=k*Mathf.PI*2/n,r=.7f+.12f*Mathf.Sin(k*2.3f+i);v[k+1]=new Vector3(Mathf.Cos(a)*r,0,Mathf.Sin(a)*r*.65f);uv[k+1]=new Vector2(Mathf.Cos(a),Mathf.Sin(a))*.5f+Vector2.one*.5f;t[k*3]=0;t[k*3+1]=(k+1)%n+1;t[k*3+2]=k+1;}
   var m=new Mesh();m.vertices=v;m.uv=uv;m.triangles=t;m.RecalculateNormals();m.RecalculateTangents();o.AddComponent<MeshFilter>().sharedMesh=m;o.AddComponent<MeshRenderer>().sharedMaterial=i%3==0?water:mud;
  }
 }
 void PlacePickup(string id,Vector3 pos){
  Transform target=game.Node(id);target.position=pos;var b=BoundsOf(target);
  // Support exactly the lowest visible geometry; use generated gear mesh only.
  var generated=target.Find("Ancient bronze gear "+id[id.Length-1]);
  if(generated)b=BoundsOf(generated);
  float top=b.min.y-.015f;
  Box("Recessed artifact shelf",new Vector3(b.center.x,top-.07f,b.center.z),new Vector3(Mathf.Max(1.05f,b.size.x+.15f),.14f,Mathf.Max(.75f,b.size.z+.15f)),wood);
  foreach(float dx in new[]{-.35f,.35f})Box("Shelf leg",new Vector3(b.center.x+dx,(top-.14f)*.5f,b.center.z),new Vector3(.10f,Mathf.Max(.1f,top-.14f),.10f),wood);
 }
 void Pickups(){
  PlacePickup("I_Gear0",new Vector3(-14.3f,.92f,-17.4f));
  PlacePickup("I_Gear1",new Vector3(-20.6f,.87f,-20.8f));
  PlacePickup("I_Gear2",new Vector3(-21.1f,.96f,-16.2f));
  PlacePickup("I_Handle",new Vector3(-21.0f,.85f,-2.3f));
  // Waist-high dividers encourage looking behind storage, without blocking access.
  Box("Storage divider",new Vector3(-20.6f,.48f,-19.7f),new Vector3(1.6f,.96f,.18f),wood);
  Box("Storage divider",new Vector3(-21.0f,.48f,-3.4f),new Vector3(1.4f,.96f,.18f),wood);
 }
 void Valves(){
  RebuildValve(game.Node("I_Valve"),new Vector3(-19.5f,2.38f,-7.98f),.34f);
  var pedestal=game.world.Find("C_ValveBase");var b=pedestal?BoundsOf(pedestal):new Bounds(game.Node("I_FinalValve").position,Vector3.one);
  Vector3 p=new Vector3(b.center.x,b.max.y+.52f,b.center.z+.30f);RebuildValve(game.Node("I_FinalValve"),p,.40f);
  Box("Final valve bearing pedestal",new Vector3(p.x,b.max.y+.22f,b.center.z),new Vector3(.16f,.44f,.25f),AsterGearwork.Metal);
 }
 void RebuildValve(Transform target,Vector3 p,float radius){
  Hide(target);target.position=p;target.rotation=Quaternion.identity;
  var root=new GameObject("Supported valve wheel").transform;root.SetParent(target,false);Vector3 s=target.lossyScale;root.localScale=new Vector3(1/Mathf.Abs(s.x),1/Mathf.Abs(s.y),1/Mathf.Abs(s.z));
  var o=new GameObject("Wheel rim and spokes");o.transform.SetParent(root,false);o.AddComponent<MeshFilter>().sharedMesh=ValveMesh(radius);o.AddComponent<MeshRenderer>().sharedMaterial=AsterGearwork.Metal;
  if(target.name=="I_Valve")supplyWheel=o.transform;else finalWheel=o.transform;
  var col=o.AddComponent<BoxCollider>();col.size=new Vector3(radius*2,radius*2,.16f);
  Box("Valve axle",p+Vector3.back*.20f,new Vector3(.12f,.12f,.42f),AsterGearwork.Metal,null,false);
  if(target.name=="I_Valve"){
   Box("Valve bearing upright",new Vector3(p.x+.70f,p.y*.5f,p.z-.20f),new Vector3(.14f,p.y,.14f),AsterGearwork.Metal);
   Box("Valve bearing cross arm",new Vector3(p.x+.35f,p.y,p.z-.20f),new Vector3(.80f,.12f,.12f),AsterGearwork.Metal);
   Box("Valve bearing foot",new Vector3(p.x+.70f,.09f,p.z-.20f),new Vector3(.40f,.18f,.40f),stone);
  }
 }
 Mesh ValveMesh(float r){
  var v=new List<Vector3>();var t=new List<int>();
  for(int i=0;i<64;i++)for(int j=0;j<8;j++){float a=i*Mathf.PI/32,b=j*Mathf.PI/4;v.Add(new Vector3((r+.035f*Mathf.Cos(b))*Mathf.Cos(a),(r+.035f*Mathf.Cos(b))*Mathf.Sin(a),.035f*Mathf.Sin(b)));}
  for(int i=0;i<64;i++)for(int j=0;j<8;j++){int a=i*8+j,b=((i+1)%64)*8+j,c=i*8+(j+1)%8,d=((i+1)%64)*8+(j+1)%8;t.AddRange(new[]{a,b,c,b,d,c});}
  // Spokes are small solid rectangular prisms.
  for(int i=0;i<5;i++){float a=i*Mathf.PI*2/5;Vector3 d=new Vector3(Mathf.Cos(a),Mathf.Sin(a),0),n=new Vector3(-d.y,d.x,0)*.025f;int b=v.Count;
   for(int z=-1;z<=1;z+=2){v.Add(d*.03f-n+Vector3.forward*z*.025f);v.Add(d*r-n+Vector3.forward*z*.025f);v.Add(d*r+n+Vector3.forward*z*.025f);v.Add(d*.03f+n+Vector3.forward*z*.025f);}
   t.AddRange(new[]{b,b+2,b+1,b,b+3,b+2,b+4,b+5,b+6,b+4,b+6,b+7});
   for(int j=0;j<4;j++){int k=(j+1)%4;t.AddRange(new[]{b+j,b+k,b+k+4,b+j,b+k+4,b+j+4});}}
  var m=new Mesh();m.SetVertices(v);m.SetTriangles(t,0);m.RecalculateNormals();return m;
 }
 void Update(){
  if(poolSurface)poolSurface.SetActive(game.state.finished);
  if(supplyWheel){
   supplyWheel.GetComponent<Renderer>().enabled=game.state.valve;
   supplyAngle=Mathf.MoveTowards(supplyAngle,game.state.valve?110:0,80*Time.deltaTime);
   supplyWheel.localRotation=Quaternion.Euler(0,0,supplyAngle);
  }
  if(finalWheel){finalAngle=Mathf.MoveTowards(finalAngle,game.state.finished?120:0,80*Time.deltaTime);finalWheel.localRotation=Quaternion.Euler(0,0,finalAngle);}
  if(gateHinge){gateAngle=Mathf.MoveTowards(gateAngle,game.state.light?95:0,70*Time.deltaTime);gateHinge.rotation=gateClosed*Quaternion.Euler(0,gateAngle,0);}
 }
}
}

using UnityEngine;
using System.Collections.Generic;
namespace Aster {
public class AsterPolish:MonoBehaviour {
 Expedition game;readonly List<Transform> handles=new List<Transform>();readonly List<float> angles=new List<float>();Material bronze,stone,wood;
 public static void Install(Expedition g){var p=new GameObject("Aster scene polish").AddComponent<AsterPolish>();p.game=g;p.Build();}
 Material MakeMaterial(Color c,bool rock=false){var m=new Material(rock?Resources.Load<Shader>("Art/AgedStone"):Shader.Find("Standard"));m.color=c;if(rock)m.mainTexture=Resources.Load<Texture2D>("Restoration/StoneGrain");else{m.SetFloat("_Glossiness",.19f);m.SetFloat("_Metallic",.25f);}return m;}
 Transform Shape(string name,PrimitiveType type,Transform parent,Vector3 position,Vector3 scale,Material mat,bool collision=false){
  var o=GameObject.CreatePrimitive(type);o.name=name;o.transform.SetParent(parent,false);o.transform.localPosition=position;o.transform.localScale=scale;o.GetComponent<Renderer>().sharedMaterial=mat;
  if(!collision){o.GetComponent<Collider>().enabled=false;Destroy(o.GetComponent<Collider>());}return o.transform;
 }
 void Build(){
  bronze=MakeMaterial(new Color(.26f,.23f,.14f));wood=MakeMaterial(new Color(.18f,.12f,.075f));stone=MakeMaterial(new Color(.38f,.36f,.29f),true);
  Physics.SyncTransforms();MakeLever("I_GateLever");MakeLever("I_Shutter");MakeLever("I_MachineLever");ReplacePickup();RoofSupports();Lighting();
 }
 void MakeLever(string id){
  var target=game.Node(id);Vector3 old=target.position,normal=Vector3.forward,contact=old-Vector3.forward*.15f;float best=2;
  foreach(var direction in new[]{Vector3.forward,Vector3.back,Vector3.left,Vector3.right})
   foreach(var hit in Physics.RaycastAll(old-direction*.65f,direction,2f,~0,QueryTriggerInteraction.Ignore)){
    string name=hit.collider.transform.parent?hit.collider.transform.parent.name:hit.collider.name;
    if(!name.ToLowerInvariant().Contains("wall")&&!hit.collider.name.ToLowerInvariant().Contains("wall"))continue;
    float d=Vector3.Distance(old,hit.point);if(d<best){best=d;contact=hit.point;normal=hit.normal;}
   }
  foreach(var r in target.GetComponentsInChildren<Renderer>(true))r.enabled=false;
  foreach(var c in target.GetComponentsInChildren<Collider>(true))c.enabled=false;
  var mount=new GameObject("Fitted bronze lever").transform;mount.SetParent(target,false);mount.position=contact;mount.rotation=Quaternion.LookRotation(normal,Vector3.up);
  Vector3 sc=target.lossyScale;mount.localScale=new Vector3(1/Mathf.Abs(sc.x),1/Mathf.Abs(sc.y),1/Mathf.Abs(sc.z));
  Shape("Masonry socket",PrimitiveType.Cube,mount,new Vector3(0,0,-.015f),new Vector3(.48f,.72f,.14f),stone,true);
  Shape("Oval bronze plate",PrimitiveType.Sphere,mount,new Vector3(0,0,.08f),new Vector3(.36f,.58f,.12f),bronze);
  for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)Shape("Rivet",PrimitiveType.Sphere,mount,new Vector3(x*.12f,y*.2f,.14f),Vector3.one*.045f,bronze);
  var axle=Shape("Pivot axle",PrimitiveType.Cylinder,mount,new Vector3(0,-.10f,.19f),new Vector3(.15f,.10f,.15f),bronze);axle.localRotation=Quaternion.Euler(90,0,0);
  var pivot=new GameObject("Animated handle pivot").transform;pivot.SetParent(mount,false);pivot.localPosition=new Vector3(0,-.10f,.24f);
  Shape("Rounded handle shaft",PrimitiveType.Capsule,pivot,new Vector3(0,.21f,0),new Vector3(.065f,.24f,.065f),bronze);
  Shape("Worn wooden grip",PrimitiveType.Capsule,pivot,new Vector3(0,.43f,0),new Vector3(.115f,.105f,.115f),wood);
  for(int i=0;i<3;i++)Shape("Grip collar",PrimitiveType.Cylinder,pivot,new Vector3(0,.35f+i*.065f,0),new Vector3(.12f,.012f,.12f),bronze);
  var box=mount.gameObject.AddComponent<BoxCollider>();box.center=new Vector3(0,.04f,.24f);box.size=new Vector3(.5f,1.2f,.7f);
  handles.Add(pivot);angles.Add(25);pivot.localRotation=Quaternion.Euler(25,0,0);
 }
 void ReplacePickup(){
  var target=game.Node("I_Disc");Vector3 pos=target.position;
  foreach(var r in target.GetComponentsInChildren<Renderer>(true))r.enabled=false;
  var disc=AncientDiscArt.Create(target);disc.transform.position=pos;disc.transform.rotation=Quaternion.Euler(90,0,0);
  var sc=target.lossyScale;disc.transform.localScale=new Vector3(.43f/Mathf.Abs(sc.x),.43f/Mathf.Abs(sc.y),.43f/Mathf.Abs(sc.z));
 }
 void RoofSupports(){
  var roofs=new List<Renderer>();foreach(var r in game.world.GetComponentsInChildren<Renderer>())if(r.name.StartsWith("MirrorRoof"))roofs.Add(r);if(roofs.Count==0)return;
  Bounds b=roofs[0].bounds;foreach(var r in roofs)b.Encapsulate(r.bounds);
  float y=b.min.y-.12f;var root=new GameObject("Mirror hall roof structure").transform;root.SetParent(transform,false);
  foreach(float x in new[]{b.min.x+.38f,b.max.x-.38f}){
   Shape("Continuous roof beam",PrimitiveType.Cube,root,new Vector3(x,y,b.center.z),new Vector3(.22f,.24f,b.size.z),wood);
   foreach(float z in new[]{b.min.z+.4f,b.max.z-.4f}){
    float h=Mathf.Max(.2f,y-.12f);
    Shape("Roof column",PrimitiveType.Cylinder,root,new Vector3(x,h*.5f,z),new Vector3(.34f,h*.5f,.34f),stone,true);
    Shape("Column capital",PrimitiveType.Cube,root,new Vector3(x,y-.19f,z),new Vector3(.58f,.20f,.58f),stone);
   }
  }
  foreach(float z in new[]{b.min.z+.4f,b.max.z-.4f})Shape("Cross beam",PrimitiveType.Cube,root,new Vector3(b.center.x,y,z),new Vector3(b.size.x,.24f,.22f),wood);
  Vector3 q=game.Node("A_Shutter").position;
  foreach(float z in new[]{q.z-1.12f,q.z+1.12f})Shape("Shutter guide rail",PrimitiveType.Cube,root,new Vector3(q.x+1.9f,q.y-.10f,z),new Vector3(6,.10f,.10f),bronze);
 }
 void Lighting(){
  RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.34f,.43f,.44f);RenderSettings.ambientEquatorColor=new Color(.23f,.29f,.25f);RenderSettings.ambientGroundColor=new Color(.13f,.14f,.10f);
  foreach(var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))if(l.type==LightType.Directional){l.color=new Color(1,.90f,.72f);l.intensity=1.05f;l.shadows=LightShadows.Soft;l.shadowStrength=.75f;}QualitySettings.shadowDistance=55;
  for(int i=0;i<7;i++){
   var o=new GameObject("Forest light shaft");o.transform.SetParent(transform,false);var line=o.AddComponent<LineRenderer>();line.sharedMaterial=new Material(Resources.Load<Shader>("Art/SoftBeam"));line.positionCount=2;
   Vector3 bottom=new Vector3(i%2==0?-26:26,0,-25+i*8);line.SetPosition(0,bottom+new Vector3(-4,12,1));line.SetPosition(1,bottom);
   line.startWidth=.35f;line.endWidth=2.1f;line.startColor=new Color(1,.88f,.60f,.025f);line.endColor=new Color(1,.91f,.72f,.055f);line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;line.receiveShadows=false;
  }
 }
 void Update(){
  bool[] pulled={game.state.gate,game.state.shutter,game.state.machine};
  for(int i=0;i<handles.Count;i++){angles[i]=Mathf.MoveTowards(angles[i],pulled[i]?145:25,120*Time.deltaTime);handles[i].localRotation=Quaternion.Euler(angles[i],0,0);}
 }
}
}

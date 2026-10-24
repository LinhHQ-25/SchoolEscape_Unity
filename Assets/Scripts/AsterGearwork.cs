using UnityEngine;
using System.Collections.Generic;
namespace Aster {
public static class AsterGearwork {
 public static readonly float[] Pitch={.63f,.525f,.42f};
 static readonly Mesh[] meshes=new Mesh[3];static readonly Texture2D[] icons=new Texture2D[3];static Material metal;
 public static Material Metal {get {if(!metal){metal=new Material(Resources.Load<Shader>("Art/AgedBronze"));metal.color=new Color(.39f,.28f,.12f);}return metal;}}
 public static GameObject Visual(Transform parent,int i){
  var o=new GameObject("Ancient bronze gear "+i);o.transform.SetParent(parent,false);o.AddComponent<MeshFilter>().sharedMesh=Geometry(i);o.AddComponent<MeshRenderer>().sharedMaterial=Metal;return o;
 }
 public static void Install(Expedition game){
  float x=game.Node("A_Gear0").position.x;
  for(int i=0;i<3;i++){
   if(i>0)x-=Pitch[i-1]+Pitch[i];
   Transform gear=game.Node("A_Gear"+i),socket=game.Node("I_Socket"+i);
   Vector3 p=gear.position;p.x=x;gear.position=p;p=socket.position;p.x=x;socket.position=p;
   Replace(gear,i,false);Replace(game.Node("I_Gear"+i),i,true);
  }
 }
 static void Replace(Transform target,int i,bool flat){
  foreach(var r in target.GetComponentsInChildren<Renderer>(true))r.enabled=false;
  foreach(var c in target.GetComponentsInChildren<Collider>(true))c.enabled=false;
  target.rotation=Quaternion.identity;
  var o=Visual(target,i);Vector3 s=target.lossyScale;o.transform.localScale=new Vector3(1/Mathf.Abs(s.x),1/Mathf.Abs(s.y),1/Mathf.Abs(s.z));
  o.transform.rotation=flat?Quaternion.Euler(90,0,0):Quaternion.Euler(0,0,i==1?12:0);
  if(flat){var box=o.AddComponent<BoxCollider>();box.size=new Vector3((Pitch[i]+.05f)*2,(Pitch[i]+.05f)*2,.18f);}
 }
 public static Mesh Geometry(int gear){
  if(meshes[gear])return meshes[gear];
  float pitch=Pitch[gear],inner=pitch*.62f,hole=pitch*.15f;int count=(18-gear*3)*8;
  var v=new List<Vector3>();var t=new List<int>();
  // Annular gear body, chamfered tooth tips, through-hole and open spoke windows.
  for(int side=0;side<2;side++)for(int ring=0;ring<2;ring++)for(int k=0;k<count;k++){
   float a=k*Mathf.PI*2/count;int q=k%8;
   float r=ring==1?inner:pitch+(q==0||q==7?-.065f:q==1||q==6?-.035f:q==2||q==5?.035f:.05f);
   v.Add(new Vector3(Mathf.Cos(a)*r,Mathf.Sin(a)*r,side==0?-.075f:.075f));
  }
  for(int k=0;k<count;k++){
   int n=(k+1)%count;
   t.AddRange(new[]{k,count+n,n,k,count+k,count+n,2*count+k,2*count+n,3*count+n,2*count+k,3*count+n,3*count+k});
   t.AddRange(new[]{k,n,2*count+n,k,2*count+n,2*count+k,count+k,3*count+n,count+n,count+k,3*count+k,3*count+n});
  }
  // Six broad radial spokes and an annular hub.
  for(int j=0;j<6;j++){float a=j*Mathf.PI/3;Vector2 d=new Vector2(Mathf.Cos(a),Mathf.Sin(a)),n=new Vector2(-d.y,d.x)*pitch*.085f;
   Extrude(v,t,new[]{d*(hole*1.2f)-n,d*(inner+.015f)-n,d*(inner+.015f)+n,d*(hole*1.2f)+n},.12f);
  }
  for(int j=0;j<48;j++){float a=j*Mathf.PI/24,b=(j+1)*Mathf.PI/24;
   Extrude(v,t,new[]{new Vector2(Mathf.Cos(a),Mathf.Sin(a))*hole,new Vector2(Mathf.Cos(b),Mathf.Sin(b))*hole,new Vector2(Mathf.Cos(b),Mathf.Sin(b))*pitch*.28f,new Vector2(Mathf.Cos(a),Mathf.Sin(a))*pitch*.28f},.20f);
  }
  var mesh=new Mesh();mesh.name="Shared gear with axle hole and spokes";mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.RecalculateNormals();mesh.RecalculateBounds();meshes[gear]=mesh;return mesh;
 }
 static void Extrude(List<Vector3> v,List<int> t,Vector2[] p,float depth){
  int b=v.Count;for(int s=0;s<2;s++)foreach(var a in p)v.Add(new Vector3(a.x,a.y,(s==0?-1:1)*depth*.5f));
  t.AddRange(new[]{b,b+2,b+1,b,b+3,b+2,b+4,b+5,b+6,b+4,b+6,b+7});
  for(int k=0;k<4;k++){int n=(k+1)%4;t.AddRange(new[]{b+k,b+n,b+n+4,b+k,b+n+4,b+k+4});}
 }
 public static Texture2D Icon(int i){
  if(icons[i])return icons[i];
  var stage=new GameObject("Gear inventory portrait stage");stage.transform.position=new Vector3(0,1000,0);
  var gear=Visual(stage.transform,i);gear.layer=30;gear.transform.localRotation=Quaternion.Euler(15,20,8);
  var cameraObject=new GameObject("Portrait camera");cameraObject.transform.SetParent(stage.transform,false);cameraObject.transform.localPosition=new Vector3(0,0,-3);
  var cam=cameraObject.AddComponent<Camera>();cam.enabled=false;cam.orthographic=true;cam.orthographicSize=Pitch[i]+.17f;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=Color.clear;cam.cullingMask=1<<30;cam.nearClipPlane=.1f;cam.farClipPlane=5;
  var lamp=new GameObject("Portrait light");lamp.transform.SetParent(stage.transform,false);lamp.transform.rotation=Quaternion.Euler(25,-35,0);var light=lamp.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.3f;light.cullingMask=1<<30;
  var rt=RenderTexture.GetTemporary(192,192,24,RenderTextureFormat.ARGB32);var previous=RenderTexture.active;cam.targetTexture=rt;
  try{cam.Render();RenderTexture.active=rt;var image=new Texture2D(192,192,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,192,192),0,0);image.Apply();icons[i]=image;}
  finally{cam.targetTexture=null;RenderTexture.active=previous;RenderTexture.ReleaseTemporary(rt);stage.SetActive(false);Object.Destroy(stage);}
  return icons[i];
 }
}
}

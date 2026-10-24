using UnityEngine;
using System.Collections.Generic;
namespace Aster {
public static class AncientDiscArt {
 static Material stone,ink;static Mesh body,front,back;
 public static GameObject Create(Transform parent) {
  if(!stone){stone=new Material(Resources.Load<Shader>("Art/AgedStone"));stone.color=new Color(.43f,.40f,.31f);stone.mainTexture=Resources.Load<Texture2D>("Restoration/StoneGrain");ink=new Material(Shader.Find("Standard"));ink.color=new Color(.13f,.115f,.075f);ink.SetFloat("_Glossiness",.04f);Build();}
  var root=new GameObject("Carved ancient stone disc");root.transform.SetParent(parent,false);
  Part(root.transform,"Weathered stone",body,stone);Part(root.transform,"Front carved spirals",front,ink);Part(root.transform,"Back clue pictograms",back,ink);return root;
 }
 static void Part(Transform p,string name,Mesh mesh,Material mat){var o=new GameObject(name);o.transform.SetParent(p,false);o.AddComponent<MeshFilter>().sharedMesh=mesh;o.AddComponent<MeshRenderer>().sharedMaterial=mat;}
 static Mesh MakeMesh(List<Vector3> v,List<int> t){var m=new Mesh();m.SetVertices(v);m.SetTriangles(t,0);m.RecalculateNormals();m.RecalculateBounds();return m;}
 static void Build(){
  int n=128;var v=new List<Vector3>();var t=new List<int>();
  for(int j=0;j<4;j++)for(int i=0;i<n;i++){
   float a=i*Mathf.PI*2/n,r=.75f+.012f*Mathf.Sin(3*a+.8f)+.008f*Mathf.Sin(7*a)-.006f*Mathf.Cos(11*a);
   r-=.035f*Mathf.Exp(-Mathf.Pow(Mathf.DeltaAngle(a*Mathf.Rad2Deg,53)/7,2));if(j==0||j==3)r-=.028f;
   v.Add(new Vector3(Mathf.Cos(a)*r,Mathf.Sin(a)*r,new[]{-.075f,-.045f,.045f,.075f}[j]));
  }
  v.Add(new Vector3(0,0,-.075f));v.Add(new Vector3(0,0,.075f));
  for(int j=0;j<3;j++)for(int i=0;i<n;i++){int a=j*n+i,b=j*n+(i+1)%n,c=(j+1)*n+i,d=(j+1)*n+(i+1)%n;t.AddRange(new[]{a,b,c,b,d,c});}
  for(int i=0;i<n;i++)t.AddRange(new[]{4*n,(i+1)%n,i,4*n+1,3*n+i,3*n+(i+1)%n});
  body=MakeMesh(v,t);front=Ornament(false);back=Ornament(true);
 }
 static Mesh Ornament(bool reverse){
  var v=new List<Vector3>();var t=new List<int>();float z=reverse?.077f:-.077f;
  System.Action<Vector2,Vector2,float> line=(a,b,w)=>{
   if(reverse){a.x=-a.x;b.x=-b.x;}Vector2 d=b-a,n=new Vector2(-d.y,d.x).normalized*w*.5f;int k=v.Count;
   v.Add(new Vector3(a.x+n.x,a.y+n.y,z));v.Add(new Vector3(b.x+n.x,b.y+n.y,z));v.Add(new Vector3(b.x-n.x,b.y-n.y,z));v.Add(new Vector3(a.x-n.x,a.y-n.y,z));
   // Thin relief strokes, on the visible surface of either face.
   if(reverse)t.AddRange(new[]{k+2,k+1,k,k+3,k+2,k});else t.AddRange(new[]{k,k+1,k+2,k,k+2,k+3});
  };
  System.Action<Vector2,float,float,float> arc=(c,r,start,end)=>{
   for(int i=0;i<64;i++){float a=Mathf.Lerp(start,end,i/64f),b=Mathf.Lerp(start,end,(i+1)/64f);line(c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r,c+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*r,.009f);}
  };
  arc(Vector2.zero,.65f,.12f,5.99f);arc(Vector2.zero,.60f,.25f,6.12f);
  for(int i=0;i<28;i++){if(i==4||i==17)continue;float a=i*Mathf.PI*2/28;Vector2 p=new Vector2(Mathf.Cos(a),Mathf.Sin(a)),q=new Vector2(-p.y,p.x);line(p*.615f-q*.024f,p*.642f,.008f);line(p*.642f,p*.615f+q*.024f,.008f);}
  if(!reverse){
   for(int k=0;k<4;k++){float phase=k*Mathf.PI*.5f;Vector2 c=new Vector2(Mathf.Cos(phase),Mathf.Sin(phase))*.28f;
    for(int j=0;j<80;j++){float a=j*.10f,b=(j+1)*.10f;line(c+new Vector2(Mathf.Cos(a+phase),Mathf.Sin(a+phase))*(.012f+j*.0025f),c+new Vector2(Mathf.Cos(b+phase),Mathf.Sin(b+phase))*(.012f+(j+1)*.0025f),.012f);}}
   for(int k=0;k<8;k++){float a=k*Mathf.PI/4;line(new Vector2(Mathf.Cos(a),Mathf.Sin(a))*.065f,new Vector2(Mathf.Cos(a+.3f),Mathf.Sin(a+.3f))*.14f,.012f);}
  }else{
   Vector2 c=new Vector2(-.36f,.05f);
   for(int i=0;i<40;i++){float a=i*Mathf.PI*2/40,b=(i+1)*Mathf.PI*2/40;line(c+new Vector2(Mathf.Sin(a)*.085f,Mathf.Cos(a)*.17f),c+new Vector2(Mathf.Sin(b)*.085f,Mathf.Cos(b)*.17f),.012f);}
   line(c+Vector2.down*.17f,c+Vector2.up*.17f,.012f);
   for(int j=-1;j<=1;j++){line(c+Vector2.up*(j*.07f),c+new Vector2(.07f,j*.07f+.05f),.008f);line(c+Vector2.up*(j*.07f),c+new Vector2(-.07f,j*.07f+.05f),.008f);}
   for(int row=-1;row<=1;row++)for(int i=0;i<25;i++){float x=-.12f+i*.24f/25,xx=x+.24f/25;line(new Vector2(x,.05f+row*.075f+.023f*Mathf.Sin(x*32)),new Vector2(xx,.05f+row*.075f+.023f*Mathf.Sin(xx*32)),.012f);}
   c=new Vector2(.36f,.05f);arc(c,.075f,0,Mathf.PI*2);
   for(int j=0;j<10;j++){float a=j*Mathf.PI/5;var d=new Vector2(Mathf.Cos(a),Mathf.Sin(a));line(c+d*.105f,c+d*.16f,.012f);}
   for(int j=0;j<2;j++){float x=-.19f+j*.38f;line(new Vector2(x-.025f,-.20f),new Vector2(x+.025f,-.20f),.009f);line(new Vector2(x+.025f,-.20f),new Vector2(x,-.175f),.009f);}
   for(int j=0;j<3;j++){float x=-.2f+j*.2f;line(new Vector2(x-.045f,-.36f),new Vector2(x,-.30f),.01f);line(new Vector2(x,-.30f),new Vector2(x+.045f,-.36f),.01f);}
  }
  line(new Vector2(-.49f,.46f),new Vector2(-.44f,.34f),.006f);line(new Vector2(-.44f,.34f),new Vector2(-.47f,.27f),.005f);
  return MakeMesh(v,t);
 }
}
}

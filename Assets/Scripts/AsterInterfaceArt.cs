using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace Aster {
public class AsterSafeArea:MonoBehaviour {
 void LateUpdate(){var r=(RectTransform)transform;var parent=(RectTransform)r.parent;var safe=Screen.safeArea;float unit=parent.rect.width/Screen.width;float scale=Mathf.Min(safe.width*unit/1600f,safe.height*unit/900f);r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.pivot=new Vector2(.5f,.5f);r.sizeDelta=new Vector2(1600,900);r.localScale=Vector3.one*scale;r.anchoredPosition=new Vector2((safe.center.x-Screen.width*.5f)*unit,(safe.center.y-Screen.height*.5f)*unit);}
}
public class AsterButtonMotion:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerDownHandler,IPointerUpHandler {
 bool hover,press;public void OnPointerEnter(PointerEventData e){hover=true;}public void OnPointerExit(PointerEventData e){hover=press=false;}public void OnPointerDown(PointerEventData e){press=true;}public void OnPointerUp(PointerEventData e){press=false;}
 void Update(){var b=GetComponent<Button>();float target=b&&b.interactable?(press?.98f:hover?1.018f:1):1;transform.localScale=Vector3.Lerp(transform.localScale,Vector3.one*target,1-Mathf.Exp(-18*Time.unscaledDeltaTime));}
}
public class AsterIcon:Graphic {
 public string kind="sun";VertexHelper mesh;
 void Line(float x,float y,float a,float b,float width=.055f){Vector2 p=new Vector2(x,y),q=new Vector2(a,b),n=(q-p).normalized;var side=new Vector2(-n.y,n.x)*width*.5f;int k=mesh.currentVertCount;Point(p+side);Point(q+side);Point(q-side);Point(p-side);mesh.AddTriangle(k,k+1,k+2);mesh.AddTriangle(k,k+2,k+3);}
 void Point(Vector2 p){var r=rectTransform.rect;mesh.AddVert(new Vector3(r.x+p.x*r.width,r.y+p.y*r.height,0),color,Vector2.zero);}
 void Ring(float x,float y,float radius,int count=32){for(int i=0;i<count;i++){float a=i*Mathf.PI*2/count,b=(i+1)*Mathf.PI*2/count;Line(x+Mathf.Cos(a)*radius,y+Mathf.Sin(a)*radius,x+Mathf.Cos(b)*radius,y+Mathf.Sin(b)*radius);}}
 void Box(float x,float y,float w,float h){Line(x,y,x+w,y);Line(x+w,y,x+w,y+h);Line(x+w,y+h,x,y+h);Line(x,y+h,x,y);}
 protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();mesh=vh;switch(kind){
 case "pause":Line(.34f,.22f,.34f,.78f,.12f);Line(.66f,.22f,.66f,.78f,.12f);break;
 case "check":Line(.15f,.48f,.4f,.22f,.08f);Line(.4f,.22f,.88f,.8f,.08f);break;
 case "book":Box(.13f,.18f,.74f,.64f);Line(.5f,.18f,.5f,.82f);Line(.23f,.64f,.39f,.64f);Line(.6f,.64f,.77f,.64f);Line(.23f,.46f,.39f,.46f);Line(.6f,.46f,.77f,.46f);break;
 case "bag":Box(.2f,.12f,.6f,.65f);Line(.35f,.77f,.35f,.92f);Line(.35f,.92f,.65f,.92f);Line(.65f,.92f,.65f,.77f);Box(.32f,.25f,.36f,.2f);break;
 case "lamp":Box(.37f,.1f,.26f,.43f);Line(.37f,.53f,.22f,.75f);Line(.63f,.53f,.78f,.75f);Line(.22f,.75f,.78f,.75f);Line(.5f,.85f,.5f,.99f);break;
 case "leaf":Line(.16f,.1f,.75f,.83f);Line(.22f,.23f,.17f,.55f);Line(.17f,.55f,.42f,.8f);Line(.42f,.8f,.85f,.91f);Line(.85f,.91f,.8f,.48f);Line(.8f,.48f,.55f,.23f);Line(.55f,.23f,.22f,.23f);Line(.4f,.4f,.38f,.64f);Line(.53f,.55f,.73f,.58f);break;
 case "water":for(int j=0;j<3;j++)for(int i=0;i<18;i++){float x=.1f+i*.8f/18,a=.1f+(i+1)*.8f/18;Line(x,.25f+j*.25f+Mathf.Sin(x*10)*.055f,a,.25f+j*.25f+Mathf.Sin(a*10)*.055f);}break;
 case "compass":Ring(.5f,.5f,.4f);Line(.35f,.25f,.45f,.6f);Line(.45f,.6f,.65f,.75f);Line(.65f,.75f,.55f,.4f);Line(.55f,.4f,.35f,.25f);break;
 default:Ring(.5f,.5f,kind=="gear"?.27f:.22f);if(kind=="gear")Ring(.5f,.5f,.1f);for(int i=0;i<10;i++){float a=i*Mathf.PI/5;Line(.5f+Mathf.Cos(a)*.33f,.5f+Mathf.Sin(a)*.33f,.5f+Mathf.Cos(a)*.46f,.5f+Mathf.Sin(a)*.46f,kind=="gear"?.12f:.05f);}break;
 }}
}
}

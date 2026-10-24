using UnityEngine;
using UnityEngine.EventSystems;
namespace Aster {
public class TouchSurface:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IDragHandler {
 public bool look; public Vector2 value,delta; public RectTransform knob; int pointer=int.MinValue;Vector2 start;
 public void OnPointerDown(PointerEventData e){if(pointer!=int.MinValue)return;pointer=e.pointerId;start=e.position;}
 public void OnDrag(PointerEventData e){if(e.pointerId!=pointer)return;if(look)delta+=e.delta;else{value=Vector2.ClampMagnitude((e.position-start)/(Screen.height*.085f),1);if(knob)knob.anchoredPosition=value*55;}}
 public void OnPointerUp(PointerEventData e){if(pointer==e.pointerId)Reset();}
 void OnDisable(){Reset();} void Reset(){pointer=int.MinValue;value=delta=Vector2.zero;if(knob)knob.anchoredPosition=Vector2.zero;}
 public Vector2 Consume(){var v=delta;delta=Vector2.zero;return v;}
}
}

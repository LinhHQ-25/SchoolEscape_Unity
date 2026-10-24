using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
namespace Aster {
public class Explorer:MonoBehaviour {
 public Expedition game;public Camera cam;public TouchSurface stick,look;public Interaction focus;public float sensitivity=1;
 bool menuView;Vector3 savedCameraPosition;Quaternion savedCameraRotation;
 CharacterController motor;float pitch,vertical,steps;Light lamp;
 Vector2 pendingLook;
 public void Setup(Expedition g){game=g;motor=gameObject.AddComponent<CharacterController>();motor.height=1.75f;motor.radius=.28f;motor.center=Vector3.up*.9f;motor.stepOffset=.25f;
 var c=new GameObject("Explorer Camera");c.transform.SetParent(transform,false);c.transform.localPosition=Vector3.up*1.62f;cam=c.AddComponent<Camera>();cam.nearClipPlane=.07f;cam.farClipPlane=130;cam.fieldOfView=68;c.tag="MainCamera";c.AddComponent<AudioListener>();lamp=c.AddComponent<Light>();lamp.type=LightType.Spot;lamp.range=14;lamp.spotAngle=60;lamp.intensity=2.5f;lamp.enabled=false;sensitivity=PlayerPrefs.GetFloat("look",1);}
 void Update(){if (game == null || game.mode != GameMode.Play)
{
    focus = null;
    pendingLook = Vector2.zero;
    return;
}
 var k=Keyboard.current;var m=Mouse.current;
 Vector2 mv=stick?stick.value:Vector2.zero,d=look?look.Consume():Vector2.zero;
 if(k!=null){
    bool right = k.dKey.isPressed || k.rightArrowKey.isPressed;
bool left = k.aKey.isPressed || k.leftArrowKey.isPressed;
bool forward = k.wKey.isPressed || k.upArrowKey.isPressed;
bool backward = k.sKey.isPressed || k.downArrowKey.isPressed;

mv += new Vector2(
    (right ? 1f : 0f) - (left ? 1f : 0f),
    (forward ? 1f : 0f) - (backward ? 1f : 0f)
);
    if(k.eKey.wasPressedThisFrame)Interact();if(k.iKey.wasPressedThisFrame||k.tabKey.wasPressedThisFrame)game.ui.Inventory();if(k.escapeKey.wasPressedThisFrame)game.ui.Pause();if(k.fKey.wasPressedThisFrame)ToggleLamp();}
 if(m!=null&&Cursor.lockState==CursorLockMode.Locked)d+=m.delta.ReadValue();
 // Tích lũy chuyển động chuột và áp dụng dần để giảm giật.
pendingLook += d * (0.11f * sensitivity);

float blend = 1f - Mathf.Exp(-35f * Time.unscaledDeltaTime);
Vector2 lookStep = pendingLook * blend;
pendingLook -= lookStep;

transform.Rotate(0f, lookStep.x, 0f);

pitch = Mathf.Clamp(pitch - lookStep.y, -75f, 75f);
cam.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
 if(motor.isGrounded&&vertical<0)vertical=-2;vertical-=18*Time.deltaTime;mv=Vector2.ClampMagnitude(mv,1);float speed=k!=null&&k.leftShiftKey.isPressed?4.4f:2.8f;motor.Move((transform.right*mv.x*speed+transform.forward*mv.y*speed+Vector3.up*vertical)*Time.deltaTime);
 if (k != null && k.f8Key.wasPressedThisFrame)
 {
     Debug.Log($"MOVE CHECK | input={mv} | timeScale={Time.timeScale} | dt={Time.deltaTime} | position={transform.position} | velocity={motor.velocity} | collision={motor.collisionFlags} | enabled={motor.enabled}");
 }

 if(new Vector2(motor.velocity.x,motor.velocity.z).sqrMagnitude>.04f&&motor.isGrounded){steps-=Time.deltaTime;if(steps<=0){game.sound.Play("step",.27f);steps=.48f;}}
 RaycastHit hit;focus=Physics.Raycast(cam.ViewportPointToRay(new Vector3(.5f,.5f)),out hit,3.1f,~0,QueryTriggerInteraction.Ignore)?hit.collider.GetComponentInParent<Interaction>():null;game.ui.Focus(focus?game.Label(focus.id):"");
 if(transform.position.y<-5)Teleport(game.Anchor("P_Spawn"),game.Anchor("P_Look"));}
 // Hold a movement key and press F8 to report contacted colliders.
 void OnControllerColliderHit(ControllerColliderHit hit)
 {
     if (Keyboard.current == null || !Keyboard.current.f8Key.wasPressedThisFrame)
         return;

     string objectPath = hit.collider.name;
     Transform parent = hit.collider.transform.parent;
     while (parent != null)
     {
         objectPath = parent.name + "/" + objectPath;
         parent = parent.parent;
     }
     Debug.Log($"MOVE HIT | object={objectPath} | normal={hit.normal}");
 }

 public void Interact(){if(focus&&game.mode==GameMode.Play)game.Act(focus.id);}
 public void ToggleLamp(){if(!game.state.flashlight){game.ui.Toast("Tìm đèn pin ở bàn trại khảo sát.");return;}lamp.enabled=!lamp.enabled;game.sound.Play("click");}
 public void MenuView(bool show){if(show==menuView)return;menuView=show;if(show){savedCameraPosition=cam.transform.localPosition;savedCameraRotation=cam.transform.localRotation;Transform spot=game.world.Find("P_Menu"),target=game.world.Find("P_MenuLook");if(spot&&target){cam.transform.position=spot.position;cam.transform.rotation=Quaternion.LookRotation(target.position-spot.position);}}else{cam.transform.localPosition=savedCameraPosition;cam.transform.localRotation=savedCameraRotation;}}
 public void Teleport(Vector3 p,Vector3 target){pendingLook=Vector2.zero;MenuView(false);motor.enabled=false;transform.position=p+Vector3.up*.06f;Vector3 dir=target-p;dir.y=0;if(dir.sqrMagnitude>.01f)transform.rotation=Quaternion.LookRotation(dir);pitch=0;vertical=0;cam.transform.localRotation=Quaternion.identity;motor.enabled=true;}
}
}
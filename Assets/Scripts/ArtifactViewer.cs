using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.XR.CoreUtils;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using System.Collections;
namespace Aster {
public class ArtifactViewer:MonoBehaviour {
 public Expedition game;GameObject rig,model;ARTrackedImageManager tracker;Camera cameraView;bool real,backSeen,tracking,opening;float yaw,zoom=1;
 readonly System.Collections.Generic.List<Renderer> inscriptions = new System.Collections.Generic.List<Renderer>();
 public bool CanRecord { get { return tracking && backSeen; } }Coroutine request;
 public string TrackingStatus {get{return opening?"Đang chuẩn bị camera…":tracking?"Đã nhận thẻ • Xoay đĩa để tìm manh mối":"Đưa toàn bộ thẻ vào camera • Giữ máy ổn định";}}
 public void Open(bool ar){if(opening)return;if(ar&&(Application.platform!=RuntimePlatform.Android||Application.isEditor)){game.ui.Toast("AR thực cần bản Android trên thiết bị hỗ trợ ARCore.");return;}real=ar;yaw=0;zoom=1;backSeen=false;tracking=!ar;game.ui.InspectHUD(ar);game.player.cam.gameObject.SetActive(false);if(ar){opening=true;request=StartCoroutine(StartAR());}else StartViewer();}
 void StartViewer(){rig=new GameObject("Artifact 3D viewer");var c=new GameObject("Viewer Camera");c.transform.SetParent(rig.transform);c.transform.position=new Vector3(0,100,-3);cameraView=c.AddComponent<Camera>();cameraView.clearFlags=CameraClearFlags.SolidColor;cameraView.backgroundColor=new Color(.025f,.11f,.13f);cameraView.cullingMask=1<<8;cameraView.nearClipPlane=.01f;cameraView.farClipPlane=10;cameraView.fieldOfView=48;c.AddComponent<AudioListener>();var l=new GameObject("Artifact Light");l.transform.SetParent(rig.transform);var light=l.AddComponent<Light>();light.type=LightType.Directional;light.intensity=.9f;light.cullingMask=1<<8;l.transform.rotation=Quaternion.Euler(35,-35,0);CreateDisc();model.transform.position=new Vector3(0,100,0);}
 IEnumerator StartAR(){
 #if UNITY_ANDROID
 if(!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera)){bool answer=false;var cb=new UnityEngine.Android.PermissionCallbacks();cb.PermissionGranted+=_=>answer=true;cb.PermissionDenied+=_=>answer=true;cb.PermissionDeniedAndDontAskAgain+=_=>answer=true;UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Camera,cb);float elapsed=0;while(!answer&&elapsed<60){elapsed+=Time.unscaledDeltaTime;yield return null;}if(!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera)){Fail("Cần quyền camera để dùng AR. Bạn vẫn có thể xem hiện vật 3D.");yield break;}}
 #endif
 yield return ARSession.CheckAvailability();if(ARSession.state==ARSessionState.NeedsInstall)yield return ARSession.Install();if(ARSession.state==ARSessionState.Unsupported||ARSession.state==ARSessionState.NeedsInstall){Fail("Thiết bị chưa hỗ trợ AR. Hãy dùng trình xem 3D.");yield break;}
 var lib=Resources.Load<XRReferenceImageLibrary>("AR/DiscLibrary");if(!lib){Fail("Thiếu thư viện thẻ AR. Trong Unity chọn Aster / Prepare project.");yield break;}
 rig=new GameObject("AR Artifact Session");rig.SetActive(false);rig.AddComponent<ARSession>();var originObj=new GameObject("XR Origin");originObj.transform.SetParent(rig.transform);var origin=originObj.AddComponent<XROrigin>();var offset=new GameObject("Camera Offset");offset.transform.SetParent(originObj.transform);origin.CameraFloorOffsetObject=offset;var c=new GameObject("AR Camera");c.transform.SetParent(offset.transform);c.tag="MainCamera";cameraView=c.AddComponent<Camera>();cameraView.nearClipPlane=.01f;cameraView.farClipPlane=20;cameraView.cullingMask=1<<8;cameraView.clearFlags=CameraClearFlags.SolidColor;c.AddComponent<AudioListener>();c.AddComponent<ARCameraManager>();c.AddComponent<ARCameraBackground>();var pose=c.AddComponent<TrackedPoseDriver>();pose.positionInput=new InputActionProperty(new InputAction("Position",InputActionType.Value,"<XRHMD>/centerEyePosition"));pose.rotationInput=new InputActionProperty(new InputAction("Rotation",InputActionType.Value,"<XRHMD>/centerEyeRotation"));origin.Camera=cameraView;
 tracker=originObj.AddComponent<ARTrackedImageManager>();tracker.referenceLibrary=lib;tracker.requestedMaxNumberOfMovingImages=1;CreateDisc();model.SetActive(false);rig.SetActive(true);opening=false;game.ui.Toast("Hướng camera vào thẻ in. Khi hiện vật xuất hiện, xoay để xem mặt sau.",8);}
 void Fail(string s){opening=false;Close();game.ui.Toast(s,8);}
 void CreateDisc(){inscriptions.Clear();model=AncientDiscArt.Create(rig.transform);
 foreach(var t in model.GetComponentsInChildren<Transform>())t.gameObject.layer=8;}
 void Circle(string name,float r,float z,Material m){var p=new Vector3[65];for(int i=0;i<=64;i++){float a=i*Mathf.PI*2/64;p[i]=new Vector3(Mathf.Cos(a)*r,Mathf.Sin(a)*r,z);}Line(p,m);}
 void Line(Vector3[] points,Material mat){var o=new GameObject("Engraving");o.transform.SetParent(model.transform,false);var line=o.AddComponent<LineRenderer>();line.useWorldSpace=false;line.sharedMaterial=mat;line.widthMultiplier=.018f;line.positionCount=points.Length;line.SetPositions(points);}
 void BackText(string value,Vector3 p,float size){var o=new GameObject("Inscription");o.transform.SetParent(model.transform,false);o.transform.localPosition=p;o.transform.localRotation=Quaternion.Euler(0,180,0);var t=o.AddComponent<TextMesh>();t.text=value;t.font=Resources.Load<Font>("Fonts/Body");t.fontSize=64;t.characterSize=size;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.color=new Color(.23f,.18f,.12f);
 var textRenderer=o.GetComponent<MeshRenderer>();
 if(t.font){var ink=new Material(Resources.Load<Shader>("Art/Engraving"));ink.mainTexture=t.font.material.mainTexture;ink.color=new Color(.18f,.14f,.08f);textRenderer.sharedMaterial=ink;}
 float width=textRenderer.bounds.size.x;
 if(width>1.12f)o.transform.localScale=Vector3.one*(1.12f/width);
 textRenderer.enabled=false;
 inscriptions.Add(textRenderer);}
 public void Rotate(float degrees){yaw=Mathf.Repeat(yaw+degrees,360);if(tracking&&yaw>=130&&yaw<=230)backSeen=true;game.sound.Play("stone",.25f);}
 public void ShowBack(){Rotate(Mathf.DeltaAngle(yaw,180f));}
 public void Zoom(float delta){zoom=Mathf.Clamp(zoom+delta,.65f,1.1f);}
 void Update(){if(!model)return;if(real){tracking=false;if(tracker)foreach(var t in tracker.trackables){if(t.trackingState!=TrackingState.Tracking)continue;tracking=true;model.transform.position=t.transform.position+t.transform.up*.09f;model.transform.rotation=t.transform.rotation*Quaternion.Euler(90,yaw,0);model.transform.localScale=Vector3.one*.15f*zoom;break;}model.SetActive(tracking);}else{model.transform.rotation=Quaternion.Euler(0,yaw,0);model.transform.localScale=Vector3.one*zoom;}if(tracking&&yaw>=130&&yaw<=230)backSeen=true;}
 void LateUpdate()
 {
     if (!model || !cameraView) return;
     // TextMesh's font material can draw through geometry. Only display text
     // when its inscribed face is directed towards the actual viewer camera.
     foreach (var inscription in inscriptions)
     {
         if (!inscription) continue;
         Vector3 towardCamera = (cameraView.transform.position - inscription.transform.position).normalized;
         inscription.enabled = tracking && Vector3.Dot(-inscription.transform.forward, towardCamera) > 0.25f;
     }
 }
 public void Record(){if(!tracking){game.ui.Toast("Chưa nhận diện thẻ. Đưa toàn bộ thẻ vào camera.");return;}if(!backSeen){game.ui.Toast("Hãy xoay 180° để đọc mặt sau trước khi ghi nhận.");return;}game.RecordClue(real);Close();game.ui.Toast("Manh mối đã lưu: LÁ → NƯỚC → MẶT TRỜI",7);}
 public void Close(){if(request!=null){StopCoroutine(request);request=null;}opening=false;if(rig)Destroy(rig);rig=null;model=null;tracker=null;game.player.cam.gameObject.SetActive(true);game.ui.Resume();}
}
}

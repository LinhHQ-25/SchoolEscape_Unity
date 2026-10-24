using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
namespace Aster {
public enum GameMode {Menu,Play,Panel,Inspect,AR,Ending}
public class Expedition:MonoBehaviour {
 public Transform world;public ExpeditionState state=new ExpeditionState();public GameMode mode;
 public Explorer player;public ExpeditionUI ui;public ExpeditionAudio sound;public ArtifactViewer viewer;public bool Started{get;private set;} public int selectedGear=-1;
 readonly Dictionary<string,Transform> nodes=new Dictionary<string,Transform>();readonly Dictionary<string,Vector3> home=new Dictionary<string,Vector3>();readonly Dictionary<string,Quaternion> rotations=new Dictionary<string,Quaternion>();
 AsterHydraulics hydraulics;
 LineRenderer[] rays=new LineRenderer[3];AudioSource waterAudio,machineAudio;float saveTimer;
 public string SavePath{get{return Path.Combine(Application.persistentDataPath,"aster-save.json");}}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){if(!Object.FindFirstObjectByType<Expedition>())new GameObject("Expedition").AddComponent<Expedition>();}
 void Start(){if(!world){var prefab=Resources.Load<GameObject>("Art/World");if(!prefab){Debug.LogError("World.fbx missing. Run SourceArt/build_world.py with Blender.");return;}world=Instantiate(prefab).transform;world.name="World";}
 var originalWorld = Resources.Load<GameObject>("Art/World");
 foreach(var t in world.GetComponentsInChildren<Transform>(true)){nodes[t.name]=t;home[t.name]=t.localPosition;rotations[t.name]=t.localRotation;if(t.name.StartsWith("C_"))CreateWorldCollider(t, originalWorld);}
 foreach(var t in nodes.Values){if(t.name.StartsWith("I_")){var a=t.gameObject.AddComponent<Interaction>();a.id=t.name;AddBox(t);}if(t.name=="A_GateLeft"||t.name=="A_GateRight"||t.name=="A_FinalLeft"||t.name=="A_FinalRight"||t.name=="A_SideGate")AddBox(t);}
 if(!Object.FindFirstObjectByType<Light>()){var sun=new GameObject("Sun").AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=1.1f;sun.transform.rotation=Quaternion.Euler(48,-35,0);RenderSettings.ambientLight=new Color(.55f,.62f,.63f);}
 AsterAtmosphere.Install(world);sound=gameObject.AddComponent<ExpeditionAudio>();sound.Setup();player=new GameObject("Explorer").AddComponent<Explorer>();player.Setup(this);viewer=gameObject.AddComponent<ArtifactViewer>();viewer.game=this;ui=gameObject.AddComponent<ExpeditionUI>();ui.Setup(this);
 waterAudio=sound.LoopAt("water",Node("A_Waterwheel"));machineAudio=sound.LoopAt("mechanism",Node("A_Gear1"));
 for(int i=0;i<3;i++){var o=new GameObject("Sunbeam "+i);rays[i]=o.AddComponent<LineRenderer>();rays[i].material=new Material(Resources.Load<Shader>("Art/SoftBeam"));rays[i].startColor=new Color(1,.85f,.52f,.18f);rays[i].endColor=new Color(1,.90f,.68f,.06f);rays[i].startWidth=.14f;rays[i].endWidth=.25f;rays[i].positionCount=2;}
 AsterRestoration.Install(this);AsterGearwork.Install(this);hydraulics=AsterHydraulics.Install(this);AsterPolish.Install(this);AsterWaterworksPolish.Install(this);Refresh();player.Teleport(Anchor("P_Spawn"),Anchor("P_Look"));ui.MainMenu();}
 // Static batching can replace the scene MeshFilter with a combined render mesh.
 // Never use that combined mesh to generate an individual object's collider.
 void CreateWorldCollider(Transform target, GameObject originalWorld)
 {
     var filter = target.GetComponent<MeshFilter>();
     if (!filter || target.GetComponent<Collider>()) return;

     Mesh collisionMesh = filter.sharedMesh;
     var renderer = target.GetComponent<Renderer>();
     if (renderer && renderer.isPartOfStaticBatch)
     {
         string path = target.name;
         Transform parent = target.parent;
         while (parent && parent != world)
         {
             path = parent.name + "/" + path;
             parent = parent.parent;
         }
         Transform original = originalWorld ? originalWorld.transform.Find(path) : null;
         var originalFilter = original ? original.GetComponent<MeshFilter>() : null;
         if (!originalFilter || !originalFilter.sharedMesh)
         {
             Debug.LogError("ASTER: Cannot find original collision mesh for " + path +
                 ". Stop Play and check Resources/Art/World.fbx.", target);
             return;
         }
         collisionMesh = originalFilter.sharedMesh;
     }
     if (!collisionMesh) return;

     // A separate child has no MeshFilter, so AddComponent cannot automatically
     // cook the giant batched render mesh before sharedMesh is assigned.
     var collisionObject = new GameObject("AsterCollision");
     collisionObject.layer = target.gameObject.layer;
     collisionObject.transform.SetParent(target, false);
     var collider = collisionObject.AddComponent<MeshCollider>();
     collider.sharedMesh = collisionMesh;
     collider.convex = false;
 }

 // Diagnostic only: preserves all world colliders and puzzle behavior.
 IEnumerator WatchBlockedMovement()
 {
     float blockedSeconds = 0f;
     float nextReport = 0f;
     while (true)
     {
         Vector3 before = player ? player.transform.position : Vector3.zero;
         yield return null;
         if (!player || mode != GameMode.Play)
         {
             blockedSeconds = 0f;
             continue;
         }

         var keyboard = UnityEngine.InputSystem.Keyboard.current;
         bool requested = keyboard != null &&
             (keyboard.wKey.isPressed || keyboard.aKey.isPressed ||
              keyboard.sKey.isPressed || keyboard.dKey.isPressed ||
              keyboard.upArrowKey.isPressed || keyboard.downArrowKey.isPressed ||
              keyboard.leftArrowKey.isPressed || keyboard.rightArrowKey.isPressed);
         if (player.stick && player.stick.value.sqrMagnitude > 0.1f)
             requested = true;
         Vector3 travelled = player.transform.position - before;
         travelled.y = 0f;
         if (!requested || travelled.magnitude > 0.05f * Time.unscaledDeltaTime)
         {
             blockedSeconds = 0f;
             continue;
         }
         blockedSeconds += Time.unscaledDeltaTime;
         if (blockedSeconds < 1f || Time.unscaledTime < nextReport)
             continue;
         nextReport = Time.unscaledTime + 8f;
         ReportMovementObstacles();
     }
 }

 void ReportMovementObstacles()
 {
     var controller = player.GetComponent<CharacterController>();
     if (!controller) return;
     Physics.SyncTransforms();
     Transform body = controller.transform;
     Vector3 scale = body.lossyScale;
     float radius = controller.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
     float height = Mathf.Max(controller.height * Mathf.Abs(scale.y), radius * 2f);
     Vector3 center = body.TransformPoint(controller.center);
     Vector3 offset = body.up * (height * 0.5f - radius);
     Collider[] nearby = Physics.OverlapCapsule(center - offset, center + offset,
         radius + controller.skinWidth + 0.03f, ~0, QueryTriggerInteraction.Ignore);
     var report = new System.Text.StringBuilder();
     report.AppendLine("[ASTER BLOCKED] Hold movement detected, but no horizontal movement.");
     report.AppendLine($"Player={body.position:F3}; timeScale={Time.timeScale}; flags={controller.collisionFlags}");
     int count = 0;
     foreach (Collider obstacle in nearby)
     {
         if (obstacle == controller || obstacle.transform.IsChildOf(body)) continue;
         if (Physics.GetIgnoreLayerCollision(body.gameObject.layer, obstacle.gameObject.layer) ||
             Physics.GetIgnoreCollision(controller, obstacle)) continue;
         string path = obstacle.name;
         for (Transform p = obstacle.transform.parent; p != null; p = p.parent)
             path = p.name + "/" + path;
         Vector3 direction;
         float depth;
         bool overlaps = Physics.ComputePenetration(controller, body.position, body.rotation,
             obstacle, obstacle.transform.position, obstacle.transform.rotation,
             out direction, out depth);
         if (!overlaps) { direction = Vector3.zero; depth = 0f; }
         report.AppendLine($"{path} | {obstacle.GetType().Name} | overlap={overlaps} | depth={depth:F4} | push={direction:F3} | boundsCenter={obstacle.bounds.center:F3} | boundsSize={obstacle.bounds.size:F3}");
         count++;
     }
     report.AppendLine($"Nearby blocking colliders: {count}. Nearby does not by itself prove obstruction.");
     Debug.Log(report.ToString(), player);
 }

 void AddBox(Transform t){if(t.GetComponent<Collider>())return;var rr=t.GetComponentsInChildren<Renderer>(true);if(rr.Length==0)return;var b=new Bounds(t.InverseTransformPoint(rr[0].bounds.center),Vector3.zero);foreach(var r in rr){var q=r.bounds;for(int i=0;i<8;i++)b.Encapsulate(t.InverseTransformPoint(q.center+Vector3.Scale(q.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1))));}var c=t.gameObject.AddComponent<BoxCollider>();c.center=b.center;c.size=Vector3.Max(b.size,Vector3.one*.12f);}
 public Transform Node(string id){return nodes[id];}public Vector3 Anchor(string id){return Node(id).position;}
 public void SetMode(GameMode m){mode=m;Cursor.lockState=m==GameMode.Play&&!Application.isMobilePlatform?CursorLockMode.Locked:CursorLockMode.None;Cursor.visible=Cursor.lockState!=CursorLockMode.Locked;}
 public void NewGame(){state=new ExpeditionState();selectedGear=-1;Started=true;sound.ResetMusic();Refresh();SnapDoors();player.Teleport(Anchor("P_Spawn"),Anchor("P_Look"));Save();ui.Resume();}
 public bool HasSave(){return ReadSave()!=null;}
 ExpeditionState ReadSave(){foreach(var path in new[]{SavePath,SavePath+".bak"})try{if(File.Exists(path)){var s=JsonUtility.FromJson<ExpeditionState>(File.ReadAllText(path));if(s!=null&&s.Valid())return s;}}catch(System.Exception){}return null;}
 public void ContinueGame(){var s=ReadSave();if(s==null){ui.Toast("Không có bản lưu hợp lệ.");return;}state=s;Started=true;Refresh();SnapDoors();string p=s.checkpoint==0?"P_Spawn":s.checkpoint==1?"P_Courtyard":s.checkpoint==2?"P_Water":"P_End";player.Teleport(Anchor(p),Anchor(s.checkpoint==0?"P_Look":"P_Inspect"));ui.Resume();}
 public void Save(){if(!Started)return;try{Directory.CreateDirectory(Application.persistentDataPath);File.WriteAllText(SavePath+".tmp",JsonUtility.ToJson(state,true));if(File.Exists(SavePath))File.Copy(SavePath,SavePath+".bak",true);File.Copy(SavePath+".tmp",SavePath,true);File.Delete(SavePath+".tmp");}catch(System.Exception e){Debug.LogWarning(e.Message);if(ui)ui.Toast("Không lưu được tiến trình. Kiểm tra dung lượng máy.");}}
 void OnApplicationPause(bool paused){if(paused){Save();if(ui&&mode==GameMode.Play)ui.Pause();}}void OnApplicationQuit(){Save();}
 void Update(){if(!player)return;if(mode==GameMode.Play){state.seconds+=Time.deltaTime;saveTimer+=Time.deltaTime;if(saveTimer>30){Save();saveTimer=0;}}AnimateDoors(false);if(state.water)Node("A_Waterwheel").Rotate(Vector3.forward,18*Time.deltaTime,Space.Self);if(state.machine)for(int i=0;i<3;i++)Node("A_Gear"+i).Rotate(Vector3.forward,(i==1?-1:1)*(24f*.63f/AsterGearwork.Pitch[i])*Time.deltaTime,Space.Self);DrawRays();}
 void Move(string id,Vector3 offset,bool snap){var t=Node(id);var target=home[id]+offset;t.localPosition=snap?target:Vector3.Lerp(t.localPosition,target,Time.deltaTime*2);}
 void DoorPair(string a,string b,bool open,bool snap){var axis=(home[b]-home[a]).normalized;Move(a,open?-axis*1.55f:Vector3.zero,snap);Move(b,open?axis*1.55f:Vector3.zero,snap);}
 void AnimateDoors(bool snap){DoorPair("A_GateLeft","A_GateRight",state.gate,snap);DoorPair("A_FinalLeft","A_FinalRight",state.machine,snap);Move("A_Shutter",state.shutter?world.InverseTransformVector(Vector3.right*3.8f):Vector3.zero,snap);}
 void SnapDoors(){AnimateDoors(true);}
 void DrawRays(){for(int i=0;i<3;i++)rays[i].enabled=state.shutter;if(!state.shutter)return;Vector3 a=Anchor("I_Mirror0"),b=Anchor("I_Mirror1"),c=Anchor("P_Receiver");rays[0].SetPosition(0,world.TransformPoint(home["A_Shutter"])+Vector3.up*.03f);rays[0].SetPosition(1,a);rays[1].SetPosition(0,a);rays[1].SetPosition(1,state.mirrors[0]==1?b:a+Quaternion.AngleAxis((state.mirrors[0]-1)*90,Vector3.up)*(b-a));rays[2].enabled=state.mirrors[0]==1;if(rays[2].enabled){rays[2].SetPosition(0,b);rays[2].SetPosition(1,state.mirrors[1]==3?c:b+Quaternion.AngleAxis((state.mirrors[1]-3)*90,Vector3.up)*(c-b));}}
 public void Refresh(){if(nodes.Count==0)return;Node("I_Journal").gameObject.SetActive(!state.journal);Node("I_Flashlight").gameObject.SetActive(!state.flashlight);Node("I_Disc").gameObject.SetActive(!state.disc);Node("I_Handle").gameObject.SetActive(!state.handle);Node("A_PoolWater").gameObject.SetActive(state.finished);
 for(int i=0;i<3;i++){Node("I_Gear"+i).gameObject.SetActive(!state.gears[i]);Node("A_Gear"+i).gameObject.SetActive(state.installed[i]);foreach(Transform t in Node("I_Ring"+i))if(t.name.StartsWith("Symbol"))t.gameObject.SetActive(t.name.StartsWith("Symbol"+state.rings[i]));}
 for(int i=0;i<2;i++)Node("I_Mirror"+i).rotation=Quaternion.AngleAxis(state.mirrors[i]*90,Vector3.up)*(world.rotation*rotations["I_Mirror"+i]);if(waterAudio){waterAudio.volume=sound.effectsVolume*.45f;if(state.water&&!waterAudio.isPlaying)waterAudio.Play();if(!state.water)waterAudio.Stop();}if(machineAudio){machineAudio.volume=sound.effectsVolume*.4f;if(state.machine&&!machineAudio.isPlaying)machineAudio.Play();if(!state.machine)machineAudio.Stop();}if(hydraulics)hydraulics.Sync();GateSymbolArtwork.Apply(this);if(ui)ui.UpdateObjective();}
 public string Label(string id){if(id.StartsWith("I_Ring"))return "Xoay vòng ký hiệu";if(id.StartsWith("I_Mirror"))return "Xoay gương 90°";if(id.StartsWith("I_Channel"))return "Xoay đoạn kênh";if(id.StartsWith("I_Gear"))return "Nhặt bánh răng";if(id.StartsWith("I_Socket"))return "Lắp bánh răng vào ổ";if(id.StartsWith("I_Note"))return "Đọc ghi chép";switch(id){case "I_Journal":return "Nhặt nhật ký";case "I_Flashlight":return "Nhặt đèn pin";case "I_Disc":return "Nhặt đĩa đá";case "I_GateLever":return "Kéo cần mở cổng";case "I_Shutter":return "Mở mái lấy sáng";case "I_Handle":return "Nhặt tay van";case "I_Valve":return "Lắp tay van / mở nước";case "I_MachineLever":return "Khởi động máy";case "I_FinalValve":return "Khôi phục nguồn nước";default:return "Tương tác";}}
 public void Act(string id){if(mode!=GameMode.Play)return;sound.Play("click",.3f);int i=id[id.Length-1]-'0';
 if(id.StartsWith("I_Ring")){ui.ChooseGateSymbol(i);return;}
 else if(id.StartsWith("I_Mirror")){if(!state.gate)return;state.mirrors[i]=(state.mirrors[i]+1)%4;sound.Play("stone");CheckLight();}
 else if(id.StartsWith("I_Channel")){if(!state.light)return;if(state.water){ui.Toast("Các đoạn kênh đã khóa đúng vị trí để giữ dòng chảy.");return;}state.channels[i]=1-state.channels[i];sound.Play("stone");CheckWater();}
 else if(id.StartsWith("I_Gear")){if(!state.water){ui.Toast("Khôi phục dòng nước trước khi sửa máy.");return;}state.gears[i]=true;sound.Play("pickup");ui.Toast("Đã nhặt bánh răng "+new[]{"lớn","vừa","nhỏ"}[i]);}
 else if(id.StartsWith("I_Socket")){if(!state.gears[i]){ui.Toast("Ổ này cần bánh răng "+new[]{"lớn","vừa","nhỏ"}[i]+". Tìm quanh xưởng.");return;}if(selectedGear!=i){sound.Play("fail");ui.Toast("Mở Túi đồ, chọn bánh răng "+new[]{"lớn","vừa","nhỏ"}[i]+" rồi lắp vào ổ này.");return;}state.installed[i]=true;selectedGear=-1;sound.Play("metal");ui.Toast("Đã lắp bánh răng đúng kích thước.");}
 else if(id.StartsWith("I_Note")){state.notes[i]=true;ui.Note(i);}
 else switch(id){case "I_Journal":state.journal=true;sound.Play("pickup");ui.Toast("Đã nhặt nhật ký. Nút Nhật ký ghi mục tiêu và manh mối.");break;
 case "I_Flashlight":state.flashlight=true;sound.Play("pickup");ui.Toast("Đã nhặt đèn pin • phím F / nút Đèn");break;
 case "I_Disc":state.disc=true;sound.Play("pickup");ui.Toast("Đã nhặt đĩa đá. Mở Túi đồ để xem mặt sau.");break;
 case "I_GateLever":if(Rules.Gate(state)){if(!state.gate){state.gate=true;state.checkpoint=1;Success("Cổng mở. Tiến vào sân và tìm mái lấy sáng.");sound.Play("gate");}}else{sound.Play("fail");ui.Toast(state.clue?"Thứ tự ký hiệu chưa đúng.":"Hãy ghi nhận manh mối ở mặt sau đĩa đá trước.");}break;
 case "I_Shutter":state.shutter=true;sound.Play("lever");CheckLight();break;
 case "I_Handle":state.handle=true;sound.Play("pickup");ui.Toast("Đã tìm được tay van. Mang tới bồn nước.");break;
 case "I_Valve":if(!state.handle){ui.Toast("Van thiếu tay xoay. Kiểm tra góc lưu trữ sau vách gỗ trong khu kênh nước.");return;}state.valve=true;sound.Play("valve");CheckWater();break;
 case "I_MachineLever":if(Rules.Machine(state)){if(!state.machine){state.machine=true;state.checkpoint=3;Success("Cỗ máy hoạt động. Cửa thánh đường đã mở.");sound.Play("gate");}}else ui.Toast("Cần có nước và đủ ba bánh răng trong ổ.");break;
 case "I_FinalValve":if(!state.machine){ui.Toast("Cần khởi động cỗ máy trước.");return;}if(!state.finished){state.finished=true;Refresh();Save();StartCoroutine(Ending());}else ui.Toast("Nguồn nước đã được khôi phục.");break;}
 Refresh();Save();}
 void CheckLight(){if(!state.light&&Rules.Light(state)){state.light=true;state.checkpoint=2;Success("Bia đá đã nhận ánh sáng. Lối tới kênh nước mở.");}}
 void CheckWater(){if(hydraulics)hydraulics.Sync();}
 public void CompleteWaterFlow(){if(state.water || !Rules.Water(state) || !hydraulics || !hydraulics.Connected)return;state.water=true;Success("Nước đã chảy tới guồng. Trục truyền động bắt đầu quay.");Refresh();Save();}
 void Success(string text){sound.Play("success");ui.Toast(text);}
 public void RecordClue(bool realAR){state.clue=true;state.arUsed|=realAR;Save();ui.Toast("Đã ghi manh mối: LÁ → NƯỚC → MẶT TRỜI");}
 public void Hint(){state.hints++;Save();ui.Toast(!state.clue?"Nhặt đĩa đá, mở Túi đồ, xoay 180° và ghi manh mối.":!state.gate?"Vòng trái: lá; giữa: nước; phải: mặt trời. Kéo cần.":!state.light?"Mở mái. Từ trạng thái đầu: gương 1 xoay 1 lần, gương 2 xoay 3 lần.":!state.water?"Lắp tay van và mở nước. Từ trạng thái đầu: xoay kênh 1 và 3.":!state.machine?"Bánh răng lớn/vừa ở bàn, nhỏ ở kệ. Lắp đúng ổ rồi kéo cần.":"Vào thánh đường, xoay van bên phải mô hình di tích.",9);}
 IEnumerator Ending(){SetMode(GameMode.Ending);ui.HideAll();sound.Ending();var c=player.cam.transform;var p=c.position;var q=c.rotation;Vector3 end=Anchor("P_End")+Vector3.up*3.8f;Vector3 target=Anchor("A_PoolWater");for(float t=0;t<7;t+=Time.deltaTime){c.position=Vector3.Lerp(p,end,Mathf.SmoothStep(0,1,t/7));c.rotation=Quaternion.Slerp(q,Quaternion.LookRotation(target-end),Mathf.SmoothStep(0,1,t/7));yield return null;}c.localPosition=Vector3.up*1.62f;c.localRotation=Quaternion.identity;ui.Results();}
}
}

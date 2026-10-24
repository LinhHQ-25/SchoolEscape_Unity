using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using System;
namespace Aster {
public class ExpeditionUI:MonoBehaviour {
 Expedition g;RectTransform root,hud,modal;Text objective,focus,toast;float toastUntil;Image focusBack;Text trackingLabel, inspectStatus;Button recordClueButton;Image[] stages=new Image[4];Font body,title;Sprite rounded;
 readonly Color ink=new Color(.035f,.13f,.15f,.96f),cream=new Color(.94f,.88f,.72f),gold=new Color(.77f,.55f,.23f),muted=new Color(.64f,.74f,.70f);
 public void Setup(Expedition game){g=game;body=Resources.Load<Font>("Fonts/Body");title=Resources.Load<Font>("Fonts/Title");rounded=Round();var o=new GameObject("Aster Interface",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));root=o.GetComponent<RectTransform>();o.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;var sc=o.GetComponent<CanvasScaler>();sc.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;sc.referenceResolution=new Vector2(1600,900);sc.matchWidthOrHeight=.5f;
 if(!UnityEngine.Object.FindFirstObjectByType<EventSystem>())new GameObject("Event System",typeof(EventSystem),typeof(InputSystemUIInputModule));var canvasRoot=root;root=Rect(canvasRoot,"Safe area",0,0,1600,900);root.gameObject.AddComponent<AsterSafeArea>();BuildHUD();toast=Text(root,"",280,802,1040,58,22,cream);toast.alignment=TextAnchor.MiddleCenter;toast.raycastTarget=false;toast.transform.SetAsLastSibling();}
 Sprite Round(){var t=new Texture2D(64,64,TextureFormat.RGBA32,false);for(int y=0;y<64;y++)for(int x=0;x<64;x++){float dx=Mathf.Max(14-x,0,x-49),dy=Mathf.Max(14-y,0,y-49);t.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(14.5f-Mathf.Sqrt(dx*dx+dy*dy))));}t.Apply();return Sprite.Create(t,new Rect(0,0,64,64),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(14,14,14,14));}
 RectTransform Rect(Transform p,string name,float x,float y,float w,float h){var o=new GameObject(name,typeof(RectTransform));var r=o.GetComponent<RectTransform>();r.SetParent(p,false);r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;}
 Image Panel(Transform p,float x,float y,float w,float h,Color color){var r=Rect(p,"Panel",x,y,w,h);var im=r.gameObject.AddComponent<Image>();im.sprite=rounded;im.type=Image.Type.Sliced;im.color=color;return im;}
 Text Text(Transform p,string value,float x,float y,float w,float h,int size,Color color,bool heading=false){var r=Rect(p,"Text",x,y,w,h);var t=r.gameObject.AddComponent<Text>();t.text=value;t.font=heading?title:body;t.fontSize=size;t.color=color;t.alignment=TextAnchor.UpperLeft;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;t.raycastTarget=false;return t;}
 Button Button(Transform p,string value,float x,float y,float w,float h,Action action,bool primary=false){var im=Panel(p,x,y,w,h,primary?gold:new Color(.10f,.25f,.27f));var b=im.gameObject.AddComponent<Button>();var cb=b.colors;cb.highlightedColor=new Color(1.15f,1.15f,1.1f);cb.pressedColor=new Color(.8f,.8f,.75f);b.colors=cb;var ol=im.gameObject.AddComponent<Outline>();ol.effectColor=new Color(.68f,.52f,.27f,.5f);ol.effectDistance=new Vector2(1,-1);im.gameObject.AddComponent<AsterButtonMotion>();var t=Text(im.transform,value,8,2,w-16,h-4,h<40?16:21,primary?ink:cream);t.resizeTextForBestFit=true;t.resizeTextMinSize=12;t.resizeTextMaxSize=h<40?16:21;t.alignment=TextAnchor.MiddleCenter;b.onClick.AddListener(()=>{g.sound.Play("click",.35f);action();});return b;}
 void Icon(Transform p,string kind,float x,float y,float size,Color color){var r=Rect(p,"Icon "+kind,x,y,size,size);var icon=r.gameObject.AddComponent<AsterIcon>();icon.kind=kind;icon.color=color;icon.raycastTarget=false;}
 void BuildHUD(){hud=Rect(root,"HUD",0,0,1600,900);var mission=Panel(hud,32,28,580,116,ink);Icon(mission.transform,"compass",18,18,36,gold);Text(mission.transform,"NHẬT KÝ HÀNH TRÌNH",68,18,480,26,16,gold);objective=Text(mission.transform,"",22,52,538,56,21,cream);
 string[] icons={"bag","book","sun","lamp","pause"};Action[] actions={Inventory,Journal,()=>g.Hint(),()=>g.player.ToggleLamp(),Pause};string[] labels={"Túi đồ","Nhật ký","Gợi ý","Đèn","Dừng"};for(int i=0;i<5;i++){var b=Button(hud,"",1188+i*76,28,64,62,actions[i]);Icon(b.transform,icons[i],17,10,30,cream);Text(b.transform,labels[i],0,43,64,18,11,muted).alignment=TextAnchor.MiddleCenter;}
 for(int i=0;i<4;i++)stages[i]=Panel(hud,54+i*42,155,26,4,gold);Text(hud,"·",775,419,50,50,32,cream).alignment=TextAnchor.MiddleCenter;focusBack=Panel(hud,495,692,610,66,new Color(.025f,.10f,.11f,.85f));focus=Text(focusBack.transform,"",18,12,574,44,22,cream);focus.alignment=TextAnchor.MiddleCenter;focusBack.gameObject.SetActive(false);
 var ls=Panel(hud,660,180,900,480,new Color(0,0,0,0));g.player.look=ls.gameObject.AddComponent<TouchSurface>();g.player.look.look=true;ls.gameObject.SetActive(Application.isMobilePlatform);
 var js=Panel(hud,45,650,170,170,new Color(.04f,.14f,.15f,.35f));g.player.stick=js.gameObject.AddComponent<TouchSurface>();var knob=Panel(js.transform,55,55,60,60,new Color(.8f,.78f,.6f,.5f)).rectTransform;knob.anchorMin=knob.anchorMax=new Vector2(.5f,.5f);knob.pivot=new Vector2(.5f,.5f);knob.anchoredPosition=Vector2.zero;g.player.stick.knob=knob;js.gameObject.SetActive(Application.isMobilePlatform);
 Button(hud,Application.isMobilePlatform?"CHẠM TƯƠNG TÁC":"TƯƠNG TÁC  ·  E",1280,756,280,70,()=>g.player.Interact(),true);Text(hud,Application.isMobilePlatform?"Cần trái: di chuyển  ·  Vuốt phải: góc nhìn":"WASD / Mũi tên  Di chuyển      CHUỘT  Góc nhìn      I  Túi đồ      ESC  Tạm dừng",290,855,1020,28,16,muted).alignment=TextAnchor.MiddleCenter;
 }
 void ClearModal(){if(modal){modal.gameObject.SetActive(false);Destroy(modal.gameObject);modal=null;trackingLabel=null;inspectStatus=null;recordClueButton=null;}}
 public void HideAll(){ClearModal();hud.gameObject.SetActive(false);}
 RectTransform Open(string heading,string subtitle="",float height=680){HideAll();g.SetMode(GameMode.Panel);modal=Rect(root,"Dialog",0,0,1600,900);Panel(modal,0,0,1600,900,new Color(0,.04f,.05f,.6f));var p=Panel(modal,250,(900-height)*.5f,1100,height,ink).rectTransform;Panel(p,40,92,1020,2,new Color(.77f,.55f,.23f,.45f));var cg=p.gameObject.AddComponent<CanvasGroup>();StartCoroutine(Reveal(cg));Text(p,heading,40,28,950,60,36,cream,true);Text(p,subtitle,40,100,1000,65,20,muted);Button(p,"Đóng  ×",890,height-90,170,52,Close);toast.transform.SetAsLastSibling();return p;}
 public void Close(){if(g.Started)Resume();else MainMenu();}
 public void Resume(){g.player.MenuView(false);ClearModal();hud.gameObject.SetActive(true);g.SetMode(GameMode.Play);UpdateObjective();}
 public void UpdateObjective(){if(objective)objective.text=Rules.Objective(g.state);bool[] done={g.state.gate,g.state.light,g.state.water,g.state.machine};for(int i=0;i<4;i++)if(stages[i])stages[i].color=done[i]?gold:new Color(.3f,.4f,.37f);}
 public void Focus(string value){
 if(g.player && g.player.focus && g.player.focus.id.StartsWith("I_Ring"))
 {
     string id=g.player.focus.id;
     int index=id[id.Length-1]-'0';
     if(index>=0 && index<g.state.rings.Length)
     {
         string[] symbols={"LÁ","NƯỚC","MẶT TRỜI","NÚI","ẤN CỔ"};
         int selection=Mathf.Clamp(g.state.rings[index],0,4);
         value="Vòng "+(index+1)+": "+symbols[selection]+"  •  Mở bảng ký hiệu";
     }
 }
 focus.text=string.IsNullOrEmpty(value)?"":value+(Application.isMobilePlatform?"":"  [E]");focusBack.gameObject.SetActive(!string.IsNullOrEmpty(value));}
 public void Toast(string value,float duration=5){toast.text=value;toastUntil=Time.unscaledTime+duration;toast.transform.SetAsLastSibling();}
 void Update(){if(toast&&Time.unscaledTime>toastUntil)toast.text="";if(trackingLabel)trackingLabel.text=g.viewer.TrackingStatus;
 if(recordClueButton)recordClueButton.interactable=g.viewer.CanRecord;
 if(inspectStatus)inspectStatus.text=g.viewer.CanRecord?"Đã thấy mặt sau • Có thể ghi manh mối":"Bấm Lật mặt sau để đọc dòng chữ trên đĩa";}
 System.Collections.IEnumerator Reveal(CanvasGroup group){for(float t=0;t<.18f;t+=Time.unscaledDeltaTime){if(!group)yield break;group.alpha=t/.18f;yield return null;}if(group)group.alpha=1;}
 public void MainMenu(){HideAll();g.SetMode(GameMode.Menu);g.player.MenuView(true);modal=Rect(root,"Main menu",0,0,1600,900);var p=Panel(modal,70,105,520,690,new Color(.025f,.11f,.12f,.93f)).rectTransform;Panel(p,32,34,64,3,gold);Text(p,"ASTER  /  CHUYẾN KHẢO SÁT",110,24,385,28,15,gold);
 Text(p,"Bí mật\nkhu di tích",32,88,465,176,55,cream,true);Icon(p,"leaf",34,272,26,gold);Icon(p,"water",77,272,26,gold);Icon(p,"sun",120,272,26,gold);Panel(p,173,284,300,1,new Color(.77f,.55f,.23f,.35f));Text(p,"Theo dấu người xưa. Giải mã hiện vật.\nĐưa dòng nước trở lại khu di tích.",32,320,456,74,21,muted);
 Button(p,"BẮT ĐẦU HÀNH TRÌNH",32,425,456,62,()=>{if(g.HasSave())ConfirmNew();else g.NewGame();},true);var b=Button(p,"Tiếp tục khám phá",32,501,456,54,()=>g.ContinueGame());b.interactable=g.HasSave();Button(p,"Cài đặt",32,571,220,50,Settings);Button(p,"Cách chơi",268,571,220,50,Help);Text(p,"KHÁM PHÁ  ·  GIẢI ĐỐ  ·  HIỆN VẬT AR",32,650,456,25,14,muted).alignment=TextAnchor.MiddleCenter;Text(modal,"MỘT CÔNG TRÌNH BỊ LÃNG QUÊN\nMột dòng nước chờ được đánh thức.",1050,765,490,72,21,cream).alignment=TextAnchor.MiddleRight;toast.transform.SetAsLastSibling();}
 void ConfirmNew(){var p=Open("Bắt đầu lại?","Bản lưu hiện tại sẽ được thay bằng hành trình mới.",410);Button(p,"Bắt đầu mới",40,230,480,65,()=>g.NewGame(),true);Button(p,"Giữ bản lưu",550,230,480,65,MainMenu);}
 public void Pause(){g.Save();var p=Open("Tạm dừng",Rules.Objective(g.state),590);Button(p,"Tiếp tục khám phá",40,205,480,65,Resume,true);Button(p,"Cài đặt",550,205,480,65,Settings);Button(p,"Hướng dẫn",40,295,480,65,Help);Button(p,"Về màn hình chính",550,295,480,65,MainMenu);Text(p,"Tiến trình được tự động lưu sau mỗi tương tác và mỗi 30 giây.",40,410,990,85,23,muted);}
 public void Inventory(){var p=Open("Túi đồ khảo sát","Quan sát hiện vật, đọc dấu tích và chọn linh kiện để khôi phục cỗ máy.");string[] names={"Nhật ký","Đèn pin","Đĩa đá","Tay van","Bánh răng lớn","Bánh răng vừa","Bánh răng nhỏ","Ghi chép"};string[] art={"Journal","Flashlight","Disc","Handle","Gear0","Gear1","Gear2","Note0"};bool[] have={g.state.journal,g.state.flashlight,g.state.disc,g.state.handle,g.state.gears[0],g.state.gears[1],g.state.gears[2],g.state.Notes>0};for(int i=0;i<8;i++){int n=i;float x=40+(i%4)*260,y=175+(i/4)*195;var card=Panel(p,x,y,240,180,new Color(.07f,.20f,.21f));var tx=i>=4&&i<=6?AsterGearwork.Icon(i-4):Resources.Load<Texture2D>("UI/Items/"+art[i]);if(tx){var r=Rect(card.transform,"Item portrait",73,8,94,94);var im=r.gameObject.AddComponent<RawImage>();im.texture=tx;im.color=have[i]?Color.white:new Color(.55f,.60f,.54f,.7f);im.raycastTarget=false;}else Icon(card.transform,i>=4&&i<=6?"gear":i==0||i==7?"book":i==1?"lamp":"sun",96,25,48,have[i]?gold:muted);Text(card.transform,names[i],10,103,220,30,19,have[i]?cream:muted).alignment=TextAnchor.MiddleCenter;
 if(have[i]){bool installed=i>=4&&i<=6&&g.state.installed[i-4];var b=Button(card.transform,i==2?"Quan sát":installed?"Đã lắp":i>=4&&i<=6?"Chọn lắp":"Đã thu thập",12,139,216,30,()=>{if(n==2)InspectChoice();else if(n>=4&&n<=6){g.selectedGear=n-4;Resume();Toast("Đã chọn "+names[n]+". Tới ổ phù hợp và tương tác.");}else if(n==0||n==7)Journal();else Toast(names[n]+" đã có trong túi.");},i==2);b.interactable=!installed;}else Text(card.transform,"Chưa tìm thấy",12,143,216,27,16,muted).alignment=TextAnchor.MiddleCenter;}
 }
 public void InspectChoice(){var p=Open("Quan sát đĩa đá","Xoay hiện vật để đọc các ký hiệu ở mặt sau.",540);Button(p,"Xem hiện vật 3D",40,205,480,70,()=>g.viewer.Open(false),true);var ar=Button(p,"Quét thẻ AR (Android)",550,205,480,70,()=>g.viewer.Open(true),true);ar.interactable=Application.platform==RuntimePlatform.Android&&!Application.isEditor;Text(p,"Đặt thẻ di tích đã in rộng 15 cm trên mặt phẳng đủ sáng. Hướng camera vào toàn bộ thẻ.\n\nBạn cũng có thể xoay và khám phá đĩa đá bằng chế độ 3D.",40,300,990,145,23,muted);}
 public void ChooseGateSymbol(int index)
 {
     if(index<0 || index>2)return;
     bool locked=g.state.gate;
     var panel=Open("Vòng đá "+(index+1),locked?
         "Cổng đã mở. Đây là các ký hiệu đã được chọn.":
         "Chọn một biểu tượng để khắc lên mặt đá.");
     for(int symbol=0;symbol<4;symbol++)
     {
         int choice=symbol;
         bool selected=g.state.rings[index]==choice;
         float x=40+symbol*260;
         var card=Button(panel,"",x,195,240,240,()=>
         {
             g.state.rings[index]=choice;
             g.sound.Play("stone");g.Refresh();g.Save();Resume();
             Toast("Vòng "+(index+1)+" đã chọn: "+GateSymbolArtwork.Names[choice]);
         },selected);
         card.interactable=!locked;
         var imageRect=Rect(card.transform,"Symbol illustration",57,20,126,126);
         var picture=imageRect.gameObject.AddComponent<RawImage>();
         picture.texture=GateSymbolArtwork.Texture(choice);picture.raycastTarget=false;
         Text(card.transform,GateSymbolArtwork.Names[choice],12,158,216,35,24,selected?ink:cream).alignment=TextAnchor.MiddleCenter;
         Text(card.transform,selected?"ĐANG CHỌN":locked?"ĐÃ KHÓA":"CHỌN BIỂU TƯỢNG",12,203,216,25,15,selected?ink:muted).alignment=TextAnchor.MiddleCenter;
     }
     string combination="Hiện tại:  "+GateSymbolArtwork.Names[g.state.rings[0]]+"  •  "+GateSymbolArtwork.Names[g.state.rings[1]]+"  •  "+GateSymbolArtwork.Names[g.state.rings[2]];
     Text(panel,combination,40,465,1010,40,23,cream).alignment=TextAnchor.MiddleCenter;
     Text(panel,g.state.clue?"Manh mối đã ghi: LÁ → NƯỚC → MẶT TRỜI":"Hãy quan sát mặt sau đĩa đá và ghi manh mối.",40,520,1000,45,21,gold).alignment=TextAnchor.MiddleCenter;
 }
 public void InspectHUD(bool ar)
 {
     HideAll();g.SetMode(ar?GameMode.AR:GameMode.Inspect);
     modal=Rect(root,"Inspect",0,0,1600,900);
     var header=Panel(modal,40,25,1520,110,ink);
     Text(header.transform,ar?"HIỆN VẬT AR":"ĐĨA ĐÁ CỔ",28,18,1200,40,28,cream,true);
     Text(header.transform,"Quan sát hai mặt để tìm dấu tích của người xưa.",28,65,1200,30,19,muted);
     var controls=Panel(modal,40,720,1520,155,ink);
     inspectStatus=Text(controls.transform,"",24,15,1450,32,20,cream);
     Button(controls.transform,"Xoay trái",24,67,170,58,()=>g.viewer.Rotate(-45));
     Button(controls.transform,"Xoay phải",207,67,170,58,()=>g.viewer.Rotate(45));
     Button(controls.transform,"Lật mặt sau",390,67,225,58,()=>g.viewer.ShowBack());
     Button(controls.transform,"−",633,67,66,58,()=>g.viewer.Zoom(-.15f));
     Button(controls.transform,"+",711,67,66,58,()=>g.viewer.Zoom(.15f));
     recordClueButton=Button(controls.transform,"Ghi manh mối",838,67,370,58,()=>g.viewer.Record(),true);
     recordClueButton.interactable=false;
     Button(controls.transform,"Quay lại",1225,67,270,58,()=>g.viewer.Close());
     if(ar)trackingLabel=Text(modal,"Đang chuẩn bị camera…",65,150,1300,44,21,gold);
     toast.transform.SetAsLastSibling();
 }
 public void Journal(){var p=Open("Nhật ký khảo sát",Rules.Objective(g.state));string[] tasks={"Cổng ký hiệu","Hành lang ánh sáng","Kênh dẫn nước","Cỗ máy cổ"};string[] desc={"Giải mã thứ tự trên đĩa đá","Dẫn tia nắng tới bia nhận","Nối dòng nước tới guồng quay","Lắp bánh răng và khởi động"};bool[] done={g.state.gate,g.state.light,g.state.water,g.state.machine};for(int i=0;i<4;i++){var c=Panel(p,40,177+i*87,490,74,new Color(.07f,.20f,.21f));Icon(c.transform,done[i]?"check":"compass",16,22,28,done[i]?gold:muted);Text(c.transform,(i+1).ToString("00")+"  "+tasks[i],60,10,405,30,22,cream);Text(c.transform,done[i]?"Đã hoàn thành":desc[i],60,41,405,23,16,muted);}var clue=Panel(p,550,177,510,335,new Color(.04f,.16f,.18f));Text(clue.transform,"DẤU TÍCH TRÊN ĐĨA ĐÁ",24,24,462,30,17,gold);
 if(g.state.clue){Icon(clue.transform,"leaf",50,90,65,gold);Icon(clue.transform,"water",221,90,65,gold);Icon(clue.transform,"sun",392,90,65,gold);Text(clue.transform,"LÁ       →       NƯỚC       →       MẶT TRỜI",26,180,460,45,20,cream).alignment=TextAnchor.MiddleCenter;Text(clue.transform,g.state.arUsed?"Đã quan sát bằng AR":"Đã quan sát bằng trình xem 3D",24,265,460,35,18,muted);}else Text(clue.transform,"Manh mối còn ẩn ở mặt sau đĩa đá.\n\nTìm hiện vật và mở Túi đồ để quan sát.",24,95,462,180,24,cream);Text(p,"Ghi chép đã tìm thấy  "+g.state.Notes+" / 3",550,540,490,35,21,muted);
 }
 public void Note(int i){string[] texts={"Nước từng nuôi dưỡng cả khu di tích. Những người thợ để lại một chuỗi thử thách nhằm bảo vệ bộ điều tiết nguồn nước.\n\nĐĩa đá bên cổng không chỉ để trang trí. Mặt sau giữ thứ tự ba dấu hiệu.","Khi mái mở, nắng sẽ chạm gương đầu tiên. Hai gương phải cùng hướng tia sáng tới bia nhận ở cuối hành lang.\n\nHãy quan sát đường sáng sau từng lần xoay.","Dòng nước cung cấp năng lượng, nhưng máy chỉ chạy khi ba bánh răng ăn khớp. Bánh lớn dẫn động bánh vừa, rồi tới bánh nhỏ.\n\nCần gạt bên phải sẽ mở cửa thánh đường."};var p=Open("Ghi chép "+(i+1),"Tư liệu của đội khảo sát trước",570);Text(p,texts[i],40,180,1000,280,25,cream);}
 void Slider(Transform p,string label,float y,float value,Action<float> change,float min=0,float max=1){Text(p,label,40,y,460,35,23,cream);var number=Text(p,"",495,y,115,35,20,gold);number.alignment=TextAnchor.MiddleRight;var back=Panel(p,50,y+56,540,10,new Color(.16f,.29f,.29f));var slide=back.gameObject.AddComponent<Slider>();var fill=Panel(back.transform,0,0,540,10,gold).rectTransform;fill.anchorMin=Vector2.zero;fill.anchorMax=Vector2.one;fill.offsetMin=fill.offsetMax=Vector2.zero;slide.fillRect=fill;var handle=Panel(back.transform,0,-8,26,26,cream).rectTransform;handle.anchorMin=handle.anchorMax=new Vector2(0,.5f);handle.pivot=new Vector2(.5f,.5f);slide.handleRect=handle;slide.targetGraphic=handle.GetComponent<Image>();slide.minValue=min;slide.maxValue=max;slide.value=value;Action<float> show=v=>number.text=min==0?Mathf.RoundToInt(v*100)+"%":v.ToString("0.0")+"×";show(value);slide.onValueChanged.AddListener(v=>{show(v);change(v);});}
 public void Settings(){var p=Open("Cài đặt","Điều chỉnh trải nghiệm cho chuyến khám phá của bạn.");Slider(p,"Nhạc nền",190,g.sound.musicVolume,v=>{g.sound.musicVolume=v;g.sound.Apply();});Slider(p,"Âm thanh môi trường & thao tác",310,g.sound.effectsVolume,v=>{g.sound.effectsVolume=v;g.sound.Apply();g.Refresh();});Slider(p,"Độ nhạy góc nhìn",430,g.player.sensitivity,v=>{g.player.sensitivity=v;PlayerPrefs.SetFloat("look",v);},.3f,2.2f);var c=Panel(p,680,180,380,365,new Color(.04f,.16f,.18f));Text(c.transform,"THẺ HIỆN VẬT AR",22,20,336,30,18,gold).alignment=TextAnchor.MiddleCenter;var r=Rect(c.transform,"AR card",110,65,160,160);var im=r.gameObject.AddComponent<RawImage>();im.texture=Resources.Load<Texture2D>("AR/DiscCard");im.raycastTarget=false;Text(c.transform,"In thẻ rộng 15 cm.\nDùng camera Android hỗ trợ AR\nđể quan sát đĩa đá ngoài đời thực.",22,250,336,95,18,cream).alignment=TextAnchor.MiddleCenter;}

 public void Help(){var p=Open("Cách chơi","Khám phá góc nhìn thứ nhất • PC và Android ngang màn hình");Text(p,"PC: WASD di chuyển · Chuột nhìn 360° · Shift đi nhanh\nE tương tác · I/Tab túi đồ · F đèn pin · Esc tạm dừng\n\nAndroid: cần trái di chuyển, vuốt vùng phải để nhìn.\nChạm Tương tác khi tên vật phẩm hiện giữa màn hình.\n\nAR chỉ dùng một thẻ để quan sát đĩa đá. Cần điện thoại hỗ trợ ARCore và quyền camera. Bản PC có trình xem 3D thay thế.\n\nNếu bí: mở Nhật ký hoặc Gợi ý. Tiến trình được tự động lưu.",40,185,1000,380,24,cream);}
 public void Results(){var p=Open("Nguồn nước đã trở lại","Những dấu tích đã kết nối. Khu di tích bắt đầu một chương mới.");Icon(p,"water",75,200,95,gold);Text(p,"HÀNH TRÌNH HOÀN THÀNH",205,213,790,40,27,gold);Text(p,"Cảm ơn bạn đã đưa dòng nước về với công trình cổ.",205,264,800,50,21,cream);string[] labels={"CÂU ĐỐ","GHI CHÉP","THỜI GIAN","GỢI Ý"};string[] values={g.state.Completed+" / 4",g.state.Notes+" / 3",TimeSpan.FromSeconds(g.state.seconds).ToString(@"hh\:mm\:ss"),g.state.hints.ToString()};for(int i=0;i<4;i++){var c=Panel(p,40+i*260,350,240,126,new Color(.07f,.20f,.21f));Text(c.transform,values[i],10,20,220,48,32,cream,true).alignment=TextAnchor.MiddleCenter;Text(c.transform,labels[i],10,83,220,25,15,gold).alignment=TextAnchor.MiddleCenter;}Text(p,g.state.arUsed?"HIỆN VẬT AR  •  Đã khám phá ngoài đời thực":"HIỆN VẬT 3D  •  Bạn có thể quay lại trải nghiệm AR trên Android",40,510,1010,46,20,muted);Button(p,"Tiếp tục khám phá",40,590,400,52,Resume,true);Button(p,"Màn hình chính",465,590,390,52,MainMenu);}
}
}

namespace Aster
{
 internal static class GateSymbolArtwork
 {
     public static readonly string[] Names={"LÁ","NƯỚC","MẶT TRỜI","NÚI","ẤN CỔ"};
     static readonly Texture2D[] textures=new Texture2D[5];
     static readonly Material[] materials=new Material[5];
     static Mesh plaqueMesh;

     public static Texture2D Texture(int symbol)
     {
         symbol=Mathf.Clamp(symbol,0,4);
         if(textures[symbol])return textures[symbol];
         var strokes=new System.Collections.Generic.List<Vector4>();
         Action<Vector2,Vector2> line=(a,b)=>strokes.Add(new Vector4(a.x,a.y,b.x,b.y));
         if(symbol==0)
         {
             Vector2 last=new Vector2(0,-.55f);
             for(int n=1;n<=64;n++)
             {
                 float a=n*Mathf.PI*2/64f;
                 Vector2 next=new Vector2(.32f*Mathf.Sin(a),-.55f*Mathf.Cos(a));
                 line(last,next);last=next;
             }
             line(new Vector2(0,-.55f),new Vector2(0,.55f));
             for(int n=-1;n<=1;n++)
             {
                 float y=n*.23f;
                 line(new Vector2(0,y),new Vector2(.25f,y+.16f));
                 line(new Vector2(0,y),new Vector2(-.25f,y+.16f));
             }
         }
         else if(symbol==1)
         {
             for(int row=-1;row<=1;row++)
             for(int n=0;n<36;n++)
             {
                 float x=-.55f+n*1.1f/36f,xx=-.55f+(n+1)*1.1f/36f;
                 line(new Vector2(x,row*.27f+Mathf.Sin(x*8)*.075f),new Vector2(xx,row*.27f+Mathf.Sin(xx*8)*.075f));
             }
         }
         else if(symbol==2)
         {
             for(int n=0;n<48;n++)
             {
                 float a=n*Mathf.PI*2/48,b=(n+1)*Mathf.PI*2/48;
                 line(new Vector2(Mathf.Cos(a),Mathf.Sin(a))*.25f,new Vector2(Mathf.Cos(b),Mathf.Sin(b))*.25f);
             }
             for(int n=0;n<12;n++)
             {
                 float a=n*Mathf.PI/6;Vector2 d=new Vector2(Mathf.Cos(a),Mathf.Sin(a));
                 line(d*.38f,d*.56f);
             }
         }
         else if(symbol==3)
         {
             line(new Vector2(-.57f,-.4f),new Vector2(-.1f,.5f));
             line(new Vector2(-.1f,.5f),new Vector2(.44f,-.4f));
             line(new Vector2(-.57f,-.4f),new Vector2(.57f,-.4f));
             line(new Vector2(.1f,.17f),new Vector2(.29f,.38f));
             line(new Vector2(.29f,.38f),new Vector2(.57f,-.4f));
             line(new Vector2(-.25f,.21f),new Vector2(-.08f,.1f));
             line(new Vector2(-.08f,.1f),new Vector2(.06f,.23f));
         }
         if(symbol==4)for(int k=0;k<3;k++){
          float phase=k*Mathf.PI*2/3;Vector2 c=new Vector2(Mathf.Cos(phase),Mathf.Sin(phase))*.23f;
          for(int j=0;j<50;j++){float a=j*.11f,b=(j+1)*.11f;line(c+new Vector2(Mathf.Cos(a+phase),Mathf.Sin(a+phase))*(.015f+j*.005f),c+new Vector2(Mathf.Cos(b+phase),Mathf.Sin(b+phase))*(.015f+(j+1)*.005f));}
         }
         const int size=160;
         var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);
         texture.name="Aster symbol "+Names[symbol];texture.wrapMode=TextureWrapMode.Clamp;
         texture.filterMode=FilterMode.Bilinear;
         Color accent=new Color(.95f,.78f,.43f);
         Color dark=new Color(.035f,.12f,.13f);
         var pixels=new Color[size*size];
         for(int y=0;y<size;y++)for(int x=0;x<size;x++)
         {
             Vector2 p=new Vector2((x+.5f)/size*2-1,(y+.5f)/size*2-1);
             float distance=10;
             foreach(Vector4 segment in strokes)
             {
                 Vector2 a=new Vector2(segment.x,segment.y),b=new Vector2(segment.z,segment.w);
                 Vector2 ab=b-a;
                 Vector2 closest=a+ab*Mathf.Clamp01(Vector2.Dot(p-a,ab)/Mathf.Max(ab.sqrMagnitude,.000001f));
                 distance=Mathf.Min(distance,(p-closest).magnitude);
             }
             float rim=Mathf.Abs(p.magnitude-.86f);
             float coverage=Mathf.Clamp01((.031f-Mathf.Min(distance,rim))/.014f);
             Color c=Color.Lerp(dark,accent,coverage);
             c.a=Mathf.Clamp01((.94f-p.magnitude)/.018f);
             pixels[y*size+x]=c;
         }
         texture.SetPixels(pixels);texture.Apply();textures[symbol]=texture;return texture;
     }

     public static void Apply(Expedition game)
     {
         if(!game.world)return;
         Vector3 outward=game.Anchor("P_Spawn")-game.Anchor("P_Look");outward.y=0;
         outward=outward.sqrMagnitude>.001f?outward.normalized:Vector3.forward;
         for(int index=0;index<3;index++)
         {
             Transform ring=game.Node("I_Ring"+index);
             // Hide the old, unreadable engravings, even when Refresh activates them.
             foreach(Transform child in ring)
                 if(child.name.StartsWith("Symbol"))
                     foreach(Renderer renderer in child.GetComponentsInChildren<Renderer>(true))renderer.enabled=false;
             Transform plaque=ring.Find("Aster visible symbol");
             if(!plaque)
             {
                 // Clear only small decorative leaves directly around this dial.
                 foreach(Renderer foliage in game.world.GetComponentsInChildren<Renderer>())
                 {
                     string name=foliage.name.ToLowerInvariant();
                     if(!(name.Contains("leafblade") || name.Contains("ivyleaf")))continue;
                     if(foliage.transform.IsChildOf(ring))continue;
                     if(Vector3.Distance(foliage.transform.position,ring.position)<.65f)
                         foliage.enabled=false;
                 }
                 var go=new GameObject("Aster visible symbol");plaque=go.transform;
                 plaque.SetParent(ring,false);
                 go.AddComponent<MeshFilter>().sharedMesh=Quad();
                 go.AddComponent<MeshRenderer>();
             }
             plaque.position=ring.position+outward*.085f;
             plaque.rotation=Quaternion.LookRotation(outward,Vector3.up);
             // World dimensions independent of FBX import scale.
             Vector3 scale=ring.lossyScale;
             plaque.localScale=new Vector3(.49f/Mathf.Max(Mathf.Abs(scale.x),.0001f),.49f/Mathf.Max(Mathf.Abs(scale.y),.0001f),1f/Mathf.Max(Mathf.Abs(scale.z),.0001f));
             int symbol=Mathf.Clamp(game.state.rings[index],0,4);
             if(!materials[symbol])
             {
                 materials[symbol]=new Material(Resources.Load<Shader>("Art/Engraving"));
                 materials[symbol].color=new Color(.15f,.12f,.075f,.9f);
                 var source=Texture(symbol);var engraved=new Texture2D(source.width,source.height,TextureFormat.RGBA32,false);
                 var pixels=source.GetPixels();
                 for(int k=0;k<pixels.Length;k++) {
                     float stroke=Mathf.Clamp01((pixels[k].r-.035f)/(.95f-.035f));
                     int x=k%source.width,y=k/source.width;
                     float edge=new Vector2((x+.5f)/source.width*2-1,(y+.5f)/source.height*2-1).magnitude;
                     pixels[k]=new Color(1,1,1,edge<.75f?stroke:0);
                 }
                 engraved.SetPixels(pixels);engraved.Apply();materials[symbol].mainTexture=engraved;
             }
             plaque.GetComponent<MeshRenderer>().sharedMaterial=materials[symbol];
         }
     }
     static Mesh Quad()
     {
         if(plaqueMesh)return plaqueMesh;
         plaqueMesh=new Mesh();plaqueMesh.name="Aster symbol plaque";
         plaqueMesh.vertices=new[]{new Vector3(-.5f,-.5f,0),new Vector3(.5f,-.5f,0),new Vector3(.5f,.5f,0),new Vector3(-.5f,.5f,0)};
         plaqueMesh.uv=new[]{new Vector2(0,0),new Vector2(1,0),new Vector2(1,1),new Vector2(0,1)};
         plaqueMesh.triangles=new[]{0,1,2,0,2,3};
         plaqueMesh.RecalculateNormals();plaqueMesh.RecalculateBounds();return plaqueMesh;
     }
 }
}

using System;
namespace Aster {
[Serializable] public class ExpeditionState {
 public int version=1,checkpoint,hints; public float seconds;
 public bool journal,flashlight,disc,clue,arUsed,gate,shutter,light,handle,valve,water,machine,finished;
 public int[] rings={4,4,4},mirrors={0,0},channels={0,0,0};
 public bool[] gears=new bool[3],installed=new bool[3],notes=new bool[3];
 public bool Valid(){return version==1&&Range(rings,3,4)&&Range(mirrors,2,3)&&Range(channels,3,1)&&gears!=null&&gears.Length==3&&installed!=null&&installed.Length==3&&notes!=null&&notes.Length==3&&checkpoint>=0&&checkpoint<=3;}
 static bool Range(int[] a,int n,int max){if(a==null||a.Length!=n)return false;foreach(int v in a)if(v<0||v>max)return false;return true;}
 public int Notes {get {int n=0;foreach(bool b in notes)if(b)n++;return n;}}
 public int Completed {get {return (gate?1:0)+(light?1:0)+(water?1:0)+(machine?1:0);}}
}
public static class Rules {
 public static bool Gate(ExpeditionState s){return s.clue&&s.rings[0]==0&&s.rings[1]==1&&s.rings[2]==2;}
 public static bool Light(ExpeditionState s){return s.gate&&s.shutter&&s.mirrors[0]==1&&s.mirrors[1]==3;}
 public static bool Water(ExpeditionState s){return s.light&&s.handle&&s.valve&&s.channels[0]==1&&s.channels[1]==0&&s.channels[2]==1;}
 public static bool Machine(ExpeditionState s){return s.water&&s.installed[0]&&s.installed[1]&&s.installed[2];}
 public static string Objective(ExpeditionState s){if(s.finished)return "Khu di tích đã hồi sinh • Tự do khám phá";if(s.machine)return "THÁNH ĐƯỜNG • Tìm van cuối và khôi phục hồ nước";if(s.water)return "XƯỞNG CƠ KHÍ • Lắp ba bánh răng và kéo cần";if(s.light)return "KÊNH NƯỚC • Tìm tay van, nối dòng chảy";if(s.gate)return "SÂN DI TÍCH • Mở mái, dẫn ánh sáng tới bia đá";if(!s.disc)return "TRẠI KHẢO SÁT • Tìm đĩa đá bên cổng";if(!s.clue)return "ĐĨA ĐÁ • Mở túi đồ, quan sát mặt sau";return "CỔNG DI TÍCH • Xếp ba biểu tượng rồi kéo cần";}
}
}

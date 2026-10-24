using UnityEngine;
using System.Collections.Generic;

namespace Aster {
// Three rotary troughs share actual inlet/outlet coordinates. Water advances
// only through connected joints; a solved flag alone never opens a dry route.
public sealed class AsterHydraulics : MonoBehaviour {
    Expedition game;
    Transform root, wheel;
    readonly Transform[] channels=new Transform[3];
    readonly GameObject[] channelWater=new GameObject[3];
    readonly int[] solution={1,0,1};
    GameObject feedWater, dischargeWater, returnWater, finalWater;
    Material stone, bronze, water;
    float travel;
    AudioSource stream;
    readonly List<ParticleSystem> splashes=new List<ParticleSystem>();
    public bool Connected { get { return JointCount()==3; } }
    public static AsterHydraulics Install(Expedition game) {
        var o=new GameObject("Restored water system");
        var component=o.AddComponent<AsterHydraulics>();
        component.game=game;component.Build();return component;
    }
    Vector3 Inlet(int i) {return channels[i].TransformPoint(new Vector3(-1.5f,.19f,0));}
    Vector3 Outlet(int i) {return channels[i].TransformPoint(new Vector3(1.5f,.09f,0));}
    int JointCount() {
        if(channels[0]==null)return 0;
        Vector3 previous=new Vector3(-19.5f,1.89f,-9);
        for(int i=0;i<3;i++) {
            if(Vector3.Distance(previous,Inlet(i))>.025f || Vector3.Dot(channels[i].right,Vector3.right)<.999f)return i;
            previous=Outlet(i);
        }
        return 3;
    }
    public void Sync() {
        // Old saves allowed changing troughs after solving. Keep a solved save physically connected.
        if(game.state.water)for(int i=0;i<3;i++)game.state.channels[i]=solution[i];
        for(int i=0;i<3;i++)channels[i].rotation=Quaternion.Euler(0,(game.state.channels[i]-solution[i])*90f,0);
        if(!game.state.valve)travel=0;
        if(game.state.water && Connected)travel=6;
        RenderFlow();
    }
    void Build() {
        root=transform;
        stone=new Material(Resources.Load<Shader>("Art/AgedStone"));stone.color=new Color(.48f,.44f,.34f);
        bronze=new Material(Shader.Find("Standard"));bronze.color=new Color(.23f,.27f,.19f);bronze.SetFloat("_Metallic",.55f);bronze.SetFloat("_Glossiness",.25f);
        water=new Material(Resources.Load<Shader>("Art/FlowWater"));
        // The basin remains open at the top. Raise its source so every segment
        // descends toward the smaller wheel; add masonry down to the ground.
        Transform tank=game.world.Find("C_Reservoir");
        Transform tankWater=game.world.Find("ReservoirWater");
        if(tank)tank.position+=Vector3.up*.75f;
        if(tankWater)tankWater.position+=Vector3.up*.75f;
        Column(new Vector3(-19,0,-5),.76f,1.65f);
        // Remove only the old disconnected channel decoration/colliders.
        foreach(Transform t in game.world.GetComponentsInChildren<Transform>(true)) {
            if(t.name.StartsWith("C_ChannelBase") || t.name.StartsWith("C_Kerb") || t.name=="Shaft")t.gameObject.SetActive(false);
        }
        for(int i=0;i<3;i++) {
            Transform t=game.Node("I_Channel"+i);channels[i]=t;
            foreach(Renderer r in t.GetComponentsInChildren<Renderer>(true))r.enabled=false;
            foreach(Collider c in t.GetComponentsInChildren<Collider>(true))c.enabled=false;
            t.position=new Vector3(-18+i*3,1.70f-i*.10f,-9);t.rotation=Quaternion.identity;t.localScale=Vector3.one;
            var handle=t.gameObject.AddComponent<BoxCollider>();handle.center=Vector3.up*.1f;handle.size=new Vector3(3,.45f,.84f);
            Vector3[] path={new Vector3(-1.5f,0.05f,0),new Vector3(1.5f,-.05f,0)};
            Trough("Rotating stone trough",path,t);
            channelWater[i]=Surface("Channel water",new[]{new Vector3(-1.5f,.19f,0),new Vector3(1.5f,.09f,0)},.56f,t);
            Column(new Vector3(t.position.x,0,t.position.z),t.position.y-.06f,.48f);
        }
        Vector3[] feed={new Vector3(-19,1.97f,-6.4f),new Vector3(-19.5f,1.94f,-7.4f),new Vector3(-19.5f,1.89f,-9)};
        FixedChannel("Reservoir outlet",feed,out feedWater);
        // Valve is now mounted on the outlet pipe, with a visible valve body.
        Transform valve=game.Node("I_Valve");valve.position=new Vector3(-19.5f,1.65f,-7.35f);
        Rod(new Vector3(-19.5f,1.65f,-7.35f),new Vector3(-19.5f,1.93f,-7.35f),.1f,bronze);
        Vector3[] outlet={new Vector3(-10.5f,1.59f,-9),new Vector3(-10.5f,1.53f,-11.15f),new Vector3(-11.4f,1.47f,-12.4f)};
        FixedChannel("Wheel supply",outlet,out dischargeWater);
        wheel=game.Node("A_Waterwheel");wheel.position=new Vector3(-12,.78f,-12.4f);wheel.localScale*=.53f;
        Surface("Water falling onto paddles",new[]{outlet[2],new Vector3(-11.45f,.55f,-12.4f)},.42f,dischargeWater.transform);
        // Discharge remains on the exterior side of the workshop, then returns
        // visibly to a collection pool; the final gate feeds the sanctuary pool.
        Vector3[] tail={new Vector3(-12,.10f,-12.4f),new Vector3(-10,.09f,-13.4f),new Vector3(-7,.075f,-13.4f),new Vector3(-5.5f,.06f,-13.4f)};
        FixedChannel("Tailrace",tail,out returnWater);
        Vector3[] sanctuary={tail[tail.Length-1],new Vector3(-5.5f,.055f,-10),new Vector3(-4.3f,.05f,-7),new Vector3(-3,.045f,-5)};
        FixedChannel("Sanctuary return channel",sanctuary,out finalWater);
        // Small stone crossings make the tailrace passable instead of an
        // invisible wall across the route.
        Bridge(new Vector3(-7.8f,.22f,-13.4f));
        // Physical shaft, bearing piers, and belt connect the wheel to the
        // existing gearboard. Nothing rotates until water reaches the wheel.
        Vector3 shaftStart=new Vector3(-12,.78f,-12.4f),shaftEnd=new Vector3(-12,.78f,-20.92f);
        Rod(shaftStart,shaftEnd,.075f,bronze);
        for(int i=0;i<4;i++)Column(new Vector3(-12,0,-13.5f-i*2),.72f,.18f);
        Vector3 drive=game.Node("A_Gear0").position;
        Belt(shaftEnd,drive,.25f);
        Column(new Vector3(-12,0,-20.92f),.70f,.32f);
        stream=game.Node("I_Valve").gameObject.AddComponent<AudioSource>();stream.clip=Resources.Load<AudioClip>("Audio/water");stream.loop=true;stream.spatialBlend=1;stream.minDistance=1;stream.maxDistance=12;
        Splash(new Vector3(-11.45f,.2f,-12.4f));
        Sync();
    }
    void Update() {
        if(game.mode!=GameMode.Play)return;
        int connected=JointCount();
        if(game.state.valve)travel=Mathf.Min(travel+Time.deltaTime,connected==3?6f:connected+1f);else travel=0;
        RenderFlow();
        if(game.state.valve && connected==3 && travel>=5f && !game.state.water)game.CompleteWaterFlow();
    }
    void RenderFlow() {
        int connected=JointCount();bool source=game.state.valve;
        if(feedWater)feedWater.SetActive(source);
        for(int i=0;i<3;i++)if(channelWater[i])channelWater[i].SetActive(source && connected>i && travel>=i+1);
        bool reaches=source && connected==3 && travel>=4;
        if(dischargeWater)dischargeWater.SetActive(reaches);
        if(returnWater)returnWater.SetActive(reaches && travel>=5);
        if(finalWater)finalWater.SetActive(reaches && game.state.finished);
        foreach(var ps in splashes) {if(reaches&&!ps.isPlaying)ps.Play();if(!reaches&&ps.isPlaying)ps.Stop();}
        if(stream){stream.volume=game.sound?game.sound.effectsVolume*.28f:.2f;if(source&&!stream.isPlaying)stream.Play();if(!source)stream.Stop();}
    }
    void FixedChannel(string name,Vector3[] path,out GameObject surface) {
        var o=new GameObject(name);o.transform.SetParent(root,false);
        var bed=new Vector3[path.Length];for(int i=0;i<path.Length;i++)bed[i]=path[i]-Vector3.up*.14f;
        Trough(name,bed,o.transform);surface=Surface(name+" flowing water",path,.56f,o.transform);
        foreach(var p in path)if(p.y>.5f)Column(new Vector3(p.x,0,p.z),p.y-.2f,.25f);
    }
    void Trough(string name,Vector3[] path,Transform parent) {
        // Continuous U-section with thick sidewalls, open at its inlet/outlet.
        var section=new[]{new Vector2(-.44f,.25f),new Vector2(-.30f,.25f),new Vector2(-.30f,0),new Vector2(.30f,0),new Vector2(.30f,.25f),new Vector2(.44f,.25f),new Vector2(.44f,-.12f),new Vector2(-.44f,-.12f)};
        var vertices=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();
        for(int i=0;i<path.Length;i++) {
            Vector3 tangent=path[Mathf.Min(i+1,path.Length-1)]-path[Mathf.Max(0,i-1)];tangent.y=0;tangent.Normalize();
            Vector3 side=Vector3.Cross(Vector3.up,tangent);
            for(int j=0;j<section.Length;j++){vertices.Add(path[i]+side*section[j].x+Vector3.up*section[j].y);uv.Add(new Vector2(i,j*.15f));}
        }
        for(int i=0;i<path.Length-1;i++)for(int j=0;j<8;j++){int a=i*8+j,b=i*8+(j+1)%8,c=a+8,d=b+8;triangles.AddRange(new[]{a,c,b,b,c,d});}
        var mesh=new Mesh();mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
        var o=new GameObject(name);o.transform.SetParent(parent,false);o.AddComponent<MeshFilter>().sharedMesh=mesh;o.AddComponent<MeshRenderer>().sharedMaterial=stone;
        // Fixed troughs collide precisely; rotary ones use their interaction box.
        if(parent.parent==root)o.AddComponent<MeshCollider>().sharedMesh=mesh;
    }
    GameObject Surface(string name,Vector3[] path,float width,Transform parent) {
        var vertices=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();float distance=0;
        for(int i=0;i<path.Length;i++) {
            if(i>0)distance+=Vector3.Distance(path[i-1],path[i]);
            Vector3 tangent=path[Mathf.Min(i+1,path.Length-1)]-path[Mathf.Max(0,i-1)];
            Vector3 side=Vector3.Cross(Vector3.up,tangent).normalized;if(side.sqrMagnitude<.1f)side=Vector3.right;
            vertices.Add(path[i]-side*width*.5f);vertices.Add(path[i]+side*width*.5f);
            uv.Add(new Vector2(0,distance));uv.Add(new Vector2(1,distance));
            if(i>0){int a=(i-1)*2;triangles.AddRange(new[]{a,a+2,a+1,a+1,a+2,a+3});}
        }
        var mesh=new Mesh();mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
        var o=new GameObject(name);o.transform.SetParent(parent,false);o.AddComponent<MeshFilter>().sharedMesh=mesh;o.AddComponent<MeshRenderer>().sharedMaterial=water;return o;
    }
    void Column(Vector3 bottom,float height,float radius) {
        if(height<=0)return;
        var o=GameObject.CreatePrimitive(PrimitiveType.Cylinder);o.name="Masonry support";o.transform.SetParent(root,false);o.transform.position=bottom+Vector3.up*height*.5f;o.transform.localScale=new Vector3(radius*2,height*.5f,radius*2);o.GetComponent<Renderer>().sharedMaterial=stone;
    }
    void Rod(Vector3 a,Vector3 b,float radius,Material material) {
        var o=GameObject.CreatePrimitive(PrimitiveType.Cylinder);o.name="Connected drive shaft";o.transform.SetParent(root,false);o.transform.position=(a+b)*.5f;o.transform.up=(b-a).normalized;o.transform.localScale=new Vector3(radius*2,Vector3.Distance(a,b)*.5f,radius*2);o.GetComponent<Renderer>().sharedMaterial=material;Destroy(o.GetComponent<Collider>());
    }
    void Belt(Vector3 a,Vector3 b,float radius) {
        Vector3 normal=Vector3.Cross((b-a).normalized,Vector3.forward)*radius;
        Rod(a+normal,b+normal,.035f,bronze);Rod(a-normal,b-normal,.035f,bronze);
        Rod(a-normal,a+normal,.035f,bronze);Rod(b-normal,b+normal,.035f,bronze);
    }
    void Bridge(Vector3 p) {
        var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name="Tailrace crossing slab";o.transform.SetParent(root,false);o.transform.position=p;o.transform.localScale=new Vector3(1.5f,.14f,1.3f);o.GetComponent<Renderer>().sharedMaterial=stone;
    }
    void Splash(Vector3 position) {
        var o=new GameObject("Wheel splash");o.transform.SetParent(root,false);o.transform.position=position;o.transform.rotation=Quaternion.Euler(-90,0,0);
        var ps=o.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        var main=ps.main;main.startLifetime=.5f;main.startSpeed=.55f;main.startSize=new ParticleSystem.MinMaxCurve(.025f,.055f);main.startColor=new Color(.75f,.88f,.85f,.65f);main.gravityModifier=.15f;main.maxParticles=60;
        var emission=ps.emission;emission.rateOverTime=28;var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Hemisphere;shape.radius=.22f;
        o.GetComponent<ParticleSystemRenderer>().sharedMaterial=new Material(Resources.Load<Shader>("Art/AsterDust"));splashes.Add(ps);
    }
}
}

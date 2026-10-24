using UnityEngine;
using System.Collections.Generic;
namespace Aster {
public static class AsterRestoration {
    static readonly Dictionary<string,Material> natureMaterials=new Dictionary<string,Material>();
    public static void Install(Expedition game) {
        Transform world=game.world;
        var root=new GameObject("Restoration scenery").transform;
        var stoneMaterials=new Dictionary<Material,Material>();
        var treePlaces=new List<Vector3>();var bushPlaces=new List<Vector3>();
        foreach(Transform t in world.GetComponentsInChildren<Transform>(true)) {
            string name=t.name.ToLowerInvariant();
            if(name.Contains("kenneynature_tree")) {
                Vector3 pos=t.position;pos.y=0;
                bool duplicate=false;foreach(var p in treePlaces)if(Vector3.Distance(p,pos)<2.5f){duplicate=true;break;}
                if(!duplicate)treePlaces.Add(pos);
                t.gameObject.SetActive(false);
            }
            if(name.Contains("kenneynature_plant_bush")) {
                bool clear=true;
                foreach(Transform item in world.GetComponentsInChildren<Transform>(true))
                    if(item.name.StartsWith("I_") && Vector2.Distance(new Vector2(item.position.x,item.position.z),new Vector2(t.position.x,t.position.z))<1.4f){clear=false;break;}
                if(clear)bushPlaces.Add(new Vector3(t.position.x,0,t.position.z));
                t.gameObject.SetActive(false);
            }
            if(name.StartsWith("leafblade") || name.StartsWith("canopy") || name.StartsWith("branch"))t.gameObject.SetActive(false);
        }
        // Keep existing transforms and source meshes. Replace only stone shading.
        foreach(Renderer renderer in world.GetComponentsInChildren<Renderer>()) {
            Material[] list=renderer.sharedMaterials;
            for(int i=0;i<list.Length;i++) {
                Material src=list[i];if(!src)continue;
                string name=src.name.Replace(" (Instance)","");
                if(name!="Stone" && name!="Sand" && name!="Edge")continue;
                Material mat;
                if(!stoneMaterials.TryGetValue(src,out mat)) {
                    mat=new Material(Resources.Load<Shader>("Art/AgedStone"));mat.name=name+" weathered";
                    mat.color=src.color;mat.mainTexture=Resources.Load<Texture2D>("Restoration/StoneGrain");stoneMaterials.Add(src,mat);
                }
                list[i]=mat;
            }
            renderer.sharedMaterials=list;
        }
        string[] trees={"NormalTree_1","NormalTree_3","BirchTree_2"};
        if(treePlaces.Count==0)for(int i=0;i<18;i++)treePlaces.Add(new Vector3(i%2==0?-25:25,0,-27+i*3));
        for(int i=0;i<treePlaces.Count;i++){var p=treePlaces[i];if(Mathf.Abs(p.x)<25&&p.z>-30&&p.z<25)continue;Plant(trees[i%3],p,6.3f+(i%4)*.55f,i*137.5f,root,true);}
        for(int belt=0;belt<3;belt++)for(int i=0;i<40;i++){
         float a=(i+belt*.43f)*Mathf.PI*2/40;
         Plant(trees[(i+belt)%3],new Vector3(Mathf.Cos(a)*(35+8*belt),0,-3+Mathf.Sin(a)*(40+8*belt)),8+(i%5)*.65f,i*137.5f,root,false);
        }
        for(int i=0;i<bushPlaces.Count;i++)Plant("Bush",bushPlaces[i],.55f+(i%4)*.10f,i*79,root,false);
        // Low vegetation stays at wall margins, away from all interactive props.
        Vector3[] patches={new Vector3(7,0,17),new Vector3(-7,0,18),new Vector3(20,0,7),new Vector3(-20,0,5),new Vector3(20,0,-11),new Vector3(-21,0,-11),new Vector3(6,0,-24),new Vector3(-6,0,-24),new Vector3(20,0,-25),new Vector3(-21,0,-25)};
        for(int i=0;i<patches.Length;i++) {
            Plant("Bush",patches[i],.65f+(i%3)*.14f,i*71,root,false);
            for(int j=0;j<3;j++)Plant("Grass_Large_Extruded",patches[i]+new Vector3(Mathf.Cos(j*2.1f),0,Mathf.Sin(j*2.1f))*.7f,.22f+j*.04f,i*41+j*55,root,false);
        }
        // Repair the obvious unsupported pickup stands with visible supports.
        var disc=game.Node("I_Disc");Vector3 discBase=disc.position;discBase.y=Underside(disc);Support(discBase,.23f,root);
        SupportShelf(world,"C_HandleShelf",root);
        SupportShelf(world,"C_Shelf",root);
        foreach(Transform t in world.GetComponentsInChildren<Transform>(true))if(t.name.StartsWith("C_NoteShelf"))SupportShelf(world,t.name,root);
        // A terrain apron prevents the finite ground plane ending against blue sky.
        var ground=GameObject.CreatePrimitive(PrimitiveType.Plane);ground.name="Outer terrain apron";ground.transform.SetParent(root,false);ground.transform.position=new Vector3(0,-.12f,0);ground.transform.localScale=new Vector3(18,1,18);
        Material earth=new Material(Resources.Load<Shader>("Art/AgedStone"));earth.color=new Color(.28f,.30f,.18f);ground.GetComponent<Renderer>().sharedMaterial=earth;
        RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogColor=new Color(.25f,.34f,.32f);RenderSettings.fogStartDistance=24;RenderSettings.fogEndDistance=78;
        if(game.Node("A_PoolWater")) {var p=game.Node("A_PoolWater").position;p.y=.02f;game.Node("A_PoolWater").position=p;}
    }
    static void Plant(string name,Vector3 position,float height,float yaw,Transform parent,bool trunk) {
        var prefab=Resources.Load<GameObject>("Nature/"+name);if(!prefab){Debug.LogWarning("Missing nature model: "+name);return;}
        var plant=Object.Instantiate(prefab,parent);plant.name="Restored "+name;plant.transform.position=position;plant.transform.rotation=Quaternion.Euler(0,yaw,0)*prefab.transform.localRotation;
        Renderer[] renderers=plant.GetComponentsInChildren<Renderer>();if(renderers.Length==0)return;
        Bounds bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
        plant.transform.localScale*=height/Mathf.Max(bounds.size.y,.01f);
        bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
        plant.transform.position+=Vector3.up*(position.y-bounds.min.y);
        foreach(var r in renderers) {
            Material[] mats=r.sharedMaterials;
            for(int i=0;i<mats.Length;i++) {
                string materialName=mats[i]?mats[i].name.Replace(" (Instance)",""):"Grass";
                Material material;
                if(!natureMaterials.TryGetValue(materialName,out material)) {
                    bool leaf=materialName.Contains("Leaves") || materialName.Contains("Grass");
                    material=new Material(leaf?Resources.Load<Shader>("Art/Foliage"):Shader.Find("Standard"));
                    material.mainTexture=Resources.Load<Texture2D>("Nature/Textures/"+materialName);
                    material.color=leaf?new Color(.78f,.85f,.66f):new Color(.70f,.68f,.59f);
                    if(!leaf){material.SetFloat("_Glossiness",.13f);var normal=Resources.Load<Texture2D>("Nature/Textures/"+materialName+"_Normal");if(normal){material.SetTexture("_BumpMap",normal);material.EnableKeyword("_NORMALMAP");}}
                    material.enableInstancing=true;natureMaterials[materialName]=material;
                }
                mats[i]=material;
            }
            r.sharedMaterials=mats;
        }
        if(trunk) {
            var collider=new GameObject("Tree trunk collider");collider.transform.SetParent(parent,false);collider.transform.position=position+Vector3.up;
            var capsule=collider.AddComponent<CapsuleCollider>();capsule.height=2;capsule.radius=.21f;
        }
    }
    static void SupportShelf(Transform world,string name,Transform root) {
        Transform shelf=world.Find(name);if(!shelf)return;
        Vector3 center=shelf.position;center.y=Underside(shelf);
        Support(center+Vector3.left*.25f,.065f,root);Support(center+Vector3.right*.25f,.065f,root);
    }
    static float Underside(Transform target) {
        var renderers=target.GetComponentsInChildren<Renderer>();
        float bottom=target.position.y;
        if(renderers.Length>0){bottom=renderers[0].bounds.min.y;foreach(var renderer in renderers)bottom=Mathf.Min(bottom,renderer.bounds.min.y);}
        return bottom;
    }
    static void Support(Vector3 top,float radius,Transform root) {
        float height=Mathf.Max(.1f,top.y);
        var o=GameObject.CreatePrimitive(PrimitiveType.Cylinder);o.name="Grounded pedestal";o.transform.SetParent(root,false);o.transform.position=new Vector3(top.x,height*.5f,top.z);o.transform.localScale=new Vector3(radius*2,height*.5f,radius*2);
        var mat=new Material(Resources.Load<Shader>("Art/AgedStone"));mat.color=new Color(.38f,.33f,.24f);o.GetComponent<Renderer>().sharedMaterial=mat;
    }
}
}

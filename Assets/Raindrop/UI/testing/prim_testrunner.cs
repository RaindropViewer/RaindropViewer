using LibreMetaverse.PrimMesher;
using OpenMetaverse;
using OpenMetaverse.Rendering;
using Plugins.CommonDependencies;
using Raindrop;
using Raindrop.Bootstrap;
using UnityEngine;
using ObjectManager = Raindrop.Presenters.ObjectManager;
using Vector3 = OpenMetaverse.Vector3;

// i make prim in scene, without connection. for testing.
public class prim_testrunner : MonoBehaviour
{
    public ObjectManager obj_mgr;
    public GridClient client => RaindropInstance.GlobalInstance.Client;

    void Start()
    {
        //1. create prim data
        var pm = Globals.renderer;
        
        Primitive prim = new Primitive();
        var const_data_boxprim = OpenMetaverse.ObjectManager.BuildBasicShape(PrimType.Box); //is ok?
        Vector3 rezpos = new Vector3(0, 0, 0);
        prim.PrimData = const_data_boxprim;

        //warn: no default position.
        pm.GenerateSimpleMeshWithNormals(prim, DetailLevel.High);
        
        

    }

}

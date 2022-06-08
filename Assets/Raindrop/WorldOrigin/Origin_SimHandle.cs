using System.Collections;
using System.Collections.Generic;
using Plugins.CommonDependencies;
using Raindrop;
using UnityEngine;

//saves the current simhandle that the origin is at.
public class Origin_SimHandle : MonoBehaviour
{
    public ulong simHandle => instance.Client.Network.CurrentSim.Handle;
    private RaindropInstance instance => ServiceLocator.Instance.Get<RaindropInstance>();
    
    public ulong GetSimHandle()
    {
        return simHandle;
    }
    
    void Update()
    {
        
    }
}

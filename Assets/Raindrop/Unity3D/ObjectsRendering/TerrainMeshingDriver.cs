using System;
using OpenMetaverse;
using Plugins.CommonDependencies;
using UnityEngine;

namespace Raindrop.Unity3D
{
    // assigns the terrain to a certain simulator.
    public class TerrainMeshingDriver : MonoBehaviour
    {
        public TerrainMeshView TerrainMeshView;

        private RaindropInstance instance { get { return RaindropInstance.GlobalInstance; } }

        private float lastRenderTime = 0;
        private void OnEnable()
        {
            var controller = TerrainMeshView.controller;
            if (controller == null){
                return;
            }

            if (instance.Client.Network.CurrentSim == null)
            {
                return;
            }
            
            Simulator sim_ref = instance.Client.Network.CurrentSim;
            controller.init(sim_ref);
            
            controller.Render(lastRenderTime);
            lastRenderTime = Time.time;
        
        }
    }
}
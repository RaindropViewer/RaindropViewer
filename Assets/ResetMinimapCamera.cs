using Raindrop;
using Raindrop.Rendering;
using Raindrop.UI.Views;
using System.Collections;
using System.Collections.Generic;
using Plugins.CommonDependencies;
using Raindrop.Utilities;
using UnityEngine;

// if connected, make minimap camera look at player.
// is not connected, look at sims origin (1000, 1000)
[RequireComponent(typeof(OrthographicCameraView))]
public class ResetMinimapCamera : MonoBehaviour
{
    private RaindropInstance instance { get { return ServiceLocator.Instance.Get<RaindropInstance>(); } }
    bool Active => instance.Client.Network.Connected;

    public OrthographicCameraView cam;
    private bool ready = false;

    private void Start()
    {
         ready = cam;
    }

    public void Set()
    {
        if (Active && ready)
        {
            OpenMetaverse.Vector3d globalPos_meters = instance.Client.Self.GlobalPosition;
            Vector3 MapPos = MapSpaceConverters.GlobalSpaceToMapSpace(globalPos_meters);
            cam.SetToGridPos(MapPos);
        } else
        {
            cam.SetToGridPos(new Vector2(1000, 1000));
        }
    }
}

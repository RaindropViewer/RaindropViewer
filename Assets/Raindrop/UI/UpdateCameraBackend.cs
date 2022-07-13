using System.Collections;
using System.Collections.Generic;
using Lean.Gui;
using Plugins.CommonDependencies;
using Raindrop;
using Raindrop.Rendering;
using UnityEngine;
using Camera = UnityEngine.Camera;

// attach this to the main camera.
// This will update the backend library on the camera position to send to server.
[RequireComponent(typeof(Camera))]
public class UpdateCameraBackend : MonoBehaviour
{
    private RaindropInstance instance;
    bool Active => instance.Client.Network.Connected &&
                   (instance.Client.Network.CurrentSim != null);

    public Camera cam;
    void Start()
    {
        cam = GetComponent<Camera>();
        instance = ServiceLocator.Instance.Get<RaindropInstance>();
    }

    private void Update()
    {
        if (Active)
        {
            //set random properties like far clip
            instance.Client.Self.Movement.Camera.Far = cam.farClipPlane;
            
            //set position
            instance.Client.Self.Movement.Camera.Position = RHelp.OMVVector3(cam.transform.position);
            
            //set rotation
            var camera_rot = RHelp.OMVQuaternion4(cam.transform.rotation);
            float yaw_rad;
            float pitch_rad;
            float roll_rad;
            camera_rot.GetEulerAngles(out roll_rad, out pitch_rad, out yaw_rad);
            instance.Client.Self.Movement.Camera.Pitch(pitch_rad);
            instance.Client.Self.Movement.Camera.Yaw(yaw_rad);
            instance.Client.Self.Movement.Camera.Roll(roll_rad);
        }
    }


}

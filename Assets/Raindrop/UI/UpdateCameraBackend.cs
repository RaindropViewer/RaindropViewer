using System.Collections;
using System.Collections.Generic;
using Lean.Gui;
using Raindrop;
using Raindrop.Rendering;
using Raindrop.ServiceLocator;
using UnityEngine;
using Camera = UnityEngine.Camera;

// attach this to the main camera to update the backend on where the avatar is looking at.
[RequireComponent(typeof(Camera))]
public class UpdateCameraBackend : MonoBehaviour
{

    private RaindropInstance instance { get { return ServiceLocator.Instance.Get<RaindropInstance>(); } }
    //private RaindropNetcom netcom { get { return instance.Netcom; } }
    bool Active => instance.Client.Network.Connected;

    public Camera cam;
    void Start()
    {
        cam = this.GetComponent<Camera>();
        Debug.LogWarning("the lookat is not implemented, although the far clip is set in the backend.");
    }

    private void Update()
    {
        if (Active)
        {
            instance.Client.Self.Movement.Camera.Far = cam.farClipPlane;
            instance.Client.Self.Movement.Camera.Position = RHelp.OMVVector3(cam.transform.position);
            
        }
    }


}

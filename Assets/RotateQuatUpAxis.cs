using Raindrop.Rendering;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateQuatUpAxis : MonoBehaviour
{
    //public Vector3 myeuler;
    public Quaternion myQuat;

    public bool unityversion;
    public bool stop;

    public OpenMetaverse.Quaternion OMVmyQuat { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        myQuat = transform.rotation;
        OMVmyQuat = RHelp.OMVQuaternion4(myQuat);
    }

    // Update is called once per frame
    void Update()
    {
        if (stop)
        {
            return;
        }

        if (unityversion)
        {
            myQuat *= Quaternion.AngleAxis(0.1f, Vector3.up);
            transform.rotation = myQuat;
        } else
        {
            OMVmyQuat *= OpenMetaverse.Quaternion.CreateFromAxisAngle(OpenMetaverse.Vector3.UnitZ, 0.1f);
            transform.rotation = RHelp.TKQuaternion4(OMVmyQuat);
        }
        //myeuler += new Vector3(0, 0.1f, 0);
        //this.gameObject.transform.eulerAngles
        //    = myeuler;


    }
}

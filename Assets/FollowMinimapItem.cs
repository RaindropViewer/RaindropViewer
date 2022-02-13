using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// follow the target but maintain my own z-height
public class FollowMinimapItem : MonoBehaviour
{
    public Transform target;
    

    // Update is called once per frame
    void Update()
    {
        this.transform.position = new Vector3(
            target.transform.position.x, 
            target.transform.position.y, 
            this.transform.position.z);
    }
}

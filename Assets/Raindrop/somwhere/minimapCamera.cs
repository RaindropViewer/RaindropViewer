using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class minimapCamera : MonoBehaviour
{
    [SerializeField]
    public GameObject lookAt;
    


    void Update()
    {
        Vector3 newpos = new Vector3( lookAt.transform.position.x, this.transform.position.y, lookAt.transform.position.z) ;
        this.transform.position = newpos;
    }
}

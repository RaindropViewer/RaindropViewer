using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// use this to set the texture to the plane.
public class TexturePlane : MonoBehaviour
{
    private Material mat;
    void Start()
    {
        mat = this.GetComponent<MeshRenderer>().material;
        ApplyTexture(Texture2D.blackTexture);
    }

    public void ApplyTexture(Texture2D tex)
    {
        mat.SetTexture("_MainTex", tex);
    }
}

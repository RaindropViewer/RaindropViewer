using System.Collections;
using System.Collections.Generic;
using OpenMetaverse;
using UnityEditor;
using UnityEngine;
using Material = UnityEngine.Material;

// use this to set the texture to the plane.
// [RequireComponent(typeof(MeshRenderer))]
public class Texturable : MonoBehaviour
{
    [Header("Allows you to re-texture object by calling ApplyTexture() ")]
    private Material mat;

    public MeshRenderer renderer;

    private bool isTextured = false;
    
    public static int texturecount = 0; 

    void Awake()
    {
        if (renderer == null)
        {
            OpenMetaverse.Logger.Log("error", Helpers.LogLevel.Error);
        }
        mat = renderer.material;
        // ApplyTexture(RandomGenericTexture());
        ApplyTexture(Texture2D.blackTexture);
    }
    //
    // private Texture2D RandomGenericTexture()
    // {
    //     Texture2D res = Texture2D.whiteTexture;
    //     switch (texturecount % 4)
    //     {
    //         case 0:
    //             res = Texture2D.blackTexture;
    //             break;
    //         case 1:
    //             res = Texture2D.redTexture;
    //             break;
    //         case 2:
    //             res = Texture2D.whiteTexture;
    //             break;
    //         case 3:
    //             res = Texture2D.grayTexture;
    //             break;
    //     }
    //     texturecount++;
    //     return res;
    // }


    public void ApplyTexture(Texture2D tex)
    {
        // do we need to clear the existing texture?
        // if (isTextured)
        // {
        //     //destroy current texture.
        //     var oldtex = mat.mainTexture;
        //     mat.SetTexture("_MainTex", Texture2D.blackTexture);
        //     //Destroy(oldtex);
        // }
        
        mat.SetTexture("_MainTex", tex);
        isTextured = true;
        
        
    }
}

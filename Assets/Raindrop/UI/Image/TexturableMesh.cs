using OpenMetaverse;
using Raindrop.Services.Bootstrap;
using UnityEngine;
using UnityEngine.Serialization;
using Logger = UnityEngine.Logger;
using Material = UnityEngine.Material;

// use this to set the texture to the plane.
public class TexturableMesh : MonoBehaviour
{
    [Header("Allows you to re-texture object by calling ApplyTexture() ")]
    private Material mat;
    [Header("the renderer component on the texturablemesh")]
    public MeshRenderer renderer;
    
    [Header("this determines the default color of the tile, when it is still not yet assigned its texture. ")]
    public bool isMapTile = false;

    [Header("this sets to true when mesh is ready and we can apply the texture.")]
    public bool isReady = false;
    [Header("if this flag is set true, and present state is 'isTextured', we need to update the texture.")]
    public bool updateRequired = true;

    public TexturedState State = TexturedState.initialising;

    public enum TexturedState
    {
        initialising,   // rendering components are still waking up.
        acceptingTextures, //componets are ready.
        defaultTexture, // rendering components have a default texture assigned.
        isTextured      // the mesh is indeed textured. 
    }
    
    void Awake()
    {
        if (renderer == null)
        {
            OpenMetaverse.Logger.Log("no renderer referenced in "+ this.ToString() + " " + this.gameObject.GetInstanceID().ToString(), Helpers.LogLevel.Error);
        }
        else
        {
            mat = renderer.material;
            State = TexturedState.acceptingTextures;
        }
    }
    void Start()
    {
        ApplyLoadingTexture();
    }

    //apply a generic, loading texture to the mesh
    public void ApplyLoadingTexture()
    {
        if (State != TexturedState.acceptingTextures)
        {
            OpenMetaverse.Logger.Log("texturable mesh is not initialised",Helpers.LogLevel.Error);
            return;
        }
        
        if (isMapTile)
        {
            ApplyTexture(TextureHelpers.GetMapVoidTexture());
        }
        else
        {
            ApplyTexture(Texture2D.grayTexture); 
        }
    }

    
    // apply a texture2d to this texturable mesh.
    public void ApplyTexture(Texture2D tex)
    {
        // not ready to apply the texture to us! - bug.
        if (State == TexturedState.initialising)
        {
            Debug.LogError("renderer is not ready when ApplyTexture(t2d) was called.");
            return;
        }
        
        // do we need to clear the existing texture?
        // if (isTextured)
        // {
        //     //destroy current texture.
        //     var oldtex = mat.mainTexture;
        //     mat.SetTexture("_MainTex", Texture2D.blackTexture);
        //     //Destroy(oldtex);
        // }
        
        mat.SetTexture("_BaseMap", tex);
        // Obsolete: variant for standard render pipeline
        // mat.SetTexture("_MainTex", tex);
        if (State == TexturedState.acceptingTextures)
        {
            State = TexturedState.defaultTexture;
        }

        if (State == TexturedState.defaultTexture)
        {
            State = TexturedState.isTextured;
        }
    }
}

internal class TextureHelpers
{
    public static Texture2D GetMapVoidTexture()
    {
        return Globals.Textures.DefaultMapVoidTexture;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Plugins.ObjectPool;
using Raindrop;
using Raindrop.Services.Bootstrap;
using UnityEngine.UI;

// An implementation of rawimage view. Attach to GO that has rawimage.
[RequireComponent(typeof(RawImage))]
public class RawImageView  : MonoBehaviour
{
    public static Texture2D defaultImg => Texture2D.blackTexture; //todo: assignment.
    public void setRawImage(Texture2D img)
    {
        //hack: delete old texture before loading new one
        unloadRawImage();
        
        this.GetComponent<RawImage>().texture = img;
    }
    public void unloadRawImage()
    {
        var image = (Texture2D)this.GetComponent<RawImage>().texture;
        if (image != null)
        {
            TexturePoolSelfImpl.GetInstance().ReturnToPool(image);
            // Object.Destroy(this.GetComponent<RawImage>().texture);
        }
        this.GetComponent<RawImage>().texture = defaultImg;
    }
}


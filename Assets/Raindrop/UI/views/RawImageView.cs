using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

// An implementation of rawimage view. Attach to GO that has rawimage.
[RequireComponent(typeof(RawImage))]
public class RawImageView  : MonoBehaviour

{
    public void setRawImage(Texture2D img)
    {
        //hack: delete old texture before loading new one
        unloadRawImage();
        
        this.GetComponent<RawImage>().texture = img;
    }
    public void unloadRawImage()
    {
        if (this.GetComponent<RawImage>().texture != null)
        {
            Object.Destroy(this.GetComponent<RawImage>().texture);
        }
        this.GetComponent<RawImage>().texture = Texture2D.grayTexture;
    }
}


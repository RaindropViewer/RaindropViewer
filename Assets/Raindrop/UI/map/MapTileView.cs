using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// An implementation of view. Attach to GO that has rawimage.
// refers to a single texture; a single tile. can refer to many sims on a single tile.
[RequireComponent(typeof(RawImage))]
public class MapTileView : MonoBehaviour
{
    public void setRawImage(Texture2D img)
    {
        //hack: delete old texture before loading new one

        if (this.GetComponent<RawImage>().texture != null)
        {
            UnityEngine.Object.Destroy(this.GetComponent<RawImage>().texture);
        }
        this.GetComponent<RawImage>().texture = img;

    }
}

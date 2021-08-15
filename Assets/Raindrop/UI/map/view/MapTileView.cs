using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Raindrop.Presenters
{
    // An implementation of view. Attach to GO that has rawimage.
    // refers to a single texture; a single tile. can refer to many sims on a single tile.
    //[RequireComponent(typeof(RawImage))]


    public class MapTileView : MonoBehaviour
    {
        public GameObject texturableGO;
        private UnityEngine.Object texturableObj;

        public void setRawImage(Texture2D img)
        {
            //hack: delete old texture before loading new one

            if (texturableObj != null)
            {
                UnityEngine.Object.Destroy(texturableObj);
            }



            setTex(img);

        }

        private void setTex(Texture2D img)
        {
            if (texturableGO.GetComponent<RawImage>() != null)
            {
                texturableObj = texturableGO.GetComponent<RawImage>().mainTexture; //rawimage
                texturableObj = img;
            }

            if (texturableGO.GetComponent<MeshRenderer>() != null)
            {
                texturableObj = texturableGO.GetComponent<MeshRenderer>().material.mainTexture; //texture2d
                texturableObj = img;
            }
        }

        private void Awake()
        {
            if (texturableGO.GetComponent<RawImage>() != null)
            {
                texturableObj = texturableGO.GetComponent<RawImage>().mainTexture; //rawimage
            }

            if (texturableGO.GetComponent<MeshRenderer>() != null)
            {
                texturableObj = texturableGO.GetComponent<MeshRenderer>().material.mainTexture; //texture2d
            }
        }
    }
}

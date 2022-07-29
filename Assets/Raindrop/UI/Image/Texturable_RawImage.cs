using System;
using System.Collections;
using System.Collections.Generic;
using OpenMetaverse;
using OpenMetaverse.Assets;
using OpenMetaverse.Imaging;
using Plugins.CommonDependencies;
using Plugins.ObjectPool;
using Raindrop;
using UnityEngine;
using Logger = OpenMetaverse.Logger;

//coordinates the loading in of some UIimage.
public class Texturable_RawImage : MonoBehaviour
{
    private RaindropInstance instance => RaindropInstance.GlobalInstance;
    
    public UUID profileID;
    public GameObject LoadingImage;
    public RawImageView ProfileTexture;
    private bool isUpdating = false;

    private void Awake()
    {
        //on awake, set it to show the spinning circle
        LoadingImage.SetActive(true);
        ProfileTexture.gameObject.SetActive(false);
    }

    public void SetImageID(UUID ID)
    {
        if (profileID != ID)
        {
            if (isUpdating)
            {
                return;
            }
            isUpdating = true;
            
            profileID = ID;
            
            //fetch texture from cache...
            instance.Client.Assets.RequestImage(ID, DataReady_Callback);
            
        }
    }

    private void DataReady_Callback(TextureRequestState state, AssetTexture assetTexture)
    {
        //1. ensure data is ok.
        if ((state == TextureRequestState.Timeout) || 
            (state == TextureRequestState.Aborted) || 
            (state == TextureRequestState.NotFound))
            {
                Logger.Log("error in fetching rawimage texture: " + state.ToString(),
                    Helpers.LogLevel.Warning);
            }

        if (state != TextureRequestState.Finished)
        {
            return;
        }
        
        //2. decipher data from jp2 to plain (bitmap style) texture2d
        var img = TexturePoolSelfImpl.GetInstance().GetFromPool(TextureFormat.RGB24);
        T2D_JP2.LoadT2DWithoutMipMaps(assetTexture.AssetData, img); //blocking.

        //3. push the texture2d to the rawimage.
        ProfileTexture.setRawImage(img);

        //4. when the image is ready to be shown, enable the image and remove spinny circle
        LoadingImage.SetActive(false);
        ProfileTexture.gameObject.SetActive(true);
        
        isUpdating = false;
    }
}

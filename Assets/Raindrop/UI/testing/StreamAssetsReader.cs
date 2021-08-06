using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;

// Class to help read the assets residing in the streamingassets folder; mostly binary and image/tga files.
public class StreamAssetsReader
{

    //reads file at relative path into bytearray
    public static void read(string path, byte[] readInto)
    {
        // all at once
        byte[] data = BetterStreamingAssets.ReadAllBytes(path);

    }

    //callback when the web asset is loaded.
    //private static void StreamAssetsReader_completed(UnityWebRequestAsyncOperation obj)
    //{
    //    byte[] result = obj.webRequest.downloadHandler.data;
    //    File.WriteAllText(Application.persistentDataPath + relative_path, ((byte)obj.webRequest.result));
    //    //throw new System.NotImplementedException();
    //}



    //you can use the coroutine way, or you can use the callback function way.
    //private void OnLoadDone(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<TextAsset> obj)
    //{
    //    // In a production environment, you should add exception handling to catch scenarios such as a null result.
    //    TextAsset data = obj.Result;
    //    byte[] data_bytes = data.bytes;
    //    var tex = OpenMetaverse.Imaging.LoadTGAClass.LoadTGA(data_bytes);

    //    float timeEnd = Time.realtimeSinceStartup;

    //    if (tex == null)
    //    {
    //        Debug.Log("reading of TGA addressable failed");
    //    }
    //    else
    //    {
    //        Debug.Log("reading of TGA addressable SUCCESS! \nTook: " + (timeEnd - timeStart) + "seconds");
    //        iv.setRawImage(tex);
    //    }

    //}


}

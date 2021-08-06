//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using System.IO;

//public class TGAFileViewerAddressablesFail : MonoBehaviour
//{

//    [SerializeField]
//    public GameObject rawImageGO;
//    private RawImageView iv;
//    [SerializeField]
//    public GameObject textGO;

//    //private List<FileInfo> fileList;
//    public int currentFileIndex = 0;
//    public bool useAddressables = true;
//    private float timeStart;

//    private int imagesCount = 5;

//    private void Start()
//    {
//        //get reference to the view.
//        iv = rawImageGO.GetComponent<RawImageView>();
//        if (iv == null)
//        {
//            throw new System.Exception("Imageview is fucked"); // fix exception type plz
//        }


//        if (useAddressables == true)
//        {
//            return;
//        }
//        else
//        {
//            Debug.LogError("TGA viewer by files/streamingassets is not implemented");
//        }
    
//        ////get list of .tgas
//        //DirectoryInfo d = new DirectoryInfo(pathToTGAFolder);
//        //FileInfo[] fi = d.GetFiles("*.tga");  // LOL!!!!!! FUCK  --- GC????
//        //fileList = new List<FileInfo>(fi);

//    }

//    public void onNextPicture()
//    {
//       ReadAndSetImageAddressable(currentFileIndex);
     
//        return;
//    }
//    public void onPrevPicture()
//    {
//            ReadAndSetImageAddressable(currentFileIndex);
//        return;
//    }

//    //you can use the coroutine way, or you can use the callback function way.
//    private void OnLoadDone(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<TextAsset> obj)
//    {
//        // In a production environment, you should add exception handling to catch scenarios such as a null result.
//        TextAsset data = obj.Result;
//        byte[] data_bytes = data.bytes;
//        var tex = OpenMetaverse.Imaging.LoadTGAClass.LoadTGA(data_bytes);

//        float timeEnd = Time.realtimeSinceStartup;

//        if (tex == null)
//        {
//            Debug.Log("reading of TGA addressable failed");
//        }
//        else
//        {
//            Debug.Log("reading of TGA addressable SUCCESS! \nTook: " + (timeEnd - timeStart) + "seconds");
//            iv.setRawImage(tex);
//        }

//    }


//    private void ReadAndSetImageAddressable(int _currentFileIndex)
//    {
//        Debug.Log("reading of TGA addressable START");
//        timeStart = Time.realtimeSinceStartup;
//        Addressables.LoadAssetAsync<TextAsset>("Assets/BundleData/openmetaverse_data/blush_alpha.tga.bytes").Completed += OnLoadDone;

//    }
//}

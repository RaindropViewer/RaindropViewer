using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Better.StreamingAssets;
using System;

public class TGAFileViewerStreamingAssets : MonoBehaviour
{
    [SerializeField]
    private string pathToTGAFolder;
    [SerializeField]
    public bool useThreads; //run the extraction and GPU pumping in non-mainthread.

    [SerializeField]
    public GameObject rawImageGO;
    private RawImageView iv;
    [SerializeField]
    public GameObject textPathGO;
    [SerializeField]
    public GameObject texttime1GO;
    [SerializeField]
    public GameObject texttime2GO;

    [SerializeField]
    public List<string> filesInStreamingAssets;//= new List<string> { "openmetaverse_data/blush_alpha.tga" };
    
    //private List<FileInfo> fileList;
    public int currentFileIndex = 0;
    private float timeStart;
    private int imagesCount;

    public string[] paths;
    private byte[] poolItemBytes; //a object just to pool memory?

    private void Awake()
    {
        //in main thread, before all uses.
        BetterStreamingAssets.Initialize();
    }

    private void Start()
    {
        //get reference to the view.
        iv = rawImageGO.GetComponent<RawImageView>();
        if (iv == null)
        {
            throw new System.Exception("Imageview is fucked"); // fix exception type plz
        }
          

        //check if the path is exist.
        if (!BetterStreamingAssets.DirectoryExists(pathToTGAFolder))
        {
            Debug.LogErrorFormat("Streaming asset dir not found: {0}", pathToTGAFolder);
            return;
        }

        //get all files.
        
        paths = BetterStreamingAssets.GetFiles(pathToTGAFolder, "*.tga", SearchOption.AllDirectories);
        imagesCount = paths.Length;

        ////get list of .tgas
        //DirectoryInfo d = new DirectoryInfo(pathToTGAFolder);
        //FileInfo[] fi = d.GetFiles("*.tga");  // LOL!!!!!! FUCK  --- GC????
        //fileList = new List<FileInfo>(fi);

    }

    public void onNextPicture()
    {

        currentFileIndex++;
        currentFileIndex %= imagesCount;
        ReadAndSetImageOpenStream(currentFileIndex);


        return;
    }
    public void onPrevPicture()
    {
         
        currentFileIndex--;
        currentFileIndex += imagesCount; //not sure if necessary -- prevent negative modulo
        currentFileIndex %= imagesCount;
        ReadAndSetImageOpenStream(currentFileIndex);
         

        return;
    }

    private void ReadAndSetImageOpenStream(int _currentFileIndex)
    {
        string filepath = paths[_currentFileIndex];

        float timeStart = Time.realtimeSinceStartup;
        float timeEndStreamOpen;
        Texture2D tex;
        using (var stream = BetterStreamingAssets.OpenRead(filepath))
        {
            timeEndStreamOpen = Time.realtimeSinceStartup;
            tex = OpenMetaverse.Imaging.LoadTGAClass.LoadTGA(stream);
        }

        //StreamAssetsReader.read(filepath, poolItemBytes); // new allocation deep inside here.
        //var tex = OpenMetaverse.Imaging.LoadTGAClass.LoadTGA(poolItemBytes);
        float timeEndTex = Time.realtimeSinceStartup;
        if (tex == null)
        {
            Debug.Log("reading of TGA at path " + filepath + " failed");
            return;
        }
        

        iv.setRawImage(tex);
        setTextPath(filepath);
        setTextTime1(timeEndStreamOpen - timeStart);
        setTextTime2(timeEndTex - timeStart);
    }

    private void setTextTime2(float v)
    {
        var text = texttime2GO.GetComponent<TextView>();
        if (text != null)
        {
            text.setText((v * 1000).ToString() + " ms");
        }
    }

    private void setTextTime1(float v)
    {
        var text = texttime1GO.GetComponent<TextView>();
        if (text != null)
        {
            text.setText((v*1000).ToString() + " ms");
        }
    }

    private void setTextPath(string filepath)
    {
        var text = textPathGO.GetComponent<TextView>();
        if (text != null)
        {
            text.setText(filepath);
        }
    }


    //IEnumerator work()
    //{

    //    Debug.Log("reading of TGA addressable START");
    //    float timeStart = Time.realtimeSinceStartup;
    //    //Debug.Log("reading of TGA addressable yield1");
    //    //yield return asyncOp;
    //    TextAsset data = asyncOp.Result;
    //    byte[] data_bytes = data.bytes;
    //        Debug.Log("loadTGA");
    //    var tex = OpenMetaverse.Imaging.LoadTGAClass.LoadTGA(data_bytes);
    //    float timeEnd = Time.realtimeSinceStartup;

    //    if (tex == null)
    //    {
    //        Debug.Log("reading of TGA addressable failed");
    //    } else
    //    {
    //        Debug.Log("reading of TGA addressable SUCCESS! \nTook: " + (timeEnd - timeStart) + "seconds");
    //        iv.setRawImage(tex);

    //    }


    //}



}

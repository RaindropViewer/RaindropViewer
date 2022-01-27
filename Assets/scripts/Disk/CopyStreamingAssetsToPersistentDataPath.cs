using System;
using System.Collections.Generic;
using System.IO;
using OpenMetaverse.ImportExport.Collada14;
using UnityEngine;
using UnityEngine.Serialization;

// on every boot, as soon as possible,
// copy any missing/changed items in streaming assets to the data path in user-accessible-space
namespace Disk
{
    public class CopyStreamingAssetsToPersistentDataPath : MonoBehaviour
    {
        [FormerlySerializedAs("doneCopy")] public bool copyIsDone = false;

        private void Awake()
        {
            BetterStreamingAssets.Initialize(); //fuck, this is easy to forget.
        
            CheckOmvDataFolderAndUpdateItIfNecessary();
            copyIsDone = true;
        }

        private void CheckOmvDataFolderAndUpdateItIfNecessary()
        {
            List<string> updatedFiles = ScanAndCopy_StreamingAssetsFolder(
                DirectoryHelpers.GetInternalCacheDir()
            );
        
            if (updatedFiles.Count > 0)
            {
                var printString = String.Join(Environment.NewLine, updatedFiles);
                Debug.Log("there were some updates to staticAssets in : " 
                          + Environment.NewLine 
                          + printString);
            }
            else
            {
                Debug.Log("staticAssets are already latest version in : " 
                          + DirectoryHelpers.GetInternalCacheDir());
            }
        
        }

        // scan all files in streamingAssets folder, and copy any changed/missing files into userDataRoot.
        // return true if some file was updated.
        private List<string> ScanAndCopy_StreamingAssetsFolder(string appRootPath)
        {
            List<String> updatedFiles = new List<string>();
            // bool someFileWasUpdated = false;

            //everything in StreamingAssets\
            Debug.Assert( BetterStreamingAssets.DirectoryExists("\\") );
            string[] paths = BetterStreamingAssets.GetFiles("\\", "*", SearchOption.AllDirectories);

            foreach (var sourcePath in paths)
            {
                string relativePath = "";
            
                //find the equivalent path in appRootPath 
                string root = Directory.GetDirectoryRoot(sourcePath); // this is \StreamingAssets\, which we wish to remove.
                if (root != null/*todo: check for failed get root.*/)
                {
                    relativePath = sourcePath.Substring(root.Length - 1); // gotta remove that \
                }
                
                string targetPath = Path.Combine(appRootPath, relativePath);
                if (!File.Exists(targetPath))
                {
                    updatedFiles.Add(targetPath);
                    DirectoryHelpers.WriteToFile(
                        BetterStreamingAssets.ReadAllBytes(sourcePath),
                        targetPath);
                }

                using (Stream fs1 = File.OpenRead(targetPath))
                using (Stream fs2 = BetterStreamingAssets.OpenRead(sourcePath))
                {
                    if (FilesStreamsAreSame(fs1, fs2))
                    {
                        //do nothing
                    }
                    else
                    {
                        updatedFiles.Add(targetPath);
                        DirectoryHelpers.WriteToFile(
                            BetterStreamingAssets.ReadAllBytes(sourcePath),
                            targetPath);
                    }
                }

            }

            return updatedFiles;
        }

        // true if same data in both files.
        // false if either source or dest file is missing
        private bool FilesStreamsAreSame(Stream fs1, Stream fs2)
        {
            return FileHelpers.FileStreamsAreEqual(fs1, fs2);
        }
    }
}

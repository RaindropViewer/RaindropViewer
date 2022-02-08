using System;
using System.Collections.Generic;
using System.IO;
using OpenMetaverse;
using UnityEngine;
using SearchOption = System.IO.SearchOption;

// on every boot, as soon as possible,
// copy any missing/changed items in streaming assets to the data path in user-accessible-space
namespace Disk
{
    public class CopyStreamingAssetsToPersistentDataPath : MonoBehaviour
    {
        public bool copyIsDone = false;

        private void Awake()
        {
            BetterStreamingAssets.Initialize(); //fuck, this is easy to forget.
        
            CheckOmvDataFolderAndUpdateItIfNecessary();
            copyIsDone = true;
        }

        private void CheckOmvDataFolderAndUpdateItIfNecessary()
        {
            List<string> updatedFiles = CopyIfRequired_StreamingAssetsFolder(
                DirectoryHelpers.GetInternalCacheDir()
            );
        
            if (updatedFiles.Count > 0)
            {
                var printString = String.Join(Environment.NewLine, updatedFiles);
                OpenMetaverse.Logger.Log(
                    "These files were copied from staticAssets : " 
                      + Environment.NewLine 
                      + printString,
                    Helpers.LogLevel.Info
                    );
            }
            else
            {
                OpenMetaverse.Logger.Log(
                    "staticAssets are already latest version in : " 
                    + DirectoryHelpers.GetInternalCacheDir(),
                    Helpers.LogLevel.Info
                );
            }
        
        }

        // scan all files in streamingAssets folder, and copy any changed/missing files into userDataRoot.
        // return true if some file was updated.
        private List<string> CopyIfRequired_StreamingAssetsFolder(string appRootPath)
        {
            List<String> updatedFiles = new List<string>();
            
            //everything in StreamingAssets\
            string slash = Path.DirectorySeparatorChar.ToString();
            if (!BetterStreamingAssets.DirectoryExists(slash))
            {
                throw new Exception("StreamingAssets root not accessible.");
            }
            string[] paths = BetterStreamingAssets.GetFiles(slash, "*", SearchOption.AllDirectories);

            if (paths.Length == 0)
            {
                throw new Exception("StreamingAssets has no files in it.");
            }
            foreach (var sourcePath in paths)
            {
                string relativePath = "";
            
                //find the equivalent path in appRootPath 
                string root = Directory.GetDirectoryRoot(sourcePath); 
                if (root != null/*todo: check for failed get root.*/)
                {
                    // gotta remove that root slash character '\'
                    relativePath = sourcePath.Substring(root.Length - 1); 
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

using System;
using System.Collections.Generic;
using System.IO;
using Disk;
using OpenMetaverse;
using SearchOption = System.IO.SearchOption;

namespace UnityScripts.Disk
{
    // Copy StreamingAsset folder's contents into appRootPath,
    // a user-accessible space.
    // because LMV library cannot access StreamingAssets using normal file IO.
    public class StaticFilesCopier
    {
        private StaticFilesCopier()
        {
        }

        private static StaticFilesCopier _instance;
        public static StaticFilesCopier GetInstance()
        {
            if (_instance == null)
            {
                BetterStreamingAssets.Initialize(); //fuck, this is easy to forget.
                _instance = new StaticFilesCopier();
            }
            return _instance;
        }

        public bool CopyIsDoneAndNoErrors = false;

        // do copy (scan the files in device, copy if not same as staticassets.).
        public int Work()
        {
            int res = CheckOmvDataFolderAndUpdateItIfNecessary();
            if (res != -1) 
                CopyIsDoneAndNoErrors = true;
            return res;
        }

        //returns -1 if failed.
        private int CheckOmvDataFolderAndUpdateItIfNecessary()
        {
            List<string> updatedFiles = null;
            try
            {
                updatedFiles = CopyIfRequired_StreamingAssetsFolder(
                    DirectoryHelpers.GetInternalStorageDir()
                );
            }
            catch (Exception e)
            {
                return -1;
            }
        
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
                    + DirectoryHelpers.GetInternalStorageDir(),
                    Helpers.LogLevel.Info
                );
            }

            return 0;
        }

        // scan all files in streamingAssets folder, and copy any changed/missing files into userDataRoot.
        // return some files that were updated.
        private List<string> CopyIfRequired_StreamingAssetsFolder(string appRootPath)
        {
            List<String> updatedFiles = new List<string>();
            
            //everything in StreamingAssets\
            string slash = Path.DirectorySeparatorChar.ToString();
            if (!BetterStreamingAssets.DirectoryExists(slash))
            {
                throw new Exception(
                    "StreamingAssets root not accessible.");
            }
            string[] paths = 
                BetterStreamingAssets.GetFiles(
                    slash,
                    "*",
                    SearchOption.AllDirectories);

            if (paths.Length == 0)
            {
                throw new Exception(
                    "StreamingAssets has no files in it.");
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
                
                //1. check for missing file
                string targetPath = Path.Combine(appRootPath, relativePath);
                if (!File.Exists(targetPath))
                {
                    updatedFiles.Add(targetPath);
                    DirectoryHelpers.WriteToFile(
                        BetterStreamingAssets.ReadAllBytes(sourcePath),
                        targetPath);
                }

                //2. check for diffs in the file.
                bool fileIsDifferent = false;
                using (Stream fs1 = File.OpenRead(targetPath))
                using (Stream fs2 = BetterStreamingAssets.OpenRead(sourcePath))
                {
                    if (FileHelpers.FileStreamsAreEqual(fs1, fs2))
                    {
                        //do nothing
                    }
                    else
                    {
                        fileIsDifferent = true;
                    }
                }
                if (fileIsDifferent)
                {
                    updatedFiles.Add(targetPath);
                    DirectoryHelpers.WriteToFile(
                        BetterStreamingAssets.ReadAllBytes(sourcePath),
                        targetPath);
                }
            }
            return updatedFiles;
        }
    }
}

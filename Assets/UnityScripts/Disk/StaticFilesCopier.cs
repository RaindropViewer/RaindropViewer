using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
            
            Task<List<string>> updatedFilesTask = null;
            try
            {
                updatedFilesTask = Copy_StreamingAssetsFolder_To_UserDataRoot_Async(
                    DirectoryHelpers.GetInternalStorageDir()
                );
            }
            catch (Exception e)
            {
                return -1;
            }

            var updatedFiles = updatedFilesTask.Result; //blocking call.
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

            return 0;
        }

        // scan all files in streamingAssets folder,
        // and copy any changed/missing files into userDataRoot.
        // return the list of files that were updated.
        private async Task<List<string>> 
            Copy_StreamingAssetsFolder_To_UserDataRoot_Async(
                string appRootPath) 
        {
            List<String> updatedFiles = new List<string>();
            
            //everything in StreamingAssets\
            string slash = Path.DirectorySeparatorChar.ToString();
            if (!BetterStreamingAssets.DirectoryExists(slash))
            {
                throw new Exception(
                    "StreamingAssets root not accessible.");
            }
            string[] fromPaths = BetterStreamingAssets.GetFiles(
                slash, 
                "*", 
                SearchOption.AllDirectories);

            if (fromPaths.Length == 0)
            {
                throw new Exception(
                    "StreamingAssets has no files in it.");
            }
            
            foreach (var fromPath in fromPaths)
            {
                var relativePath = RemoveRootFolderFromPath(fromPath);
                string targetPath = Path.Combine(appRootPath, relativePath);

                //1. check for missing file
                bool fileIsMissing = !File.Exists(targetPath);
                if (fileIsMissing)
                {
                    updatedFiles.Add(targetPath);
                    DirectoryHelpers.WriteToFileAsync(
                        BetterStreamingAssets.ReadAllBytes(fromPath),
                        targetPath);
                    continue;
                }

                //2. check for diffs in the file.
                bool fileIsDifferent = false;
                using (Stream fs1 = File.OpenRead(targetPath))
                using (Stream fs2 = BetterStreamingAssets.OpenRead(fromPath))
                {
                    if (!FileHelpers.FileStreamsAreEqual(fs1, fs2))
                    {
                        fileIsDifferent = true;
                    }
                }
                if (fileIsDifferent)
                {
                    updatedFiles.Add(targetPath);
                    DirectoryHelpers.WriteToFileAsync(
                        BetterStreamingAssets.ReadAllBytes(fromPath),
                        targetPath);
                }
            }
            return updatedFiles;

            // remove root folder:
            // ie:   /streamingAssets/a/b/ -> /a/b/
            string RemoveRootFolderFromPath(string fullPath)
            {
                string relativePath = "";

                //find the equivalent path in appRootPath 
                string fromPath_Root = Directory.GetDirectoryRoot(fullPath);
                if (fromPath_Root != null /*todo: check for failed get root.*/)
                {
                    // gotta remove that root slash character '\'
                    relativePath = fullPath.Substring(fromPath_Root.Length - 1);
                }

                return relativePath;
            }
        }
    }
}

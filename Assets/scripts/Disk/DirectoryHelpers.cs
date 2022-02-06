using System.IO;
using UnityEngine;

namespace Disk
{
    public static class DirectoryHelpers
    {
        
        // returns SD card if the bool is true OR the sd card is not available.
        // otherwise i will return internal-but-shared storage
        private static string GetAndroidExternalFilesDir(bool preferSDcard)
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    // Get all available external file directories (emulated and sdCards)
                    AndroidJavaObject[] externalFilesDirectories = context.Call<AndroidJavaObject[],AndroidJavaObject[]>("getExternalFilesDirs", null);
                    AndroidJavaObject emulated = null;
                    AndroidJavaObject sdCard = null;

                    for (int i = 0; i < externalFilesDirectories.Length; i++)
                    {
                        AndroidJavaObject directory = externalFilesDirectories[i];
                        using (AndroidJavaClass environment = new AndroidJavaClass("android.os.Environment"))
                        {
                            // Check which one is the emulated and which the sdCard.
                            bool isRemovable = environment.CallStatic<bool> ("isExternalStorageRemovable", directory);
                            bool isEmulated = environment.CallStatic<bool> ("isExternalStorageEmulated", directory);
                            if (isEmulated)
                                emulated = directory;
                            else if (isRemovable && isEmulated == false)
                                sdCard = directory;
                        }
                    }
                    // Return the sdCard if available
                    if (sdCard != null && preferSDcard)
                        return sdCard.Call<string>("getAbsolutePath");
                    else
                        return emulated.Call<string>("getAbsolutePath");
                }
            }
        }

        // gives us the base directory where we should be storing the cache files
        // returns the internal storage if android.
        public static string GetInternalCacheDir()
        {
            #if UNITY_EDITOR
                return Application.persistentDataPath;
            #endif
            #if UNITY_ANDROID
                return GetAndroidExternalFilesDir(false);
            #endif
        }

        #if UNITY_ANDROID
        // returns path to the external storage in android phone.
        // returns null if the sd card is not inserted
        public static string Android_GetExternalCacheDir_WithInternalAsFallback()
        {
            return GetAndroidExternalFilesDir(true); //todo : correctly implement this.
        }
        #endif

        
        //easily write to a file
        //filePath = fully-specified file path
        public static void WriteToFile(byte[] outbytes, string filePath)
        {
            //create parent subfolders
            var parentDir = Path.GetDirectoryName(filePath);
            System.IO.Directory.CreateDirectory(parentDir);
                
            //write file
            System.IO.File.WriteAllBytes(filePath, outbytes);
            // Debug.Log($"write: {filePath} ");
        }
    }
}
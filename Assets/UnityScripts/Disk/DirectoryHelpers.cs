using System;
using System.IO;
using OpenMetaverse;
using UnityEngine;

namespace Disk
{
    public static class DirectoryHelpers
    {
        
        // Gives us the user-accessible "external directories" in Android.
        // Params: preferSDcard - set true if we want the SD card directory
        //               - if SD card is not present, the emulated external directory will be returned.
        public static string GetAndroidExternalFilesDir(bool preferSDcard)
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
        public static string GetInternalStorageDir()
        {
            #if UNITY_EDITOR || UNITY_STANDALONE_WIN
                return Application.persistentDataPath; //appdata
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
            try
            {
                System.IO.File.WriteAllBytes(filePath, outbytes);
            }
            catch (IOException e)
            {
                OpenMetaverse.Logger.Log("error writing bytes to filepath : "+ filePath + " " + e.ToString(), Helpers.LogLevel.Error);
            }
            Debug.Log($"write: {filePath} ");
        }
        
        //easily write to a file
        //filePath = fully-specified file path
        public static void WriteToFile(string toWrite, string filePath)
        {
            //create parent subfolders
            var parentDir = Path.GetDirectoryName(filePath);
            System.IO.Directory.CreateDirectory(parentDir);
                
            //write file
            try
            {
                System.IO.File.WriteAllText(filePath, toWrite);
            }
            catch (IOException e)
            {
                OpenMetaverse.Logger.Log("error writing lines to filepath : "+ filePath, Helpers.LogLevel.Error);
            }
        }
    }
}
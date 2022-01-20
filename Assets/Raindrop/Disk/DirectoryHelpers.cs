using UnityEngine;

namespace Raindrop.Disk
{
    public class DirectoryHelpers
    {
        
            // returns SD card if the bool is true OR the sd card is not available.
            // otherwise i will return internal-but-shared storage
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

            //gives us the base directory where we should be storing the cache files
            public static string GetCacheDir()
            {
                return Application.persistentDataPath; //todo : correctly implement this.

            }
    }
}
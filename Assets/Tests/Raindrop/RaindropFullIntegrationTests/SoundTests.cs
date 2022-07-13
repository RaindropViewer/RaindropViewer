using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using OpenMetaverse;
using Plugins.CommonDependencies;
using Raindrop;
using Raindrop.Bootstrap;
using Raindrop.Media;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.Raindrop.SoundTests
{
    [TestFixture()]
    public class SoundTests
    {
        // TODO: note that current soundtests is not failing on warnings of the type:
        // Failed to initialize the sound system: Raindrop.Media.MediaException: FMOD error! ERR_MEMORY - Not enough memory or resources.
        
        [SetUp]
        public void Setup()
        {
            RaindropLoader.Load();
        }

        [TearDown]
        public void TearDown()
        {
            RaindropLoader.Unload();
        }

        // copy sample audio to cache.
        // play sound is called.
        //
        // Expected: no exceptions thrown. you will hear audio play.
        [UnityTest]
        public IEnumerator OfflineCachedSound_IsPlayable_NoError()
        {
            yield return new WaitForSeconds(5);

            var instance = ServiceLocator.Instance.Get<RaindropInstance>();
            var internalDir = instance.UserDir;

            //copy the sound file to cache path in a haphazard manner:
            string relativePath_SoundFile = Path.Combine("test", "sound", "00000000-0000-0000-0000-000000000000");
            var fromPath = Path.Combine(internalDir, relativePath_SoundFile);
            var toPath = Path.Combine(instance.Client.Settings.ASSET_CACHE_DIR, "0", "0", "00000000-0000-0000-0000-000000000000");
            DeleteTestFile(toPath);
            CopyTestFile(fromPath, toPath);

            instance.MediaManager.PlayUISound(UUID.Zero);

            Debug.Log("play sound");
            yield return new WaitForSeconds(20);

            DeleteTestFile(toPath);
            yield break;

            static void DeleteTestFile(string toPath)
            {
                if (File.Exists(toPath))
                {
                    File.Delete(toPath);
                }
            }

            static void CopyTestFile(string fromPath, string toPath)
            {
                try
                {
                    File.Copy(fromPath, toPath);
                }
                catch (Exception e)
                {
                    Debug.Log("failed to copy sample sound file for the test: " + e.ToString());
                }
            }
        }

        // Prerequisite: you have copied a ogg file into the
        // cache at /cache/0/0/00000000-0000-0000-0000-000000000000

        // you will hear audio play. no exceptions thrown. green tick.
        [UnityTest]
        public IEnumerator OfflineUncachedSound_Playable_ButNoSound()
        {
            var instance = ServiceLocator.Instance.Get<RaindropInstance>();

            instance.MediaManager.PlayUISound(UUID.Zero); 

            Debug.Log("play sound");
            yield return new WaitForSeconds(20);

            yield break;
        }
    
        // detect problems with raindrop instance starting and stopping again.
        [UnityTest]
        public IEnumerator StabilityTest_Restart_RaindropInstance()
        {
            int i = 8;
            while (i-- > 0)
            {
                RaindropInstance instance = new RaindropInstance(new GridClient());
                instance.MediaManager.PlayUISound(UUID.Zero);

                yield return new WaitForSeconds(4);
                
                instance.CleanUp();
                instance = null;
            }

            yield break;
        }
        
    }
}
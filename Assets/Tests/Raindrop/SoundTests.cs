using System.Collections;
using NUnit.Framework;
using OpenMetaverse;
using Raindrop;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.Raindrop
{
    [TestFixture()]
    public class SoundTests
    {
        // Prerequisite: you have copied a ogg file into the
        // cache at /cache/0/0/00000000-0000-0000-0000-000000000000
        
        // you will hear audio play. no exceptions thrown. green tick.
        [UnityTest]
        public IEnumerator Play_CachedSound_IsSuccessful()
        {
            RaindropInstance instance = new RaindropInstance(new GridClient());

            instance.MediaManager.PlayUISound(UUID.Zero);

            instance.CleanUp();
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
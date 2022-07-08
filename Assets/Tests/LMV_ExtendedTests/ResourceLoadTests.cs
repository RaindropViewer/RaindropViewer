using System.Collections;
using System.IO;
using NUnit.Framework;
using OpenMetaverse;
using OpenMetaverse.Imaging;
using UnityEngine;
using UnityEngine.TestTools;

namespace Raindrop.Tests.LMV_ExtendedTests
{
    [TestFixture]

    public class ResourceLoadTests
    {
        #region Setup, teardown
        private static RaindropInstance instance;

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            instance.CleanUp();
        }
        
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            GameObject mainThreadDispatcher = 
                new GameObject("mainThreadDispatcher");
            mainThreadDispatcher.AddComponent<UnityMainThreadDispatcher>();

            // make sure the Files are already copied to the runtime folder!
            Helpers.DoStartupCopy();
            
            instance = new RaindropInstance(new GridClient());
        }
        #endregion
        
        [UnityTest]
        public IEnumerator ResourceLoad_TGA_GetResourceStream()
        {
            string res_file_name = "upperbody_color.tga";
            using (Stream stream = 
                   OpenMetaverse.Helpers.GetResourceStream(
                       res_file_name, 
                       OpenMetaverse.Settings.RESOURCE_DIR))
            {
                Assert.True(stream != null,
                    "Unable to open resource file " +
                    $"{res_file_name}" + 
                    "in dir " + 
                    $"{OpenMetaverse.Settings.RESOURCE_DIR}");  
            }
            yield break;
        }
        
        [UnityTest]
        public IEnumerator ResourceLoad_TGA_LoadResourceLayer()
        {
            string res_file_name = "upperbody_color.tga";
            
            var res = Baker.LoadResourceLayer(res_file_name);

            yield break;
        }
    }
}
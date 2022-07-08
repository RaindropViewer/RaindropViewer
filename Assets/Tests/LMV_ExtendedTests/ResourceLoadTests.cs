using System.Collections;
using System.IO;
using NUnit.Framework;
using OpenMetaverse;
using OpenMetaverse.Imaging;
using UnityEngine.TestTools;

namespace Raindrop.Tests.LMV_ExtendedTests
{
    [TestFixture]

    public class ResourceLoadTests
    {
        [UnityTest]
        public IEnumerator ResourceLoad_TGA_GetResourceStream()
        {
            //create
            var instance = new RaindropInstance(new GridClient());
            
            string res_file_name = "upperbody_color.tga";
            using (Stream stream = OpenMetaverse.Helpers.GetResourceStream(res_file_name, OpenMetaverse.Settings.RESOURCE_DIR))
            {
                //can open => passed! :)
                Assert.Pass();  
            }
            
            //cleanup
            if (instance != null)
            {
                instance.CleanUp();
                instance = null;
            }

            yield break;
        }
        
        [UnityTest]
        public IEnumerator ResourceLoad_TGA_LoadResourceLayer()
        {
            //create
            var instance = new RaindropInstance(new GridClient());
            
            string res_file_name = "upperbody_color.tga";
            
            var res = Baker.LoadResourceLayer(res_file_name);

            //cleanup
            if (instance != null)
            {
                instance.CleanUp();
                instance = null;
            }

            yield break;
        }
    }
}
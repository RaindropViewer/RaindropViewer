using System.Collections;
using NUnit.Framework;
using OpenMetaverse;
using Plugins.CommonDependencies;
using Raindrop.Bootstrap;
using Raindrop.Services;
using Raindrop.Tests.RaindropFullIntegrationTests.InputSubroutines;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Raindrop.Tests.RaindropFullIntegrationTests
{
    [TestFixture()]
    public class EulaAcceptanceTests
    {
        [UnityTest]
        //go the the flat file and reset the EulaAccepted field.
        public IEnumerator ResetEulaAcceptance_InBackEnd()
        {
            var instance = new RaindropInstance(new GridClient());
            instance.GlobalSettings["EulaAccepted"] = false;
            instance.GlobalSettings.Save();

            //cleanup
            if (instance != null)
            {
                instance.CleanUp();
                instance = null;
            }

            instance = new RaindropInstance(new GridClient());
            Assert.False(instance.GlobalSettings["EulaAccepted"]);
            
            //cleanup
            if (instance != null)
            {
                instance.CleanUp();
                instance = null;
            }

            yield break;
        }

        [UnityTest]
        // the EULA screen is visible and accepting the EULA takes effect in the next run.
        // 1. internally set the eula is false.
        // 2. restart the ui and check it is showing the eula screen.
        // 3. accept the eula.
        // 4. restart the ui and check it is showing welcome screen
        public IEnumerator Can_Accept_EULA()
        {
            RaindropLoader.Load();
            yield return new WaitForSeconds(2);

            //1. reject the EULA
            var instance = RaindropInstance.GlobalInstance;
            instance.GlobalSettings["EulaAccepted"] = false;
            instance.GlobalSettings.Save();
            Assert.False(instance.GlobalSettings["EulaAccepted"]);
            
            //2 restart the UI. 
            UIService ui = ServiceLocator.Instance.Get<UIService>();
            ui.initialise();
            
            //2a. assert the eula prompt is present
            Assert.True(ui.GetPresentCanvasType() == CanvasType.Eula);
            
            //2b. accept the eula
            if (ui.GetPresentCanvasType() == CanvasType.Eula)
            {
                // well, we need to agree to eula first.
                yield return Login.accepttheeula();
                yield return new WaitForSeconds(2);
            }
            
            //3 restart the UI. 
            ui.initialise();
            yield return new WaitForSeconds(2);

            //3b. should not be eula screen
            if (ui.GetPresentCanvasType() == CanvasType.Eula)
            {
                Assert.Fail("eula is accepted, but the eula screen is appearing on startup");
            }

            if (ui.GetPresentCanvasType() == CanvasType.Welcome)
            {
                Assert.Pass();
            }
            
            RaindropLoader.Unload();
            yield break;
        }
    }
}
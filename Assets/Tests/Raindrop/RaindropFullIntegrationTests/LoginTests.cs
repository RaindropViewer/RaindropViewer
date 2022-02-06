using System.Collections;
using NUnit.Framework;
using Raindrop;
using Raindrop.Netcom;
using Raindrop.Presenters;
using Raindrop.ServiceLocator;
using Raindrop.Services;
using Tests.Raindrop.RaindropFullIntegrationTests.InputSubroutines;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.Raindrop.RaindropFullIntegrationTests
{
    /*
     * UI-intensive tests for the login functionality. the main scene will be loaded.
     * 
     */
    [TestFixture()]
    public class LoginTests
    {
        private static string _username = "***REMOVED*** Resident"; // fixme: move this to some xml
        private static string _password = "***REMOVED***";
        private RaindropNetcom netcom { get { return instance.Netcom; } }
        private RaindropInstance instance { get { return ServiceLocator.Instance.Get<RaindropInstance>(); } }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            //load the main scene.
            SceneManager.LoadScene("Scenes/MainScene"); 
        }

        [UnityTest]
        //assert that the loading main scene have no huge errors.
        public IEnumerator AbleToAssertPass()
        {
            Assert.Pass();
            yield break;
        }

        [UnityTest]
        //login UI-backend-UI test
        /* 1. [UI] enter creds, press login button
         * 2. check backend API, login is true
         * 3. [UI] press logout button.
         * 4. check is logged out
         *
         * 5. then do the above again.
         * 6. passed
         */
        public IEnumerator LoginLogoutTest()
        {
            //0. is the scene ready?
            var vm = Get_ViewsManager();

            //1. get the refence to the UI service.
            var srvLocator = ServiceLocator.Instance;
            UIService uiSrv = null;
            try
            {
                uiSrv = srvLocator.Get<UIService>();
            }
            catch
            {
                Assert.Fail("UIService unavailable.");
            }
            // uiSrv.resetUI();
            
            //1a. accept the eula if needed.
            if (uiSrv.ScreensManager.TopCanvas.canvasType == CanvasType.Eula)
            {
                // well, we need to agree to eula first.
                yield return Login.accepttheeula();
                yield return new WaitForSeconds(2);
            }

            SetClientSettings();
            
            int times = 2;
            for (int i = 0; i < times; i++)
            {
                //1b. we are on the welcome screen. now navigate to the login screen.
                if (uiSrv.ScreensManager.TopCanvas.canvasType == CanvasType.Welcome)
                {
                    yield return new WaitForSeconds(2);
                    yield return Utils.UIHelpers.Click_Button_Welcome2LoginScreen();
                }
                else
                {
                    Assert.Fail("expected to be on the welcome screen!");
                }
                
                //1b. we are on the login screen. do login. assert logged in.
                Assert.IsTrue(uiSrv.ScreensManager.TopCanvas.canvasType == CanvasType.Login);
                LoginPresenterIsAvailable(vm);
            
                yield return Login.StartLogin(_username, _password);
            
                //assert the backend API; that we are logged in.
                Assert.True(instance.Client.Network.Connected == true, "check API that we are logged in");
            
                //finally, disconnect. assert disconnected.
                UIHelpers.Click_ButtonByUnityName("LogoutBtn");
                yield return new WaitForSeconds(10);
                Assert.True(instance.Client.Network.Connected == false, "check API that we are logged out");
                
                yield return new WaitForSeconds(4);
                
            }
            
            yield break;
        }

        private static GameObject Get_ViewsManager()
        {
            var gm = GameObject.Find("GameManager");
            var vm = GameObject.Find("ViewsManager");
            var mm = GameObject.Find("ModalManager");
            var go = GameObject.Find("GlobalOverlay");
            UnityEngine.Assertions.Assert.IsTrue(gm & vm & mm & go);

            return vm;
        }

        private static void LoginPresenterIsAvailable(GameObject vm)
        {
            var loginPresenter
                = vm.GetComponent<ScreensManager>().getForegroundCanvas().GetComponent<LoginPresenter>();
            Assert.True(loginPresenter != null);
        }
        
        private void SetClientSettings()
        {
            //Extra: set reducedsettings so that we do not use too many dependencies
            instance.Client.Settings.ALWAYS_DECODE_OBJECTS = false;
            instance.Client.Settings.ALWAYS_REQUEST_OBJECTS = false;
            instance.Client.Settings.MULTIPLE_SIMS = false;
            instance.Client.Settings.SEND_AGENT_APPEARANCE = false;
            instance.Client.Settings.USE_ASSET_CACHE = false;
            instance.Client.Settings.STORE_LAND_PATCHES = false;
            instance.Client.Settings.STORE_LAND_PATCHES = false;
        }
        
        public class Utils
        {
            /*
             * A whole bunch of methods to click buttons and type into inputfields.
             */
            public class UIHelpers
            {

                public static IEnumerator Click_Button_Welcome2LoginScreen()
                {
                    Assert.IsTrue(
                        InputSubroutines.UIHelpers.Click_ButtonByUnityName("LetsGo!")
                        );
                    yield return new WaitForSeconds(2); // if you remvoe this one you will fail the test.
                }
            }
        }
    }

}

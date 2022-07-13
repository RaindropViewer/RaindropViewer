using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Plugins.CommonDependencies;
using Raindrop.Bootstrap;
using Raindrop.Netcom;
using Raindrop.Services;
using Raindrop.Tests.RaindropFullIntegrationTests.InputSubroutines;
using Tests;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Raindrop.Tests.RaindropFullIntegrationTests
{
    /*
     * UI intensive, full integration tests for the login functionality. 
     */
    [TestFixture()]
    public class LoginTests
    {
        private RaindropNetcom netcom { get { return instance.Netcom; } }
        private RaindropInstance instance { get { return ServiceLocator.Instance.Get<RaindropInstance>(); } }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            //load the main scene.
            RaindropLoader.Load();
        }
        
        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            RaindropLoader.Unload();
        }

        [UnityTest]
        //assert that the loading main scene have no huge errors.
        public IEnumerator LoadScene_MainScene_IsOK()
        {
            yield return new WaitForSeconds(10);
            Assert.Pass();
            yield break;
        }

        // UI Test: (Login, logout) x2
        /* X. [UI, EULA] maybe need to accept EULA 
         * 1. [UI, welcome] select the desired grid in the dropdown
         * 2. [UI, welcome] select the desired grid in the dropdown
         * 3. enter creds, press login button
         * 4. check backend API, login is true
         * 5. [UI] press logout button.
         * 6. check is logged out
         *
         * 7. then do the above x2.
         * 6. passed
         */
        [UnityTest]
        [Timeout(100000000)]
        public IEnumerator LoginLogoutTest()
        {
            GetTo_LoginScreen();

            SetClient_SettingsMinimal();
            
            var uiSrv = ServiceLocator.Instance.Get<UIService>();

            
            //Finally, perform 2x login-logout for each test-user!
            for (int loginCredIdx = 0; loginCredIdx < Secrets.GridUsers.Count; loginCredIdx++)
            {
                int times = 2;
                yield return Login.DoExhaustiveLogins(
                    Secrets.GridUsers[loginCredIdx], 
                    Secrets.GridPass[loginCredIdx],
                    Secrets._gridFriendlyNames[loginCredIdx],
                    times,
                    instance, uiSrv);
            }
            
            yield break;
        }

        [UnityTest]
        [Timeout(100000000)]
        public IEnumerator LoginAndDoNothing()
        {
            int userIdx = 2;
            
            Debug.Log("Logging to " + Secrets._gridFriendlyNames[userIdx]);
            
            GetTo_LoginScreen();

            // SetClient_SettingsMinimal();
            
            var uiSrv = ServiceLocator.Instance.Get<UIService>();

            //finally, do login.
            //1b. we are on the welcome screen.
            Assert.True(
                uiSrv.GetPresentCanvasType() == CanvasType.Welcome,
                "expect current view to be welcome canvas");
            yield return new WaitForSeconds(5);
                
            // 1c. do "select grid by ui"
            //get knwon grids
            var grids = instance.GridManger.Grids;
            string friendlyName_grid = Secrets._gridFriendlyNames[userIdx];
            yield return LoginTests.Utils.UIHelpers.Click_Dropdown_Then_Select_ByString(
                "GridDropdown",
                friendlyName_grid
            );
                
            //now navigate to the login screen.
            yield return new WaitForSeconds(2);
            yield return LoginTests.Utils.UIHelpers.Click_Button_Welcome2LoginScreen();
                
            //1b. we are on the login screen. do login. assert logged in.
            Assert.IsTrue(uiSrv.GetPresentCanvasType() == CanvasType.Login);
            yield return Login.StartLogin(Secrets.GridUsers[userIdx], Secrets.GridPass[userIdx]);
            // Assert.True(uiSrv._loadingController.IsVisible, "expected: loading screen is visible.");
            yield return new WaitForSeconds(12);

            //for login successful, the loading will fade by itself.
            //UIHelpers.Click_ButtonByUnityName("CloseLoadingScreenButton");
            yield return new WaitForSeconds(8);
            Assert.False(uiSrv._loadingController.isInteractable, "login has failed. the message is " + instance.Client.Network.LoginMessage);
            
            //assert the backend API; that we are logged in.
            Assert.True(instance.Client.Network.Connected == true, "check API that we are logged in");

            
                        
            yield return new WaitForSeconds(10000000);
 
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

        private void SetClient_SettingsMinimal()
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
        
        public static IEnumerator GetTo_LoginScreen()
        {
            //0. is the scene ready?
            yield return new WaitForSeconds(2);
            var viewsManager = Get_ViewsManager();

            //1. get UI service.
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
            
            //1a. accept the eula if it appears.
            if (uiSrv.GetPresentCanvasType() == CanvasType.Eula)
            {
                // well, we need to agree to eula first.
                yield return Login.accepttheeula();
                yield return new WaitForSeconds(2);
            }
            
            
            
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
                        global::Raindrop.Tests.RaindropFullIntegrationTests.InputSubroutines.UIHelpers.Click_ButtonByUnityName("LetsGo!")
                        );
                    yield return new WaitForSeconds(2); // if you remvoe this one you will fail the test.
                }

                // click a dropdown by name,
                // selecting the entry that matches the string
                public static IEnumerator Click_Dropdown_Then_Select_ByString(string DropdownName,
                                                                            string targetDropdownEntry)
                {
                    // var dd =
                    //     InputSubroutines.UIHelpers.Click_Dropdown_ByUnityName(DropdownName);
                    //
                    // Assert.NotNull(dd);
                    // yield return new WaitForSeconds(1);

                    var dd = GameObject.Find(DropdownName);
                    Assert.NotNull(dd);
                    var TMP_dd = dd.GetComponent<TMP_Dropdown>();
                    Assert.NotNull(TMP_dd);

                    Assert.IsTrue(
                        global::Raindrop.Tests.RaindropFullIntegrationTests.InputSubroutines.UIHelpers.Click_DropdownEntry_ByName(
                            TMP_dd,
                            targetDropdownEntry),
                        "The entry " + targetDropdownEntry.ToString() + " does not exist"
                    );
                    yield return new WaitForSeconds(1);
                }
            }
        }
    }

}

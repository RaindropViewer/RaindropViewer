using NUnit.Framework;
using OpenMetaverse;
using Raindrop.Netcom;
using Raindrop.Presenters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lean.Gui;
using NUnit.Framework.Internal;
using OpenMetaverse.Assets;
using Raindrop.Services;
using TMPro;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Raindrop.Tests
{
    /*
     * These are UI-intensive tests. the main scene will be loaded.
     * 
     */
    [TestFixture()]
    public class RaindropIntegrationTests
    {
        private bool loggedin;
        private bool testfail = false;

        private RaindropNetcom netcom { get { return instance.Netcom; } }
        private RaindropInstance instance { get { return ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>(); } }

        
        [SetUp]
        public void Setup()
        {
            //load the main scene.
            SceneManager.LoadScene("Scenes/MainScene"); 
        }

        [TearDown]
        public void TearDown()
        {
            netcom.Logout();
            Application.Quit();
        }

        
        [UnityTest]
        //login UI-backend-UI test
        /* 1. [UI] enter creds, press login button
         * 2. check backend API, login is true
         * 3. [UI] press logout button.
         * 4. check is logged out
         * 5. passed.
         */
        public IEnumerator LoginLogoutTest()
        {
            
            //0. is the scene ready?
            var gm = GameObject.Find("GameManager");
            var vm = GameObject.Find("ViewsManager");
            var mm = GameObject.Find("ModalManager");
            var go = GameObject.Find("GlobalOverlay");

            //1. get the refence to the UI service.
            var srvLocator = ServiceLocator.ServiceLocator.Instance;
            var UISrv = srvLocator.Get<UIService>();
            // srvLocator.Get<UIService>().resetUI();
            
            //1a. accept the eula if needed.
            if (UISrv.canvasManager.topCanvas.canvasType == CanvasType.Eula)
            {
                // well, we need to agree to eula first.
                Utils.UI.accepttheeula();
            }
            
            //1b. we are on the welcome screen. now navigate to the login screen.
            if (UISrv.canvasManager.topCanvas.canvasType == CanvasType.Welcome)
            {
                yield return new WaitForSeconds(2);
                Utils.UI.Click_Button_Welcome2LoginScreen();
            }
            else
            {
                Assert.Fail("expected to be on the login screen!");
            }
            
            //Extra: set reducedsettings so that we do not use too many dependencies
            instance.Client.Settings.ALWAYS_DECODE_OBJECTS = false;
            instance.Client.Settings.ALWAYS_REQUEST_OBJECTS = false;
            instance.Client.Settings.MULTIPLE_SIMS= false;
            instance.Client.Settings.SEND_AGENT_APPEARANCE= false;
            instance.Client.Settings.USE_ASSET_CACHE= false;
            instance.Client.Settings.STORE_LAND_PATCHES= false;
            instance.Client.Settings.STORE_LAND_PATCHES= false;
            
            //1b. we are on the login screen. do login. assert logged in.
            Assert.IsTrue(UISrv.canvasManager.topCanvas.canvasType == CanvasType.Login);
            yield return new WaitForSeconds(2);
            var loginPresenter
                = vm.GetComponent<CanvasManager>().getForegroundCanvas().GetComponent<LoginPresenter>();
            string _username = "***REMOVED*** Resident";
            Utils.UI.Set_TMPInputField_ofGameObjectName("UserTextField", _username);
            string _password = "***REMOVED***";
            Utils.UI.Set_TMPInputField_ofGameObjectName("PassTextField", _password);
            yield return new WaitForSeconds(2);
            Utils.UI.ClickButtonByUnityName("LoginBtn");
            //assert the backend API; that we are logged in.
            yield return new WaitForSeconds(20);
            Assert.True(instance.Client.Network.Connected == true, "check API that we are logged in");
            
            //finally, disconnect. assert disconnected.
            Utils.UI.Click_DisconnectButton();
            yield return new WaitForSeconds(5);
            Assert.True(instance.Client.Network.Connected == false, "check API that we are logged out");
            
            
            
            Assert.Pass();
            // yield break;

        }
        public async Task GetTestTaskAsync(int delay)
        {
            Debug.Log("1");
            await Task.Delay(TimeSpan.FromMilliseconds(delay));
            Debug.Log("2");
            //await Task.Run(async () => await Task.Delay(TimeSpan.FromSeconds(delay)));
            //Debug.Log("3");
            //await Task.Delay(TimeSpan.FromMilliseconds(200));
            //Debug.Log("4");
        }


        public class Utils
        {
            /*
             * A whole bunch of methods to click buttons and type into inputfields.
             */
            public class UI
            {
                public static bool ClickButtonByUnityName(string gameObjectName)
                {
                    var btn = GameObject.Find(gameObjectName);
                    Assert.IsNotNull(btn, "Missing button " + btn.ToString());

                    if (btn.GetComponent<Button>())
                    {
                        btn.GetComponent<Button>().onClick.Invoke();
                        return true;
                    }
                    if (btn.GetComponent<LeanButton>())
                    {
                        btn.GetComponent<LeanButton>().OnClick.Invoke();
                        return true;
                    }

                    return false;
                }

                public static void Set_TMPInputField_ofGameObjectName(string go_name, string input)
                {
                    var go = GameObject.Find(go_name);
                    Assert.True(go != null, "unable to find gameobject of name " + go_name);
                    var tmp = go.GetComponent<TMP_InputField>();
                    Assert.True(tmp != null, "Gameobject + " + go_name + " does not have TMP inputfield component ");
                    tmp.ActivateInputField();
                    tmp.text = input;
                    tmp.onValueChanged.Invoke(input);
                    tmp.onSubmit.Invoke(input);
                }

                public static void Click_DisconnectButton()
                {
                    ClickButtonByUnityName("LogoutBtn");
                }

                //accept the eula:
                /* 1. toggle true
                 * 2. click "next" btn
                 */

                // on the welcome screen, click the go button.
                public static void accepttheeula()
                {
                    string eulaCheckbox = "AgreeToggle";
                    var checkboxEULA = GameObject.Find(eulaCheckbox);
                    Assert.IsNotNull(checkboxEULA, "Missing checkbox " + eulaCheckbox);
                    checkboxEULA.GetComponent<Toggle>().onValueChanged.Invoke(true);
            
                    string eulaCloseBtn = "NextButton";
                    Assert.IsTrue(RaindropIntegrationTests.Utils.UI.ClickButtonByUnityName(eulaCloseBtn));
                }

                public static void Click_Button_Welcome2LoginScreen()
                {
                    Assert.IsTrue(RaindropIntegrationTests.Utils.UI.ClickButtonByUnityName("LetsGo!"));
                }
            }
        }
    }

}

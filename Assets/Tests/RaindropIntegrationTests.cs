using NUnit.Framework;
using OpenMetaverse;
using Raindrop.Netcom;
using Raindrop.Presenters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lean.Gui;
using NUnit.Framework.Internal;
using OpenMetaverse.Assets;
using OpenMetaverse.Imaging;
using OpenMetaverse.ImportExport.Collada14;
using Raindrop.Services;
using TMPro;
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
        private static string _username;
        private static string _password;
        private RaindropNetcom netcom { get { return instance.Netcom; } }
        private RaindropInstance instance { get { return ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>(); } }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            //load the main scene.
            SceneManager.LoadScene("Scenes/MainScene"); 
        }
        //
        // [TearDown]
        // public void TearDown()
        // {
        //     SceneManager.CreateScene("some scene");
        //     
        // }
        //

        // decode a j2p into a managed image. convert it to t2d. save it to disk. 
        // the goal is to make sure the image is not upside down.
        // [UnityTest]
        // public IEnumerator decode_j2p_variant1()
        // {
        //     //decode
        //     var relax_b = File.ReadAllBytes("C:\\Users\\Alexis\\Pictures\\menhara.jp2");
        //     ManagedImage im;
        //     OpenJPEG.DecodeToImage(relax_b, out im);
        //     
        //     //convert to unityland
        //     Texture2D t2d;
        //     t2d = im.ExportTex2D();
        //     
        //     //print.
        //     var bytes = t2d.EncodeToJPG(100);
        //     System.IO.File.WriteAllBytes(Path.Combine(Application.persistentDataPath, "relax.jpg"), bytes);
        //
        //     
        //     yield break;
        // }

        // [UnityTest]
        // // managedimage -> texture2d -> managed image test
        // /* 1. create managedimage 2x2 red-blue-green- black image by code.
        //  * 2. call convert to texture2d.
        //  * 3. print to screen.
        //  * 4. convert back to managed image.
        //  * 5. print to disk.
        //  */ //weird ass test
        // public IEnumerator ManagedImage_Texture2D_conversions()
        // {
        //     //1 load the image using unity's texture 2d (known to be correct.).
        //     var tex = new Texture2D(1024,1024);
        //     var b = File.ReadAllBytes("C:\\Users\\Alexis\\Pictures\\menhara.jpg");
        //     tex.LoadImage(b);
        //         // OpenMetaverse.Imaging.LoadTGAClass.LoadTGA(
        //         // "C:\\Users\\Alexis\\Pictures\\menhara.tga");
        //         
        //         
        //     
        //     //1b. check image loading integrity.
        //     // var bytesa = tex.EncodeToJPG(100);
        //     // System.IO.File.WriteAllBytes(Path.Combine(Application.persistentDataPath, "menhara_from_t2d.jpg"), bytesa);
        //     // mi.Blue[2] = 0xFF; 
        //     // mi.Red[0] = 0xFF; 
        //     // mi.Green[1] = 0xFF;
        //     // // mi.Blue = new byte[]{0x00,0x00,0xFF,0x00}; 
        //     // mi.Red = new byte[]{0xFF,0x00,0x00,0x00}; 
        //     // mi.Green = new byte[]{0x00,0xFF,0x00,0x00}; 
        //     
        //     //2 print mi to disk in some easy to read format.
        //     // error! this image is written to disk upside down!.
        //     ManagedImage mi = new ManagedImage(tex);
        //     var tgaBytes = mi.ExportTGA();
        //     Debug.Log("writing to "+ Path.Combine(Application.persistentDataPath, "exportTGA_managed_image1.tga").ToString());
        //     System.IO.File.WriteAllBytes(Path.Combine(Application.persistentDataPath, "exportTGA_managed_image1.tga"), tgaBytes);
        //
        //     //3. convert to t2d and show on screen.
        //     Texture2D t2d = mi.ExportTex2D();
        //     // plane.make
        //     // plane.show(t2d)
        //     
        //     var bytes = t2d.EncodeToJPG(100);
        //     System.IO.File.WriteAllBytes(Path.Combine(Application.persistentDataPath, "EncodeToJPG_Texture2d.jpg"), bytes);
        //
        //     yield return new WaitForSeconds(1);
        //     
        //     //4. convert back to tga, save it as a 2nd file.
        //     ManagedImage mi2 = new ManagedImage(t2d);
        //     // error:this method writes a up-side down image.
        //     var tgaBytes2 = mi2.ExportTGA();
        //     System.IO.File.WriteAllBytes(Path.Combine(Application.persistentDataPath, "exportTGA_managedimage2.tga"), tgaBytes2);
        //     
        //     Assert.Pass();
        //     yield break;
        // }

        [UnityTest]
        //assert pass is working (teardown is ok)
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
         * 5. passed.
         */
        public IEnumerator LoginLogoutTest()
        {
            //0. is the scene ready?
            var vm = Get_ViewsManager();

            //1. get the refence to the UI service.
            var srvLocator = ServiceLocator.ServiceLocator.Instance;
            var UISrv = srvLocator.Get<UIService>();
            // srvLocator.Get<UIService>().resetUI();
            
            //1a. accept the eula if needed.
            if (UISrv.canvasManager.topCanvas.canvasType == CanvasType.Eula)
            {
                // well, we need to agree to eula first.
                Utils.UIHelpers.accepttheeula();
            }
            
            //1b. we are on the welcome screen. now navigate to the login screen.
            if (UISrv.canvasManager.topCanvas.canvasType == CanvasType.Welcome)
            {
                yield return new WaitForSeconds(2);
                Utils.UIHelpers.Click_Button_Welcome2LoginScreen();
            }
            else
            {
                Assert.Fail("expected to be on the welcome screen!");
            }
            
            SetClientSettings();

            //1b. we are on the login screen. do login. assert logged in.
            Assert.IsTrue(UISrv.canvasManager.topCanvas.canvasType == CanvasType.Login);
            yield return new WaitForSeconds(2); // if you remvoe this one you will fail the test.
            LoginPresenterIsAvailable(vm);
            TypeUserAndPassIntoLoginPanel();
            yield return new WaitForSeconds(2);
            Utils.UIHelpers.ClickButtonByUnityName("LoginBtn");
            
            //assert the backend API; that we are logged in.
            yield return new WaitForSeconds(20);
            Assert.True(instance.Client.Network.Connected == true, "check API that we are logged in");
            
            //finally, disconnect. assert disconnected.
            Utils.UIHelpers.Click_DisconnectButton();
            yield return new WaitForSeconds(5);
            Assert.True(instance.Client.Network.Connected == false, "check API that we are logged out");
            
            
            
            yield return new WaitForSeconds(30);
            
            
            
            
            //try to login again!
            
            //1b. we are on the welcome screen. now navigate to the login screen.
            if (UISrv.canvasManager.topCanvas.canvasType == CanvasType.Welcome)
            {
                yield return new WaitForSeconds(2);
                Utils.UIHelpers.Click_Button_Welcome2LoginScreen();
            }
            else
            {
                Assert.Fail("expected to be on the welcome screen!");
            }
            Assert.IsTrue(UISrv.canvasManager.topCanvas.canvasType == CanvasType.Login);
            Assert.True(instance.Client.Network.Connected == false, "check API that we are logged out");
            LoginPresenterIsAvailable(vm);
            TypeUserAndPassIntoLoginPanel();
            yield return new WaitForSeconds(2);
            Utils.UIHelpers.ClickButtonByUnityName("LoginBtn");

            //assert the backend API; that we are logged in.
            yield return new WaitForSeconds(20);
            Assert.True(instance.Client.Network.Connected == true, "check API that we are logged in");
            
            //finally, disconnect. assert disconnected.
            Utils.UIHelpers.Click_DisconnectButton();
            yield return new WaitForSeconds(5);
            Assert.True(instance.Client.Network.Connected == false, "check API that we are logged out");
            
                        
            Assert.Pass();
            Debug.Log("test is ok!");
            yield return new WaitForSeconds(30);
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
                = vm.GetComponent<CanvasManager>().getForegroundCanvas().GetComponent<LoginPresenter>();
            Assert.True(loginPresenter != null);
        }
        
        private static void TypeUserAndPassIntoLoginPanel()
        {
            _username = "***REMOVED*** Resident";
            Utils.UIHelpers.Set_TMPInputField_ofGameObjectName("UserTextField", _username);
            _password = "***REMOVED***";
            Utils.UIHelpers.Set_TMPInputField_ofGameObjectName("PassTextField", _password);
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
        //
        // [Test]
        // public IEnumerator LoginAndDownloadTexture()
        // {
        //     
        //     
        //     
        //     
        // }

        public class Utils
        {
            /*
             * A whole bunch of methods to click buttons and type into inputfields.
             */
            public class UIHelpers
            {
                public static bool ClickButtonByUnityName(string gameObjectName)
                {
                    var btn = GameObject.Find(gameObjectName);
                    Assert.IsNotNull(btn, "Missing button supposed to have name: " + gameObjectName);

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
                    Assert.IsTrue(RaindropIntegrationTests.Utils.UIHelpers.ClickButtonByUnityName(eulaCloseBtn));
                }

                public static void Click_Button_Welcome2LoginScreen()
                {
                    Assert.IsTrue(RaindropIntegrationTests.Utils.UIHelpers.ClickButtonByUnityName("LetsGo!"));
                }
            }
        }
    }

}

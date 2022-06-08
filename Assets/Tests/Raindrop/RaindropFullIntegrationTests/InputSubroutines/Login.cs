using System.Collections;
using Raindrop.Services;
using UnityEngine;
using UnityEngine.UI;
using Assert = NUnit.Framework.Assert;

namespace Raindrop.Tests.RaindropFullIntegrationTests.InputSubroutines
{
    /*
     * UI intensive, full integration subroutines for the pressing buttons to login.
     */
    public class Login
    {
        // type the user Credentials and click the login button.
        // Require: Is on CanvasType.Login
        public static IEnumerator StartLogin(string username, string password)
        {
            TypeUserAndPassIntoLoginPanel(username, password);
            yield return new WaitForSeconds(2);
            UIHelpers.Click_ButtonByUnityName("LoginBtn");
        }

        public static void TypeUserAndPassIntoLoginPanel(string _username, string _password)
        {
            UIHelpers.Keyboard_TMPInputField_ByUnityName("UserTextField", _username);
            UIHelpers.Keyboard_TMPInputField_ByUnityName("PassTextField", _password);
        }
        
        /*  accept the eula:
         * 1. toggle true
         * 2. click "next" btn
         * Require: Is on CanvasType.EULA
         */
        public static IEnumerator accepttheeula()
        {
            string eulaCheckbox = "AgreeToggle";
            var checkboxEULA = GameObject.Find(eulaCheckbox);
            Assert.IsNotNull(checkboxEULA, "Missing checkbox " + eulaCheckbox);
            checkboxEULA.GetComponent<Toggle>().onValueChanged.Invoke(true);
            yield return new WaitForSeconds(2);
            
            string eulaCloseBtn = "NextButton";
            Assert.IsTrue(UIHelpers.Click_ButtonByUnityName(eulaCloseBtn));
            yield return new WaitForSeconds(2);
        }

        /*
         * Perform login, for loginCount times.
         * Require: Is on CanvasType.Welcome 
         */
        public static IEnumerator DoExhaustiveLogins(
            string username,
            string password,
            string gridFriendlyName,
            int loginCount,
            RaindropInstance instance,
            UIService uiSrv)// < todo: nani?
        {
            Assert.False(uiSrv is null);
            
            for (int i = 0; i < loginCount; i++)
            {
                //1b. we are on the welcome screen.
                Assert.True(
                    uiSrv.GetPresentCanvasType() == CanvasType.Welcome,
                    "expect current view to be welcome canvas. instead, it is : " +
                    uiSrv.GetPresentCanvasType());
                yield return new WaitForSeconds(2);
                
                // 1c. do "select grid by ui"
                //get knwon grids
                var grids = instance.GridManger.Grids;
                string friendlyName_grid = gridFriendlyName;
                yield return LoginTests.Utils.UIHelpers.Click_Dropdown_Then_Select_ByString(
                    "GridDropdown",
                    friendlyName_grid
                );
                
                //now navigate to the login screen.
                yield return new WaitForSeconds(2);
                yield return LoginTests.Utils.UIHelpers.Click_Button_Welcome2LoginScreen();
                
                //1b. we are on the login screen. do login. assert logged in.
                Assert.IsTrue(uiSrv.GetPresentCanvasType() == CanvasType.Login);

                yield return Login.StartLogin(username, password);
                // Assert.True(uiSrv._loadingController.IsVisible, "expected: loading screen is visible.");
                yield return new WaitForSeconds(12);

                //for login successful, the loading will fade by itself.
                //UIHelpers.Click_ButtonByUnityName("CloseLoadingScreenButton");
                yield return new WaitForSeconds(8);
                Assert.False(uiSrv._loadingController.isInteractable,
                    $"login has failed for {gridFriendlyName}. the message is {instance.Client.Network.LoginMessage}");
            
                //assert the backend API; that we are logged in.
                Assert.True(instance.Client.Network.Connected == true, "check API that we are logged in");
            
                //finally, disconnect. assert disconnected.
                UIHelpers.Click_ButtonByUnityName("LogoutBtn");
                yield return new WaitForSeconds(10);
                Assert.True(instance.Client.Network.Connected == false, "check API that we are logged out");
                
                yield return new WaitForSeconds(4);
            }
        }
    }
}
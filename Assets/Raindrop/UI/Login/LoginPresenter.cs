using Raindrop;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using System;
using Raindrop.Services;
using Settings = Raindrop.Settings;
using UnityEngine.UI;
using UniRx;
using TMPro;
using UnityEngine.Serialization;


// view(unity-object) -- presenter(this,monobehavior) -- controller() -- model (raindropinstance singleton)
namespace Raindrop.Presenters
{
    //custom URL is not supported.
    //grid selection is supported.
    //not support custom input into dropboxes yet.

    public class LoginPresenter : MonoBehaviour, ILoginView
    {
        private RaindropInstance instance => ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>();

        private UIService uimanager
        {
            get
            {
                try
                {
                    return ServiceLocator.ServiceLocator.Instance.Get<UIService>();
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        #region UI elements - the 'view' in MVP
        public Button LoginButton;
        public TMP_InputField usernameField;
        public TMP_InputField passwordField;
        public Toggle TOSCheckbox;
        public Toggle RememberCheckbox;
        public TMP_Dropdown locationDropdown;
        private LoginLocationDropdown loginLocationDropdown; //wraps the above
        public TMP_Text credError;
        #endregion

        #region internal representations

        public string Username { get; set; }

        public string Password { get; set; }

        public bool isSaveCredentials
        {
            get => RememberCheckbox.isOn;
            set
            {
                RememberCheckbox.isOn = value;
            }
        } 

        public bool agreeTOS { get; set; } = true;

        public int loginLocation { get; }

        [FormerlySerializedAs("LoginButton_IsClickable")] public ReactiveProperty<bool> loginButtonIsClickable = new ReactiveProperty<bool>(true);

        //is empty string if there is no error.
        public ReactiveProperty<string> credParserErrorString = new ReactiveProperty<string>();

        //public string uninitialised = "(unknown)";

        private object lblLoginStatus;

        private bool cbRememberBool;

        #endregion

        #region init

        public LoginController controller;
        private ILoginView _loginViewImplementation;

        void Start()
        {
            //0 create dropdown for login location 
            loginLocationDropdown = new LoginLocationDropdown(locationDropdown);

            //create controller 
            controller = new LoginController(this);
            
            //3 hookup reactive UIs.
            LoginButton.onClick.AsObservable().Subscribe(_ => OnLoginBtnClick()); //when clicked, runs this method.

            loginButtonIsClickable.AsObservable().Subscribe(_ => { SetLoginButtonVisibilty(_); }); //update the login button availabilty according to this boolean.

            usernameField.onValueChanged.AsObservable().Subscribe(_ =>
            {
                Username = _;
                credParserErrorString.Value = "";
            });  //change username property.
            passwordField.onValueChanged.AsObservable().Subscribe(_ =>
            {
                Password = _;
                
                credParserErrorString.Value = "";
            });  //change password.

            RememberCheckbox.OnValueChangedAsObservable().Subscribe(_ => cbRememberBool = _); //when toggle checkbox, set boolean to the same value as the toggle-state
            TOSCheckbox.OnValueChangedAsObservable().Subscribe(_ => agreeTOS = _); //when toggle checkbox, set boolean to the same value as the toggle-state

            //loginMsg.AsObservable().Where(_ => ! _.Equals("")).Subscribe(_ => UpdateLoginModalContent("Logging in process...", _));

            credParserErrorString.AsObservable().Subscribe(_ => OnCredParseError(_));
            
            //gridDropdown.OnValueChangedAsObservable().Subscribe(_ => gridSelectedItem = _);
            //customURLCheckbox.onValueChanged.AsObservable().Subscribe(_ => cbCustomURL = _); //change username property.

            //DropdownMenuWithEntry = UserDropdownMenu.GetComponent<DropdownMenuWithEntry>();

        }

        private void OnEnable()
        {
            //check if is logged in.
            if(instance.Netcom.IsLoggedIn)
                return;
            if (instance.Netcom.IsLoggingIn)
                return;
            
            //otherwise, reset the login fields.
            
        }

        private void SetLoginButtonVisibilty(bool _)
        {
            LoginButton.interactable = _;
        }

        // show the error text.
        private void OnCredParseError(string _)
        {
            credError.text = _;
        }

        // //updates the text in the login modal.
        // private void UpdateLoginModalContent(string header, string message)
        // {
        //     if (uimanager == null)
        //         return;
        //     if (!uimanager.ready)
        //         return;
        //
        //     if (header == "")
        //     {
        //         header = "Logging in... ";
        //     }
        //     uimanager.modalManager.setLoginModalText(header, message);
        // }
        //
        //
        // private void Close_LoginModal_Slow()
        // {
        //     uimanager.modalManager.fadeLoginModal();
        // }



        // slowly turns the button back on again.
        IEnumerator EnableButtonCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay / 1000f);
            loginButtonIsClickable.Value = true;
        }
        
        // 1. open the login status modal.
        // 2. create a timeout timer -> on time out, allow user to close the login status modal.
        public void OnLoginBtnClick()
        {
            loginButtonIsClickable.Value = false;
            // LoginButton.interactable = false;
            instance.MediaManager.PlayUISound(UISounds.Click);

            //guard username
            string parsedFirstname = "";
            string parsedLastname = "";
            bool isValidUser = controller.splitUserName(Username, out parsedFirstname, out parsedLastname);
            if (!isValidUser)
            {
                credParserErrorString.Value = "bad username. Allowable examples: Kitty Graves , FlyingFox Resident , FlyingFox ";
                Debug.LogError(" username error; invalid username input : " + Username);
                StartCoroutine(EnableButtonCoroutine(1500));
                return;
            }
            
            //guard password
            if (Password.Length <= 0)
            {                
                credParserErrorString.Value = "bad password";
                Debug.LogError(" password error!");
                StartCoroutine(EnableButtonCoroutine(1500));
                return;
            }

            if (controller.isDoingLogin)
            {
                return;
            }
            
            //do login.
            controller.BeginLogin(parsedFirstname, parsedLastname, Password, agreeTOS);
        }

        #endregion

        public void GoToGameView()
        {
            uimanager.ScreensManager.PopAndPush(CanvasType.Game);
        }
    }

    public class BadUserNameException : Exception
    {
        public BadUserNameException(string message)
        {
        }
    }
}
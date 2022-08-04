using System.Collections;
using Plugins.CommonDependencies;
using Raindrop.Presenters;
using Raindrop.Services;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

// view(unity-object) -has component-> presenter(this class file) -has a class instance of-> LoginController() -directly references backendAPI-> RaindropInstance(backend model/api)
namespace Raindrop.UI.Login
{
    public class LoginPresenter : MonoBehaviour
    {
        private RaindropInstance instance => RaindropInstance.GlobalInstance;

        private UIService Uimanager =>
            ServiceLocator.Instance.Get<UIService>();

        #region UI elements - the 'view' in MVP
        //main
        public Button LoginButton;
        public TMP_InputField usernameField;
        public TMP_InputField passwordField;
        //options
        public Toggle TOSCheckbox;
        public Toggle RememberCheckbox;
        public LoginLocationDropdown loginLocationDropdown; 
        public TMP_Text credError;
        #endregion

        #region internal representations

        public string Username = "";

        public string Password = "";

        // public bool isSaveCredentials => true;

        public bool agreeTOS { get; set; } = true;

        public int loginLocation => loginLocationDropdown.value;

        [FormerlySerializedAs("LoginButton_IsClickable")] public ReactiveProperty<bool> loginButtonIsClickable = new ReactiveProperty<bool>(true);

        //is empty string if there is no error.
        public ReactiveProperty<string> credParserErrorString = new ReactiveProperty<string>();

        //public string uninitialised = "(unknown)";

        private object lblLoginStatus;
        
        #endregion

        #region init

        public LoginController Controller;

        void Start()
        {

            //create controller 
            Controller = new LoginController(this);
            
            //3 hookup reactive UIs.
            HookupUIEvents();
        }

        private void HookupUIEvents()
        {
            LoginButton.onClick.AsObservable()
                .Subscribe(_ => OnLoginBtnClick())
                .AddTo(this);

            loginButtonIsClickable.AsObservable()
                .Subscribe(_ =>
                {
                    SetLoginButtonVisibilty(_);
                })
                .AddTo(this); //update the login button availabilty according to this boolean.

            usernameField.onValueChanged.AsObservable()
                .Subscribe(_ =>
                {
                    Username = _;
                    credParserErrorString.Value = "";
                })
                .AddTo(this); //change username property.
            passwordField.onValueChanged.AsObservable()
                .Subscribe(_ =>
                {
                    Password = _;

                    credParserErrorString.Value = "";
                })
                .AddTo(this); //change password.

            TOSCheckbox.OnValueChangedAsObservable()
                .Subscribe(_ => agreeTOS = _)
                .AddTo(this); //when toggle checkbox, set boolean to the same value as the toggle-state

            //loginMsg.AsObservable().Where(_ => ! _.Equals("")).Subscribe(_ => UpdateLoginModalContent("Logging in process...", _));

            credParserErrorString.AsObservable()
                .Subscribe(_ => OnCredParseError(_))
                .AddTo(this);

            //gridDropdown.OnValueChangedAsObservable().Subscribe(_ => gridSelectedItem = _);
            //customURLCheckbox.onValueChanged.AsObservable().Subscribe(_ => cbCustomURL = _); //change username property.

            //DropdownMenuWithEntry = UserDropdownMenu.GetComponent<DropdownMenuWithEntry>();
            // RememberCheckbox.onValueChanged.AddListener(OnRememberCredsToggle);
        }

        // private void OnRememberCredsToggle(bool isOn)
        // {
        //     instance.GlobalSettings["remember_login"] = isOn;
        // }

        private void OnEnable()
        {
            //check if is logged in.
            if(instance.Netcom.IsLoggedIn)
                return;
            
            //otherwise, reset the login fields.
            
        }

        private void SetLoginButtonVisibilty(bool _)
        {
            if (LoginButton == null)
                return;
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
            instance.MediaManager.PlayUISound(UISounds.Click);

            //guard username
            string parsedFirstname = "";
            string parsedLastname = "";
            bool isValidUser = Controller.splitUserName(Username, out parsedFirstname, out parsedLastname);
            if (!isValidUser)
            {
                credParserErrorString.Value = "bad username. Allowable examples: Kitty Graves , FlyingFox Resident , FlyingFox ";
                Debug.LogWarning(" username error; invalid username input : " + Username);
                StartCoroutine(EnableButtonCoroutine(1500));
                return;
            }
            
            //guard password
            if (Password.Length <= 0)
            {
                credParserErrorString.Value = "bad password";
                Debug.LogWarning(" password error!");
                StartCoroutine(EnableButtonCoroutine(1500));
                return;
            }

            //do login.
            Controller.BeginLogin(parsedFirstname, parsedLastname, Password, agreeTOS);
        }

        #endregion

    }
}
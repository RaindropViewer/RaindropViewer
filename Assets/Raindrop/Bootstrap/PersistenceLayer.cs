using Plugins.CommonDependencies;
using Raindrop.Services;
using UnityEngine;
using UnityEngine.Assertions;

namespace Raindrop.Bootstrap
{
    //maintain the impression of "persistence" of the connection
    // on app is running:
        // if the connection is cut, I will inform the user with a modal.
    // on app start:
        // if the app was killed by OS or user force swipe, I will ask user if they want to reconnect.
        // if the app terminated normally, I will ask the user if they want to reconnect. 
    public class PersistenceLayer : MonoBehaviour
    {
        private void Start()
        {
            AskUser_LoginUsingPreviousCredentials();
        }

        // "would you like to login to the last active connection?"
        private void AskUser_LoginUsingPreviousCredentials()
        {
            var ui = ServiceLocator.Instance.Get<UIService>();
            Assert.IsNotNull(ui, "ui is not found.");
            // var instance = ServiceLocator.RaindropInstance.GlobalInstance;
            // ui.modalManager.showModal_NtfGeneric("Quick Re-log",
            //     "Would you like to login with previous credentials: \n" +
            //     instance.GlobalSettings["a"]
            //     );
        }
    }
}
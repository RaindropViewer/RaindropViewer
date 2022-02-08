using System;
using UnityEngine;

namespace Raindrop.Services.Bootstrap
{
    //maintain the impression of "persistence" of the connection
    // on app is running:
        // if the connection is cut, I will inform the user with a modal.
    // on app start:
        // if the app was killed by OS or user force swipe, I will ask user if they want to reconnect.
        // if the app terminated normally, I will ask the user if they want to reconnect. 
    public class PersistenceLayer : MonoBehaviour
    {
        private void Awake()
        {
            AskUser_LoginUsingPreviousCredentials();
        }

        private void AskUser_LoginUsingPreviousCredentials()
        {
            var ui = ServiceLocator.ServiceLocator.Instance.Get<UIService>();
            var instance = ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>();
            ui.modalManager.showModalNotification("Quick Re-log",
                "Would you like to login with previous credentials: \n" +
                instance.GlobalSettings["a"]
                );
        }
    }
}
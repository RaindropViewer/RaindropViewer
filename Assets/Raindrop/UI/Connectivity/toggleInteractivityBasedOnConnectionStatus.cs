// using System.Collections;
// using System.Collections.Generic;
// using OpenMetaverse;
// using Raindrop;
// using Raindrop.ServiceLocator;
// using UnityEngine;
//
// //this one have bug : update does not run when disabled.
// the original intent of this script, was to toggle visibliilty/interactivity of the object it is attached to, based on the connectivity state of the backend
// public class toggleInteractivityBasedOnConnectionStatus : MonoBehaviour
// {
//     private RaindropInstance _instance;
//
//     // Start is called before the first frame update
//     void Start()
//     {
//         notConnected();
//         
//         _instance = RaindropInstance.GlobalInstance;
//     }
//
//
//     // Update is called once per frame
//     void Update()
//     {
//         if (_instance.Client.Network.Connected == false)
//         {
//             notConnected();
//         }
//         else
//         {
//             yesConnected();
//         }
//     }
//     
//     
//     private void notConnected()
//     {
//         this.gameObject.SetActive(false);
//     }
//     private void yesConnected()
//     {        
//         this.gameObject.SetActive(true);
//
//     }
// }

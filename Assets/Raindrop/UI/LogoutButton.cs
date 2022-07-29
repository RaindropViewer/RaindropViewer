using Plugins.CommonDependencies;
using UnityEngine;
using UnityEngine.UI;

namespace Raindrop.UI
{
    [RequireComponent(typeof(Button))]
    public class LogoutButton : MonoBehaviour
    {
        //get own references.
        void Awake()
        {
            Button btn = this.GetComponent<Button>();
            btn.onClick.AddListener(Logout);
        }

        private void Logout()
        {
            Debug.Log("logout requested by user UI");
            RaindropInstance.GlobalInstance.Netcom.Logout();
        }
    }
}

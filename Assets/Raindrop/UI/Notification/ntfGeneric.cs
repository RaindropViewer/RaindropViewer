using System;
using Plugins.CommonDependencies;
using Raindrop.Services;
using UnityEngine;

namespace Raindrop
{
    //generic blue modal with a message and a "ok" button.
    public class ntfGeneric : MonoBehaviour
    {
        private RaindropInstance instance =>
            RaindropInstance.GlobalInstance;

        public TMPro.TMP_Text txtMessage;
        public void Init(string msg)
        {
            // txtMessage.BackColor = instance.MainForm.NotificationBackground;
            txtMessage.text = msg.Replace("\n", "\r\n");
            
            // Fire off event
            // NotificationEventArgs args = new NotificationEventArgs(instance) {Text = txtMessage.Text};
            // args.Buttons.Add(btnOk);
            // FireNotificationCallback(args);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {

            var ui = ServiceLocator.Instance.Get<UIService>();
            ui.ModalsManager.UnShow(this.gameObject);
            // instance.Notification.RemoveNotification(this);
        }
    }
}
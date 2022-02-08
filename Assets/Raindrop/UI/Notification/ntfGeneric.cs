// using System;
//
// namespace Raindrop
// {
//     //generic blue modal with a message and a "ok" button.
//     public class ntfGeneric
//     {
//         private RaindropInstance instance;
//
//         public ntfGeneric(RadegastInstance instance, string msg)
//             : base(NotificationType.Generic)
//         {
//             InitializeComponent();
//
//             this.instance = instance;
//             txtMessage.BackColor = instance.MainForm.NotificationBackground;
//             txtMessage.Text = msg.Replace("\n", "\r\n");
//             if (msg.Length < 100)
//             {
//                 txtMessage.ScrollBars = ScrollBars.None;
//             }
//             btnOk.Focus();
//
//             // Fire off event
//             NotificationEventArgs args = new NotificationEventArgs(instance) {Text = txtMessage.Text};
//             args.Buttons.Add(btnOk);
//             FireNotificationCallback(args);
//
//             GUI.GuiHelpers.ApplyGuiFixes(this);
//         }
//
//         private void btnOk_Click(object sender, EventArgs e)
//         {
//             instance.Notification.RemoveNotification(this);
//         }
//     }
// }
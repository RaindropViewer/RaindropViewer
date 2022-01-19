using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Raindrop.UI.Notification
{
    //in-game notifications
    //such as.
    // 1. unread IM
    // 2. object returns
    // 3. friend request
    // 4. etc.
    class Notification
    {
        public Notification()
        {
        }

        //raise the notification.
        

        public static void DisplayNotification(string msg, ChatBufferTextStyle statusDarkBlue)
        {
            Debug.Log(msg);
        }
    }
}

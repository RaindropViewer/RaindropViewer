using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Raindrop.UI.map.view
{

    //handle the touches within the map.
    class MapTouchHandler : MonoBehaviour
    {

        void update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Either touch down or Mouse click
                Debug.Log("tap.");
            }
        }

    }
}

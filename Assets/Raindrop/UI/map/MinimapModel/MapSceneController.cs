using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using Raindrop.Map;
using Raindrop.UI.Views;
using Raindrop.Netcom;
using OpenMetaverse;
using Raindrop.Map.Model;
using Vector2 = UnityEngine.Vector2;
using Raindrop.UI.Model;

namespace Raindrop.UI.Presenters
{
    //some key ideas:
    /* if my finger is clicking the map, I have intention to navigate and pan the map.
     * if my finger is not clicking the map, I have no intention to do anything with the map.
     *
     * if i tap the map, the informaiton of the coordinate i tap will be displayed as a card.
     *
     * i only fetch the map if my camera is shooting at it.
     * 
     */

    public class MapSceneController
    {
        public event EventHandler<string> MapClicked;
        public virtual void OnMapClick(ulong regionCoords) //protected virtual method
        {
            //if ProcessCompleted is not null then call delegate
            MapClicked?.Invoke(this, regionCoords.ToString() + "to implement sim name and pos as string in event. ");
        }

        public MapSceneController(MapUIView mapUIView, MapScenePresenter mapScenePresenter)
        {
            //mapSceneView.gameObject.SetActive(true);
        }


    }
}

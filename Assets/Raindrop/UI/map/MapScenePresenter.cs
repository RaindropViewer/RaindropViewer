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
    /// <summary>
    /// Presenter
    /// I am used by 
    /// 1. mapUI (go)
    /// 2. mapScene (go)
    /// I do not have an update loop. I am driven by the above 2 players.
    /// </summary>

    public class MapScenePresenter
    {
        private static MapScenePresenter _Instance;
        //private MapSceneView mapSceneView;
        private MapModel mapModel;
        private bool needRepaint;

        GridLayer visibleRanges;

        public event EventHandler<string> MapClicked;
        public virtual void OnMapClick(ulong regionCoords) //protected virtual method
        {
            //if ProcessCompleted is not null then call delegate
            MapClicked?.Invoke(this, regionCoords.ToString() + "to implement sim name and pos as string in event. ");
        }

        public static MapScenePresenter getInstance()
        {
            return _Instance ?? (_Instance = new MapScenePresenter());
        }

        private RaindropInstance instance { get { return ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>(); } }
        private RaindropNetcom netcom { get { return instance.Netcom; } }
        private GridClient client { get { return instance.Client; } }
        bool Active => instance.Client.Network.Connected;


        private MapScenePresenter(/*MapSceneView mapSceneView*/)
        {
            //this.mapSceneView = mapSceneView;


            //mapFetcher = ServiceLocator.ServiceLocator.Instance.Get<MapBackend>();
            mapModel = new MapModel();
        }

        public void setViewAt(Vector2 at)
        {


        }

        public void reset()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// get a list of region handles that are visible to the camera.
        /// </summary>
        /// <returns></returns>
        public HashSet<ulong> getVisibleRegionHandles(Vector2 min, Vector2 max)
        {
            // for loop from bounds to bounds.
            HashSet<ulong> visiblehandles = new HashSet<ulong>();
            int vert_min = (int)min.y;
            int vert_max = (int)max.y;
            int horz_min = (int)min.x;
            int horz_max = (int)max.x;
            for (int i = horz_min; i < horz_max; i++)
            {
                for (int j = vert_min; j < vert_max; j++)
                {
                    ulong region = OpenMetaverse.Utils.UIntsToLong((uint)(i * 256), (uint)(j * 256));
                    visiblehandles.Add(region);
                }
            }

            //if (visiblehandles.Count > 30)
            //{
            //    throw new Exception("too many tiles to downlaod bro!");
            //}

            return visiblehandles;
        }

        internal void RedrawMap()
        {

            //visibleRanges = getVisibleRegionHandles();

            GridLayer gridLayer = new OpenMetaverse.GridLayer{
                Bottom = 1,
                Left = 1,
                Right =1,
                Top = 1
            };
            throw new NotImplementedException();
        }
    }
}

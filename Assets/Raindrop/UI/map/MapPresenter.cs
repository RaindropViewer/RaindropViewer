//using OpenMetaverse;
//using OpenMetaverse.Assets;
//using OpenMetaverse.Imaging;
//using Raindrop.Map.Model;
//using Raindrop.Netcom;
//using Raindrop.UI;
//using Raindrop.UI.Views;
//using System;
//using System.Collections.Generic;
//using UniRx;
//using Unity;
//using UnityEngine;
//using UnityEngine.UI;
//using Vector2 = UnityEngine.Vector2;
//using Vector3 = UnityEngine.Vector3;

//namespace Raindrop.UI.Presenters
//{
//    public class MapPresenter
//    {
//        private MapViewer mapViewer;
//        private MapSceneRenderer mapSceneView;
//        private MapBackend mapFetcher;
//        private bool needRepaint;

//        private RaindropInstance instance { get { return ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>(); } }
//        private RaindropNetcom netcom { get { return instance.Netcom; } }
//        private GridClient client { get { return instance.Client; } }
//        bool Active => instance.Client.Network.Connected;

//        /// <summary>
//        /// ctor -- takes in a UI view and a gameobject view
//        /// </summary>
//        /// <param name="mapViewer"></param>
//        public MapPresenter(MapViewer mapViewer, MapSceneRenderer mapSceneView)
//        {
//            this.mapViewer = mapViewer;
//            this.mapSceneView = mapSceneView;

//        }


//    }
//}
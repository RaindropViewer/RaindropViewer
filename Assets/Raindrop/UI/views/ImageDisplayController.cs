using System;
using OpenMetaverse;
using UnityEngine;

namespace Raindrop.UI.views
{
    public class ImageDisplayController
    {
        private RaindropInstance instance;
        private GridClient client => instance.Client;
        private UUID imageID;

        byte[] j2kdata;
        Texture2D image;
        bool allowSave = false;

        public event EventHandler<ImageUpdatedEventArgs> ImageUpdated;

        
        public bool AllowUpdateImage = false;

        public ImageDisplayController(ImageDisplayView view)
        {
            view.InitializeView();

        }

        
    }


    public class ImageUpdatedEventArgs : EventArgs
    {
        public UUID NewImageID;

        public ImageUpdatedEventArgs(UUID imageID)
        {
            NewImageID = imageID;
        }
    }
}
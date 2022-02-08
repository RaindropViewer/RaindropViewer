using System;
using UnityEngine;
using UnityEngine.UI;

namespace Raindrop.UI.views
{
    // [RequireComponent(typeof(Image))]
    public class ImageDisplayView : MonoBehaviour
    {
        private ImageDisplayController control;

        private void Awake()
        {
            control = new ImageDisplayController(this);
        }

        public void InitializeView()
        {
            var image = GetComponent<RawImage>();
            image.texture = Texture2D.blackTexture;
        }
    }
}
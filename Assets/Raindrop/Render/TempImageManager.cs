using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Raindrop.Render
{
    //manages a pool of images :)
    public static class TempImageManager
    {

        static List<Texture2D> tempObjects;

        static TempImageManager()
        {
            tempObjects = new List<Texture2D>();
        }

        /// <summary>
        /// Call this from some MonoBehaviour in OnDisable to destroy all objects.
        /// </summary>
        public static void OnDisable()
        {
            Clear();
        }

        public static void Clear()
        {
            // Destroy all temp objects in the manager
            for (int i = 0; i < tempObjects.Count; i++)
            {
                if (tempObjects[i] == null) continue;
                Texture2D.Destroy(tempObjects[i]);
            }
            tempObjects.Clear(); // clear the list
        }

        /// <summary>
        /// Adds a temp object to the manager.
        /// </summary>
        /// <param name="obj">Texture</param>
        public static void Add(Texture2D obj)
        {
            if (obj == null) return;
            if (tempObjects.Contains(obj)) return; // already in the list
            tempObjects.Add(obj); // add to list
        }

        /// <summary>
        /// Destroys the object and removes it from the manager.
        /// </summary>
        /// <param name="obj">Object</param>
        public static void Destroy(Texture2D obj)
        {
            if (obj == null) return;
            tempObjects.Remove(obj); // remove from list
            Texture2D.Destroy(obj); // destroy the object
        }

        /// <summary>
        /// Creates a temporary texture and stores it in the manager.
        /// </summary>
        /// <param name="width">Width of the texture</param>
        /// <param name="height">Height of the texture</param>
        /// <returns>Texture2D</returns>
        public static Texture2D CreateTexture2D(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height);
            tex.hideFlags = HideFlags.HideAndDontSave;
            tempObjects.Add(tex);
            return tex;
        }

    }
}

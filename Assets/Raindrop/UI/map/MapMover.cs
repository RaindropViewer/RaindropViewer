using System;
using UnityEngine;
using Raindrop;
using OpenMetaverse;

namespace Raindrop.Presenters
{
    //an instance of a map 'look at' ; the focal point of camera.
    public class MapMover : MonoBehaviour
    {
        //grid coordinates * 256
        [SerializeField]
        public uint lookAt_x = 1000 * 256;
        [SerializeField]
        public uint lookAt_y = 1000 * 256;



        ////get the grid location that we are looking at.
        //public OpenMetaverse.Vector2 GetLookAt()
        //{
        //    return new OpenMetaverse.Vector2(lookAt_x,lookAt_y);
        //}

        public ulong GetLookAt()
        {
            return Utils.UIntsToLong(lookAt_x, lookAt_y);

        }

    }
}
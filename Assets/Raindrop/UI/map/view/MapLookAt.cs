﻿using System;
using UnityEngine;
using Raindrop;
using OpenMetaverse;

namespace Raindrop.UI.Views
{
    //an instance of a map 'look at' ; the focal point of camera.
    public class MapLookAt : MonoBehaviour
    {
        //grid coordinates * 256
        [SerializeField]
        public uint lookAt_x => Convert.ToUInt32(floatingLookAt_x * 256); //1000 * 256;
        [SerializeField]
        public uint lookAt_y => Convert.ToUInt32(floatingLookAt_y * 256); //1000 * 256;

        //grid coordinates * 1
        [SerializeField]
        private float floatingLookAt_x = 1000;
        [SerializeField]
        private float floatingLookAt_y = 1000;

        //values when the finger is not released yet.
        [SerializeField]
        public float prev_floatingLookAt_x = 1000;
        [SerializeField]
        public float prev_floatingLookAt_y = 1000;


        /// <summary>
        /// get lookat of the camera in ( gridCoord * 256 ) units
        /// </summary>
        /// <returns></returns>

        public ulong GetLookAt()
        {
            return Utils.UIntsToLong(lookAt_x, lookAt_y);

        }

        /// <summary>
        /// Move the *floating* lookAt by this amount.
        /// moves by an relative amount - that is, all parameters of this call parameters are relative to the *inital touch location*.
        /// </summary>
        /// <param name="x">change of x - in units where there are 256 units per Sim. </param>
        /// <param name="y"></param>
        public void MoveFloatingLookAt_Relative(float x, float y)
        {
            floatingLookAt_x = prev_floatingLookAt_x + x;
            floatingLookAt_y = prev_floatingLookAt_y + y;

            Debug.Log(x);
            Debug.Log(y);

            updateThisPos(floatingLookAt_x, floatingLookAt_y);
        }

        private void updateThisPos(float floatingLookAt_x, float floatingLookAt_y)
        {
            this.transform.position = new UnityEngine.Vector3(floatingLookAt_x, this.transform.position.x, floatingLookAt_y);
        }

        /// <summary>
        /// Sets the lookAt to a final position. Call this when the finger is released.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void MoveFloatingLookAt_Relative_OnRelease(float x, float y)
        {
            floatingLookAt_x = prev_floatingLookAt_x + x;
            floatingLookAt_y = prev_floatingLookAt_y + y;

            prev_floatingLookAt_x = floatingLookAt_x;
            prev_floatingLookAt_y = floatingLookAt_y;

        }

        private void Awake()
        {


        }

    }
}
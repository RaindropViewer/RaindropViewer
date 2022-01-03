// 
// Radegast Metaverse Client
// Copyright (c) 2009-2014, Radegast Development Team
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 
//     * Redistributions of source code must retain the above copyright notice,
//       this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the application "Radegast", nor the names of its
//       contributors may be used to endorse or promote products derived from
//       this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
// $Id$
//
using System;
using System.Timers;
using OpenMetaverse;
using Debug = UnityEngine.Debug;

namespace Raindrop
{
    // this class periodically updated the user movement to the backend.
    //      the backend (Agent Managerr Movement.cs) has a timer that sends the
    //      movment at 2.5 s per movement packet.
    
    //to have fancy movements, we can add extend this class. (orbit, 3rd person camera movements...)
    public class RaindropMovement : IDisposable
    {
        private RaindropInstance instance;
        private GridClient client { get { return instance.Client; } }
        private Timer timer;
        private Vector3 forward = new Vector3(1, 0, 0);
        private bool turningLeft = false; 
        private bool turningRight = false;
        
        private bool movingForward = false; // ^
        private bool movingBackward = false; // v
        private bool movingLeftward = false; // <
        private bool movingRightward = false; // >
        
        private bool modified = false;
        private bool isMoving => movingForward | movingBackward | movingLeftward | movingRightward;

        public bool TurningLeft
        {
            get
            {
                return turningLeft;
            }
            set
            {
                //modified = true;
                turningLeft = value;
                if (value)
                {
                    timer_Elapsed(null, null);
                    timer.Enabled = true;
                }
                else
                {
                    timer.Enabled = false;
                    client.Self.Movement.TurnLeft = false;
                    client.Self.Movement.SendUpdate(true);
                }
            }
        }

        public bool TurningRight
        {
            get
            {
                return turningRight;
            }
            set
            {
                //modified = true;
                turningRight = value;
                if (value)
                {
                    //start movement immediate, set up the timer for more updates soon.
                    timer_Elapsed(null, null);
                    timer.Enabled = true;
                }
                else
                {
                    //stop movmeent immiate, stop timer updates.
                    timer.Enabled = false;
                    client.Self.Movement.TurnRight = false;
                    client.Self.Movement.SendUpdate(true);
                }
            }
        }

        
        public RaindropMovement(RaindropInstance instance)
        {
            this.instance = instance;
            timer = new System.Timers.Timer(1000);
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            timer.Enabled = false;
        }

        public void Dispose()
        {
            timer.Enabled = false;
            timer.Dispose();
            timer = null;
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Debug.Log("movement timer ");
            OpenMetaverse.Logger.DebugLog("movement timer ");
            float delta = (float)timer.Interval / 1000f;
            if (turningLeft)
            {
                client.Self.Movement.TurnLeft = true;
                client.Self.Movement.BodyRotation = client.Self.Movement.BodyRotation * Quaternion.CreateFromAxisAngle(Vector3.UnitZ, delta);
                client.Self.Movement.SendUpdate(true);
            }
            else if (turningRight)
            {
                client.Self.Movement.TurnRight = true;
                client.Self.Movement.BodyRotation = client.Self.Movement.BodyRotation * Quaternion.CreateFromAxisAngle(Vector3.UnitZ, -delta);
                client.Self.Movement.SendUpdate(true);
            }
            
            //update every periodic interval if i am still moving.
            if (isMoving)
            {
                client.Self.Movement.SendUpdate(true);
            }
        }

        //tell the movement machine that we are moving forward.
        public void setForward()
        {
            client.Self.Movement.AtPos = true;
            client.Self.Movement.AtNeg = false;
        }
        public void setBackward()
        {
            client.Self.Movement.AtPos = false;
            client.Self.Movement.AtNeg = true;
        }

        public void stopMovementAndSendUpdate()
        {
            client.Self.Movement.AtPos = false;
            client.Self.Movement.AtNeg = false;
            client.Self.Movement.LeftPos = false;
            client.Self.Movement.LeftNeg = false;
            client.Self.Movement.SendUpdate(false);
        }

        public void setRightward()
        {
            client.Self.Movement.LeftPos = false;
            client.Self.Movement.LeftNeg = true;
        }
        public void setLeftward()
        {
            client.Self.Movement.LeftPos = true;
            client.Self.Movement.LeftNeg = false;
        }

        public void setTurningLeft()
        {
            client.Self.Movement.TurnLeft = true;
            client.Self.Movement.TurnRight = false;
        }

        public void setTurningRight()
        {
            client.Self.Movement.TurnLeft = false;
            client.Self.Movement.TurnRight = true;
        }
    }
}

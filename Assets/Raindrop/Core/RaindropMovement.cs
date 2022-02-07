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
using UnityEngine;
using Debug = UnityEngine.Debug;
using Quaternion = OpenMetaverse.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = OpenMetaverse.Vector3;

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
        private bool isTurning = false; 
        private uint _prev;

        public void SetTurningRight()
        {
            //change mvmtpacket
            client.Self.Movement.TurnRight = true;
            client.Self.Movement.TurnLeft = false;
            
            //start turning
            isTurning = true;
            //TurnStart();
        }

        public void SetTurningLeft()
        {
            //change mvmtpacket
            client.Self.Movement.TurnRight = false;
            client.Self.Movement.TurnLeft = true;
            
            isTurning = true;
            //start turning
            //TurnStart();
        }
        public void SetTurningStop()
        {
            //change mvmtpacket
            client.Self.Movement.TurnRight = false;
            client.Self.Movement.TurnLeft = false;
            
            isTurning = false;
            //stop turning
            //TurnStop();
        }

        private void TurnStart()
        {
            timer_Elapsed(null, null);
            timer.Enabled = true; //this timer is only required for turning.
        }
        private void TurnStop()
        {
            timer.Enabled = false;
            SendMovementPacketIfChanged();
        }


        public RaindropMovement(RaindropInstance instance)
        {
            this.instance = instance;
            timer = new System.Timers.Timer(100); //seems like turn left and right will have 100 timer.
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            timer.Enabled = false;
        }

        public void Dispose()
        {
            timer.Enabled = false;
            timer.Dispose();
            timer = null;
        }

        //handle the turning that causes new rotation per second.
        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            float delta = (float)timer.Interval / 1000f;
            if (isTurning) {
                //client.Self.Movement.BodyRotation = client.Self.Movement.BodyRotation * Quaternion.CreateFromAxisAngle(Vector3.UnitZ, delta);
                
                SendMovementPacketIfChanged();
            } 
            else
            { //not turning
                SendMovementPacketIfChanged();
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


        // stop moving.
        public void zero2DInput()
        {
            client.Self.Movement.AtPos = false;
            client.Self.Movement.AtNeg = false;
            client.Self.Movement.LeftPos = false;
            client.Self.Movement.LeftNeg = false;
            
            SendMovementPacketIfChanged();
        }

        
        public void set2DInput(Vector2 arg0)
        {
            //note: is impossible to be no movement due to that is being handled by zero2DInput.

            bool isUp = arg0.y > 0;
            bool isDown = arg0.y < 0;
            bool isNoVert = !isUp && !isDown;
            
            bool isRight = arg0.x > 0;
            bool isLeft = arg0.x < 0;
            bool isNoHorz = !isLeft && !isRight;

            if (isUp) 
            {
                setForward();
            }
            if (isDown)
            {
                setBackward();
            }
            if (isRight)
            {
                setRightward();
            }
            if (isLeft)
            {
                setLeftward();
            }

            SendMovementPacketIfChanged();
        }

        // this is a safer method to send packets, as it makes sure we don't send non-helpful information.
        private void SendMovementPacketIfChanged()
        {
            //hacky: moving means send. not moving, then have to check if changes has occured.
            if (isTurning)
            {
                client.Self.Movement.SendUpdate(true);
                return;
            }
            
            var present = client.Self.Movement.AgentControls;
            if (_prev != present)
            {
                _prev = client.Self.Movement.AgentControls;
                client.Self.Movement.SendUpdate(true);
            }
        }

        // user's direct input processed here.
        public void setCameraInputs(object zero)
        {
            
            
        }
    }

}

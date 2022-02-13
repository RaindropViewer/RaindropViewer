using System;
using System.Timers;
using OpenMetaverse;
using UnityEngine;
using Quaternion = OpenMetaverse.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = OpenMetaverse.Vector3;

namespace Raindrop
{
    // 1. this class periodically updated the user movement to the backend.
    //      the backend (Agent Managerr Movement.cs) has a timer that sends the
    //      movment at 2.5 s per movement packet.
    // 2. however, sometimes, such as stoppage of all movements, the timer is stopped
    //      and a packet is sent to mark the stoppage of all movements.
    
    public class RaindropMovement : IDisposable
    {
        private RaindropInstance instance;
        private GridClient client { get { return instance.Client; } }
        private Timer timer; //rate-limiter to 10hz.
        private Vector3 forward = new Vector3(1, 0, 0);
        //private bool isTurning = false; 
        private bool jumping = false;
        //private uint _prev;

        public bool Jump
        {
            get => jumping;
            set
            {
                jumping = value;
                client.Self.Jump(value);
            }
        }

        ////equivalent to press left arrow.
        //public void SetTurningRight()
        //{
        //    //change mvmtpacket
        //    client.Self.Movement.TurnRight = true;
        //    client.Self.Movement.TurnLeft = false;
            
        //    //start turning
        //    isTurning = true;
        //    //TurnStart();
        //}

        //public void SetTurningLeft()
        //{
        //    //change mvmtpacket
        //    client.Self.Movement.TurnRight = false;
        //    client.Self.Movement.TurnLeft = true;
            
        //    isTurning = true;
        //    //start turning
        //    //TurnStart();
        //}
        //public void SetTurningStop()
        //{
        //    //change mvmtpacket
        //    client.Self.Movement.TurnRight = false;
        //    client.Self.Movement.TurnLeft = false;
            
        //    isTurning = false;
        //    //stop turning
        //    //TurnStop();
        //}

        //private void TurnTimerStart()
        //{
        //    timer_Elapsed(null, null);
        //    timer.Enabled = true; //this timer is only required for turning.
        //}
        //private void TurnTimerStop()
        //{
        //    timer.Enabled = false;
        //    SendMovementPacket_RateLimited();
        //}


        public RaindropMovement(RaindropInstance instance)
        {
            this.instance = instance;
            timer = new System.Timers.Timer(100); //seems like turn left and right will have 100 timer.
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            timer.Enabled = true;
        }

        public void Dispose()
        {
            timer.Enabled = false;
            timer.Dispose();
            timer = null;
        }

        //send input data every 100s.
        // since the view is freakishly updating the avatar's heading every frame,
        // it is better to rate-limit instead of detecting input changes.
        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //float delta = (float)timer.Interval / 1000f;
            //if (isTurning) {

            client.Self.Movement.SendUpdate(true);

            //} 
            //else
            //{ 
            //    SendMovementPacketIfChanged();
            //}
        }

        //tell the movement machine that we are moving forward.
        //public void setForward()
        //{
        //    client.Self.Movement.AtPos = true;
        //    client.Self.Movement.AtNeg = false;
        //}
        //public void setBackward()
        //{
        //    client.Self.Movement.AtPos = false;
        //    client.Self.Movement.AtNeg = true;
        //}

        //public void setRightward()
        //{
        //    client.Self.Movement.LeftPos = false;
        //    client.Self.Movement.LeftNeg = true;
        //}
        //public void setLeftward()
        //{
        //    client.Self.Movement.LeftPos = true;
        //    client.Self.Movement.LeftNeg = false;
        //}



        // send discrete, boolean input to the backend.
        public void SetWasdInput(
            bool up,
            bool down,
            bool left,
            bool right)
        {

            if (up) 
            {
                client.Self.Movement.AtPos = true;
                client.Self.Movement.AtNeg = false;
            }
            if (down)
            {
                client.Self.Movement.AtNeg = true;
                client.Self.Movement.AtPos = false;
            }
            if ((down == false) && ( up == false))
            {
                client.Self.Movement.AtPos = false;
                client.Self.Movement.AtNeg = false;
            }


            if (right)
            {
                client.Self.Movement.LeftNeg = true;
                client.Self.Movement.LeftPos = false;
            }
            if (left)
            {
                client.Self.Movement.LeftPos = true;
                client.Self.Movement.LeftNeg = false;
            }
            if ((left == false) && (right == false))
            {
                client.Self.Movement.LeftPos = false;
                client.Self.Movement.LeftNeg = false;
            }

            bool noInput = (left == false) &&
                           (right == false) &&
                           (up == false) &&
                           (down == false);
            if (noInput)
            {
                client.Self.Movement.AtPos = false;
                client.Self.Movement.AtNeg = false;
                client.Self.Movement.LeftPos = false;
                client.Self.Movement.LeftNeg = false;
            }

        }

        // this is a safer method to send packets, as it makes sure we don't send non-helpful information.
        //private void SendMovementPacket_RateLimited()
        //{
        //    ////hacky: moving means send. not moving, then have to check if changes has occured.
        //    //if (isTurning)
        //    //{
        //    //    client.Self.Movement.SendUpdate(true);
        //    //    return;
        //    //}

        //    //var present = client.Self.Movement.AgentControls;
        //    //if (_prev != present)
        //    //{
        //    //    _prev = client.Self.Movement.AgentControls;
        //    //    client.Self.Movement.SendUpdate(true);
        //    //}
        //}


        // instantly set the heading of the avatar.
        // input is right-handed and relative from world-forward
        //expected degrees!
        public void SetHeading(float heading)
        {
            var quat = Quaternion.CreateFromEulers(0,0,heading * Mathf.Deg2Rad);
            client.Self.Movement.BodyRotation = quat;
        }
    }

}

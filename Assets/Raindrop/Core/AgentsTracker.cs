using System;
using System.Collections.Generic;
using OpenMetaverse;
using Raindrop.Services.Bootstrap;

namespace Raindrop
{
    //belonging to raindropinstance, this class keeps track of known avatars and the region they are in.

    // in particular, it tracks : 
    // region handle
    // 
    public class AgentsTracker
    {
        //hashtable of agents, and the sim they are last known to reside in.
        public readonly Dictionary<UUID, NearbyAvatar> agentInfos = new Dictionary<UUID, NearbyAvatar>();
        private readonly RaindropInstance instance;
        private GridClient client => instance.Client;
        
        /// <summary>
        /// List of nearby avatars (radar data)
        /// </summary>
        public List<NearbyAvatar> NearbyAvatars
        {
            get
            {
                List<NearbyAvatar> res = new List<NearbyAvatar>();

                lock (agentInfos)
                {
                    foreach (var kvPair in agentInfos)
                    {
                        res.Add(kvPair.Value);
                    }
                }

                return res;
            }
        }

        
        public AgentsTracker(RaindropInstance instance)
        {
            this.instance = instance;
            
            RegisterClientEvents(instance.Client);   
        }
        
        
        private void RegisterClientEvents(GridClient client)
        {
            client.Grid.CoarseLocationUpdate += new EventHandler<CoarseLocationUpdateEventArgs>(Grid_CoarseLocationUpdate);
            // client.Self.TeleportProgress += new EventHandler<TeleportEventArgs>(Self_TeleportProgress);
            client.Network.SimDisconnected += new EventHandler<SimDisconnectedEventArgs>(Network_SimDisconnected);
        }

        #region eventHandlers
        void Grid_CoarseLocationUpdate(object sender, CoarseLocationUpdateEventArgs e)
        {
            try
            {
                UpdateRadar(e);
            }
            catch { }
        }
        
        void UpdateRadar(CoarseLocationUpdateEventArgs e)
        {
            //guard: if the current sim is not known, we should not update the radar.
            if (client.Network.CurrentSim == null /*|| client.Network.CurrentSim.Handle != sim.Handle*/)
            {
                return;
            }

            //guard: if we are on non-main thread, we should give this function call to the main thread. 
            if (!Globals.isOnMainThread())
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() => {
                    UpdateRadar(e);
                });
                return;
            }

            // later on we can set this with something from the GUI
            const double MAX_DISTANCE = 362.0; // one sim a corner to corner distance
            lock (agentInfos)
                try
                {
                    Vector3d mypos = e.Simulator.AvatarPositions.ContainsKey(client.Self.AgentID)
                                        ? StateManager.ToVector3D(e.Simulator.Handle, e.Simulator.AvatarPositions[client.Self.AgentID])
                                        : client.Self.GlobalPosition;

                    // CoarseLocationUpdate gives us height of 0 when actual height is
                    // between 1024-4096m.
                    if (mypos.Z < 0.1)
                    {
                        mypos.Z = client.Self.GlobalPosition.Z;
                    }

                    List<UUID> existing = new List<UUID>();
                    List<UUID> removed = new List<UUID>(e.RemovedEntries);

                    //1 for each avatar that is in the coarseupdate packet, if it is new to us, we start to track it. 
                    e.Simulator.AvatarPositions.ForEach(delegate(KeyValuePair<UUID, Vector3> avi)
                    {
                        existing.Add(avi.Key);
                        //is avatar is new to us:
                        if (!agentInfos.ContainsKey(avi.Key))
                        {
                            // string name = instance.Names.Get(avi.Key);
                            // item.Tag = avi.Key;
                            agentInfos[avi.Key] = new NearbyAvatar()
                            {
                                //Distance = d, --will be set in the next lines...
                                ID = avi.Key,
                                Name = instance.Names.Get(avi.Key),
                                simHandle = e.Simulator.Handle
                                
                            };
                                // e.Simulator.Handle;
                        }
                    });

                    //2 for each avatar that we are tracking, 
                    // compute the distance-away and update it
                    foreach (var agentInfo in agentInfos)
                    {
                        UUID agentID = (UUID)agentInfo.Key;

                        if (agentInfos[agentID].simHandle != e.Simulator.Handle)
                        {
                            // not for this sim
                            continue;
                        }

                        if (agentID == client.Self.AgentID)
                        {
                            if (instance.Names.Mode != NameMode.Standard)
                                agentInfos[agentID].Name = instance.Names.Get(agentID);
                            continue;
                        }

                        //the AvatarPostions is checked once more because it changes wildly on its own
                        //even though the !existing should have been adequate
                        Vector3 agentPos;
                        if (!existing.Contains(agentID) || !e.Simulator.AvatarPositions.TryGetValue(agentID, out agentPos))
                        {
                            // not here anymore
                            removed.Add(agentID);
                            continue;
                        }

                        Avatar foundAvi = e.Simulator.ObjectsAvatars.Find(av => av.ID == agentID);
                        
                        // fix: replace CoarseLocationUpdate's height with known, more accurate height. due to edge case.
                        
                        // CoarseLocationUpdate gives us height of 0 when actual height is
                        // between 1024-4096m on OpenSim grids. 1020 on SL
                        bool unknownAltitude = instance.Netcom.LoginOptions.Grid.Platform == "SecondLife" ? agentPos.Z == 1020f : agentPos.Z == 0f;
                        if (unknownAltitude && (foundAvi != null))
                        {
                            ExtractAltitudeFromSimulatorModel(e, foundAvi, ref agentPos);

                            void ExtractAltitudeFromSimulatorModel(
                                CoarseLocationUpdateEventArgs _eventArg, 
                                Avatar avatar,
                                ref Vector3 agentPos_reference)
                            {
                                if (avatar.HasNoParent())
                                {
                                    agentPos_reference.Z = avatar.Position.Z;
                                }
                                else
                                {
                                    if (_eventArg.Simulator.ObjectsPrimitives.ContainsKey(avatar.ParentID))
                                    {
                                        agentPos_reference.Z = _eventArg.Simulator.ObjectsPrimitives[avatar.ParentID].Position.Z;
                                    }
                                }
                                return;
                            }
                        }

                        //distance calculation and updating:
                        Vector3d agentPos_Double = StateManager.ToVector3D(e.Simulator.Handle, agentPos);
                        int d = (int)Vector3d.Distance(
                            agentPos_Double,
                            mypos);
                        //guard clause:
                        if (IsTooFarAway(d))
                        {
                            removed.Add(agentID);
                            continue;
                        }
                        //finally, update the distance
                        NearbyAvatar avatarData = agentInfo.Value;
                        if (unknownAltitude)
                        {
                            avatarData.Distance = -1;
                        }
                        else
                        {
                            avatarData.Distance = d;
                        }
                    }

                    //3. removal of de-tracked avis.
                    foreach (UUID key in removed)
                    {
                        agentInfos.Remove(key);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("Grid_OnCoarseLocationUpdate: " + ex, Helpers.LogLevel.Error, client);
                }

            bool IsTooFarAway(int d)
            {
                return e.Simulator != client.Network.CurrentSim && d > MAX_DISTANCE;
            }
        }
        
        private void Network_SimDisconnected(object sender, SimDisconnectedEventArgs e)
        {
            //when the sim is disconnected, avatars in that sim are no longer tracked.
            try
            {
                if (!Globals.isOnMainThread())
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => {
                        Network_SimDisconnected(sender, e);
                    });
                    return;
                }
                lock (agentInfos)
                {
                    var h = e.Simulator.Handle;
                    //1. create removal list.
                    List<UUID> remove = new List<UUID>();
                    foreach (var agentInfo in agentInfos)
                    {
                        var agentData = agentInfo.Value;
                        if (agentData.simHandle == h)
                        {
                            remove.Add(agentInfo.Key);
                        }
                    }
                    if (remove.Count == 0) return;
                    //2. use removal list to remove items from dictionary:
                    try
                    {
                        foreach (UUID key in remove)
                        {
                            agentInfos.Remove(key);
                        }
                    }
                    finally
                    {
                        
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.DebugLog("Failed to update radar: " + ex);
            }
        }


        #endregion
    }
    
    /// <summary>
    /// Element of list of nearby avatars
    /// </summary>
    public class NearbyAvatar
    {
        public UUID ID { get; set; }
        public ulong simHandle { get; set; }
        public string Name { get; set; }
        public int Distance { get; set; }
    }
}
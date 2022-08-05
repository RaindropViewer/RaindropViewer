﻿/**
 * Radegast Metaverse Client
 * Copyright(c) 2009-2014, Radegast Development Team
 * Copyright(c) 2016-2020, Sjofn, LLC
 * All rights reserved.
 *  
 * Radegast is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published
 * by the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program.If not, see<https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using OpenMetaverse.Assets;
using Plugins.CommonDependencies;

namespace Raindrop.Core
{
    public class GestureManager
    {
        private class GestureTrigger
        {
            public string TriggerLower { get; set; }
            public string Replacement { get; set; }
            public UUID AssetID { get; set; }
        }

        /// <summary>Gesture manager instance</summary>
        public static GestureManager Instance { get; } = new GestureManager();

        private Dictionary<UUID, GestureTrigger> Gestures { get; } = new Dictionary<UUID, GestureTrigger>();
        private Random Random { get; } = new Random();

        /// <summary>
        /// Begins monitoring for changes in gestures.
        /// </summary>
        public void BeginMonitoring()
        {
            var client = RaindropInstance.GlobalInstance.Client;
            client.Inventory.Store.InventoryObjectAdded += Store_InventoryObjectAdded;
            client.Inventory.Store.InventoryObjectUpdated += Store_InventoryObjectUpdated;

            foreach (var item in client.Inventory.Store.Items)
            {
                var gesture = item.Value.Data as InventoryGesture;
                if (gesture == null)
                {
                    continue;
                }

                UpdateInventoryGesture(gesture);
            }
        }

        /// <summary>
        /// Stops monitoring for changes in gestures.
        /// </summary>
        public void StopMonitoring()
        {
            var client = RaindropInstance.GlobalInstance.Client;
            client.Inventory.Store.InventoryObjectAdded -= Store_InventoryObjectAdded;
            client.Inventory.Store.InventoryObjectUpdated -= Store_InventoryObjectUpdated;
        }

        /// <summary>
        /// Processes a chat message and activates a gesture if a gesture trigger is present.
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>New message after gesture has been triggered and trigger word has been replaced</returns>
        public string PreProcessChatMessage(string message)
        {
            var outString = new StringBuilder(message.Length);
            var words = message.Split(' ');
            var gestureWasTriggered = false;

            foreach (var word in words)
            {
                if (gestureWasTriggered)
                {
                    outString.Append(word);
                    outString.Append(' ');
                }
                else
                {
                    gestureWasTriggered = ProcessWord(word, outString);
                }
            }

            // Remove trailing space that was added above at the end of our new sentence
            if (outString.Length > 0 && outString[outString.Length - 1] == ' ')
            {
                outString.Remove(outString.Length - 1, 1);
            }

            return outString.ToString();
        }

        /// <summary>
        /// Checks a single word for a gesture trigger and appends the final word to the output
        /// </summary>
        /// <param name="word">Word to check for gesture triggers</param>
        /// <param name="outString">Where to output the word or replacement word to</param>
        /// <returns>True if a gesture trigger was executed.</returns>
        private bool ProcessWord(string word, StringBuilder outString)
        {
            var possibleTriggers = new List<GestureTrigger>();
            var client = RaindropInstance.GlobalInstance.Client;
            var lowerWord = word.ToLower();

            client.Self.ActiveGestures.ForEach(pair =>
            {
                var activeGestureID = pair.Key;
                if (!Gestures.ContainsKey(activeGestureID))
                {
                    return;
                }
                var gesture = Gestures[activeGestureID];

                if (lowerWord != gesture.TriggerLower)
                {
                    return;
                }

                possibleTriggers.Add(gesture);
            });

            if (possibleTriggers.Count == 0)
            {
                outString.Append(word);
                outString.Append(' ');
                return false;
            }

            GestureTrigger gestureToPlay;
            if (possibleTriggers.Count > 1)
            {
                var gestureIndexToPlay = Random.Next(possibleTriggers.Count);
                gestureToPlay = possibleTriggers[gestureIndexToPlay];
            }
            else
            {
                gestureToPlay = possibleTriggers[0];
            }

            client.Self.PlayGesture(gestureToPlay.AssetID);

            if (!String.IsNullOrEmpty(gestureToPlay.Replacement))
            {
                outString.Append(gestureToPlay.Replacement);
                outString.Append(' ');
            }

            return true;
        }

        /// <summary>
        /// Handles any addition or update to gesture items in the inventory.
        /// </summary>
        /// <param name="gesture">Gesture that was added or updated.</param>
        private void UpdateInventoryGesture(InventoryGesture gesture)
        {
            var client = RaindropInstance.GlobalInstance.Client;

            client.Assets.RequestAsset(gesture.AssetUUID, AssetType.Gesture, false, (_, asset) =>
            {
                if (!(asset is AssetGesture assetGesture))
                {
                    return;
                }

                if (!assetGesture.Decode())
                {
                    return;
                }

                if (!Gestures.ContainsKey(gesture.UUID))
                {
                    Gestures.Add(gesture.UUID, new GestureTrigger());
                }

                if (Gestures.TryGetValue(gesture.UUID, out var existingGestureTrigger))
                {
                    existingGestureTrigger.TriggerLower = assetGesture.Trigger.ToLower();
                    existingGestureTrigger.Replacement = assetGesture.ReplaceWith;
                    existingGestureTrigger.AssetID = assetGesture.AssetID;
                }
            });
        }

        private void Store_InventoryObjectUpdated(object sender, InventoryObjectUpdatedEventArgs e)
        {
            if (!(e.NewObject is InventoryGesture gesture))
            {
                return;
            }

            UpdateInventoryGesture(gesture);
        }

        private void Store_InventoryObjectAdded(object sender, InventoryObjectAddedEventArgs e)
        {
            if (!(e.Obj is InventoryGesture gesture))
            {
                return;
            }

            UpdateInventoryGesture(gesture);
        }
    }
}

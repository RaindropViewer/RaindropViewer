using System;
using System.Threading;
using OpenMetaverse;
using Plugins.CommonDependencies;
using Raindrop.Services.Bootstrap;
using TMPro;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Raindrop.UI.Views
{
    public class ParcelInfoPopup : MonoBehaviour
    {
        private GridClient Client => ServiceLocator.Instance.Get<RaindropInstance>().Client;

        public TMP_Text parcelNameTMP;
        public string parcelName;

        // open a parcel info that shows detail about this tap pos (handle)
        public void Open(ulong global_handle)
        {
            this.gameObject.SetActive(true);
            GetTargetParcel(global_handle);
        }
        
        
        void GetTargetParcel(ulong global_handle)
        {
            ThreadPool.QueueUserWorkItem(sync =>
            {
                uint globalX, globalY;
                Utils.LongToUInts(global_handle, out globalX,out globalY);
                
                float localX, localY;
                ulong regionHandle = Helpers.GlobalPosToRegionHandle(globalX, globalY, out localX, out localY);
                
                UUID parcelID = Client.Parcels.RequestRemoteParcelID(
                    new OpenMetaverse.Vector3((float)( localX), (float)(localY), 20f),
                    regionHandle, UUID.Zero);
                if (parcelID != UUID.Zero)
                {
                    ManualResetEvent done = new ManualResetEvent(false);
                    EventHandler<ParcelInfoReplyEventArgs> handler = (sender, e) =>
                    {
                        if (e.Parcel.ID == parcelID)
                        {
                            parcelName = e.Parcel.Name;
                            done.Set();
                            
                            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                                this.Render(e);
                            });
                            
                        }
                    };
                    Client.Parcels.ParcelInfoReply += handler;
                    Client.Parcels.RequestParcelInfo(parcelID);
                    done.WaitOne(30 * 1000, false);
                    Client.Parcels.ParcelInfoReply -= handler;
                }
            });
        }

        private void Render(ParcelInfoReplyEventArgs parcelInfoReplyEventArgs)
        {
            parcelNameTMP.text = parcelName;
        }
    }
}
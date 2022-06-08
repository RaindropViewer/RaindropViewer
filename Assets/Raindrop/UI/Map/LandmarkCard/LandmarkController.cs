using System;
using OpenMetaverse;
using OpenMetaverse.Assets;
using Plugins.CommonDependencies;
using Raindrop;
using Raindrop.Netcom;
using Raindrop.Services.Bootstrap;

public class LandmarkController : IDisposable
{
    // public Image image;
    public string parcelName;
    public string simName;
    public string localCoords;
    public string parcelDesc;

    private LandmarkView view;
    
    #region  shared data
    public object mutex;
    public bool DataReadyToRender; //if true, we should update the view. if it is false, we are not yet ready to update view.
    
    // first layer of peeling asset data
    private InventoryLandmark landmark;
    private AssetLandmark decodedLandmark;
    private UUID localRegionID;
    private Vector3 localPosition;

    private UUID parcelID;
    private ParcelInfo parcel;
    private bool parcelLocation = false;
    
    // public AssetLandmark Landmark
    // {
    //     set
    //     {
    //         this.modified = true;
    //         this.decodedLandmark = value; //currently empty, will be filled by network request later.
    //     }
    // }
    #endregion

    #region globalrefs
    private RaindropInstance instance { get { return ServiceLocator.Instance.Get<RaindropInstance>(); } }
    private RaindropNetcom netcom { get { return instance.Netcom; } }
    private GridClient client { get { return instance.Client; } }
    bool Active => instance.Client.Network.Connected;
    #endregion
    
    public LandmarkController(LandmarkView landmarkView)
    {
        view = landmarkView;
        
        client.Grid.RegionHandleReply += new EventHandler<RegionHandleReplyEventArgs>(Grid_RegionHandleReply);
        client.Parcels.ParcelInfoReply += new EventHandler<ParcelInfoReplyEventArgs>(Parcels_ParcelInfoReply);
    }

    public void Dispose()
    {
        Landmark_Disposed();
    }
    
    void Landmark_Disposed()
    {
        client.Grid.RegionHandleReply -= new EventHandler<RegionHandleReplyEventArgs>(Grid_RegionHandleReply);
        client.Parcels.ParcelInfoReply -= new EventHandler<ParcelInfoReplyEventArgs>(Parcels_ParcelInfoReply);
    }


    //set the landmark to be shown
    public void SetLandmark(InventoryLandmark inventoryLandmark)
    {
        view.InitialiseView();
        this.DataReadyToRender = false;
        
        RequestLandmarkFromServer(inventoryLandmark);
        // RenderLandmark
    }
    
    public void SetLandmark(UUID parcelID)
    {
        this.parcelID = parcelID;
        
        parcelLocation = true;
        client.Parcels.RequestParcelInfo(parcelID);
    }

    #region Async and sync soubroutines to get the data to show to user.

    private void RequestLandmarkFromServer(InventoryLandmark landmarkModel)
    {
        client.Assets.RequestAsset(
            landmarkModel.AssetUUID,
            landmarkModel.AssetType,
            true,
            Assets_OnAssetReceived);
    }

    // when the landmark object is ready, we will render the textual parts to the view.
    void Assets_OnAssetReceived(AssetDownload transfer, Asset asset)
    {
        if (transfer.Success && asset.AssetType == AssetType.Landmark)
        {
            decodedLandmark = (AssetLandmark)asset;
            decodedLandmark.Decode();
            localPosition = decodedLandmark.Position;
            localRegionID = decodedLandmark.RegionID;
            // next step: Get image.
            // next step: Get Regionname.
            // next step: Get Regiondescription.
            client.Grid.RequestRegionHandle(decodedLandmark.RegionID);
            // client.Grid.RequestMapItems(decodedLandmark.RegionID);
        }
    }

    
    void Grid_RegionHandleReply(object sender, RegionHandleReplyEventArgs e)
    {
        if (decodedLandmark == null || decodedLandmark.RegionID != e.RegionID) return;

        parcelID = client.Parcels.RequestRemoteParcelID(decodedLandmark.Position, e.RegionHandle, e.RegionID);
        if (parcelID != UUID.Zero)
        {
            client.Parcels.RequestParcelInfo(parcelID);
        }
    }
    
    // finally, we can haz datas.
    void Parcels_ParcelInfoReply(object sender, ParcelInfoReplyEventArgs e)
    {
        if (e.Parcel.ID != parcelID) return;

        if (! UnityMainThreadDispatcher.isOnMainThread())
        {
            UnityMainThreadDispatcher.Instance().Enqueue(
                () => Parcels_ParcelInfoReply(sender,e)
                );
            return;
        }

        parcel = e.Parcel;

        // pnlDetail.Visible = true;
        // if (parcel.SnapshotID != UUID.Zero)
        // {
        //     SLImageHandler img = new SLImageHandler(instance, parcel.SnapshotID, "") {Dock = DockStyle.Fill};
        //     pnlDetail.Controls.Add(img);
        //     pnlDetail.Disposed += (senderx, ex) =>
        //     {
        //         img.Dispose();
        //     };
        //     img.BringToFront();
        // }
        //
        // btnTeleport.Enabled = true;
        // btnShowOnMap.Enabled = true;

        if (parcelLocation)
        {
            localPosition = new Vector3
            {
                X = parcel.GlobalX % 256,
                Y = parcel.GlobalY % 256,
                Z = parcel.GlobalZ
            };
        }

        // txtParcelName.Text = decodedLandmark == null 
        //     ? $"{parcel.Name} - {parcel.SimName} " 
        //     : $"{parcel.Name} - {parcel.SimName} ({(int) decodedLandmark.Position.X}, {(int) decodedLandmark.Position.Y}, {(int) decodedLandmark.Position.Z}) ";
        //
        // txtParcelDescription.Text = parcel.Description;
        //
        // view.RenderView(parcelLocation, parcelName, parcelDesc, teleportBool, showOnMapBool, );
    }
    #endregion
    
    
    private void UpdateInternalData(AssetLandmark lm)
    {
        lock (mutex)
        {
            this.parcelName = lm.RegionID.ToString();

            DataReadyToRender = true;
        }
    }
    
}
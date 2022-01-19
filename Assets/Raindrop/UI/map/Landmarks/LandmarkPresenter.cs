using OpenMetaverse;
using OpenMetaverse.Assets;
using Raindrop;
using Raindrop.Netcom;
using Raindrop.ServiceLocator;

public class LandmarkPresenter
{
    // public Image image;
    public string parcelName;
    public string simName;
    public string localCoords;
    public string parcelDesc;

    private LandmarkView view;
    private LandmarkModel model;


    #region  shared data

    public object mutex;
    
    public bool modified; //if modified is true, we should update the view.
    
    private InventoryLandmark landmark;
    private AssetLandmark decodedLandmark; //currently empty, will be filled by network request later.
    private UUID parcelID;
    private ParcelInfo parcel;
    private Vector3 localPosition;
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


    // todo: someone needs to pass the inventory landmark to me! :)
    public LandmarkPresenter(LandmarkView landmarkView, InventoryLandmark invLandmark)
    {
        view = landmarkView;
        // model = new LandmarkModel();

        RequestLandmarkFromServer(invLandmark);

    }

    private void RequestLandmarkFromServer(InventoryLandmark landmarkModel)
    {
        client.Assets.RequestAsset(landmarkModel.AssetUUID, landmarkModel.AssetType, true, Assets_OnAssetReceived);
    }

    void Assets_OnAssetReceived(AssetDownload transfer, Asset asset)
    {
        if (transfer.Success && asset.AssetType == AssetType.Landmark)
        {
            decodedLandmark = (AssetLandmark)asset;
            decodedLandmark.Decode();
            localPosition = decodedLandmark.Position;
            client.Grid.RequestRegionHandle(decodedLandmark.RegionID);
        }
    }

    private void UpdateInternalData(AssetLandmark lm)
    {
        lock (mutex)
        {
            this.parcelName = lm.RegionID.ToString();

            modified = true;
        }
    }
    
}
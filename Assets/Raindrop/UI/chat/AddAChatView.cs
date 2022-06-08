using System.Collections.Generic;
using OpenMetaverse;
using Plugins.CommonDependencies;
using Raindrop;
using Raindrop.Services;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

// View logic for the modal that allows us to select a person to IM with. 
public class AddAChatView : MonoBehaviour
{
    public RaindropInstance instance => ServiceLocator.Instance.Get<RaindropInstance>();

    public Button FriendsTab;
    public Button GroupsTab;
    public Button NearbyTab;
    
    [FormerlySerializedAs("PeopleList")] public PeopleListView PeopleListView;
    private bool IsBusy = false;

    // enum PeopleSource
    // {
    //     Friends,
    //     Groups, //special type of IM : group chat
    //     Nearby
    // }
    
    // Start is called before the first frame update
    void Start()
    {
        FriendsTab.onClick.AddListener(OnFriends);
        GroupsTab.onClick.AddListener(OnGroup);
        NearbyTab.onClick.AddListener(OnNearby);
    }

    private void OnGroup()
    {
        var ui = ServiceLocator.Instance.Get<UIService>();
        ModalsManager.PushModal_NotImplementedYet(this);
    }

    private void OnNearby()
    {
        if (IsBusy)
            return;
        
        IsBusy = true;
        //populate list with nearby people.
        List<AvatarData> peopleDatas = new List<AvatarData>();
        var nearbyData = GetNearbyData();
        // var namesList = GetNames(uuidList);
        // var ImagesList = GetImageUUIDs(uuidList);
        PeopleListView.Set(nearbyData, this);
        IsBusy = false;

    }

    //get list of nearby people's data 
    private List<AvatarData> GetNearbyData()
    {
        List<AvatarData> res = new List<AvatarData>();
        foreach (NearbyAvatar nearbyAvatar in instance.AgentsTracker.NearbyAvatars)
        {
            res.Add(new AvatarData(
                nearbyAvatar.Name,
                nearbyAvatar.ID,
                UUID.Zero, //todo. how to cache the avatar's image uuid?
                true,
                nearbyAvatar.Distance)); //avatars reported by the surrounding sims should be online, right?
        }

        return res;
    }

    private void OnFriends()
    {
        if (IsBusy)
            return;
        
        IsBusy = true;
        //populate list with friends.
        List<AvatarData> peopleDatas = new List<AvatarData>();
        var friendDatas = GetFriendsData();
        PeopleListView.Set(friendDatas, this);
        IsBusy = false;

    }
    //
    //
    // private List<UUID> GetImageUUIDs(List<UUID> uuidList)
    // {
    //     
    // }
    //
    // private List<string> GetNames(List<UUID> uuidList)
    // {
    //     instance.Names.
    //     
    // }

    private List<AvatarData>  GetFriendsData()
    {
        List<AvatarData> datas = new List<AvatarData>();
        
        InternalDictionary<UUID, FriendInfo> friendDict = instance.Client.Friends.FriendList;
        var UUIDs = friendDict.FindAll(alwaysTrue);
        List<FriendInfo> friendDatas = friendDict.FindAll(alwaysTrue_Info);

        for (int i = 0; i < friendDatas.Count; i++)
        {
            var friend = friendDatas[i];
            var uuid = friend.UUID;
            var isOnline = friend.IsOnline;
            var name = friend.Name;
            
            datas.Add(new AvatarData(name, uuid, UUID.Zero, isOnline, -1));
        }
        
        return datas;
    }
    
    //LOL!
    private bool alwaysTrue(UUID obj)
    {
        return true;
    }
    private bool alwaysTrue_Info(FriendInfo obj)
    {
        return true;
    }

    public void CloseModal()
    {
        Destroy(this);
    }
}

public class AvatarData
{
    public readonly string Name;
    public readonly UUID Uuid;
    public readonly UUID ImageUuid;
    public readonly bool IsOnline;
    public readonly int Distance;

    public AvatarData(string name, UUID UUID, UUID ImageUUID, bool IsOnline, int distance)
    {
        Name = name;
        Uuid = UUID;
        ImageUuid = ImageUUID;
        this.IsOnline = IsOnline;
        this.Distance = distance;
    }
}


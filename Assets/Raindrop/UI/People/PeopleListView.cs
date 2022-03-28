using System;
using System.Collections;
using System.Collections.Generic;
using OpenMetaverse;
using UnityEngine;

// attach this when you want to populate a list of people in the UI.
public class PeopleListView : MonoBehaviour
{
    public List<PeopleView> people;
    public GameObject peopleViewPrefab;

    public Transform Root_PeopleListView;

    public void Set(List<AvatarData> PeopleDatas, AddAChatView addAChatView)
    {
        //1. clear present list
        foreach (var person in people)
        {
            //todo: destroy
            Destroy(person.gameObject);
        }
        people.Clear();
        
        //2. create new list.
        for (int i = 0; i < PeopleDatas.Count; i++)
        {
            var item = Instantiate(peopleViewPrefab, Root_PeopleListView);
            var view = item.GetComponent<PeopleView>();
            view.Init(PeopleDatas[i].Uuid,
                PeopleDatas[i].Name,
                PeopleDatas[i].ImageUuid, 
                PeopleDatas[i].IsOnline,
                PeopleDatas[i].Distance,
                addAChatView);
            people.Add(view);
        }
    }
    
    
    //set the list of people in UI.
    // public void Set(List<UUID> userIDs,
    //                 List<String> names,
    //                 List<UUID> imageIDs,
    //                 List<bool> isOnline ){
    //     //1. clear present list
    //     foreach (var person in people)
    //     {
    //         //todo: destroy
    //         Destroy(person.gameObject);
    //     }
    //     people.Clear();
    //     
    //     //2. create new list.
    //     for (int i = 0; i < names.Count; i++)
    //     {
    //         var item = Instantiate(peopleViewPrefab, Root_PeopleListView);
    //         var view = item.GetComponent<PeopleView>();
    //         view.Init(userIDs[i], names[i], imageIDs[i], isOnline[i], dis);
    //         people.Add(view);
    //     }
    // }
    
}
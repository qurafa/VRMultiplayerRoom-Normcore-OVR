/************************************************************************************
Copyright : Copyright 2019 (c) Speak Geek (PTY), LTD and its affiliates. All rights reserved.

Developer : Dylan Holshausen

Script Description : Helper for Normcore Realtime Component

************************************************************************************/

using UnityEngine;
using Normal.Realtime;
using System;
using UnityEngine.UIElements;
using HandPhysicsToolkit.Modules.Part.ContactDetection;
using static UnityEngine.UI.Image;
using System.Collections.Generic;

public class realtimeHelper : MonoBehaviour
{

    [SerializeField]
    private List<GameObject> _localPlayer;
    [SerializeField]
    private string playerPrefabName;
    [SerializeField]
    private string roomName;

    private Realtime _Realtime;
    private Transform spawnTransform;
    private void Start()
    {
        _Realtime = GetComponent<Realtime>();

        _Realtime.didConnectToRoom += _Realtime_didConnectToRoom;

        //Connect to Preset Code
        //_Realtime.Connect(roomName);
    }


    //Realtime Event when Connecting to a Room
    private void _Realtime_didConnectToRoom(Realtime realtime)
    {
        int id = _Realtime.clientID;

        if (!spawnTransform)
        {
            //if not then get from one of the default positions
            spawnTransform = _localPlayer[0].transform;//either its current position

            foreach (GameObject g in GameObject.FindGameObjectsWithTag("spawn"))
            {
                if (g.name.Equals(id.ToString()))
                {
                    spawnTransform = g.transform; break;
                }
            }
        }

        foreach (GameObject l in _localPlayer)
            l.transform.SetPositionAndRotation(spawnTransform.position, spawnTransform.rotation);

        GameObject newPlayer = Realtime.Instantiate(playerPrefabName, spawnTransform.position, spawnTransform.rotation, new Realtime.InstantiateOptions
        {
            ownedByClient = true,
            preventOwnershipTakeover = true,
            destroyWhenOwnerLeaves = true,
            destroyWhenLastClientLeaves = true,
            useInstance = _Realtime,
        });
        RequestOwnerShip(newPlayer);

        if (id == 0)
            AllRequestOwnerShip();
    }

    private void RequestOwnerShip(GameObject o)
    {
        if(o.TryGetComponent<RealtimeView>(out RealtimeView rtView))
            rtView.RequestOwnership();

        if (o.TryGetComponent<RealtimeTransform>(out RealtimeTransform rtTransform))
            rtTransform.RequestOwnership();

        for(int c = 0; c < o.transform.childCount; c++)
            RequestOwnerShip(o.transform.GetChild(c).gameObject);

        return;
    }

    private void AllRequestOwnerShip()
    {
        var rViews = FindObjectsByType<RealtimeView>(FindObjectsSortMode.None);
        var rTransforms = FindObjectsByType<RealtimeTransform>(FindObjectsSortMode.None);

        foreach (RealtimeView v in rViews)
        {
            if(v.isUnownedSelf)
                v.RequestOwnership();
        }
        foreach(RealtimeTransform t in rTransforms)
        {
            if (t.isUnownedSelf)
                t.RequestOwnership();
        }
    }

    //Generate Random String
    private string randomString(int length)
    {
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var stringChars = new char[length];
        var random = new System.Random();

        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }

        var finalString = new String(stringChars);

        return finalString;
    }

    public void JoinRoom(Transform transform)
    {
        spawnTransform = transform;
        


        //Connect to room
        _Realtime.Connect(roomName);
    }
}

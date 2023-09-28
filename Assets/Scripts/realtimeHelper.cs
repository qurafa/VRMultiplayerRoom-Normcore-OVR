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

public class realtimeHelper : MonoBehaviour
{
    private Realtime _Realtime;
    [SerializeField]
    private string playerPrefabName;
    [SerializeField]
    private string roomName;

    private void Start()
    {
        _Realtime = GetComponent<Realtime>();

        _Realtime.didConnectToRoom += _Realtime_didConnectToRoom;

        //Connect to Preset Code
        _Realtime.Connect(roomName);
    }


    //Realtime Event when Connecting to a Room
    private void _Realtime_didConnectToRoom(Realtime realtime)
    {
        GameObject newPlayer = Realtime.Instantiate(playerPrefabName);
        RequestOwnerShip(newPlayer);

        if (_Realtime.clientID == 0)
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
}

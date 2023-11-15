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
using UnityEngine.SceneManagement;
using Oculus.Interaction.Samples;

public class realtimeHelper : MonoBehaviour
{

    [SerializeField]
    private GameObject _localPlayer;
    [SerializeField]
    private string playerPrefabName;
    [SerializeField]
    private string roomName;
    [SerializeField]
    private GameObject room;
    [SerializeField]
    private List<GameObject> objectsToSpawn;

    private Realtime _Realtime;
    private Transform spawnTransform;
    private void Start()
    {
        _Realtime = GetComponent<Realtime>();

        _Realtime.didConnectToRoom += _Realtime_didConnectToRoom;

        _Realtime.didDisconnectFromRoom += _Realtime_didDisconnectFromRoom;

        //Connect to Preset Code
        //_Realtime.Connect(roomName);
    }


    private void Update()
    {
        if (_Realtime.connected) return;

        if((OVRInput.GetActiveController() != OVRInput.Controller.Hands) && OVRInput.GetUp(OVRInput.Button.Start))
            _Realtime.Connect(roomName);
    }

    //Realtime Event when Connecting to a Room
    private void _Realtime_didConnectToRoom(Realtime realtime)
    {
        int id = _Realtime.clientID;

        if (!spawnTransform)
        {
            Debug.Log("spawnTransform not set");
            Debug.Log("Setting spawnTransform.....");
            //if not then get from one of the default positions
            spawnTransform = _localPlayer.transform;//either its current position

            foreach (GameObject g in GameObject.FindGameObjectsWithTag("spawn"))
            {
                if (g.name.Equals(id.ToString()))
                {
                    spawnTransform = g.transform; break;
                }
            }
            Debug.Log(".....spawnTransform set");
        }

        _localPlayer.transform.SetPositionAndRotation(spawnTransform.position, spawnTransform.rotation);

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
        {
            AllRequestOwnerShip();
        }
    }

    private void _Realtime_didDisconnectFromRoom(Realtime realtime)
    {
        AdditiveSceneLoader aSL = GetComponent<AdditiveSceneLoader>();

        //take us back the lobby when we disconnect
        aSL?.LoadScene(0);
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
        var rViews = FindObjectsOfType<RealtimeView>();//GetComponents<RealtimeView>();//FindObjectsByType<RealtimeView>(FindObjectsSortMode.InstanceID);
        var rTransforms = FindObjectsOfType<RealtimeTransform>();//GetComponents<RealtimeTransform>();//FindObjectsByType<RealtimeTransform>(FindObjectsSortMode.InstanceID);

        foreach (RealtimeView v in rViews)
        {
            v.RequestOwnership();
        }
        foreach(RealtimeTransform t in rTransforms)
        {
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

    public void JoinMainRoomByOffset(Transform offset)
    {
        spawnTransform = new GameObject().transform;

        _Realtime.Connect(roomName);
    }
    
    public void JoinMainRoomByOffset(MyTransform offset)
    {
        spawnTransform = new GameObject().transform;

        spawnTransform.position = new Vector3(_localPlayer.transform.position.x + offset.position.x,
            _localPlayer.transform.position.y + offset.position.y,
            _localPlayer.transform.position.z + offset.position.z);

        spawnTransform.eulerAngles = new Vector3(_localPlayer.transform.eulerAngles.x + offset.eulerAngles.x,
            _localPlayer.transform.eulerAngles.y + offset.eulerAngles.y,
            _localPlayer.transform.eulerAngles.z + offset.eulerAngles.z);

        Debug.Log("SpawnTransform: " + spawnTransform.position.ToString() + " " + spawnTransform.eulerAngles.ToString());

        _Realtime.Connect(roomName);
    }

    public void JoinMainRoom(Transform transform)
    {
        spawnTransform = transform;
        _Realtime.Connect(roomName);
    }
    public void JoinMainRoom(MyTransform transform)
    {
        spawnTransform = new GameObject().transform;
        spawnTransform.position = room.transform.position + transform.position;
        spawnTransform.eulerAngles = transform.eulerAngles;
        spawnTransform.RotateAround(room.transform.position, Vector3.up, -transform.rotAbt.y);
        Debug.Log($"PlayerCenterReference {spawnTransform.transform.position.ToString()} Room {room.transform.position}");

        _Realtime.Connect(roomName);
    }
    
    public void JoinMainRoom(Vector3 pos, Quaternion rot)
    {
        spawnTransform = new GameObject().transform;
        //spawnTransform = transform;
        spawnTransform.SetPositionAndRotation(pos, rot);
        spawnTransform.localScale = Vector3.one;

        JoinMainRoom(spawnTransform);
    }

    public void JoinLobby()
    {

    }

    private void OnDisable()
    {
        _Realtime.didConnectToRoom -= _Realtime_didConnectToRoom;

        _Realtime.didDisconnectFromRoom -= _Realtime_didDisconnectFromRoom;
    }
}

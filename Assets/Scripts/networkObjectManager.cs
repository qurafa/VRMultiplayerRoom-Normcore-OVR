﻿/************************************************************************************
Copyright : Copyright 2019 (c) Speak Geek (PTY), LTD and its affiliates. All rights reserved.

Developer : Dylan Holshausen

Script Description : Enable/Disable Objects Based on Remote/Local Player

************************************************************************************/

using UnityEngine;
using Normal.Realtime;
using HandPhysicsToolkit.Modules.Avatar;

public class networkObjectManager : MonoBehaviour
{
/*    public Camera[] localCameras;

    public AudioListener localAudioListener;

    public OVRCameraRig localOVRCameraRig;

    public OVRManager localOVRManager;

    public OVRHeadsetEmulator localEmulator;*/

    public GameObject[] Hands;

    //public GameObject[] setInactive;

    public RealtimeView rtView;

    private bool _Init = false;

    private void Update()
    {
        if (!_Init)
            Init();
    }

    private void Init()
    {
        if (rtView == null)
            return;

        transform.name = rtView.ownerID.ToString();

        //SEARCH FOR GAMEOBJECTS WITH THE TAG 'SPAWN' AND SET TRANSFORM THE SAME
        //BASED ON OWNER ID
        foreach(GameObject myGO in GameObject.FindGameObjectsWithTag("spawn"))
        {
            if (myGO.name == transform.name)
            {
                transform.position = myGO.transform.position;
                transform.rotation = myGO.transform.rotation;
            }
        }

        //IF THIS IS NOT OUR REALTIME VIEW
        if (!rtView.isOwnedLocally) {
/*            foreach (GameObject g in setInactive)
                Destroy(g);

            Destroy(localAudioListener);
            Destroy(localOVRCameraRig);
            Destroy(localOVRManager);
            Destroy(localEmulator);

            foreach(Camera cam in localCameras)
                Destroy(cam);*/

            foreach (GameObject hand in Hands)
            {
                //OVR SKELETON
                if (hand.GetComponent<OVRSkeleton>())
                {
                    Destroy(hand.GetComponent<OVRSkeleton>());
                }

                //OVR HAND
                if (hand.GetComponent<OVRHand>())
                {
                    Destroy(hand.GetComponent<OVRHand>());
                }
            }

            foreach(ReprView rV in GetComponentsInChildren<ReprView>())
                Destroy(rV);

            foreach(ReprModel rM in GetComponentsInChildren<ReprModel>())
                Destroy(rM);
        }
        else
        {
            //REQUEST OWNERSHIP OF EACH CHILD REALTIMEVIEW
            foreach (RealtimeView childRTView in GetComponentsInChildren<RealtimeView>())
            {
                childRTView.RequestOwnership();
            }

            //REQUEST OWNERSHIP OF EACH CHILD REALTIMETRANSFORM
            foreach (RealtimeTransform childRTTransform in GetComponentsInChildren<RealtimeTransform>())
            {
                childRTTransform.RequestOwnership();
            }

            if (Application.isEditor)
            {
                //LOOP THROUGH HAND COMPONENTS AND DISABLE OVR COMPONENTS
                foreach (GameObject hand in Hands)
                {
                    //OVR SKELETON
                    if (hand.GetComponent<OVRSkeleton>())
                    {
                        Destroy(hand.GetComponent<OVRSkeleton>());
                    }

                    //OVR HAND
                    if (hand.GetComponent<OVRHand>())
                    {
                        Destroy(hand.GetComponent<OVRHand>());
                    }
                }
            }
        }

        //LOOP THROUGH HAND COMPONENTS AND READY OUR LOCAL HANDS
        foreach (GameObject hand in Hands)
        {
            //Speak Geek Quest Hand
            if (hand.GetComponentInChildren<SpeakGeekOculusQuestHand>())
            {
                hand.GetComponentInChildren<SpeakGeekOculusQuestHand>().readyHand();
            }
        }

        _Init = true;
    }
}

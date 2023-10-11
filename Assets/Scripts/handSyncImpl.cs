﻿/************************************************************************************
Copyright : Copyright 2019 (c) Speak Geek (PTY), LTD and its affiliates. All rights reserved.

Developer : Matthew Morris

Script Description : 

************************************************************************************/

using HandPhysicsToolkit.Modules.Avatar;
using Oculus.Interaction.Input;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

using static OVRSkeleton;

public class handSyncImpl : MonoBehaviour
{
    [Header("These two fields need to match the OVRSkelton fields")]
    [SerializeField]
    SkeletonType skeletonType;
    [SerializeField]
    private bool _updateRootPose = false;
    [SerializeField]
    private bool _updateRootScale = false;

    //This hands bones
    private Transform _localRoot;
    private Transform _remoteRoot;
    private List<Transform> _remoteBones = new List<Transform>();
    private List<Transform> _localBones = new List<Transform>();

    //This list is used to get all children recursively
    private List<Transform> listOfChildren = new List<Transform>();

    //This hands mesh renderer
    [SerializeField]
    private SkinnedMeshRenderer _mySkinMeshRenderer;

    //References to Oculus Objects
    private OVRSkeleton _myOVRSkeleton;
    private IOVRSkeletonDataProvider _dataProvider;

    //References to scripts managing networking of hands
    public handPoseModelSync _SGHandSync;
    public Normal.Realtime.RealtimeView rtView;

    public bool handReady = false;

    //Ensures the transform is uniform
    private void Awake()
    {
        transform.eulerAngles = Vector3.zero;
        transform.localPosition = Vector3.zero;
        transform.localScale = new Vector3(1f, 1f, 1f);
    }

    //Recreate hand structure to replicate Oculus
    //This orders all the bones within the list, setting the finger tips last.
    public void readyHand()
    {
        Debug.Log("Ready Hand");
        //_SGHandSync = GetComponent<handPoseModelSync>();

        switch (skeletonType)
        {
            case SkeletonType.HandLeft:
                if (GameObject.FindWithTag("l_slave"))
                    _localRoot = GameObject.FindWithTag("l_slave").transform;
                else return;
                break;
            case SkeletonType.HandRight:
                if (GameObject.FindWithTag("r_slave"))
                    _localRoot = GameObject.FindWithTag("r_slave").transform;
                else return;
                break;
            default:
                Debug.Log("Hand Skeleton not set");
                return;
        }

        //And get all local bones
        foreach (Transform child in _localRoot)
        {
            //_localBones = new List<Transform>();
            if (child.name.ToLower().Contains("wrist"))
            {

                listOfChildren = new List<Transform>();
                GetChildRecursive(transform);
                List<Transform> _remoteBonesTemp = listOfChildren;

                listOfChildren = new List<Transform>();
                GetChildRecursive(_localRoot);
                int index = 0;

                //We need bones to be in the same order as oculus
                //So we add all the bones and keep a reference to 5 finger tips. (OVRSkeleton sets these bone id's last)
                //We then add finger tips back to bones to they are last.
                List<Transform> fingerTipsLocal = new List<Transform>();
                List<Transform> fingerTipsRemote = new List<Transform>();
                foreach (var bone in listOfChildren)
                {
                    if (bone.name.Contains("_") && bone.tag.Equals("AnimFollow"))
                    {
                        
                        if (bone.name.Contains("tip"))
                        {
                            //Keep reference to finger tips, local and remote
                            fingerTipsLocal.Add(bone);
                            fingerTipsRemote.Add(_remoteBonesTemp[index]);
                        }
                        else if (bone.name.ToLower().Contains("wrist") || 
                            _remoteBonesTemp[index].name.ToLower().Contains("wrist"))
                        {
                            if (bone.name.ToLower().Contains("wrist")) _localRoot = bone;
                            if (_remoteBonesTemp[index].name.ToLower().Contains("wrist")) _remoteRoot = _remoteBonesTemp[index];
                        }
                        else
                        {
                            _localBones.Add(bone);
                            _remoteBones.Add(_remoteBonesTemp[index]);
                        }
                        //Debug.Log("Local Bone Name: " + bone.name + " Remote Bone Name: " + _remoteBonesTemp[index].name);
                        //_remoteBonesTemp[index].SetPositionAndRotation(bone.position, bone.rotation);
                        index++;
                    }
                }
                //And finger tips back to bones
                for(int x = 0; x < fingerTipsLocal.Count; x++)
                {
                    _localBones.Add(fingerTipsLocal[x]);
                    _remoteBones.Add(fingerTipsRemote[x]);
                }
                
                break;
            }
        }

        //Initialize the skinnedMeshRender and assign the bones.
        //_mySkinMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        _mySkinMeshRenderer.enabled = true;
        //_mySkinMeshRenderer.bones = _remoteBones.ToArray();

        //Hands are now ready
        handReady = true;

        //getting and assigning the OVR Skeleton for the hand data
        //IOVRSkeletonDataProvider holds the hand rotation data. So we get reference to the same DataProvider as the oculus hand we are mimicing.
        //if (_dataProvider == null && _myOVRSkeleton != null)
        //{
            //_dataProvider = _myOVRSkeleton.GetComponent<IOVRSkeletonDataProvider>();
        //}
    }

    //Everything within Update has to do with the local hand.
    private void Update()
    {
        //Only update if the hand is ready
        if (!handReady)
            return;

        //Check network ownership
        if (rtView != null)
        {
            if (!rtView.isOwnedLocally)
            {
                return;
            }
        }

        //Ensure we still have the DataProvider otherwise attempt to find it again.
        //If we do then update our hand from the data provider.
        //if (_dataProvider == null && _myOVRSkeleton != null)
        //{
            //_dataProvider = _myOVRSkeleton.GetComponent<IOVRSkeletonDataProvider>();
        //}
        //else
        //{
            //var data = _dataProvider.GetSkeletonPoseData();
            //dataToSend is a string of all the relevant data controlling the movement of the hand, which needs to be sent over the network.
            string dataToSend = "";

            //We check oculus data confidence
            //if (data.IsDataValid && data.IsDataHighConfidence)
            //{
                _mySkinMeshRenderer.enabled = true;

                dataToSend += "1|";

                //Replicates oculus root pose handling, should match oculus OVRSkeleton
                if (_updateRootPose)
                {
                    //Vector3 p = _localRoot.position.FromFlippedZVector3f();
                    //_remoteRoot.localRotation = data.RootPose.Orientation.FromFlippedZQuatf();

                    dataToSend += "1|";
                    dataToSend += _localRoot.position.x + "|" + _localRoot.position.y + "|" + _localRoot.position.z + "|";
                    dataToSend += _localRoot.rotation.x + "|" + _localRoot.rotation.y + "|" + _localRoot.rotation.z + "|" + _localRoot.rotation.w + "|";
                }
                else
                {
                    dataToSend += "0|";
                    dataToSend += "0|0|0|";
                    dataToSend += "0|0|0|";
                }

                //Replicates oculus root scale handling, should match oculus OVRSkeleton
                if (_updateRootScale)
                {
                    /*transform.localScale = new Vector3(data.RootScale, data.RootScale, data.RootScale);*/

                    dataToSend += "1|";
                    dataToSend += _localRoot.localScale.x + "|" + _localRoot.localScale.y + "|" + _localRoot.localScale.z + "|";
                }
                else
                {
                    dataToSend += "0|";
                    dataToSend += "0|0|0|";
                }

                //Set bone transform from SkeletonPoseData
                for (var i = 0; i < _localBones.Count; ++i)
                {
                    //_bones[i].transform.localRotation = data.BoneRotations[i].FromFlippedZQuatf();
                    //Debug.Log("LocalBones Name Sendng: " + _localBones[i].name);

                    dataToSend += _localBones[i].position.x + "|" + _localBones[i].position.y + "|" + _localBones[i].position.z + "|";
                    dataToSend += _localBones[i].rotation.x + "|" + _localBones[i].rotation.y + "|" + _localBones[i].rotation.z + "|" + _localBones[i].rotation.w + "|";
                }
            //}
            //else
            //{

                //If data confidence is low, hide hand
                //_mySkinMeshRenderer.enabled = false;

                //dataToSend = "0|";
            //}

            //Don't send TrackedData if we are the editor.
            //if (!Application.isEditor)
            //{
                if (rtView != null)
                {
                    if (rtView.isOwnedLocally)
                    {
                        Debug.Log("Sending: " + dataToSend);
                        _SGHandSync.SetTrackedData(dataToSend);
                    }
                }
            //}
        //}
    }

    //Everything within Update has to do with the remote hand.
    public void updateFromNormCore(string netHandData)
    {
        if (!handReady)
            return;
        //Debug.Log("NetHandData: " + netHandData);

        if (netHandData == "")
            return;

        if (rtView != null)
        {
            if (rtView.isOwnedLocally)
            {
                _mySkinMeshRenderer.enabled = false;
                return;
            }
        }

        Debug.Log("Receiving: " + netHandData);
        string[] netHandDataArr = netHandData.Split('|');

        if (netHandDataArr[0] == "0")
        {
            _mySkinMeshRenderer.enabled = false;

            return;
        }
        else if (netHandDataArr[0] == "1")
        {
            _mySkinMeshRenderer.enabled = true;
        }

        if (netHandDataArr[1] == "1")
        {
            Vector3 pos = new Vector3(float.Parse(netHandDataArr[2], CultureInfo.InvariantCulture), float.Parse(netHandDataArr[3], CultureInfo.InvariantCulture), float.Parse(netHandDataArr[4], CultureInfo.InvariantCulture));
            Quaternion rot = new Quaternion(float.Parse(netHandDataArr[5], CultureInfo.InvariantCulture), float.Parse(netHandDataArr[6], CultureInfo.InvariantCulture), float.Parse(netHandDataArr[7], CultureInfo.InvariantCulture), float.Parse(netHandDataArr[8], CultureInfo.InvariantCulture));
            _remoteRoot.SetPositionAndRotation(pos, rot);
        }

        if (netHandDataArr[9] == "1")
        {
            _remoteRoot.localScale = new Vector3(float.Parse(netHandDataArr[10], CultureInfo.InvariantCulture), float.Parse(netHandDataArr[11], CultureInfo.InvariantCulture), float.Parse(netHandDataArr[12], CultureInfo.InvariantCulture));
        }

        for (var i = 0; i < _remoteBones.Count; ++i)
        {
            Debug.Log(_remoteBones[i].name);
            Vector3 pos = new Vector3(float.Parse(netHandDataArr[13 + i]), float.Parse(netHandDataArr[14 + i]), float.Parse(netHandDataArr[15 + i]));
            Quaternion rot = new Quaternion(float.Parse(netHandDataArr[16 + i]), float.Parse(netHandDataArr[17 + i]), float.Parse(netHandDataArr[18 + i]), float.Parse(netHandDataArr[19 + i]));
            _remoteBones[i].SetPositionAndRotation(_remoteBones[i].position, rot);
        }
    }

    //Helper function to tranverse children and get reference to their transforms.
    private void GetChildRecursive(Transform obj)
    {
        if (null == obj)
            return;

        foreach (Transform child in obj.transform)
        {
            if (null == child)
                continue;

            if (child != obj)
            {
                listOfChildren.Add(child);
            }
            GetChildRecursive(child);
        }
    }

    private OVRSkeleton GetOVRSkeleton()
    {
        if (_myOVRSkeleton != null)
            return _myOVRSkeleton;

        OVRSkeleton[] skeletons = GetComponents<OVRSkeleton>();

        foreach (OVRSkeleton s in skeletons)
        {
            if(s.GetSkeletonType() == skeletonType)
            {
                _myOVRSkeleton = s;
                break;
            }
        }

        return _myOVRSkeleton;
    }

    public static Quaternion FromFlippedZQuatf(Quaternion q)
    {
        return new Quaternion() { x = -q.x, y = -q.y, z = q.z, w = q.w };
    }
}

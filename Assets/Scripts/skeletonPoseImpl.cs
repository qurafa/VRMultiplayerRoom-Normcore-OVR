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

public class skeletonPoseImpl : MonoBehaviour
{
    [Header("These two fields need to match the OVRSkelton fields")]
    [SerializeField]
    SkeletonPart skeletonPart;
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
    private Renderer _myRenderer;

    //References to Oculus Objects
    private OVRSkeleton _myOVRSkeleton;
    private IOVRSkeletonDataProvider _dataProvider;

    //References to scripts managing networking of hands
    public skeletonPose skeletonPose;
    public Normal.Realtime.RealtimeView rtView;

    public bool skeletonReady = false;

    private DataManager _dataManager;

    public enum SkeletonPart
    {
        None = 0,
        Head = 1,
        LeftHand = 2, 
        RightHand = 3,
    }

    //Ensures the transform is uniform
    private void Awake()
    {
        transform.eulerAngles = Vector3.zero;
        transform.localPosition = Vector3.zero;
        transform.localScale = new Vector3(1f, 1f, 1f);
    }

    //Recreate hand structure to replicate Oculus
    //This orders all the bones within the list, setting the finger tips last.
    public void ReadySkeleton()
    {
        Debug.Log("Ready Skeleton");
        //_SGHandSync = GetComponent<handPoseModelSync>();

        switch (skeletonPart)
        {
            case SkeletonPart.LeftHand:
                if (GameObject.FindWithTag("l_slave"))
                    _localRoot = GameObject.FindWithTag("l_slave").transform;
                else
                {
                    Debug.Log($"{skeletonPart} Skeleton not set");
                    return;
                }
                break;
            case SkeletonPart.RightHand:
                if (GameObject.FindWithTag("r_slave"))
                    _localRoot = GameObject.FindWithTag("r_slave").transform;
                else
                {
                    Debug.Log($"{skeletonPart} Skeleton not set");
                    return;
                }
                break;
            case SkeletonPart.Head:
                if (GameObject.FindWithTag("MainCamera"))
                {
                    _localRoot = GameObject.FindWithTag("MainCamera").transform;
                    _remoteRoot = this.transform;
                }
                else
                {
                    Debug.Log($"{skeletonPart} Skeleton not set");
                    return;
                }
                break;
            default:
                Debug.Log($"{skeletonPart} Skeleton not set");
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
                //Debug.Log("Hand Ready, length: " + _remoteBones.Count);
                break;
            }
        }

        //Initialize the skinnedMeshRender and assign the bones.
        //_mySkinMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        _myRenderer.enabled = true;
        //_mySkinMeshRenderer.bones = _remoteBones.ToArray();

        //get the data manager
        _dataManager = FindFirstObjectByType<DataManager>();

        //Hands are now ready
        skeletonReady = true;
    }

    //Everything within Update has to do with the local hand.
    private void Update()
    {
        //Only update if the hand is ready
        if (!skeletonReady)
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
        //_mySkinMeshRenderer.enabled = true;

        dataToSend += "1|";

        //Replicates oculus root pose handling, should match oculus OVRSkeleton
        if (_updateRootPose)
        {
            //Vector3 p = _localRoot.position.FromFlippedZVector3f();
            //_remoteRoot.localRotation = data.RootPose.Orientation.FromFlippedZQuatf();

            dataToSend += "1|";
            Vector3 pos = _localRoot.position;//FromFlippedZVector3f(_localRoot.position);
            Vector3 rot = _localRoot.eulerAngles;//FromFlippedZQuatf(_localRoot.rotation).eulerAngles;

            dataToSend += pos.x + "|" + pos.y + "|" + pos.z + "|";
            dataToSend += rot.x + "|" + rot.y + "|" + rot.z + "|";
            _dataManager.UpdatePlayerFile(rtView.ownerIDSelf, _localRoot.transform);
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
            Vector3 rot = _localBones[i].localEulerAngles;//FromFlippedZQuatf(_localBones[i].localRotation).eulerAngles;
            dataToSend += rot.x + "|" + rot.y + "|" + rot.z + "|";
            _dataManager.UpdatePlayerFile(rtView.ownerIDSelf, _localBones[i].transform);
        }

        skeletonPose.SetTrackedData(dataToSend);
        //}
        //else
        //{

        //If data confidence is low, hide hand
        //_mySkinMeshRenderer.enabled = false;

        //dataToSend = "0|";
    }

    //Everything within Update has to do with the remote hand.
    public void UpdateFromNormCore(string netHandData)
    {
        if (!skeletonReady)
            return;

        if (netHandData == "")
            return;

        if (rtView != null)
        {
            if (rtView.isOwnedLocally)
            {
                _myRenderer.enabled = false;
                return;
            }
        }

        string[] netHandDataArr = netHandData.Split('|');

        if (netHandDataArr[0] == "0")
        {
            _myRenderer.enabled = false;
            return;
        }
        else if (netHandDataArr[0] == "1")
        {
            _myRenderer.enabled = true;
        }

        if (netHandDataArr[1] == "1")
        {
            _remoteRoot.position = new Vector3(float.Parse(netHandDataArr[2], CultureInfo.InvariantCulture), float.Parse(netHandDataArr[3], CultureInfo.InvariantCulture), float.Parse(netHandDataArr[4], CultureInfo.InvariantCulture));
            _remoteRoot.eulerAngles = new Vector3(float.Parse(netHandDataArr[5], CultureInfo.InvariantCulture), float.Parse(netHandDataArr[6], CultureInfo.InvariantCulture), float.Parse(netHandDataArr[7], CultureInfo.InvariantCulture));
            _dataManager.UpdatePlayerFile(rtView.ownerIDSelf, _remoteRoot.transform);
        }

        if (netHandDataArr[8] == "1")
        {
            _remoteRoot.localScale = new Vector3(float.Parse(netHandDataArr[9], CultureInfo.InvariantCulture), float.Parse(netHandDataArr[10], CultureInfo.InvariantCulture), float.Parse(netHandDataArr[11], CultureInfo.InvariantCulture));
        }

        for (var i = 0; i < _remoteBones.Count; ++i)
        {
            int tempBoneCount = i * 3;
            _remoteBones[i].localEulerAngles = new Vector3(float.Parse(netHandDataArr[12 + tempBoneCount], CultureInfo.InvariantCulture), float.Parse(netHandDataArr[13 + tempBoneCount], CultureInfo.InvariantCulture), float.Parse(netHandDataArr[14 + tempBoneCount], CultureInfo.InvariantCulture));
            _dataManager.UpdatePlayerFile(rtView.ownerIDSelf, _remoteBones[i].transform);
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

    public static Quaternion FromFlippedZQuatf(Quaternion q)
    {
        return new Quaternion() { x = -q.x, y = -q.y, z = q.z, w = q.w };
    }

    public static Vector3 FromFlippedZVector3f(Vector3 v)
    {
        return new Vector3() { x = v.x, y = v.y, z = -v.z };
    }
}

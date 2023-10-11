/************************************************************************************
Copyright : Copyright 2019 (c) Speak Geek (PTY), LTD and its affiliates. All rights reserved.

Developer : Dylan Holshausen

Script Description : Sync Hand Pose Model Data Through Normcore

************************************************************************************/

using Normal.Realtime;

public class handPoseModelSync : RealtimeComponent <handPoseModel>
{
    //Private Variables
    //private handPoseModel _model;

    //Public Variables
    public handSyncImpl handSyncImpl;
    
    private void Start()
    {
        //Reference Our Oculus Hand Script That Gets/Applies Bone Data to the Hands
        if(!handSyncImpl)
            handSyncImpl = GetComponent<handSyncImpl>();
    }

    protected override void OnRealtimeModelReplaced(handPoseModel previousModel, handPoseModel currentModel)
    {
        if (previousModel != null)
        {
            previousModel.skeletonTrackedDataDidChange -= TrackedDataDidChange;
        }

        if (currentModel != null)
        {
            if (currentModel.isFreshModel)
                SetTrackedData("0|");

            UpdateTrackedData();

            currentModel.skeletonTrackedDataDidChange += TrackedDataDidChange;
        }
    }

    /*private handPoseModel model
    {
        set
        {
            if (_model != null)
            {
                _model.skeletonTrackedDataDidChange -= TrackedDataDidChange;
            }

            _model = value;

            if (_model != null)
            {
                if (_model.skeletonTrackedData != null)
                {
                    UpdateTrackedData();

                    _model.skeletonTrackedDataDidChange += TrackedDataDidChange;
                }
            }
        }
    }*/

    private void TrackedDataDidChange(handPoseModel model, string value)
    {
        UpdateTrackedData();
    }

    private void UpdateTrackedData()
    {
        if (model == null)
            return;

        if (model.skeletonTrackedData == "")
            return;

        //Send Received Hand/Bone Data to Update Function in SG Quest Hand Script
        handSyncImpl.updateFromNormCore(model.skeletonTrackedData);
    }

    public void SetTrackedData(string trackedData)
    {
        model.skeletonTrackedData = trackedData;
    }
}

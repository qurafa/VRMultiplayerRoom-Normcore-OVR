using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RealtimeModel]
public partial class skeletonPoseModel
{
    [RealtimeProperty(1, true, true)]
    private string _skeletonTrackedData;
}

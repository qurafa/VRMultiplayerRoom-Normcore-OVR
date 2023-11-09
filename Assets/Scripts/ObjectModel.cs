using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

[RealtimeModel]
public partial class ObjectModel
{
    /// <summary>
    /// Tracking state,
    /// 0 - idle,
    /// 1 - tracking,
    /// 2 - stopped tracking.
    /// </summary>
    [RealtimeProperty(1, true)] private int _trackingState = 0;

    public void UpdateTrackingState(int track)
    {
        trackingState = track;
    }

    public delegate void EventHandler(int track);
    public event EventHandler eventComplete;

    [RealtimeCallback(RealtimeModelEvent.OnDidRead)]
    private void DidRead()
    {
        if (eventComplete != null)
            eventComplete(trackingState);
    }
}

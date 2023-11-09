using Meta.WitAi.Utilities;
using Normal.Realtime;
using OVRTouchSample;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackingTrigger : MonoBehaviour
{
    [SerializeField]
    private ObjectModelImpl obj;

    [SerializeField]
    private bool tracking = false;

    private HashSet<GameObject> colliders;

    private bool stopTracking = false;
    private readonly float touchingLimit = 3;
    private float touchingCount = 0;
    private float releaseLimit = 3;
    private float releaseCount = 0;
    private float stopTrackCount = 0;
    private float stopTrackLimit = 3;

    private string _statusWRTBox = "Outside Box";

    /// <summary>
    /// status of the object wrt to the box.
    /// 0 - outside box,
    /// 1 - entering box,
    /// 2 - inside box.
    /// </summary>
    private float _boxState = 0;

    private void Start()
    {
        colliders = new HashSet<GameObject>();
        obj.UpdateTS(0);//set to idle tracking state
    }

    private void FixedUpdate()
    {
        if (!tracking) return;

        if(touchingCount >= touchingLimit)
        {
            obj.UpdateTS(1);//set to tracking when touching/holding
        }
        else if(releaseCount >= releaseLimit)
        {
            obj.UpdateTS(1);//set to idle when released
        }

        if(stopTrackCount >= stopTrackLimit)
        {
            obj.UpdateTS(2);//set to stop tracking when touches the bottom
            tracking = false;
        }

        if(colliders.Count > 0)
            touchingCount += Time.deltaTime;
        else
            releaseCount += Time.deltaTime;

        if(stopTracking)
            stopTrackCount += Time.deltaTime;
    }

    public string GetStatus()
    {
        return _statusWRTBox;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($".....................{collision.collider.tag}");

        if (collision.collider.CompareTag("GrabCol"))
        {
            colliders?.Add(collision.gameObject);
            releaseCount = 0;
        }
        if(collision.collider.CompareTag("Bottom"))
        {
            stopTracking = true;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.CompareTag("GrabCol"))
        {
            if(!colliders.Contains(collision.collider.gameObject))
                colliders?.Add(collision.collider.gameObject);
        }
        if (collision.collider.CompareTag("Bottom"))
        {
            stopTracking = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        colliders?.Remove(collision.collider.gameObject);
        if (colliders.Count <= 0 && touchingCount >= touchingLimit)
            touchingCount = 0;

        if (collision.collider.CompareTag("Bottom"))
        {
            stopTracking = false;
            stopTrackCount = 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Trigger"))
        {
            if (other.name.Equals("Box")) _statusWRTBox = "Inside Box";
            else _statusWRTBox = $"Entering {other.name}";
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Trigger"))
        {
            if (other.name.Equals("Box")) _statusWRTBox = "Inside Box";
            else _statusWRTBox = $"Entering {other.name}";
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Trigger"))
        {
            _statusWRTBox = "Outside Box";
        }
    }
}
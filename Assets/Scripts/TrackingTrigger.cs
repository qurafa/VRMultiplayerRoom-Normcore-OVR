using Meta.WitAi.Utilities;
using Normal.Realtime;
using Oculus.Interaction;
using OVRTouchSample;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackingTrigger : MonoBehaviour
{
    [SerializeField]
    private ObjectModelImpl obj;

    /// <summary>
    /// Whether to track this object or not
    /// </summary>
    [SerializeField]
    private bool tracking = false;

    private HashSet<GameObject> colliders;

    private bool stopTracking = false;
    private readonly float touchingLimit = 1.5f;
    private float touchingCount = 0;
    private float releaseLimit = 3;
    private float releaseCount = 0;
    private float stopTrackCount = 0;
    private float stopTrackLimit = 3;

    /// <summary>
    /// Status of the object with reference to the box.
    /// Either "Outside Box" or "Inside Box" or "Entering {hole name}"
    /// </summary>
    private string _statusWRTBox = "Outside Box";

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
            obj.UpdateTS(0);//set to idle when released
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

    /// <summary>
    /// Get the status of the object with respect to the box
    /// </summary>
    /// <returns>The status of the object with respect to the box</returns>
    public string GetStatus()
    {
        return _statusWRTBox;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($".....................Tag entered: {collision.collider.tag}, Object Entered: {collision.collider.name}");

        if (collision.collider.CompareTag("GrabCol"))
        {
            colliders?.Add(collision.collider.gameObject);
            releaseCount = 0;
        }

    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.CompareTag("GrabCol"))
        {
            if(!colliders.Contains(collision.collider.gameObject))
                colliders?.Add(collision.collider.gameObject);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("GrabCol"))
        {
            colliders?.Remove(collision.collider.gameObject);
            if (colliders.Count <= 0)
                touchingCount = 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Trigger"))
        {
            if (other.name.Equals("Box")) _statusWRTBox = "Inside Box";
            else _statusWRTBox = $"Entering {other.name}";
        }
        if (other.CompareTag("Bottom"))
        {
            stopTracking = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Trigger"))
        {
            if (other.name.Equals("Box")) _statusWRTBox = "Inside Box";
            else _statusWRTBox = $"Entering {other.name}";
        }
        if (other.CompareTag("Bottom"))
        {
            stopTracking = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Trigger"))
        {
            _statusWRTBox = "Outside Box";
        }
        if (other.CompareTag("Bottom"))
        {
            stopTracking = false;
            stopTrackCount = 0;
        }
    }
}
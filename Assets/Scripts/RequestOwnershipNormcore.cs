using Normal.Realtime;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RealtimeTransform), typeof(RealtimeView))]
public class RequestOwnershipNormcore : MonoBehaviour
{
    private RealtimeView m_RealtimeView;
    private RealtimeTransform m_RealtimeTransform;

    List<GameObject> m_List;

    //add a listener to the selectEntered so it requests ownership when the object is grabbed
    private void OnEnable()
    {
        m_List = new List<GameObject>();
        m_RealtimeView = GetComponent<RealtimeView>();
        m_RealtimeTransform = GetComponent<RealtimeTransform>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "AnimFollow" && (m_RealtimeView.isOwnedRemotelyInHierarchy || m_RealtimeView.isUnownedInHierarchy))
        {
            Debug.Log("Requesting Ownership");
            m_RealtimeView.RequestOwnership();
            m_RealtimeTransform.RequestOwnership();
            Debug.Log("Ownership Request Done");
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        /*if (collision.gameObject.tag == "AnimFollow" && m_RealtimeView.isOwnedLocallyInHierarchy)
        {
            Debug.Log("Clearing Ownership");
            m_RealtimeView.ClearOwnership();
            Debug.Log("Ownership Clear Done");
        }*/
    }
}

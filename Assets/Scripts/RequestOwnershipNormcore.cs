using Normal.Realtime;
using UnityEngine;

[RequireComponent(typeof(RealtimeTransform), typeof(RealtimeView))]
public class RequestOwnershipNormcore : MonoBehaviour
{
    private RealtimeView m_RealtimeView;
    private RealtimeTransform m_RealtimeTransform;

    //add a listener to the selectEntered so it requests ownership when the object is grabbed
    private void OnEnable()
    {
        m_RealtimeView = GetComponent<RealtimeView>();
        m_RealtimeTransform = GetComponent<RealtimeTransform>();
    }

    /*private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "GrabCol")
        {
            m_RealtimeView.RequestOwnership();
            m_RealtimeTransform.RequestOwnership();
        }
    }*/
}

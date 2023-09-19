using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ObjectGrabHelper : MonoBehaviour
{
    private Rigidbody _rb;

    // Start is called before the first frame update
    void Start()
    {
        _rb = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log($"Kinematic on is {_rb.isKinematic}, Gravity on is {_rb.useGravity}");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.gameObject.tag == "GrabCol")
        {
            //_rb.isKinematic = true;
            _rb.useGravity = false;
            Debug.Log($"Kinematic On, Gravity Off");
        }
        
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.gameObject.tag == "GrabCol")
        {
            //_rb.isKinematic = false;
            _rb.useGravity = true;
            Debug.Log("Kinematic Off, Gravity On");
        }
    }
}

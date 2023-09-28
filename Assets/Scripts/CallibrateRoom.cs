using Oculus.Interaction.OVR.Input;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CallibrateRoom : MonoBehaviour
{
    /// <summary>
    /// Room we're calibrating
    /// </summary>
    [SerializeField]
    private GameObject _room;
    /// <summary>
    /// What should be referenced when setting the rotation of the room
    /// </summary>
    [SerializeField]
    private GameObject _rotationReference;
    /// <summary>
    /// Objects to be ignored when callibrating the room
    /// Added so things like object collision does not affect
    /// </summary>
    [SerializeField]
    private List<GameObject> _ignores;

    private Rigidbody _roomRB;

    enum Mode
    {
        Standby,
        CalibratingPos,
        CalibratingRot
    }

    private Mode _mode;
    
    private float direction = 0.0f;
    private readonly float rotFactor = 0.05f;

    Mode mode
    {
        get { return _mode; }
        set
        {
            _mode = value;

            ModeChanged();
        }
    }

    void Start()
    {
        _roomRB = _room.GetComponent<Rigidbody>();
        mode = Mode.Standby;
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.GetActiveController() != OVRInput.Controller.Hands)
        {
            //if the A or X button is pressed
            if (OVRInput.GetUp(OVRInput.Button.One) || OVRInput.GetUp(OVRInput.Button.Three))
            {
                Debug.Log("A or X was pressed");

                switch (mode)
                {
                    case Mode.Standby:
                        mode = Mode.CalibratingPos;
                        break;
                    case Mode.CalibratingPos:
                        mode = Mode.Standby;
                        break;
                    case Mode.CalibratingRot:
                        mode = Mode.Standby;
                        break;
                    default:
                        Debug.Log("CalibrateRoom mode not set");
                        return;
                }
            }
            //if the B or Y button is pressed
            if (OVRInput.GetUp(OVRInput.Button.Two) || OVRInput.GetUp(OVRInput.Button.Four))
            {
                switch (mode)
                {
                    case Mode.Standby:
                        mode = Mode.CalibratingRot;
                        direction = (OVRInput.GetUp(OVRInput.Button.Two)) ? 1 : -1;
                        break;
                    case Mode.CalibratingPos:
                        mode = Mode.Standby;
                        direction = 0;
                        break;
                    case Mode.CalibratingRot:
                        mode = Mode.Standby;
                        direction = 0;
                        break;
                    default:
                        Debug.Log("CalibrateRoom mode not set");
                        return;
                }
            }
        }

        if(mode == Mode.CalibratingRot)
            _roomRB.transform.RotateAround(_rotationReference.transform.position, Vector3.up, rotFactor*direction);
    }

    private void ModeChanged()
    {
        if (mode == Mode.Standby)
        {
            _roomRB.constraints = RigidbodyConstraints.FreezeAll;
            ToggleIgnores(true);
        }
        else if (mode == Mode.CalibratingPos)
        {
            ToggleIgnores(false);
            _roomRB.constraints = RigidbodyConstraints.FreezeRotation;
        }
        else if (mode == Mode.CalibratingRot)
        {
            ToggleIgnores(false);
            //rotate so the forward is parallel to the rotation reference
            
        }
        else
        {
            Debug.Log("CalibrateRoom mode not set");
        }
    }

    private void ToggleIgnores(bool val)
    {
        foreach(GameObject g in _ignores)
        {
            g.SetActive(val);
        }
    }
}

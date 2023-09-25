using Oculus.Interaction.OVR.Input;
using System.Collections;
using System.Collections.Generic;
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
        CalibratingPos
    }

    private Mode _mode;
    
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
        if(OVRInput.GetUp(OVRInput.RawButton.A) || OVRInput.GetUp(OVRInput.RawButton.X))
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
                default:
                    Debug.Log("CalibrateRoom mode not set");
                    return;
            }
        }
    }

    private void ModeChanged()
    {
        if(mode == Mode.Standby)
        {
            _roomRB.constraints = RigidbodyConstraints.FreezeAll;
            ToggleIgnores(true);
        }
        else if(mode == Mode.CalibratingPos)
        {
            _roomRB.constraints = RigidbodyConstraints.FreezeRotation;
            ToggleIgnores(false);
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

using HandPhysicsToolkit.Modules.Avatar;
using Meta.WitAi.Speech;
using Normal.Realtime;
using Oculus.Interaction.OVR.Input;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using Unity.VisualScripting;
using UnityEditor.Search;
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

/*    private List<Transform> _ignores;
    private List<Rigidbody> _ignoresRigidbody;*/
    /// <summary>
    /// The RealtimeView of the Room
    /// </summary>
    [SerializeField]
    private RealtimeView _rtView;
    /// <summary>
    /// The RealtimeTransform of the Room
    /// </summary>
    [SerializeField]
    private RealtimeTransform _rtTransform;

    private Rigidbody _roomRB;
    private List<Transform> _listOfChildren = new List<Transform>();

    enum Mode
    {
        Standby,
        CalibratingPos,
        CalibratingRot
    }
    private Mode _mode;

    [SerializeField]
    private float direction = 0.0f;
    public readonly float rotFactor = 0.05f;

    [SerializeField]
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
        if (!_rotationReference)
            _rotationReference = GameObject.FindWithTag("MainCamera");

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
            ToggleOwnership(false);
        }
        else if (mode == Mode.CalibratingPos)
        {
            ToggleOwnership(true);
            ToggleIgnores(false);
            _roomRB.constraints = RigidbodyConstraints.FreezeRotation;
        }
        else if (mode == Mode.CalibratingRot)
        {
            ToggleOwnership(true);
            ToggleIgnores(false);
            _roomRB.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePosition;
            //rotate so the forward is parallel to the rotation reference
            
        }
        else
        {
            Debug.Log("CalibrateRoom mode not set");
        }
    }

    private void ToggleOwnership(bool val)
    {
        _listOfChildren.Clear();
        GetChildRecursive(_room.transform);
        if (val)
        {
            if (_rtView) _rtView.RequestOwnership();
            if (_rtTransform) _rtTransform.RequestOwnership();
            foreach (Transform t in _listOfChildren)
            {
                if (t.TryGetComponent<RealtimeView>(out RealtimeView rTV))
                    rTV.RequestOwnership();
                if (t.TryGetComponent<RealtimeTransform>(out RealtimeTransform rTT))
                    rTT.RequestOwnership();
            }
        }
        else
        {
            _rtView.ClearOwnership();
            _rtTransform.ClearOwnership();
            foreach (Transform t in _listOfChildren)
            {
                if (t.TryGetComponent<RealtimeView>(out RealtimeView rTV))
                    rTV.ClearOwnership();
                if (t.TryGetComponent<RealtimeTransform>(out RealtimeTransform rTT))
                    rTT.ClearOwnership();
            }
        }
        
    }

    private void GetChildRecursive(Transform obj)
    {
        if (null == obj)
            return;

        foreach (Transform child in obj)
        {
            if (null == child)
                continue;

            if (child != obj)
            {
                _listOfChildren.Add(child);
            }
            GetChildRecursive(child);
        }
    }

    private void ToggleIgnores(bool val)
    {
        if (val)
        {
            Exec();
            //SetLocalTransform();
        }
        else
        {
            //SaveTransform();
            Exec();
        }

        void Exec()
        {
            foreach(GameObject g in _ignores)
            {
                g.SetActive(val);
            }
        }
    }

/*    private void SaveTransform()
    {
        if (_ignoresTransform == null || _ignoresTransform.Count <= 0 || _ignoresTransform.Count < _ignores.Count)
        {
            _ignoresTransform = new List<Transform>();
            for (int i = 0; i < _ignores.Count; i++)
                _ignoresTransform.Add(_ignores[i]);
        }
        else
        {
            for (int i = 0; i < _ignores.Count; i++)
                _ignoresTransform[i] = _ignores[i];
        }
    }

    private void SetTransform()
    {
        if (_ignoresTransform == null || _ignoresTransform.Count < _ignores.Count)
            SaveTransform();

        for (int i = 0; i < _ignores.Count; i++)
            _ignores[i].SetPositionAndRotation(_ignoresTransform[i].position, _ignoresTransform[i].rotation);
    }

    private void SetLocalTransform()
    {
        if (_ignoresTransform == null || _ignoresTransform.Count < _ignores.Count)
            SaveTransform();

        for (int i = 0; i < _ignores.Count; i++)
            _ignores[i].SetLocalPositionAndRotation(_ignoresTransform[i].localPosition, _ignoresTransform[i].localRotation);
    }*/
}

using Normal.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    [SerializeField]
    List<GameObject> _toTrack = new List<GameObject>();
    [SerializeField]
    private bool _canTrack;
    [SerializeField]
    private Realtime _realTime;

    // Start is called before the first frame update
    void Start()
    {
        //create a file for the trial;
        CreateFile();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void CreateFile()
    {
        if (!_canTrack) return;

        foreach (var track in _toTrack)
        {
            
        }
    }
}

using System.IO;
using Normal.Realtime;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;

public class DataManager : MonoBehaviour
{
    [SerializeField]
    List<GameObject> _toTrack = new List<GameObject>();
    [SerializeField]
    private bool _canTrack;
    [SerializeField]
    private Realtime _realTime;
    /// <summary>
    /// How often should data be read to the output files
    /// </summary>
    [SerializeField]
    private float _interval;

    private string _header = "Object, Time, XPos, Ypos, ZPos, XRot, YRot, ZRot\n";
    string path = @"../";
    private int id = 0;
    private bool fileCreated = false;

    FileStream stream = null;
    // Start is called before the first frame update
    void Start()
    {
        //create a file for the trial;
        CreateFile();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateFile();
    }
    
    private void CreateFile()
    {
        if (!_canTrack) return;

        path += $"p{id}_{DateTime.Now.Day}_{DateTime.Now.Month}_{DateTime.Now.Year}.csv";//something to identify the participant

        stream = File.Create(path);
        Debug.Log($"File {path} Exists: {File.Exists(path)} and the Time is {Time.time}");

        File.AppendAllText( path, _header );
    }

    private void UpdateFile()
    {
        if(!_canTrack) return;

        if (!fileCreated) return;

        string update = "";
        foreach(GameObject track in _toTrack)
        {
            update += $"{track.name},{Time.time}," +
                $"{track.transform.position.x},{track.transform.position.y},{track.transform.position.z}," +
                $"{track.transform.eulerAngles.x},{track.transform.eulerAngles.y},{track.transform.eulerAngles.z}\n";
        }

        File.AppendAllText(path, update);
    }

    public void SetID(int id)
    {
        this.id = id;
    }
}

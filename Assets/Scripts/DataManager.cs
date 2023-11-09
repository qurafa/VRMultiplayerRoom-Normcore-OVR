using System.IO;
using Normal.Realtime;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;

public class DataManager : MonoBehaviour
{
    [SerializeField]
    HashSet<GameObject> _toTrack = new HashSet<GameObject>();
    [SerializeField]
    private bool _canTrack;
    [SerializeField]
    private Realtime _realTime;
    /// <summary>
    /// How often should data be read to the output files in seconds
    /// </summary>
    [SerializeField]
    private float _timeInterval;

    private string _objectHeader = "Object,Owner,Time,XPos,Ypos,ZPos,XRot,YRot,ZRot,Status\n";
    private string _playerHeader = "Bone,Time,XPos,YPos,ZPos,XRot,YRot,ZRot\n";
    string filePath;
    private int id = 0;
    private bool fileCreated = false;

    private float timeCounter = 0;

    //list of player IDs and the path to their data file
    private Dictionary<int, string> _playerFile;

    // Start is called before the first frame update
    void Start()
    {
        //create a file for the trial;
        CreateObjectsFile();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(timeCounter >= _timeInterval) {
            UpdateObjectFile();

            timeCounter = 0;
        }

        timeCounter += Time.deltaTime;
    }
    
    private void CreateObjectsFile()
    {
        if (!_canTrack) return;

        filePath += $"{Application.persistentDataPath}/objectsData_{System.DateTime.Now.ToString("yyyy-MM-dd-HH_mm_ss")}.csv";//something to identify the participant

        File.WriteAllText(filePath, _objectHeader);

        //Debug.Log($"File Path is: {filePath}");

        fileCreated = true;
    }

    private void UpdateObjectFile()
    {
        if(!_canTrack) return;

        if (!fileCreated) return;

        if(_toTrack.Count <= 0) return;

        string update = "";
        int ownerID = -1;
        foreach (GameObject track in _toTrack)
        {
            ownerID = track.TryGetComponent<RealtimeView>(out RealtimeView rV)? rV.ownerIDInHierarchy : ownerID;
            string status = (track.TryGetComponent<TrackingTrigger>(out TrackingTrigger tT)) ? tT.GetStatus() : track.name;
            update += $"{track.name},{ownerID},{DateTime.Now.TimeOfDay}," +
                $"{track.transform.position.x},{track.transform.position.y},{track.transform.position.z}," +
                $"{track.transform.eulerAngles.x},{track.transform.eulerAngles.y},{track.transform.eulerAngles.z}," +
                $"{status}\n";
        }
        File.AppendAllText(filePath, update);
    }

    public void SetID(int id)
    {
        this.id = id;
        CreateObjectsFile();
    }

    public void AddObjectTrack(GameObject g)
    {
        _toTrack.Add(g);
    }

    public void RemoveObjectTrack(GameObject g)
    {
        _toTrack.Remove(g);
    }

    //create a file to store skeleton data for each player
    public void CreatePlayerFile(int playerID)
    {
        if (_playerFile == null)
            _playerFile = new Dictionary<int, string>();

        string playerFilePath = $"{Application.persistentDataPath}/p{playerID}_skeletonData_{System.DateTime.Now.ToString("yyyy-MM-dd-HH_mm_ss")}.csv";

        File.WriteAllText(playerFilePath, _playerHeader);
        _playerFile.Add(playerID, playerFilePath);
    }

    public void UpdatePlayerFile(int playerID, Transform bone)
    {
        string update = $"{bone.name},{DateTime.Now.TimeOfDay}," +
            $"{bone.position.x},{bone.position.y},{bone.position.z}," +
            $"{bone.eulerAngles.x},{bone.eulerAngles.y},{bone.eulerAngles.z}\n";

        string path = _playerFile[playerID];
        File.AppendAllText(path, update);
    }
}

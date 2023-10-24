using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using UnityEngine.SceneManagement;

public class AdditiveSceneLoader : MonoBehaviour
{
    [SerializeField]
    private Realtime _realTime;
    [SerializeField]
    private string _roomName;
    [SerializeField]
    private int sceneIndexToLoad;
    [SerializeField]
    private int currentSceneIndex;

    private bool isLoading;

    private MyTransform _playerTransform;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void LoadScene(MyTransform playerTransform)
    {
        if(isLoading) return;

        Debug.Log("Loading Scene....");

        isLoading = true;
        _playerTransform = playerTransform;

        StartCoroutine(LoadSceneAdditive());
    }

    IEnumerator LoadSceneAdditive() {
        _realTime.Disconnect();
        var loadAsync = SceneManager.LoadSceneAsync(sceneIndexToLoad);

        while(!loadAsync.isDone) yield return null;

        Debug.Log("getting realtime helper");
        realtimeHelper helper = FindObjectOfType<realtimeHelper>();

        if (helper) Debug.Log("Helper Name: " + helper.name);
        helper.JoinRoom(_playerTransform);

        /*_Realtime = FindObjectsOfType<Realtime>();

        foreach(var rt in _Realtime)
        {
            if (!rt.connected)
                rt.Connect(_roomName);
        }*/
        isLoading = false;
    }
}

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
    private int currentSceneIndex;

    private bool isLoading;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    /// <summary>
    /// Load the next scene specified by "id"
    /// </summary>
    /// <param name="playerTransform"></param>
    /// <param name="id"></param>
    public void LoadScene(int id)
    {
        if (isLoading) return;
        if (currentSceneIndex == id) return;

        Debug.Log("Loading Scene....");

        isLoading = true;
        StartCoroutine(LoadSceneAdditive(id));
    }

    /// <summary>
    /// Load the next scene placing the "player" with the given MyTransform information
    /// </summary>
    /// <param name="playerTransform"></param>
    /// <param name="id"></param>
    public void LoadScene(MyTransform playerTransform, int id)
    {
        if(isLoading) return;
        if (currentSceneIndex == id) return;

        Debug.Log("Loading Scene....");

        isLoading = true;
        StartCoroutine(LoadSceneAdditive(playerTransform, id));
    }

    /// <summary>
    /// Load the next scene with the given id only
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    IEnumerator LoadSceneAdditive(int id) {
        //get the current Realtime in the scene and disconnect
        _realTime = GetComponent<Realtime>();
        _realTime?.Disconnect();
        var loadAsync = SceneManager.LoadSceneAsync(id);

        while(!loadAsync.isDone) yield return null;        

        //set the new values when we're done loading the scene
        currentSceneIndex = id;
        isLoading = false;
    }

    /// <summary>
    /// Load the next scene with the given id and player transform to set location
    /// using the realtimeHelper
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    IEnumerator LoadSceneAdditive(MyTransform transform, int id)
    {
        //get the current Realtime in the scene and disconnect
        _realTime = GetComponent<Realtime>();
        _realTime?.Disconnect();
        var loadAsync = SceneManager.LoadSceneAsync(id);

        while (!loadAsync.isDone) yield return null;

        Debug.Log("getting realtime helper");
        realtimeHelper helper = FindObjectOfType<realtimeHelper>();

        if (helper) Debug.Log("Helper Name: " + helper.name);
        helper.JoinMainRoom(transform);

        currentSceneIndex = id;
        isLoading = false;
    }
}

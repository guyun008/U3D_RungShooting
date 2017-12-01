using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
public class GameStateManager : MonoBehaviour {
    public string[] scenesToLoad;
    Scene[] scenes;
    List<string> sceneNames = new List<string>();
    // Use this for initialization
    void Start () {
        
        scenes = SceneManager.GetAllScenes();
        foreach (var item in scenes) {
            sceneNames.Add(item.name);
        }

        foreach (var item in scenesToLoad) {
            if (!IsSceneLoaded(item))
                SceneManager.LoadScene(item, LoadSceneMode.Additive);
            else
                Debug.Log(item + " was already loaded so did not load.");
        }
	}

    public bool IsSceneLoaded(string sceneName) {
        return sceneNames.Contains(sceneName);
    }
}

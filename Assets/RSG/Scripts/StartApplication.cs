using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartApplication : MonoBehaviour {
    void Start () {
        StartCoroutine (LoadGame ());
    }

    IEnumerator LoadGame () {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync (1);

        while (!asyncLoad.isDone) {
            yield return null;
        }
    }
}
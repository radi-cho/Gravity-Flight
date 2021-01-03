using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public Controller controller;
    public GameObject pointer;
    public GameObject ignoreOrbitObject;
    public GameObject nextTutorial;
    public SpriteRenderer pointerRenderer;
    public float pausedDelay = 3.5f;
    public int collisionsReaming = 1;
    public bool isTutorialEnabled = true;
    public bool isTutorialActive = false;
    public bool finitePauseDelay = true;

    public void Start()
    {
        pointerRenderer.color = new Color(1, 1, 1, 0);
        if (PlayerPrefs.HasKey("tutorial_complete")) isTutorialEnabled = false;
        if (SceneManager.GetActiveScene().buildIndex - 2 == 1 && !PlayerPrefs.HasKey("tutorial_begin"))
        {
            PlayerPrefs.SetInt("tutorial_begin", 1);
            Firebase.Analytics.FirebaseAnalytics.LogEvent(Firebase.Analytics.FirebaseAnalytics.EventTutorialBegin);
        }
    }

    public void BeginRotation()
    {
        controller.pauseMotion = 1f;
        pointer.SetActive(false);
        isTutorialActive = false;
    }

    public void StopRotation()
    {
        // stop rotation and let the tutorial begin ;)
        controller.pauseMotion = 0;
        pointer.SetActive(true);
        StartCoroutine(FadeImage());
        if (finitePauseDelay) StartCoroutine(Timeout(BeginRotation, pausedDelay));
        isTutorialActive = true;
    }

    IEnumerator Timeout(Action callback, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (isTutorialEnabled) callback();
    }

    IEnumerator FadeImage()
    {
        float pauseStep = Time.deltaTime / 2;
        for (float i = 0; i <= 1; i += pauseStep)
        {
            pointerRenderer.color = new Color(1, 1, 1, i);
            yield return null;
        }

        for (float i = 1; i >= 0; i -= pauseStep)
        {
            pointerRenderer.color = new Color(1, 1, 1, i);
            yield return null;
        }

        if (!finitePauseDelay) StartCoroutine(FadeImage());
    }

    void OnTriggerEnter2D(Collider2D trigger)
    {
        if (isTutorialEnabled && trigger.gameObject.name == "starman")
        {
            collisionsReaming -= 1;
            if (collisionsReaming <= 0) StopRotation();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Starman : MonoBehaviour
{
    public static string currentSprite;
    Sprite StarmanSprite;

    public void Awake()
    {
        currentSprite = PlayerPrefs.GetString("spaceship", PurchaseSpaceship.defaultSprite);
        StarmanSprite = Resources.Load<Sprite>(currentSprite);
        transform.gameObject.GetComponent<SpriteRenderer>().sprite = StarmanSprite;
    }

    void OnTriggerEnter2D(Collider2D trigger)
    {
        if (trigger.gameObject.name == "finish")
        {
            // I love the winning, I can take the losing,
            // but most of all I Love to play.
            Controller.finished = true;
            int levelId = SceneManager.GetActiveScene().buildIndex - 2;
            string levelName = "Level" + levelId.ToString();

            if (!PlayerPrefs.HasKey(levelName))
            {
                PlayerPrefs.SetString(levelName, "Completed");
                PlayerPrefs.SetString("lastCompletedLevel", levelId.ToString());
                MainMenu.lastCompletedLevel = levelId.ToString();
                Firebase.Analytics.FirebaseAnalytics.LogEvent(
                    Firebase.Analytics.FirebaseAnalytics.EventLevelUp,
                    new Firebase.Analytics.Parameter[] {
                        new Firebase.Analytics.Parameter (
                            Firebase.Analytics.FirebaseAnalytics.ParameterLevel, levelId),
                    }
                );

                Sync.SessionSync();
            }
        }
        else if (trigger.gameObject.name == "crystal")
        {
            Controller.crystalSoundPlaying = true;
            MainMenu.UpdateCrystals(MainMenu.crystals + 1, true);
            Destroy(trigger.gameObject);
        }
        else if (trigger.gameObject.name == "Tutorial")
        {
            // Do nothing for now.
        }
        else
        {
            // We all die. The goal isn’t to live forever,
            // the goal is to create something that will.
            Controller.isDying = true;
        }
    }
}
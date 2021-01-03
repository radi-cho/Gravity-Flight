using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crystal : MonoBehaviour
{
    public string id;

    void Start()
    {
        if (PlayerPrefs.HasKey("crystal-" + id))
        {
            transform.gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D trigger)
    {
        if (trigger.gameObject.name == "starman")
        {
            PlayerPrefs.SetString("crystal-" + id, "collected");
        }
    }
}
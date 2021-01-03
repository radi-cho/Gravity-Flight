using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Level : MonoBehaviour
{
    public int id;

    void Awake()
    {
        Button levelButton = transform.gameObject.GetComponent<Button>();
        TextMeshProUGUI levelIdText = levelButton.GetComponentInChildren<TextMeshProUGUI>();

        if (id <= 1 || PlayerPrefs.HasKey("Level" + (id - 1).ToString()))
        {
            levelButton.interactable = true;
            levelIdText.SetText(id.ToString());
        }
        else
        {
            levelIdText.SetText("?");
            levelButton.interactable = false;
        }
    }
}
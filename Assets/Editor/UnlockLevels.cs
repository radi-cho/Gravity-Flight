using System.Collections;
using UnityEditor;
using UnityEngine;

public class PlayerPrefsEditor : EditorWindow
{
    [MenuItem("Edit/Player Prefs")]
    public static void openWindow()
    {

        PlayerPrefsEditor window = (PlayerPrefsEditor)EditorWindow.GetWindow(typeof(PlayerPrefsEditor));
        window.titleContent = new GUIContent("Player Prefs");
        window.Show();

    }

    public int levelCount = 28;
    void OnGUI()
    {
        EditorGUILayout.LabelField("Player Prefs Levels", EditorStyles.boldLabel);
        if (GUILayout.Button("Unlock all levels"))
        {
            for (int i = 0; i < levelCount; i++)
            {
                PlayerPrefs.SetString("Level" + (i + 1), "Completed");
            }

            PlayerPrefs.SetString("lastCompletedLevel", levelCount.ToString());
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    public AudioSource music;

    void Start()
    {
        if (SoundFX.musicOn)
        {
            music.Play();
        }
    }

    public void Play(int level)
    {
        SceneManager.LoadScene(level + 2);
    }
}
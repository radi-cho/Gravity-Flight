using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class SoundFX : MonoBehaviour
{
    public AudioSource audio;
    public static AudioSource backgroundMenuMusic;
    public static bool musicOn = true;
    public static bool soundOn = true;

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);

        soundOn = PlayerPrefs.GetInt("soundOn", 1) == 1;
        if (musicOn != (PlayerPrefs.GetInt("musicOn", 1) == 1))
        {
            SetMusicMode();
        }
    }

    void Update()
    {
        if (!audio.isPlaying)
        {
            if (Controller.removeSFX && transform.gameObject.tag.Contains("in-game"))
            {
                Destroy(transform.gameObject);
                Controller.removeSFX = false;
            }

            if (MainMenu.removeSFX && transform.gameObject.tag.Contains("menu"))
            {
                Destroy(transform.gameObject);
                MainMenu.removeSFX = false;
            }
        }
    }

    public void PlaySound(AudioClip sound)
    {
        if (soundOn)
        {
            audio.clip = sound;
            audio.Play();
        }
    }

    public void SetMusicMode()
    {
        musicOn = !musicOn;

        if (musicOn)
        {
            if (backgroundMenuMusic != null) backgroundMenuMusic.Play();
        }
        else
        {
            if (backgroundMenuMusic != null) backgroundMenuMusic.Stop();
        }

        PlayerPrefs.SetInt("musicOn", musicOn ? 1 : 0);
    }

    public void SetSoundMode()
    {
        soundOn = !soundOn;
        PlayerPrefs.SetInt("soundOn", soundOn ? 1 : 0);
    }
}
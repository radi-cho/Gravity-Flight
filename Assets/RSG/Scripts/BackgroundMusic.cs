using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusic : MonoBehaviour
{
    public AudioSource source;
    public AudioClip[] music;
    private int audioPlaying;

    void Start()
    {
        if (SoundFX.musicOn)
        {
            audioPlaying = Random.Range(0, music.Length);
            source.clip = music[audioPlaying];
            source.Play();
        }
    }

    void FixedUpdate()
    {
        if (SoundFX.musicOn && !source.isPlaying)
        {
            audioPlaying += Random.Range(0, music.Length);
            source.clip = music[audioPlaying];
            source.Play();
        }
    }
}
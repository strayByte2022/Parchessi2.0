using UnityEngine;

public class AudioChange : MonoBehaviour
{

    public AudioSource clip;

    public void Awake()
    {
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();

        foreach (AudioSource audioSource in allAudioSources)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
        clip.Play();

    }

}
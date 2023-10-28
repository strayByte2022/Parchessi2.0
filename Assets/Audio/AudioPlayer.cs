
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    public static AudioPlayer instance;

    public AudioSource bgm;
    public AudioSource bgm1;
    public AudioSource click;
    public AudioSource tab;
    public AudioSource type;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySound(AudioSource clip)
    {
        clip.Play();
    }
}
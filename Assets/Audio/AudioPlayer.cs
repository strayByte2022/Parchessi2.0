
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    public static AudioPlayer instance;

    public AudioSource bgm;
    public AudioSource bgm1;
    public AudioSource click;
    public AudioSource tab;
    public AudioSource type;
    public AudioSource dice_start;
    public AudioSource dice_end;

    public AudioSource card;

    public AudioSource rock;
    public AudioSource leaf;
    public AudioSource flame;
    public AudioSource bubble;



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
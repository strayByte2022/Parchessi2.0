using UnityEngine;
using UnityEngine.Video;


public class BGVideoPlayer : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string targetSortingLayerName = "YourTargetSortingLayer";

    void Start()
    {
        // Ensure you have a reference to the VideoPlayer component in the Inspector.
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        // Change the sorting layer of the Video Player to the target sorting layer.
        videoPlayer.targetCamera.gameObject.GetComponent<Canvas>().sortingLayerName = targetSortingLayerName;
    }
}
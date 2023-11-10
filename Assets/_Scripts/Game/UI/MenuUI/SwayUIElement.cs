using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwayUIElement : MonoBehaviour
{
    public float leanAmount = 10.0f;
    public float leanTime = 1.0f;


    private void Start()
    {

        // Set the initial rotation
        this.transform.localEulerAngles = Vector3.zero;

        // Create a loop that leans the icon back and forth
        LeanTween.rotateLocal(this.gameObject, new Vector3(0, 0, leanAmount), leanTime)
            .setEase(LeanTweenType.easeInOutSine)  // Use easeInOutSine for a smooth back-and-forth motion
            .setLoopPingPong();  // Loop the animation in a ping-pong fashion
    }
}

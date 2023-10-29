using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinUI : MonoBehaviour
{
    public GameObject confetti1;
    public GameObject confetti2;
    public GameObject confetti3;
    public GameObject winText;
    public GameObject bg;

    private void Awake()
    {
        confetti1.transform.localScale = Vector3.zero;
        confetti2.transform.localScale = Vector3.zero;
        confetti3.transform.localScale = Vector3.zero;
        winText.transform.localScale = Vector3.zero;
        bg.LeanAlphaVertex(0f, 0.01f);
    }

    public void BG()
    {
        Debug.Log("hi");
        bg.LeanAlphaVertex(255f, 1f).setEaseInQuint();
    }

    public void WinConfetti()
    {
        confetti1.LeanScale(Vector2.one, .75f).setEaseInExpo();
        confetti2.LeanScale(Vector2.one, .8f).setEaseInExpo();
        confetti3.LeanScale(Vector2.one, .85f).setEaseInExpo().setOnComplete(WinText);
    }

    public void WinText()
    {
        winText.LeanScale(Vector2.one, .35f).setEaseInSine();
        winText.LeanRotateAround(Vector3.one, 4f, 0.3f);
    }
}


using System;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Button))]
public abstract class PhaseManipulateButtonControllerDependency : PlayerControllerCompositionDependency
{
    protected Button Button;

    protected void Awake()
    {
        
        Button = GetComponent<Button>();
        
        
        
        GameManager.Instance.OnGameStart += GameSetUp;
        //Invoke(nameof(DelaySetUp), 0.5f);
    }

    protected void Start()
    {
        DelaySetUp();
    }

    private void DelaySetUp()
    {
        GameManager.Instance.OnGameStart += GameSetUp;
    }
    
    protected abstract void GameSetUp();
}

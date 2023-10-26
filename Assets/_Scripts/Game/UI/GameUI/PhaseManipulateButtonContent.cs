using System;
using _Scripts.UI.GameUI;
using UnityEngine;
using UnityEngine.UI;

public class PhaseManipulateButtonContent : MonoBehaviour
{
    PlayerController _playerController;
    
    [SerializeField] private Button _waitButton;
    [SerializeField] private EndTurnButtonController _endTurnButtonController;
    [SerializeField] private EndRollButtonController _endRollButtonController;
    [SerializeField] private StartRollButtonController _startRollButtonController;
    
    private GameObject _currentActiveButton;

    private void Awake()
    {
        
        GameManager.Instance.OnClientPlayerControllerSetUp += GameSetUp;
        DisableAllExceptWait();
    }

    private void GameSetUp()
    {
        _playerController = GameManager.Instance.ClientOwnerPlayerController;
        _playerController.PlayerTurnController.CurrentPlayerPhase.OnValueChanged += CurrentPlayerPhaseChanged;
    }
    
    private void DisableAllExceptWait()
    {
        _endTurnButtonController.gameObject.SetActive(false);
        _endRollButtonController.gameObject.SetActive(false);
        _startRollButtonController.gameObject.SetActive(false);
        _waitButton.gameObject.SetActive(true);
        
        _currentActiveButton = _waitButton.gameObject;
    }
    
    private void CurrentPlayerPhaseChanged(PlayerTurnController.PlayerPhase previousValue, PlayerTurnController.PlayerPhase newValue)
    {
        switch (newValue)
        {
            case PlayerTurnController.PlayerPhase.Wait:
                SwapButton(_waitButton.gameObject);
                break;
            case PlayerTurnController.PlayerPhase.Preparation:
                SwapButton(_startRollButtonController.gameObject);
                break;
            case PlayerTurnController.PlayerPhase.Roll:
                SwapButton(_endRollButtonController.gameObject);
                break;
            case PlayerTurnController.PlayerPhase.Subsequence:
                SwapButton(_endTurnButtonController.gameObject);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newValue), newValue, null);
        }
    }

    private void SwapButton(GameObject newButton)
    {
        if (_currentActiveButton == null) return;

        _currentActiveButton.SetActive(false);
        _currentActiveButton = newButton;
        _currentActiveButton.SetActive(true);
    }
    
    
    
    
}

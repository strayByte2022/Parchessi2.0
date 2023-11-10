using System;
using _Scripts.UI.GameUI;
using UnityEngine;
using UnityEngine.UI;

public class PhaseManipulateButtonContent : MonoBehaviour
{
    PlayerController _playerController;
    
    [SerializeField] private Button _waitButton;
    [SerializeField] private EndSubsequenceButtonController _endSubsequenceButtonController;
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
        _endSubsequenceButtonController.gameObject.SetActive(false);
        _endRollButtonController.gameObject.SetActive(false);
        _startRollButtonController.gameObject.SetActive(false);
        _waitButton.gameObject.SetActive(true);
        
        _currentActiveButton = _waitButton.gameObject;
    }
    
    private void CurrentPlayerPhaseChanged(PlayerTurnController.PlayerPhase previousValue, PlayerTurnController.PlayerPhase newValue)
    {
        switch (newValue)
        {
            case PlayerTurnController.PlayerPhase.WaitPhase:
                SwapButton(_waitButton.gameObject);
                break;
            case PlayerTurnController.PlayerPhase.PreparationPhase:
                SwapButton(_startRollButtonController.gameObject);
                break;
            case PlayerTurnController.PlayerPhase.RollPhase:
                SwapButton(_endRollButtonController.gameObject);
                break;
            case PlayerTurnController.PlayerPhase.SubsequencePhase:
                SwapButton(_endSubsequenceButtonController.gameObject);
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

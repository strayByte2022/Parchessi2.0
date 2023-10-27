using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndSubsequenceButtonController : PhaseManipulateButtonControllerDependency
{
    protected override void GameSetUp()
    {
        Button.onClick.AddListener(GameManager.Instance.ClientOwnerPlayerController.PlayerTurnController.EndSubsequencePhaseServerRPC);
    }
}

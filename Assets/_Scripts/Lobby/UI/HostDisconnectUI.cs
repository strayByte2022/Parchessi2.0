using System.Collections;
using System.Collections.Generic;
using _Scripts.Managers.Network;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class HostDisconnectUI : MonoBehaviour {


    [FormerlySerializedAs("playAgainButton")] [SerializeField] private Button _playAgainButton;


    private void Awake() {
        _playAgainButton.onClick.AddListener(() => {
            AssetSceneManager.LoadScene(AssetSceneManager.AssetScene.MainMenuScene.ToString());
        });
    }

    private void Start() {
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;

        Hide();
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId) {
        if (clientId == NetworkManager.ServerClientId) {
            // Server is shutting down
            Show();
        }
    }

    private void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

}
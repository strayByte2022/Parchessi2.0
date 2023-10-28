using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour 
{


    [FormerlySerializedAs("mainMenuButton")] [SerializeField] private Button _mainMenuButton;
    [FormerlySerializedAs("readyButton")] [SerializeField] private Button _readyButton;
    [FormerlySerializedAs("lobbyNameText")] [SerializeField] private TextMeshProUGUI _lobbyNameText;
    [FormerlySerializedAs("lobbyCodeText")] [SerializeField] private TextMeshProUGUI _lobbyCodeText;


    private void Awake() {
        _mainMenuButton.onClick.AddListener(() => {
            GameLobbyManager.Instance.LeaveLobby();
            NetworkManager.Singleton.Shutdown();
            AssetSceneManager.LoadScene(AssetSceneManager.AssetScene.MainMenuScene.ToString());
            if (AudioPlayer.instance != null)
            {
                AudioPlayer.instance.PlaySound(AudioPlayer.instance.click);
                AudioPlayer.instance.PlaySound(AudioPlayer.instance.tab);

            }
        });
        _readyButton.onClick.AddListener(() => {
            CharacterSelectReadyManager.Instance.SetPlayerReady();
            if (AudioPlayer.instance != null)
            {
                AudioPlayer.instance.PlaySound(AudioPlayer.instance.click);
                AudioPlayer.instance.PlaySound(AudioPlayer.instance.tab);

            }
        });
    }

    private void Start() {
        Lobby lobby = GameLobbyManager.Instance.GetLobby();

        _lobbyNameText.text = "Lobby Name: " + lobby.Name;
        _lobbyCodeText.text = "Lobby Code: " + lobby.LobbyCode;
    }
}
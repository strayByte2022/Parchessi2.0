using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour 
{
    [FormerlySerializedAs("mainMenuButton")] [SerializeField] private Button _mainMenuButton;
    [FormerlySerializedAs("createLobbyButton")] [SerializeField] private Button _createLobbyButton;
    [FormerlySerializedAs("quickJoinButton")] [SerializeField] private Button _quickJoinButton;
    [FormerlySerializedAs("joinCodeButton")] [SerializeField] private Button _joinCodeButton;
    [FormerlySerializedAs("joinCodeInputField")] [SerializeField] private TMP_InputField _joinCodeInputField;
    [FormerlySerializedAs("playerNameInputField")] [SerializeField] private TMP_InputField _playerNameInputField;
    [FormerlySerializedAs("lobbyCreateUI")] [SerializeField] private LobbyCreateUI _lobbyCreateUI;
    [FormerlySerializedAs("lobbyContainer")] [SerializeField] private Transform _lobbyContainer;
    [FormerlySerializedAs("lobbyTemplate")] [SerializeField] private Transform _lobbyTemplate;


    private void Awake() {
        _mainMenuButton.onClick.AddListener(() => {
            GameLobbyManager.Instance.LeaveLobby();
            AssetSceneManager.LoadScene(AssetSceneManager.AssetScene.MainMenuScene.ToString());
            if (AudioPlayer.instance != null)
            {
                AudioPlayer.instance.PlaySound(AudioPlayer.instance.tab);
            }
        });
        _createLobbyButton.onClick.AddListener(() => {
            _lobbyCreateUI.Show();
            if (AudioPlayer.instance != null)
            {
                AudioPlayer.instance.PlaySound(AudioPlayer.instance.click);
                AudioPlayer.instance.PlaySound(AudioPlayer.instance.tab);

            }
        });
        _quickJoinButton.onClick.AddListener(() => {
            GameLobbyManager.Instance.QuickJoin();
            if (AudioPlayer.instance != null)
            {
                AudioPlayer.instance.PlaySound(AudioPlayer.instance.click);
            }
        });
        _joinCodeButton.onClick.AddListener(() => {
            GameLobbyManager.Instance.JoinWithCode(_joinCodeInputField.text);
            if (AudioPlayer.instance != null)
            {
                AudioPlayer.instance.PlaySound(AudioPlayer.instance.click);
            }
        });

        _lobbyTemplate.gameObject.SetActive(false);
    }

    private void Start() {
        _playerNameInputField.text = GameMultiplayerManager.Instance.GetPlayerName();
        _playerNameInputField.onValueChanged.AddListener((string newText) => {
            GameMultiplayerManager.Instance.SetPlayerName(newText);
            if (AudioPlayer.instance != null)
            {
                AudioPlayer.instance.PlaySound(AudioPlayer.instance.type);
            }
        });

        GameLobbyManager.Instance.OnLobbyListChanged += KitchenGameLobby_OnLobbyListChanged;
        UpdateLobbyList(new List<Lobby>());
    }

    private void KitchenGameLobby_OnLobbyListChanged(object sender, GameLobbyManager.OnLobbyListChangedEventArgs e) {
        UpdateLobbyList(e.LobbyList);
    }

    private void UpdateLobbyList(List<Lobby> lobbyList) {
        foreach (Transform child in _lobbyContainer) {
            if (child == _lobbyTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach (Lobby lobby in lobbyList) {
            Transform lobbyTransform = Instantiate(_lobbyTemplate, _lobbyContainer);
            lobbyTransform.gameObject.SetActive(true);
            lobbyTransform.GetComponent<LobbyListSingleUI>().SetLobby(lobby);
        }
    }

    private void OnDestroy() {
        GameLobbyManager.Instance.OnLobbyListChanged -= KitchenGameLobby_OnLobbyListChanged;
    }

}
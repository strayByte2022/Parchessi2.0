using System.Collections;
using System.Collections.Generic;
using _Scripts.NetworkContainter;
using _Scripts.Scriptable_Objects;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CharacterSelectPlayer : MonoBehaviour 
{
    
    [FormerlySerializedAs("playerIndex")] [SerializeField] private int _playerIndex;
    [FormerlySerializedAs("readyGameObject")] [SerializeField] private GameObject _readyGameObject;
    
    [FormerlySerializedAs("kickButton")] [SerializeField] private Button _kickButton;
    [FormerlySerializedAs("playerNameText")] [SerializeField] private TextMeshPro _playerNameText;

    [FormerlySerializedAs("deckId")] [SerializeField] private ChampionDescription _championDescription;

    [SerializeField] private SpriteRenderer _championSpriteRenderer;

    private void Awake() 
    {
        _kickButton.onClick.AddListener(() => {
            PlayerContainer playerData = GameMultiplayerManager.Instance.GetPlayerContainerFromPlayerIndex(_playerIndex);
            GameLobbyManager.Instance.KickPlayer(playerData.PlayerID.ToString());
            GameMultiplayerManager.Instance.KickPlayer(playerData.ClientID);
        });
    }

    private void Start() 
    {
        GameMultiplayerManager.Instance.OnPlayerContainerNetworkListChanged += KitchenGameMultiplayer_OnPlayerContainerNetworkListChanged;
        CharacterSelectReadyManager.Instance.OnReadyChanged += CharacterSelectReady_OnReadyChanged;

        _kickButton.gameObject.SetActive(NetworkManager.Singleton.IsServer);

        UpdatePlayer();
    }

    private void CharacterSelectReady_OnReadyChanged(object sender, System.EventArgs e) {
        UpdatePlayer();
    }

    private void KitchenGameMultiplayer_OnPlayerContainerNetworkListChanged(object sender, System.EventArgs e) {
        UpdatePlayer();
    }

    private void UpdatePlayer() {
        if (GameMultiplayerManager.Instance.IsPlayerIndexConnected(_playerIndex)) 
        {
            Show();

            PlayerContainer playerData = GameMultiplayerManager.Instance.GetPlayerContainerFromPlayerIndex(_playerIndex);

            _readyGameObject.SetActive(CharacterSelectReadyManager.Instance.IsPlayerReady(playerData.ClientID));

            _playerNameText.text = playerData.PlayerName.ToString();
            
            LoadChampionDescription( playerData.ChampionID);
            
        } 
        else 
        {
            Hide();
        }
    }
    
    private void LoadChampionDescription(int championId)
    {
        _championDescription = GameMultiplayerManager.Instance.GetPlayerChampion(championId);
        if (_championDescription == null) return;
        _championSpriteRenderer.sprite = _championDescription.ChampionSprite;
    }

    private void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

    private void OnDestroy() {
        if (GameMultiplayerManager.Instance != null) GameMultiplayerManager.Instance.OnPlayerContainerNetworkListChanged -= KitchenGameMultiplayer_OnPlayerContainerNetworkListChanged;
    }


}
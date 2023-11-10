using System.Collections;
using System.Collections.Generic;
using _Scripts.Scriptable_Objects;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CharacterColorSelectSingleUI : MonoBehaviour 
{
    
    [SerializeField] private int _championIndex;
    [FormerlySerializedAs("image")] [SerializeField] private Image _image;
    [FormerlySerializedAs("selectedGameObject")] [SerializeField] private GameObject _selectedGameObject;

    private ChampionDescription _championDescription;
    
    
    private void Awake() {
        GetComponent<Button>().onClick.AddListener(() => {
            var playerChampion = GameMultiplayerManager.Instance.GetPlayerChampion(_championIndex);
            GameMultiplayerManager.Instance.ChangePlayerChampion(playerChampion.ChampionID);
        });
    }

    private void Start() {
        GameMultiplayerManager.Instance.OnPlayerContainerNetworkListChanged += KitchenGameMultiplayer_OnPlayerDataNetworkListChanged;
        
        LoadChampionDescription();
        UpdateIsSelected();
    }

    private void LoadChampionDescription()
    {
        _championDescription = GameMultiplayerManager.Instance.GetPlayerChampion(_championIndex);
        if (_championDescription == null) return;
        _image.sprite = _championDescription.ChampionSprite;
    }
    
    private void KitchenGameMultiplayer_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e) {
        UpdateIsSelected();
    }

    private void UpdateIsSelected() {
        if (GameMultiplayerManager.Instance.GetPlayerContainer().ChampionID == _championIndex) {
            _selectedGameObject.SetActive(true);
        } else {
            _selectedGameObject.SetActive(false);
        }
    }

    private void OnDestroy() {
        if (GameMultiplayerManager.Instance != null) GameMultiplayerManager.Instance.OnPlayerContainerNetworkListChanged -= KitchenGameMultiplayer_OnPlayerDataNetworkListChanged;
    }
}
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LobbyCreateUI : MonoBehaviour
{
    [FormerlySerializedAs("closeButton")] [SerializeField] private Button _closeButton;
    [FormerlySerializedAs("createPublicButton")] [SerializeField] private Button _createPublicButton;
    [FormerlySerializedAs("createPrivateButton")] [SerializeField] private Button _createPrivateButton;
    [FormerlySerializedAs("lobbyNameInputField")] [SerializeField] private TMP_InputField _lobbyNameInputField;

    private void Awake() {
        _createPublicButton.onClick.AddListener(() => {
            GameLobbyManager.Instance.CreateLobby(_lobbyNameInputField.text, false);
        });
        _createPrivateButton.onClick.AddListener(() => {
            GameLobbyManager.Instance.CreateLobby(_lobbyNameInputField.text, true);
        });
        _closeButton.onClick.AddListener(() => {
            Hide();
        });
    }

    private void Start() {
        Hide();
        gameObject.transform.localScale = new Vector3(0, 0, 0);

    }

    public void Show() {
        gameObject.SetActive(true);
        _createPublicButton.Select();
        gameObject.LeanScale(Vector2.one, .5f).setEaseInBack();

    }

    private void Hide() {
        gameObject.LeanScale(Vector2.zero, .3f).setEaseInOutBack().setOnComplete(Deactivate);
    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
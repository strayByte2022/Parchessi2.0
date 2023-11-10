using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerDataBehavior : MonoBehaviour
{
    [SerializeField]
    PlayerData m_PlayerData;

    public PlayerData PlayerData => m_PlayerData;

    public string Name => m_PlayerData.Name;
    public int Record => m_PlayerData.Record;
    
    public UnityEvent OnPlayerUpdated = new UnityEvent();

    public void UpdatePlayer(PlayerData playerData)
    {
        if(!playerData.Equals(m_PlayerData))
        {
            m_PlayerData = playerData;
            OnPlayerUpdated.Invoke();
        }
    }
}

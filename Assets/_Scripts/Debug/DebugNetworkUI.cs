using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class DebugNetworkUI : MonoBehaviour
{

    [SerializeField] Button _server, _host, _client;
    // Start is called before the first frame update
    void Awake()
    {
        _server.onClick.AddListener( () =>
        {
            NetworkManager.Singleton.StartServer();
            Debug.Log("Start Server");
            DestroyThis();
        });
        
        _host.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            
            Debug.Log("Start Host");
            DestroyThis();
        });
        
        _client.onClick.AddListener( () =>
        {
            NetworkManager.Singleton.StartClient();
            
            Debug.Log("Start Client");
            DestroyThis();
        });
    }

    void DestroyThis()
    {
        Destroy(this.gameObject);
    }
    
}

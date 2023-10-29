using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class LobbyGroupUI : MonoBehaviour
{

    [FormerlySerializedAs("lobbyGroup")] public GameObject LobbyGroup;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Awake()
    {
        LobbyGroup.transform.localScale = new Vector3(0,0,0);
        LobbyGroup.LeanScale(Vector2.one, 2.5f).setEaseInOutBounce();
    }
}

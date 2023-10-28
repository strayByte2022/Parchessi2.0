using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugScript : MonoBehaviour
{
    // An instance of the ScriptableObject defined above.
    public PawnDescription spawnValues;
    private void Start()
    {
        Instantiate(spawnValues.GetMapPawnPrefab());
    }
}

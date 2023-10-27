using System.Collections;
using System.Collections.Generic;
using _Scripts.Player.Pawn;
using QFSW.QC;
using Shun_Unity_Editor;
using UnityEngine;

public class MapCell : MonoBehaviour
{
    private readonly List<MapPawn> _stayingPlayerPawns = new(); 
    
    [SerializeField] private List<Transform> _mapSpotTransforms;    
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object being dragged entered this cell's collider
        if (other.CompareTag("DraggableObject"))
        {
            // Perform teleport logic or other actions
            TeleportObject(other.transform);
        }
    }

    void TeleportObject(Transform objToTeleport)
    {
        // Implement logic to teleport the object to this cell
        objToTeleport.position = transform.position;
    }

    public List<MapPawn> GetAllPawn()
    {
        return _stayingPlayerPawns;
    }

    public bool CheckEnterable()
    {
        return _stayingPlayerPawns.Count < _mapSpotTransforms.Count;
    }
    
    public void EnterPawn(MapPawn mapPawn)
    {
        if (_stayingPlayerPawns.Count >= _mapSpotTransforms.Count)
        {
            Debug.LogError("MapCell is full");
            return;
        }

        _stayingPlayerPawns.Add( mapPawn );
    }

    public void RemovePawn(MapPawn mapPawn)
    {
        if(_stayingPlayerPawns.Contains(mapPawn)) _stayingPlayerPawns.Remove(mapPawn);
    }

    public Transform GetEmptySpot()
    {
        return _stayingPlayerPawns.Count < _mapSpotTransforms.Count ? _mapSpotTransforms[_stayingPlayerPawns.Count] : null;
    }
}

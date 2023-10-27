using _Scripts.NetworkContainter;
using _Scripts.Player.Pawn;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "PawnDescription", menuName = "ScriptableObjects/PawnDescription", order = 1)]
public class PawnDescription : ScriptableObject
{
    public int PawnID;
    public Sprite PawnSprite;
    public int PawnMaxHealth;
    public int PawnAttackDamage;
    public int PawnMovementSpeed;
    
    public string AxieHex;

    [SerializeField] private MapPawn _mapPawnPrefab;

    public MapPawn GetMapPawnPrefab()
    {
        return _mapPawnPrefab;
    }

    public PawnContainer GetPawnContainer()
    {
        return new PawnContainer
        {
            PawnID = PawnID,
            
            PawnStatContainer = new PawnStatContainer()
            {
                CurrentHealth = PawnMaxHealth,
                MaxHealth = PawnMaxHealth,
                AttackDamage = PawnAttackDamage,
                MovementSpeed = PawnMovementSpeed
            }
            
        };
    }
}
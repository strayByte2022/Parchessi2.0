using UnityEngine;

namespace _Scripts.Scriptable_Objects
{
    [CreateAssetMenu(fileName = "DeckDescription", menuName = "ScriptableObjects/DeckDescription", order = 1)]
    public class DeckDescription : ScriptableObject
    {
        public int DeckID;
        public CardDescription[] CardDescriptions;
        public PawnCardDescription[] PawnCardDescriptions;
        
    }
}
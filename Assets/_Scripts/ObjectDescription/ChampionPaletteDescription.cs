using UnityEngine;

namespace _Scripts.Scriptable_Objects
{
    [CreateAssetMenu(fileName = "ChampionPaletteDescription", menuName = "ScriptableObjects/ChampionPaletteDescription", order = 1)]
    public class ChampionPaletteDescription : ScriptableObject
    {
        public Color PrimaryColor = Color.white;
        public Color OnPrimaryColor = Color.black;
        public Color SecondaryColor = Color.gray;
        public Color OnSecondaryColor = Color.black;
        public Color PrimaryOutlineColor = Color.black;
        
    }
}
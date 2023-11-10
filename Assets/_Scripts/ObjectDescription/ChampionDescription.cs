﻿using UnityEngine;

namespace _Scripts.Scriptable_Objects
{
    [CreateAssetMenu(fileName = "ChampionDescription", menuName = "ScriptableObjects/ChampionDescription", order = 1)]
    public class ChampionDescription : ScriptableObject
    {
        public int ChampionID;
        public string ChampionName;
        public Sprite ChampionSprite;
        
        public CardPaletteDescription ChampionPaletteDescription;
        public DeckDescription DeckDescription;
    }
}

using System;
using _Scripts.Scriptable_Objects;
using Shapes;
using UnityEngine;

public class HandDiceRegionVisual : MonoBehaviour
{
    [SerializeField] ChampionPaletteDescription _championPaletteDescription;
    
    [SerializeField] ShapeRenderer _outline;
    [SerializeField] ShapeRenderer _background;


    private void Start()
    {
        LoadVisual();
    }

    public void LoadVisual()
    {
        if (_championPaletteDescription == null)
        {
            return;
        }
        
        _outline.Color = _championPaletteDescription.PrimaryOutlineColor;
        _background.Color = _championPaletteDescription.PrimaryColor;
        
    }
    
    
}

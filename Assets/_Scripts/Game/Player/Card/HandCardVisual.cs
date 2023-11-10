using System;
using _Scripts.Scriptable_Objects;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

[RequireComponent(typeof(HandCard))]
public class HandCardVisual : MonoBehaviour
{
    protected HandCard HandCard;
    protected CardDescription CardDescription;

    [SerializeField] protected TMP_Text _cardName;
    
    [Tooltip("Description template with placeholders for variables, e.g., 'Do {0} damage'")]
    [SerializeField] protected TMP_Text _cardEffectDescription;
    [SerializeField] protected SpriteRenderer _cardImage;
    [SerializeField] protected TMP_Text _cardCost;

    [SerializeField] protected SpriteRenderer _cardBorderSprite;
    [SerializeField] protected SpriteRenderer _cardEffectBoxSprite;
    [SerializeField] protected SpriteRenderer _cardBannerBoxSprite;
    [SerializeField] protected SpriteRenderer _cardImageBoxSprite;
    
    [SerializeField] protected SortingGroup _frontSortingGroup, _backSortingGroup;

    protected virtual void Awake()
    {
        HandCard = GetComponent<HandCard>();
        HandCard.OnInitialize += InitializeVisual;
    }

    private void InitializeVisual()
    {
        CardDescription = HandCard.CardDescription;
        LoadCard();
        LoadCardColor();
    }

    protected virtual void LoadCard()
    {
        if(CardDescription == null) return;
    
        _cardName.text = CardDescription.CardName;
        _cardImage.sprite = CardDescription.CardSprite;
        _cardCost.text = CardDescription.CardCost.ToString();
        
        object[] intValueObjects = new object[CardDescription.CardEffectIntVariables.Length];
        for (int i = 0; i < CardDescription.CardEffectIntVariables.Length; i++)
        {
            intValueObjects[i] = CardDescription.CardEffectIntVariables[i];
        }
        
        _cardEffectDescription.text = string.Format(CardDescription.CardEffectDescription, args: intValueObjects);

    }
    
    protected virtual void LoadCardColor()
    {
        if(CardDescription == null || CardDescription.CardPaletteDescription == null) return;
        CardPaletteDescription cardPaletteDescription = CardDescription.CardPaletteDescription;
        
        _cardBorderSprite.color = cardPaletteDescription.CardBorderColor;
        _cardEffectBoxSprite.color =cardPaletteDescription.CardEffectBoxColor;
        _cardBannerBoxSprite.color = cardPaletteDescription.CardBannerBoxColor;
        _cardImageBoxSprite.color = cardPaletteDescription.CardImageBoxColor;
        
    }
    
}

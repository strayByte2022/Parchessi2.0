using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts.Managers.Game;
using _Scripts.NetworkContainter;
using _Scripts.Player.Card;
using _Scripts.Simulation;
using UnityEngine;

public class PlayerCardHand : PlayerControllerCompositionDependency
{
    [SerializeField] private int _maxCards = 5;
    private readonly Dictionary<int, HandCard> _containerIndexToHandCardDictionary = new Dictionary<int, HandCard>();
    private HandCardRegion _handCardRegion;

    public override void Initialize(PlayerController playerController)
    {
        base.Initialize(playerController);
        _handCardRegion = GetComponent<HandCardRegion>();
    }
    
    public HandCard GetHandCard(int cardContainerIndex)
    {
        _containerIndexToHandCardDictionary.TryGetValue(cardContainerIndex, out var handCard);
        return handCard;
    }

    public void AddCardToHand(CardContainer cardContainer, int cardContainerIndex)
    {
        if (_maxCards <= _containerIndexToHandCardDictionary.Count) return;
        
        var handCard = CreateCardHand(cardContainer, cardContainerIndex);
        
        _containerIndexToHandCardDictionary.Add(cardContainerIndex, handCard);
        _handCardRegion.TryAddCard(handCard.GetComponent<HandCardDragAndTargeter>());
        if (AudioPlayer.instance != null)
        {
            AudioPlayer.instance.PlaySound(AudioPlayer.instance.card);
        }

    }

    public void FailAddCardToHand(CardContainer cardContainer)
    {
        var handCard = CreateCardHand(cardContainer, -1);

        SimulationManager.Instance.AddSimulationPackage(handCard.Discard());
    }

    private HandCard CreateCardHand(CardContainer cardContainer, int cardContainerIndex)
    {
        switch (cardContainer.CardType)
        {
            case CardType.Action:
                var cardDescription = GameResourceManager.Instance.GetCardDescription(cardContainer.CardID);
                var handCard = Instantiate(cardDescription.GetHandCardPrefab(), MapManager.Instance.GetDeck().transform.position, Quaternion.identity);
                handCard.Initialize(this, cardDescription, cardContainerIndex, PlayerController.OwnerClientId, IsOwner);
                return handCard;
            
            case CardType.Pawn:
                var pawnCardDescription = GameResourceManager.Instance.GetPawnCardDescription(cardContainer.CardID);
                var pawnHandCard = Instantiate(pawnCardDescription.GetPawnHandCardPrefab(), MapManager.Instance.GetDeck().transform.position, Quaternion.identity);
                pawnHandCard.Initialize(this, pawnCardDescription, cardContainerIndex, PlayerController.OwnerClientId, IsOwner);
                return pawnHandCard;
            
            default:
                return null;
        }
        
    }


    public void PlayCard(HandCard handCard)
    {
        if (IsOwner)
        {
            PlayerController.PlayerResourceController.RemoveCardFromHandServerRPC(handCard.ContainerIndex);
        }
        else
        {
            _handCardRegion.RemoveCard(handCard.GetComponent<HandCardDragAndTargeter>());
        }
        
        _containerIndexToHandCardDictionary.Remove(handCard.ContainerIndex);
    }
    
}
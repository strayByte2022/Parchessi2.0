using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts.Player;
using _Scripts.Player.Pawn;
using _Scripts.Scriptable_Objects;
using AxieMixer.Unity;
using UnityEngine;
using UnityUtilities;


public class GameResourceManager : PersistentSingletonMonoBehaviour<GameResourceManager>
{
    public PlayerController PlayerControllerPrefab;
    
    public PlayerCardHand PlayerCardHandPrefab;
    public PlayerDiceHand PlayerDiceHandPrefab;

    private readonly Dictionary<int, CardDescription> _cardDescriptionsDictionary = new();
    private readonly Dictionary<int, DiceDescription> _diceDescriptionsDictionary = new();
    private readonly Dictionary<int, PawnDescription> _pawnDescriptionsDictionary = new();
    private readonly Dictionary<int, PawnCardDescription> _pawnCardDescriptionsDictionary = new();
    private readonly Dictionary<int, DeckDescription> _deckDescriptionsDictionary = new(); 
    
    const string CARD_DESCRIPTIONS_PATH = "CardDescriptions";
    const string DICE_DESCRIPTIONS_PATH = "DiceDescriptions";
    const string PAWN_DESCRIPTIONS_PATH = "PawnDescriptions";
    const string PAWN_CARD_DESCRIPTIONS_PATH = "PawnCardDescriptions";
    const string DECK_DESCRIPTIONS_PATH = "DeckDescriptions";
    
    
    protected override void Awake()
    {
        base.Awake();
        
        LoadCardDescriptions();
        LoadDiceDescriptions();
        LoadPawnDescriptions();
        LoadPawnCardDescriptions();
        LoadDeckDescriptions();
        
    }

    private void Start()
    {
        InitMixer();
    }

    private void InitMixer()
    {
        
        if (Mixer.Builder == null)
        {
            Mixer.Init();
            Debug.Log("Mixer.Init()");
        }
        if (Mixer.Builder == null)
        {
            Debug.LogError("Mixer.Builder is null");
        }
    }
    
    private void LoadCardDescriptions()
    {
        CardDescription[] cardDescriptions = Resources.LoadAll<CardDescription>(CARD_DESCRIPTIONS_PATH);
        foreach (CardDescription cardDescription in cardDescriptions)
        {
            _cardDescriptionsDictionary[cardDescription.CardID] = cardDescription;
        }
    }

    public CardDescription GetCardDescription(int cardID)
    {
        if (_cardDescriptionsDictionary.TryGetValue(cardID, out CardDescription cardDescription))
        {
            return cardDescription;
        }

        Debug.LogWarning("CardDescription not found for CardID: " + cardID);
        return null;
    }

    private void LoadDiceDescriptions()
    {
        DiceDescription[] diceDescriptions = Resources.LoadAll<DiceDescription>(DICE_DESCRIPTIONS_PATH);
        foreach (DiceDescription diceDescription in diceDescriptions)
        {
            _diceDescriptionsDictionary[diceDescription.DiceID] = diceDescription;
        }
    }

    public DiceDescription GetDiceDescription(int diceID)
    {
        if (_diceDescriptionsDictionary.TryGetValue(diceID, out DiceDescription diceDescription))
        {
            return diceDescription;
        }

        Debug.LogWarning("DiceDescription not found for DiceID: " + diceID);
        return null;
    }

    private void LoadPawnDescriptions()
    {
        PawnDescription[] pawnDescriptions = Resources.LoadAll<PawnDescription>(PAWN_DESCRIPTIONS_PATH);
        foreach (PawnDescription pawnDescription in pawnDescriptions)
        {
            _pawnDescriptionsDictionary[pawnDescription.PawnID] = pawnDescription;
        }
    }

    public PawnDescription GetPawnDescription(int pawnID)
    {
        if (_pawnDescriptionsDictionary.TryGetValue(pawnID, out PawnDescription pawnDescription))
        {
            return pawnDescription;
        }

        Debug.LogWarning("PawnDescription not found for PawnID: " + pawnID);
        return null;
    }
    
    private void LoadPawnCardDescriptions()
    {
        PawnCardDescription[] pawnCardDescriptions = Resources.LoadAll<PawnCardDescription>(PAWN_CARD_DESCRIPTIONS_PATH);
        foreach (PawnCardDescription pawnCardDescription in pawnCardDescriptions)
        {
            _pawnCardDescriptionsDictionary[pawnCardDescription.CardID] = pawnCardDescription;
        }
    }
    
    public PawnCardDescription GetPawnCardDescription(int pawnCardID)
    {
        if (_pawnCardDescriptionsDictionary.TryGetValue(pawnCardID, out PawnCardDescription pawnCardDescription))
        {
            return pawnCardDescription;
        }

        Debug.LogWarning("PawnCardDescription not found for PawnCardID: " + pawnCardID);
        return null;
    }
    
    private void LoadDeckDescriptions()
    {
        DeckDescription[] deckDescriptions = Resources.LoadAll<DeckDescription>(DECK_DESCRIPTIONS_PATH);
        foreach (DeckDescription deckDescription in deckDescriptions)
        {
            _deckDescriptionsDictionary[deckDescription.DeckID] = deckDescription;
        }
    }
    
    public DeckDescription GetDeckDescription(int deckID)
    {
        if (_deckDescriptionsDictionary.TryGetValue(deckID, out DeckDescription deckDescription))
        {
            return deckDescription;
        }

        Debug.LogWarning("DeckDescription not found for DeckID: " + deckID);
        return null;
    }
}

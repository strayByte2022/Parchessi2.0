
using System;
using System.Threading.Tasks;
using _Scripts.Managers.Game;
using _Scripts.Player.Card;
using _Scripts.Player.Dice;
using _Scripts.Player.Pawn;
using _Scripts.Scriptable_Objects;
using _Scripts.Simulation;
using DG.Tweening;
using Shun_Card_System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HandCard : PlayerEntity, ITargeter
{
    public bool IsOwner { get; protected set; }
    
    [Header("Hand Card")]
    protected PlayerCardHand PlayerCardHand;
    public CardDescription CardDescription { get; protected set; }
    
    [SerializeField] protected HandCardFace HandCardFace;
    public Action OnInitialize { get; set;}
    
    [Header("Animations ")]
    [SerializeField] protected float MoveToMiddleDuration = 0.5f;
    [SerializeField] protected Ease MoveToMiddleEase = Ease.OutCubic;

    
    public void Initialize(PlayerCardHand playerCardHand, CardDescription cardDescription, int containerIndex, ulong ownerClientID, bool isOwner)
    {
        IsOwner = isOwner;
        PlayerCardHand = playerCardHand;
        Initialize(containerIndex, ownerClientID);
        InitializeCardDescription(cardDescription);
        InitializeCardFace();
        
    }

    protected virtual void InitializeCardDescription(CardDescription cardDescription)
    {
        CardDescription = cardDescription;
    }

    protected virtual void InitializeCardFace()
    {
        HandCardFace = GetComponent<HandCardFace>();
        HandCardFace.SetCardFace(HandCardFace.CardFaceType.Back, true);
        if (IsOwner)
        {
            HandCardFace.SetCardFace(HandCardFace.CardFaceType.Front, false);   
        }
    }
    
    private void Start()
    {
        OnInitialize?.Invoke();
        OnInitialize = null;
    }

    public virtual bool CheckTargeteeValid(ITargetee targetee)
    {
        if (targetee.TargetType == TargetType.Pawn || targetee.TargetType == TargetType.Empty)
            return true;
        else return false;
    }
    
    
    
    public virtual SimulationPackage ExecuteTargeter<TTargetee>(TTargetee targetee) where TTargetee : ITargetee
    {
        
        var package = new SimulationPackage();

        package.AddToPackage(HandCardFace.SetCardFace(HandCardFace.CardFaceType.Front));
            
        
        if (targetee is MapPawn playerPawn)
        {
            
            package.AddToPackage(() =>
            {
                // Inherit this class and write Card effect
                Debug.Log(name + " Card drag to Pawn " + playerPawn.name);
                PlayerCardHand.PlayCard(this);

                Destroy();
                
            });
        }
        else if (targetee is PlayerEmptyTarget playerEmptyTarget)
        {
            package.AddToPackage(() =>
            {
            
                // Inherit this class and write Card effect
                Debug.Log(name + " Card drag to Empty ");
                PlayerCardHand.PlayCard(this);

                Destroy();
                
            });
        }
        
        
        return package;
    }


    public virtual SimulationPackage Discard()
    {
        var package = new SimulationPackage();
        package.AddToPackage(() =>
        {
            Debug.Log("Discard Card");
            Destroy();
        });
        return package;
    }

    protected virtual void Destroy()
    {
        if (TryGetComponent<BaseDraggableObject>(out var baseDraggableObject))
            baseDraggableObject.Destroy();
        Destroy(gameObject);
    }

    protected virtual Tween MoveToMiddleScreen()
    {
        return transform
            .DOMove(MapManager.Instance.GetEmptyTarget().GetMonoBehavior().transform.position, MoveToMiddleDuration)
            .SetEase(MoveToMiddleEase);
    }
    
}

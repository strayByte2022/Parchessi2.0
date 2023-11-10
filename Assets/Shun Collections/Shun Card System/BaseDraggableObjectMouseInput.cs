using System.Collections.Generic;
using Shun_Card_System;
using Shun_Utility;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Shun_Card_System
{
    public class BaseDraggableObjectMouseInput 
    {
        protected Vector3 MouseWorldPosition;
        protected RaycastHit2D[] MouseCastHits;
    
        [Header("Hover Objects")]
        protected List<IMouseHoverable> LastHoverMouseInteractableGameObjects = new();
        public bool IsHoveringCard => LastHoverMouseInteractableGameObjects.Count != 0;

        [Header("Drag Objects")]
        protected Vector3 CardOffset;
        protected BaseDraggableObject DraggingObject;
        protected BaseDraggableObjectRegion LastDraggableObjectRegion;
        protected BaseDraggableObjectHolder LastDraggableObjectHolder;
        protected BaseCardButton LastCardButton;

        public bool IsDraggingCard
        {
            get;
            private set;
        }

    
        public virtual void UpdateMouseInput()
        {
            UpdateMousePosition();
            CastMouse();
            if(!IsDraggingCard) UpdateHoverObject();
            
            if (Input.GetMouseButtonUp(0))
            {
                EndDrag();
            }
            
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                StartDragCard();
            }

            if (Input.GetMouseButton(0))
            {
                DragObject();
            }

        }

        #region CAST

        protected virtual void UpdateMousePosition()
        {
            Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            MouseWorldPosition = new Vector3(worldMousePosition.x, worldMousePosition.y, 0);
        }

        protected virtual void CastMouse()
        {
            MouseCastHits = Physics2D.RaycastAll(MouseWorldPosition, Vector2.zero);
        }

        #endregion
    

        #region HOVER

        protected virtual void UpdateHoverObject()
        {
            var hoveringMouseInteractableGameObject = FindAllIMouseInteractableInMouseCast();

            var endHoverInteractableGameObjects = SetOperations.SetDifference(LastHoverMouseInteractableGameObjects, hoveringMouseInteractableGameObject);
            var startHoverInteractableGameObjects =  SetOperations.SetDifference(hoveringMouseInteractableGameObject, LastHoverMouseInteractableGameObjects);

            foreach (var interactable in endHoverInteractableGameObjects)
            {
                if (interactable.IsHovering) interactable.EndHover();
            }
            foreach (var interactable in startHoverInteractableGameObjects)
            {
                if (!interactable.IsHovering) interactable.StartHover();
            }

            LastHoverMouseInteractableGameObjects = hoveringMouseInteractableGameObject;
        }
    
    
        protected virtual IMouseHoverable FindFirstIMouseInteractableInMouseCast()
        {
            foreach (var hit in MouseCastHits)
            {
                var characterCardButton = hit.transform.gameObject.GetComponent<BaseCardButton>();
                if (characterCardButton != null && characterCardButton.IsHoverable)
                {
                    //Debug.Log("Mouse find "+ gameObject.name);
                    return characterCardButton;
                }
            
                var characterCardGameObject = hit.transform.gameObject.GetComponent<BaseDraggableObject>();
                if (characterCardGameObject != null && characterCardGameObject.IsDraggable)
                {
                    //Debug.Log("Mouse find "+ gameObject.name);
                    return characterCardGameObject;
                }
            }

            return null;
        }
    
        protected virtual List<IMouseHoverable> FindAllIMouseInteractableInMouseCast()
        {
            List<IMouseHoverable> mouseInteractableGameObjects = new(); 
            foreach (var hit in MouseCastHits)
            {
                var characterCardButton = hit.transform.gameObject.GetComponent<BaseCardButton>();
                if (characterCardButton != null && characterCardButton.IsHoverable)
                {
                    mouseInteractableGameObjects.Add(characterCardButton);
                }
            
                var characterCardGameObject = hit.transform.gameObject.GetComponent<BaseDraggableObject>();
                if (characterCardGameObject != null && characterCardGameObject.IsDraggable)
                {
                    //Debug.Log("Mouse find "+ gameObject.name);
                    mouseInteractableGameObjects.Add(characterCardGameObject);
                }
            }

            return mouseInteractableGameObjects;
        }
    
        #endregion
    
    
        protected virtual TResult FindFirstInMouseCast<TResult>()
        {
            foreach (var hit in MouseCastHits)
            {
                var result = hit.transform.gameObject.GetComponent<TResult>();
                if (result != null)
                {
                    //Debug.Log("Mouse find "+ gameObject.name);
                    return result;
                }
            }

            //Debug.Log("Mouse cannot find "+ typeof(TResult));
            return default;
        }

    
        protected bool StartDragCard()
        {
            // Check for button first
            LastCardButton = FindFirstInMouseCast<BaseCardButton>();

            if (LastCardButton != null && LastCardButton.IsHoverable)
            {
                LastCardButton.Select();
                return true;
            } 

            // Check for card game object second
            DraggingObject = FindFirstInMouseCast<BaseDraggableObject>();

            if (DraggingObject == null || !DraggingObject.IsDraggable || !DetachCardToHolder())
            {
                DraggingObject = null;
                return false;
            }
            
            // Successfully detach card
            CardOffset = DraggingObject.transform.position - MouseWorldPosition;
            IsDraggingCard = true;

            DraggingObject.StartDrag();
            DraggingObject.SetMouseInput(this);
            return true;
        
        }

        protected void DragObject()
        {
            if (!IsDraggingCard) return; 
        
            DraggingObject.transform.position = MouseWorldPosition + CardOffset;
        
        }

        private void EndDrag()
        {
            if (!IsDraggingCard) return;

            
            DraggingObject.EndDrag();
            if (DraggingObject != null) DraggingObject.RemoveMouseInput(this);
            AttachCardToHolder();

            DraggingObject = null;
            LastDraggableObjectHolder = null;
            LastDraggableObjectRegion = null;
            IsDraggingCard = false;

        }

        public void ForceEndDragAndDetachTemporary()
        {
            if (LastDraggableObjectRegion != null) // remove the temporary in last region
            {
                LastDraggableObjectRegion.RemoveTemporary(DraggingObject);
                
            }
            
            DraggingObject = null;
            LastDraggableObjectHolder = null;
            LastDraggableObjectRegion = null;
            IsDraggingCard = false;
        }
    
        protected virtual bool DetachCardToHolder()
        {
            if (DraggingObject.IsDestroyed) return false;
            
            // Check the card region base on card game object or card holder, to TakeOutTemporary
            LastDraggableObjectRegion = FindFirstInMouseCast<BaseDraggableObjectRegion>();
            if (LastDraggableObjectRegion == null)
            {
                LastDraggableObjectHolder = FindFirstInMouseCast<BaseDraggableObjectHolder>();
                if (LastDraggableObjectHolder == null)
                {
                    return true;
                }

                LastDraggableObjectRegion = LastDraggableObjectHolder.DraggableObjectRegion;
            }
            else
            {
                LastDraggableObjectHolder = LastDraggableObjectRegion.FindCardPlaceHolder(DraggingObject);
            }

            // Having got the region and holder, take the card out temporary
            if ((!LastDraggableObjectRegion.CheckCompatibleObject(DraggingObject) )||
            (LastDraggableObjectRegion.CheckCompatibleObject(DraggingObject) 
             && LastDraggableObjectRegion.TakeOutTemporary(DraggingObject, LastDraggableObjectHolder))) return true;
        
            LastDraggableObjectHolder = null;
            LastDraggableObjectRegion = null;

            return false;

        }

        protected void AttachCardToHolder()
        {
            if (DraggingObject == null || DraggingObject.IsDestroyed) return;
            var dropRegion = FindFirstInMouseCast<BaseDraggableObjectRegion>();
            var dropHolder = FindFirstInMouseCast<BaseDraggableObjectHolder>();
        
            if (dropHolder == null)
            {
                if (dropRegion != null && dropRegion != LastDraggableObjectRegion &&
                    dropRegion.TryAddCard(DraggingObject, dropHolder)) // Successfully add to the drop region
                {
                    if (LastDraggableObjectHolder != null) // remove the temporary in last region
                    {
                        LastDraggableObjectRegion.RemoveTemporary(DraggingObject);
                        return;
                    }
                }
            
                if (LastDraggableObjectRegion != null) // Unsuccessfully add to drop region or it is the same region
                    LastDraggableObjectRegion.ReAddTemporary(DraggingObject);
            }
            else
            {
                if (dropRegion == null) 
                    dropRegion = dropHolder.DraggableObjectRegion;
                
                if (dropRegion == null) // No region to drop anyway
                {
                    if(LastDraggableObjectRegion != null) LastDraggableObjectRegion.ReAddTemporary(DraggingObject);
                }

                if (dropRegion.CardMiddleInsertionStyle == BaseDraggableObjectRegion.MiddleInsertionStyle.Swap)
                {
                    var targetCard = dropHolder.DraggableObject;
                    if (targetCard != null && LastDraggableObjectRegion != null && dropRegion.TakeOutTemporary(targetCard, dropHolder))
                    {
                        LastDraggableObjectRegion.ReAddTemporary(targetCard);
                        dropRegion.ReAddTemporary(DraggingObject);
                        
                        return;
                    }
                    
                }
                
                if (!dropRegion.TryAddCard(DraggingObject, dropHolder))
                {
                    if(LastDraggableObjectRegion != null) LastDraggableObjectRegion.ReAddTemporary(DraggingObject);
                }
                
                if (LastDraggableObjectHolder != null)
                {
                    LastDraggableObjectRegion.RemoveTemporary(DraggingObject);
                }

            }

        }
    
    }
}


using Shun_Card_System;
using UnityEngine;

public class HandDraggableObjectMouseInput : BaseDraggableObjectMouseInput
{
    private LayerMask _layerMask;
    
    public HandDraggableObjectMouseInput(LayerMask layerMask = default)
    {
        _layerMask = layerMask;
    }
    
    protected override void CastMouse()
    {
        MouseCastHits = Physics2D.RaycastAll(MouseWorldPosition, Vector2.zero, int.MaxValue, _layerMask);
    }
    
    protected override TResult FindFirstInMouseCast<TResult>()
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
}

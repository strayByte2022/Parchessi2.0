using UnityEngine;

public abstract class PlayerControllerCompositionDependency : MonoBehaviour
{
    protected PlayerController PlayerController;
    protected bool IsOwner = false;

    public virtual void Initialize(PlayerController playerController)
    {
        PlayerController = playerController;
        IsOwner = playerController.IsOwner;
    }
}

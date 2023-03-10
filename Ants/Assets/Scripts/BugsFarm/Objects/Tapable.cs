using UnityEngine;

public class Tapable : MonoBehaviour
{
    //[SerializeField] private TouchCollider _touchCollider;
    //protected TouchCollider TouchCollider => _touchCollider;
    protected virtual PanelID Allowed => PanelID.None;

    // public virtual void SetColliderAllowed(bool allowed)
    // {
    //     _touchCollider.SetAllow(allowed);
    // }
    //
    // private void OnEnable()
    // {
    //     _touchCollider.OnTouch += OnTouch;
    // }
    //
    // private void OnDisable()
    // {
    //     _touchCollider.OnTouch -= OnTouch;
    // }

    private void OnTouch()
    {
        if (Tools.IsPointerOverGameObject())
            return;
        // if (UI_Control.Instance.IsOpened(Allowed))
        // {
        //     // Special case - PanelID.MoveUpgrade is opened, but water is not full because ants are drinking right now!
        //
        //     OnTappedSpecial();
        //     return;
        // }


        //if (UI_Control.IsModalPanelOpened)
        //    return;


        OnTapped();
    }

    protected virtual void OnTapped() { }
    protected virtual void OnTappedSpecial() { }
    protected virtual void OnHolded() { }
}


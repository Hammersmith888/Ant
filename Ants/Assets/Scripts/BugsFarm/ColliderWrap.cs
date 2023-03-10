using UnityEngine;


public class ColliderWrap // TODO: Not used
{
    private bool _colliderEnabled;
    private bool _colliderAllowed;
    private Collider2D _collider;

    public ColliderWrap()
    {
        _colliderAllowed = true;
    }
    public void OnEnable(Collider2D collider)
    {
        if (_collider) return;

        _collider = collider;
        _colliderEnabled = _collider.enabled;
    }
    public void SetColliderEnabled(bool enabled)
    {
        _colliderEnabled = enabled;
        SetCollider();
    }
    public void SetColliderAllowed(bool allowed)
    {
        _colliderAllowed = allowed;

        SetCollider();
    }
    private void SetCollider()
    {
        if (_collider)
            _collider.enabled = _colliderEnabled && _colliderAllowed;
    }
}


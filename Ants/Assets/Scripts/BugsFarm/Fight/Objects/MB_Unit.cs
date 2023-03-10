using Spine.Unity;
using TMPro;
using UnityEngine;

public class MB_Unit : MonoBehaviour
{
    [SerializeField] private Transform _spineParent;
    [SerializeField] private SkeletonAnimation _spine;
    [SerializeField] private HPBar _hpBar;
    [SerializeField] private BoxCollider2D _collider;
    [SerializeField] private bool _defaultLookDirLeft;
    [SerializeField] private TextMeshPro[] damageText;
    [SerializeField] private GameObject shield;
    
    public Vector2 HitPos => (Vector2) transform.position + _collider.offset;
    public float ExtentForward => transform.localScale.x * (_collider.size.x / 2 + _collider.offset.x * lookDir);
    public float ExtentBackward => transform.localScale.x * (_collider.size.x / 2 - _collider.offset.x * lookDir);
    public float Extent => transform.localScale.x * _collider.size.x;
    public HPBar HpBar => _hpBar;

    public SkeletonAnimation Spine => _spine;
    public Unit Unit;
    public Transform ArrowPos;
    public Material Material;
    
    private int lookDir => Unit.IsPlayerSide ? 1 : -1;
    public Transform SpineParent => _spineParent;
    public BoxCollider2D Collider => _collider;
    public TextMeshPro[] DamageText => damageText;

    public GameObject Shield => shield;


    public void SetHpBar(float value) => _hpBar.Set(value);
    public void HideHpBar() => _hpBar.gameObject.SetActive(false);

    public void LookTo(float lookTarget) => SetLookDir(lookTarget - transform.position.x);
    public void SetLookDir(float dir) => SetLookDir(dir < 0);

    public void SetLookDir(bool lookLeft)
    {
        bool prefabLookLeft = lookLeft ^ _defaultLookDirLeft;
        Transform spine = _spineParent != null ? _spineParent : _spine.transform;

        GameTools.Set_LookDir(spine, prefabLookLeft);
        GameTools.Set_LookDir(_hpBar.transform, prefabLookLeft);
    }
}
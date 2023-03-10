using BugsFarm.BuildingSystem;
using UnityEngine;


public class MB_DigGround : A_MB_Placeable_T<DigGroundStock>
{
    public IPosSide Point => _gSide;

    public override string ResourceAmount => "";
    public override bool ShowCoin => false;

#pragma warning disable 0649

    [SerializeField] MB_PosSide _gSide;
    [SerializeField] Sprite[] _stages;

#pragma warning restore 0649

    public void SetSprite(int stage)
    {
        SpriteRenderer.sprite = _stages[stage];
        // SetColliderAllowed(stage > 0);
    }
    protected override void OnSetPlacePos()
    {

    }


}


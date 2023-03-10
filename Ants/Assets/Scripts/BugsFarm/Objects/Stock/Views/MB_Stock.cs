using BugsFarm.BuildingSystem;
using UnityEngine;

public class MB_Stock : A_MB_Placeable_T<APlaceable>
{
    public override string ResourceAmount => ""; // "в разработке";
    public override bool ShowCoin => false;
    public IPosSide[] PointsBuidling => _gSidesBuilding;

    [SerializeField] private MB_PosSide[] _gSidesBuilding;
    protected override void OnSetPlacePos()
    {
        CalcSidesBuilding();
    }

    private void CalcSidesBuilding()
    {
        if (_gSidesBuilding == null)
            return;
        
    }
}
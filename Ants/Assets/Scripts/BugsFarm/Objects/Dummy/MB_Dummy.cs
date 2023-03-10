using BugsFarm.BuildingSystem;
using UnityEngine;


public class MB_Dummy : A_MB_Placeable_T<Dummy>
{
    public override string ResourceAmount => "";            // "в разработке";
    public override bool ShowCoin => false;
    public IPosSide[] PointsBuidling => _gSidesBuilding;

    [SerializeField] MB_PosSide[] _gSidesBuilding;
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


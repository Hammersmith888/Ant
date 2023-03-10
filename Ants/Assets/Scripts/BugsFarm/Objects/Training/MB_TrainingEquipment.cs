using System.Collections.Generic;
using BugsFarm.BuildingSystem;
using UnityEngine;

public class MB_TrainingEquipment : A_MB_Placeable_T<TrainingEquipment>
{
    public override string ResourceAmount => "";
    public override bool ShowCoin => false;

    public IPosSide[] Points => _gSides;
    public IPosSide[] PointsArrowTargetBuild => _gSides_ArrowTargetBuild;

#pragma warning disable 0649

    [SerializeField] MB_PosSide[] _gSides;
    [SerializeField] MB_PosSide[] _gSides_ArrowTargetBuild;

#pragma warning restore 0649

    protected override void OnSetPlacePos()
    {
        SetPosSides(_gSides);
        SetPosSides(_gSides_ArrowTargetBuild);
    }

    private void SetPosSides(IEnumerable<MB_PosSide> points)
    {

    }
}
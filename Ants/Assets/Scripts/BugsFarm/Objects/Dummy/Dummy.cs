using System;
using BugsFarm.BuildingSystem;

[Serializable]
public class Dummy : APlaceable_T<MB_Dummy>
{
    protected override IPosSide[] PointsBuilding => _mb.PointsBuidling;


    public Dummy(ObjType type, int subType, int placeNum)
        : base(type, subType, placeNum)
    { }
}


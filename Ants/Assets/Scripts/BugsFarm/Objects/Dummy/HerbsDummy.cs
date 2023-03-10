using System;
using BugsFarm.AstarGraph;
using BugsFarm.BuildingSystem;

[Serializable]
public class HerbsDummy : APlaceable_T<MB_Dummy>
{
    public float QuantityMax => 100f;

    public HerbsDummy() : base(ObjType.None, 0, -1)
    {
    }

    public IPosSide GetRandomPosition(AntType antType, int mask)
    {
        return default;
        //SPosSide position = AstarTools.GetRandomNode(antType, mask);
        // return position;
    }
}
using System;
using BugsFarm.BuildingSystem;
using BugsFarm.UnitSystem.Obsolete;

[Serializable]
public class TrainingEquipment : APlaceable_T<MB_TrainingEquipment>
{
    private Occupyable<AntPresenter> _occupyable = new Occupyable<AntPresenter>();
    private IPosSide[] Points => _mb.Points;
    protected override IPosSide[] PointsBuilding =>  Type == ObjType.str_ArrowTarget ?
                                                       _mb.PointsArrowTargetBuild :
                                                       Points;

    protected virtual int MaxOccupyCount { get; }
    protected int CountOccupants;

    public TrainingEquipment(ObjType type, int placeNum, int maxOccupants)
        : base(type, 0, placeNum)
    {
        MaxOccupyCount = maxOccupants;
    }
    public override void Init(A_MB_Placeable mb)
    {
        base.Init(mb);
        _occupyable.Init(Points);
    }
    public override void PostSpawnInit()
    {
        base.PostSpawnInit();
        _occupyable.PostSpawnInit();
    }
    public bool Occupy(AntPresenter ant)
    {
        if (TryOccupy() && _occupyable.OccupyRandom(ant, out _))
        {
            CountOccupants++;
            return true;
        }

        return false;
    }

    public bool TryOccupy()
    {
        return IsReady && CountOccupants < MaxOccupyCount && _occupyable.TryOccupy();
    }
    public IPosSide GetPoint(AntPresenter occupant)
    {
        return Points[_occupyable.Occupied[occupant]];
    }
    public void Free(AntPresenter ant)
    {
        if (_occupyable.FreeOccupy(ant))
        {
            CountOccupants--;
        }
    }
}


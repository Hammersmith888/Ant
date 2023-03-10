using System;
using BugsFarm.BuildingSystem;
using BugsFarm.SimulationSystem;
using BugsFarm.SimulationSystem.Obsolete;
using UnityEngine;

[Serializable]
public class GoldminePresenter : APlaceable_T<MB_Goldmine>
{
    public bool IsFull => GoldCur == GoldMax;
    public int GoldCur { get; private set; }
    public int GoldMax => Mathf.RoundToInt(UpgradeLevelCur.param1);
    public int GoldPerCycle => Mathf.RoundToInt(UpgradeLevelCur.param2);
    public MB_Goldmine MbGoldmine => _mb;
    protected override IPosSide[] PointsBuilding => _mb.PointsBuidling;
    
    private int _workerCount = 0;

    public GoldminePresenter(int placeNum) : base(ObjType.str_Goldmine, 0, placeNum){}
    public override void Update()
    {
        base.Update();

        if (SimulationOld.Type == SimulationType.Accurate)
            _mb.Simulate();
    }
    public bool TryOccupyGoldmine()
    {
        return IsReady && _workerCount == 0 && !IsFull;
    }
    public bool OccupyGoldmine()
    {
        if (TryOccupyGoldmine())
        {
            _workerCount++;
            return true;
        }
        return false;
    }
    public void OccupyFree()
    {
        _workerCount = 0;
    }
    public bool AddGold(int gold)
    {
        GoldCur = Mathf.Min(GoldCur + gold, GoldMax);

        return GoldCur == GoldMax;
    }
    public int CollectGold()
    {
        var collected = GoldCur;
        GoldCur = 0;

        return collected;
    }
}
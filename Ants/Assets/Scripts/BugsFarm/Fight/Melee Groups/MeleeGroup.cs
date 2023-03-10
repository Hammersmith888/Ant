using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class MeleeGroup
{
    public bool IsFixed { get; private set; }
    public bool IsArchers { get; private set; }
    public float Size { get; private set; }

    public bool IsCalculated => units.Count(u => !u.pos.HasValue) == 0;

    public List<MeleeGroupUnit> units = new List<MeleeGroupUnit>();


    public MeleeGroup()
    {
        IsArchers = true;
        IsFixed = true;
    }


    public MeleeGroup(Unit unit1, Unit unit2)
    {
       bool unit1first = Tools.RandomBool();

        MeleeGroupUnit mgu1 = unit1.AI_Unit.MeleeGroupUnit;
        MeleeGroupUnit mgu2 = unit2.AI_Unit.MeleeGroupUnit;

        units.Add(unit1first ? mgu1 : mgu2);
        units.Add(unit1first ? mgu2 : mgu1);
    }

    public void Join(Unit unit, Unit target)
    {
        int tgt_i = IndexOf(target);

        int countL = tgt_i;
        int countR = units.Count - tgt_i - 1;

        bool left =
                IsProtectedFromRight(target.IsPlayerSide, tgt_i) ||
                !IsProtectedFromLeft(target.IsPlayerSide, tgt_i) &&
                countL < countR
            ;
        int index = left ? 0 : units.Count;

        units.Insert(index, unit.AI_Unit.MeleeGroupUnit);
    }

    public bool IsProtected(Unit unit)
    {
        int index = IndexOf(unit);
        bool side = unit.IsPlayerSide;

        return
            IsProtectedFromLeft(side, index) &&
            IsProtectedFromRight(side, index)
            ;
    }


    private int IndexOf(Unit unit) => units.FindIndex(u => u.unit == unit);
    private bool IsProtectedFromLeft(bool side, int index) => IsProtected(side, index, -1);
    private bool IsProtectedFromRight(bool side, int index) => IsProtected(side, index, +1);

    private bool IsProtected(bool side, int index, int increment)
    {
        // Prohibit more than 1 enemy unit behind archers
        if (
            IsArchers &&
            increment == -1 &&
            units.Count > 0 &&
            units[0].unit.IsPlayerSide != side
        )
            return true;

        for (
            int i = index + increment;
            i >= 0 && i < units.Count;
            i += increment
        )
            if (units[i].unit.IsPlayerSide == side)
                return true;

        return false;
    }

    public void CalcSize()
    {
        Size = 0;

        foreach (MeleeGroupUnit mgu in units)
            Size += mgu.unit.MB_Unit.Extent;
    }

    public void SetPositions()
    {
        SetPositions(0, 1);
        SetPositions(units.Count - 1, -1);
    }

    private void SetPositions(int start_i, int increment)
    {
        float? pos = null;

        for (int i = start_i; i >= 0 && i < units.Count; i += increment)
        {
            MeleeGroupUnit mgu = units[i];

            if (pos.HasValue)
            {
                pos += increment * Mathf.Abs(mgu.unit.MB_Unit.ExtentBackward);
                mgu.pos = pos;
                pos += increment * Mathf.Abs(mgu.unit.MB_Unit.ExtentForward);
            }
            else if (mgu.pos.HasValue)
            {
                pos = mgu.pos;
                pos += increment * Mathf.Abs(mgu.unit.MB_Unit.ExtentForward);
            }
        }
    }

    public void cb_Died(MeleeGroupUnit mgu)
    {
        units.Remove(mgu);

        // TODO: refact or comment !!!
        if (mgu.unit.AI_Unit.Target == null)
            return;

        foreach (Unit unit in mgu.unit.AI_Unit.Target.AI_Unit.MeleeAttackers)
        {
            unit.AI_Unit.MeleeGroupUnit.pos = null;

            unit.AI_Unit.TransitionExt(UnitState.TargetOutOfSight);
        }

        SetPositions();
    }
}
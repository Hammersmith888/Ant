using System.Collections.Generic;
using System.Linq;
using BugsFarm.Services;

public static class MeleeGroups
{
    static List<MeleeGroup> _meleeGroups = new List<MeleeGroup>();
    static List<float> _gaps = new List<float>(3);

    public static void Clear()
    {
        _meleeGroups.Clear();
    }

    public static void Archers()
    {
        MeleeGroup group = new MeleeGroup();
        var archers = Squads.Player.units
            .Where(u => u.IsAlive && BattleService.AttackRange(u.AntType) > 0);

        foreach (Unit archer in archers)
        {
            MeleeGroupUnit mgu = archer.AI_Unit.MeleeGroupUnit;

            mgu.group = group;
            mgu.pos = archer.MB_Unit.transform.position.x;

            group.units.Add(archer.AI_Unit.MeleeGroupUnit);
        }

        _meleeGroups.Add(group);
    }

    private static MeleeGroup RegisterNew(Unit unit1, Unit unit2)
    {
        MeleeGroup group = new MeleeGroup(unit1, unit2);

        _meleeGroups.Add(group);

        return group;
    }

    public static void Attack(Unit unit, Unit target)
    {
        MeleeGroup groupUnit = unit.AI_Unit.MeleeGroupUnit.group;
        MeleeGroup groupTarget = target.AI_Unit.MeleeGroupUnit.group;

        if (groupTarget == null)
        {
            MeleeGroup group = RegisterNew(unit, target);

            Attack(unit, target, group);
            Attack(target, unit, group);
        }
        else
        {
            if (groupUnit != groupTarget)
                groupTarget.Join(unit, target);

            Attack(unit, target, groupTarget);
        }
    }

    private static void Attack(Unit unit, Unit target, MeleeGroup group)
    {
        MeleeGroupUnit mgu = unit.AI_Unit.MeleeGroupUnit;

        if (mgu.group != group)
        {
            mgu.pos = null;

            mgu.group?.units.Remove(mgu);
        }

        unit.AI_Unit.MeleeGroupUnit.group = group;
        unit.AI_Unit.SetMeleeTarget(target);

        target.AI_Unit.HandleUnitEvent(unit, UnitEvent.MeleeGoingToAttackYou);
    }
}
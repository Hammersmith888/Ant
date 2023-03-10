using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.Services;
using BugsFarm.UnitSystem;
using BugsFarm.UnitSystem.Obsolete;
using UnityEngine;

[Serializable]
public class AI_MeleeUnit : AI_Unit
{
    public override bool IsOnTheWay =>
        State == UnitState.TargetOutOfSight && _sm_RunAndWait.State == GoAndWaitState.Run;

    SmMu_RunAndWait _sm_RunAndWait;
    SmMu_Attack _sm_Attack;
    SmMu_Walk _sm_Walk;

    static List<Unit> _enemies = new List<Unit>();

    public AI_MeleeUnit(Unit unit)
        : base(unit)
    {
        _sm_RunAndWait = new SmMu_RunAndWait(unit, false);
        _sm_Walk = new SmMu_Walk(unit);
        _sm_Attack = new SmMu_Attack(this);
    }


    public override void Init(MB_Unit mb_Unit)
    {
        base.Init(mb_Unit);

        _sm_RunAndWait.Inject(mb_Unit, _antAnimator);
        _sm_Walk.Init(mb_Unit, _antAnimator);
        _sm_Attack.Init(_antAnimator, _unit.Level);
    }

    public override void Update()
    {
        switch (State)
        {
            case UnitState.SelectTarget:
                SelectTarget();
                break;

            case UnitState.TargetOutOfSight:
                if (_sm_RunAndWait.IsTaskEnd)
                    Transition(UnitState.Attack);
                break;
        }

        base.Update();
    }

    public override void HandleUnitEvent(Unit unit, UnitEvent unitEvent)
    {
        base.HandleUnitEvent(unit, unitEvent);

        if (State == UnitState.Dead)
            return;

        switch (unitEvent)
        {
            case UnitEvent.Destroyed:
                if (unit == Target)
                {
                    Target = null;
                    Transition(UnitState.SelectTarget);
                }

                break;
        }
    }

    private void SelectTarget()
    {
        int AR(Unit u) => BattleService.AttackRange(u.AntType); // Attack Range
        int MA(Unit u) => u.AI_Unit.MA_Count; // Melee Attackers count
        float DI(Unit u) => Mathf.Abs(_mb_Unit.transform.position.x - u.MB_Unit.transform.position.x); // Distance


        // Select from Melee Attackers
        _enemies.Clear();
        _enemies.AddRange(_meleeAttackers);
        _enemies.Sort((a, b) =>
            1 * DI(a).CompareTo(DI(b))
        );
        Unit target = _enemies.FirstOrDefault();
        if (target != null)
        {
            MeleeGroups.Attack(_unit, target);
            return;
        }

        // Select from ALL
        _enemies.Clear();
        //_enemies.AddRange( Squads.GetSquad( !_isPlayerSide ).units.Where( x => x.IsAlive && !x.AI_Unit.IsMeleeProtected() ) );
        _enemies.Sort((a, b) =>
            100 * AR(a).CompareTo(AR(b)) +
            10 * MA(a).CompareTo(MA(b)) +
            1 * DI(a).CompareTo(DI(b))
        );
        target = _enemies.FirstOrDefault();
        if (target != null)
            MeleeGroups.Attack(_unit, target);
    }

    public override void SetMeleeTarget(Unit target)
    {
        Target = target;

        Transition(UnitState.TargetOutOfSight);
    }

    protected override ATask GetTask()
    {
        switch (State)
        {
            case UnitState.TargetOutOfSight: return _sm_RunAndWait;
            case UnitState.Walk: return _sm_Walk;
            case UnitState.Attack: return _sm_Attack;

            default: return base.GetTask();
        }
    }
}
using System;
using System.Collections.Generic;
using BugsFarm.AnimationsSystem;
using BugsFarm.Game;
using BugsFarm.UnitSystem;
using BugsFarm.UnitSystem.Obsolete;
using AntAnimator = BugsFarm.UnitSystem.Obsolete.AntAnimator;

public enum UnitEvent
{
    None,

    MeleeGoingToAttackYou,
    Destroyed,
}


public enum UnitState
{
    None,

    Walk,

    ExitFromCave,
    PrepareToFight,
    SelectTarget,
    TargetOutOfSight,
    Attack,
    Win,
    EnterCave,

    Dead,
}


[Serializable]
public abstract class AI_Unit : AStateMachine<UnitState>
{
    public AntType Type => _type;
    public List<Unit> MeleeAttackers => _meleeAttackers;

    public int MA_Count => _meleeAttackers.Count;
    public bool IsMeleeProtected() => MeleeGroupUnit.group?.IsProtected(_unit) ?? false;

    public virtual bool IsOnTheWay { get; } = false;

    [NonSerialized] protected MB_Unit _mb_Unit;
    [NonSerialized] protected AntAnimator _antAnimator;

    public Unit Target { get; protected set; }
    public MeleeGroupUnit MeleeGroupUnit { get; }

    protected readonly bool _isPlayerSide;
    protected readonly AntType _type;
    protected readonly Unit _unit;

    protected List<Unit> _meleeAttackers = new List<Unit>();

    SM_Go _sm_Go = new SM_Go();
    SM_UnitDeath _sm_UnitDeath = new SM_UnitDeath();
    SmMu_RunAndWait _sm_PrepareToFight;
    
    protected virtual ATask GetTask()
    {
        switch (State)
        {
            case UnitState.ExitFromCave:
            case UnitState.EnterCave: return _sm_Go;

            case UnitState.PrepareToFight: return _sm_PrepareToFight;
            case UnitState.Dead: return _sm_UnitDeath;
            default: return null;
        }
    }

    public virtual void SetMeleeTarget(Unit target)
    {
    }

    protected AI_Unit(Unit unit)
    {
        _isPlayerSide = unit.IsPlayerSide;
        _type = unit.AntType;
        _unit = unit;

        MeleeGroupUnit = new MeleeGroupUnit(unit);
        _sm_PrepareToFight = new SmMu_RunAndWait(unit, true);
    }

    public virtual void Init(MB_Unit mb_Unit)
    {
        _mb_Unit = mb_Unit;
        _antAnimator = new AntAnimator(_type, mb_Unit.Spine, null, null);

        _sm_PrepareToFight.Inject(mb_Unit, _antAnimator);
        _sm_Go.Init(mb_Unit, _antAnimator, _type);
        _sm_UnitDeath.Init(mb_Unit, _antAnimator);

        GameEvents.OnUnitDied += u => HandleUnitEvent(u, UnitEvent.Destroyed);
    }

    public void TransitionExt(UnitState state)
    {
        if (State != UnitState.Dead)
            Transition(state);
    }

    public virtual void HandleUnitEvent(Unit unit, UnitEvent unitEvent)
    {
        switch (unitEvent)
        {
            case UnitEvent.MeleeGoingToAttackYou:
                _meleeAttackers.Add(unit);
                break;
            case UnitEvent.Destroyed:
                _meleeAttackers.Remove(unit);
                break;
        }
    }

    public override void Update()
    {
        switch (State)
        {
            case UnitState.ExitFromCave:
            case UnitState.EnterCave:
                if (_sm_Go.IsTaskEnd)
                {
                    Transition(UnitState.None);
                    GameEvents.OnUnitCaveWalkDone?.Invoke(_unit);
                }

                break;
        }

        GetTask()?.Update();
    }

    protected override void OnEnter()
    {
        switch (State)
        {
            case UnitState.Win:
                SetAnim();
                break;
        }

        GetTask()?.TaskStart();
    }

    private void SetAnim() => _antAnimator.SetAnim(GetAnim());

    AnimKey GetAnim()
    {
        switch (State)
        {
            case UnitState.Win: return AnimKey.Idle;
            default: return AnimKey.None;
        }
    }
}
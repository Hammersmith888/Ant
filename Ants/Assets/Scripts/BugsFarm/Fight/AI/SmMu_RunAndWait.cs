using System;
using BugsFarm.AnimationsSystem;
using BugsFarm.UnitSystem;
using BugsFarm.UnitSystem.Obsolete;
using BugsFarm.Views.Hack;
using AntAnimator = BugsFarm.UnitSystem.Obsolete.AntAnimator;

public enum GoAndWaitState
{
    None,

    Run,
    WaitForTarget,
}

[Serializable]
public class SmMu_RunAndWait : AStateMachine<GoAndWaitState>
{
    private float? TargetPos => _prepareToFightMode ? _randomPos : _unit.AI_Unit.MeleeGroupUnit.pos;

    private float? LookTo => _prepareToFightMode ? -100 : _unit.AI_Unit.Target?.MB_Unit.transform.position.x;

    private bool IsWaitDone => _prepareToFightMode ? false : !_unit.AI_Unit.Target?.AI_Unit.IsOnTheWay ?? false;

    [NonSerialized] private AntAnimator _antAnimator;
    [NonSerialized] private MB_Unit _mb_Unit;
    [NonSerialized] private float _speed;
    [NonSerialized] private float _animationTimeScaleMul;

    private Unit _unit;
    private bool _prepareToFightMode;
    private float _randomPos;
    
    public SmMu_RunAndWait(Unit unit, bool prepareToFightMode)
    {
        _unit = unit;
        _prepareToFightMode = prepareToFightMode;
    }

    public void Inject(MB_Unit mb_Unit, AntAnimator antAnimator)
    {
        _mb_Unit = mb_Unit;
        _antAnimator = antAnimator;

        CfgUnit config = Data_Fight.Instance.units[_unit.AntType];
        _speed = config.Speed * HackRefsView.Instance.BattleSettings.unitSpeedMul;
        _animationTimeScaleMul = _speed * config.SpeedToAnimationTimeScaleMul;
    }

    public override bool TaskStart()
    {
        Transition(GoAndWaitState.Run);

        return true;
    }

    public override void Update()
    {
        /*switch (State)
        {
            case GoAndWaitState.Run:
                LookToTargetPos(); // (!) Must be called here, not in OnEnter(). Because, during OnEnter() there could be still no target.

                if (
                    TargetPos.HasValue &&
                    UnitWalk.Go(_mb_Unit, TargetPos.Value, _speed)
                )
                    Transition(GoAndWaitState.WaitForTarget);
                break;

            case GoAndWaitState.WaitForTarget:
                LookToTarget();

                if (IsWaitDone)
                    Transition(GoAndWaitState.None);
                break;
        }*/
    }

    protected override void OnEnter()
    {
        switch (State)
        {
            case GoAndWaitState.Run:
            case GoAndWaitState.WaitForTarget:
                SetAnim();
                break;
        }
    }

    private void LookToTargetPos()
    {
        if (TargetPos.HasValue)
            _mb_Unit.LookTo(TargetPos.Value);
    }
    
    private void LookToTarget()
    {
        if (LookTo.HasValue)
            _mb_Unit.LookTo(LookTo.Value);
    }

    private void SetAnim() => _antAnimator.SetAnim(GetAnim(), _animationTimeScaleMul);

    private AnimKey GetAnim()
    {
        switch (State)
        {
            case GoAndWaitState.Run:
                switch (_unit.AntType)
                {
                    case AntType.Pikeman: return AnimKey.Run;
                    default: return AnimKey.Walk;
                }

            case GoAndWaitState.WaitForTarget: return AnimKey.Idle;

            default: return AnimKey.None;
        }
    }
}
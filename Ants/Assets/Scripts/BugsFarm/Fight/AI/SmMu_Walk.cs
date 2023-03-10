using System;
using BugsFarm.AnimationsSystem;
using BugsFarm.UnitSystem;
using BugsFarm.UnitSystem.Obsolete;
using BugsFarm.Views.Hack;
using AntAnimator = BugsFarm.UnitSystem.Obsolete.AntAnimator;
using Random = UnityEngine.Random;

public enum MeleeWalkState
{
    None,

    Stay,
    Go,
}

public class SmMu_Walk : AStateMachine<MeleeWalkState>
{
    [NonSerialized] private AntAnimator _antAnimator;
    [NonSerialized] private MB_Unit _mb_Unit;
    [NonSerialized] private float _speed;

    private Unit _unit;
    private Timer _timerStay = new Timer();
    private float _target;

    public SmMu_Walk(Unit unit)
    {
        _unit = unit;
    }

    public void Init(MB_Unit mb_Unit, AntAnimator antAnimator)
    {
        const float speedMul = .5f;

        _mb_Unit = mb_Unit;
        _antAnimator = antAnimator;
        _speed = Data_Fight.Instance.units[_unit.AntType].Speed * HackRefsView.Instance.BattleSettings.unitSpeedMul * speedMul;
    }

    public override bool TaskStart()
    {
        Transition(Tools.RandomBool() ? MeleeWalkState.Stay : MeleeWalkState.Go);
        return true;
    }

    public override void Update()
    {
        /*switch (State)
        {
            case MeleeWalkState.Stay:
                if (_timerStay.IsReady)
                    Transition(MeleeWalkState.Go);
                break;

            case MeleeWalkState.Go:
                if (UnitWalk.Go(_mb_Unit, _target, _speed))
                    Transition(MeleeWalkState.Stay);
                break;
        }*/
    }

    protected override void OnEnter()
    {
        switch (State)
        {
            case MeleeWalkState.Stay:
                OnEnter_Stay();
                break;
            case MeleeWalkState.Go:
                OnEnter_Go();
                break;
        }
    }

    private void OnEnter_Stay()
    {
        _timerStay.Set(Random.Range(1f, 6f));
        _mb_Unit.SetLookDir(Tools.RandomBool());
        //SetAnim();
    }

    private void OnEnter_Go()
    {
        _mb_Unit.SetLookDir(_target - _mb_Unit.transform.position.x);
        //SetAnim();
    }

    private void SetAnim() => _antAnimator.SetAnim(GetAnim());

    private AnimKey GetAnim()
    {
        switch (State)
        {
            case MeleeWalkState.Go: return AnimKey.Walk;
            case MeleeWalkState.Stay: return AnimKey.Idle;
            default: return AnimKey.None;
        }
    }
}
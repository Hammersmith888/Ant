using System;
using BugsFarm.AnimationsSystem;
using BugsFarm.UnitSystem;
using BugsFarm.UnitSystem.Obsolete;
using BugsFarm.Views.Hack;
using UnityEngine;
using AntAnimator = BugsFarm.UnitSystem.Obsolete.AntAnimator;

public enum GoState
{
    None,

    Go,
}


[Serializable]
public class SM_Go : AStateMachine<GoState>
{
    [NonSerialized] private AntAnimator _antAnimator;
    [NonSerialized] private MB_Unit _mb_Unit;
    [NonSerialized] private float _speedOriginal;
    [NonSerialized] private float _speedSnail;
    [NonSerialized] private float _speedToWalkAnimTimescale;
    [NonSerialized] private float _speedToWalkAnimTimescaleArcher;

    private AntType _type;
    private float _target;
    private float _posInsideCave;
    private float _posOutsideCave;
    private bool _toRun;

    public float Speed_01 => _toRun ? 1 : .5f;
    public float Speed => Mathf.Max(_speedOriginal * Speed_01, _speedSnail);
    public float WalkAnimTimescaleMul => Speed * (_toRun ? _speedToWalkAnimTimescale : _speedToWalkAnimTimescaleArcher);

    public void Init(MB_Unit mb_Unit, AntAnimator antAnimator, AntType type)
    {
        _mb_Unit = mb_Unit;
        _antAnimator = antAnimator;
        _type = type;

        CfgUnit cfgUnit = Data_Fight.Instance.units[type];
        CfgUnit cfgSnail = Data_Fight.Instance.units[AntType.Snail];
        CfgUnit cfgArcher = Data_Fight.Instance.units[AntType.Archer];
        _speedOriginal = HackRefsView.Instance.BattleSettings.unitSpeedMul * cfgUnit.Speed;
        _speedSnail = HackRefsView.Instance.BattleSettings.unitSpeedMul * cfgSnail.Speed;

        _speedToWalkAnimTimescale = cfgUnit.SpeedToAnimationTimeScaleMul;
        _speedToWalkAnimTimescaleArcher = cfgArcher.SpeedToAnimationTimeScaleMul;
    }

    public void Setup(float target, float posInsideCave, float posOutsideCave, bool toRun)
    {
        _target = target;
        _posInsideCave = posInsideCave;
        _posOutsideCave = posOutsideCave;
        _toRun = toRun;
    }

    public override bool TaskStart()
    {
        Transition(GoState.Go);
        return true;
    }

    public override void Update()
    {
        switch (State)
        {
            case GoState.Go:
                if (UnitWalk.Go(_mb_Unit, _target, Speed))
                    Transition(GoState.None);
                break;
        }
    }

    protected override void OnEnter()
    {
        switch (State)
        {
            case GoState.Go:
                _mb_Unit.LookTo(_target);
                SetAnim();
                break;

            case GoState.None:
                SetAnim();
                break;
        }
    }

    private void SetAnim() => _antAnimator.SetAnim(GetAnim(), WalkAnimTimescaleMul);

    private AnimKey GetAnim()
    {
        switch (State)
        {
            case GoState.Go:
                switch (_type)
                {
                    case AntType.Pikeman: return _toRun ? AnimKey.Run : AnimKey.Walk;

                    default: return AnimKey.Walk;
                }

            case GoState.None: return AnimKey.Idle;

            default: return AnimKey.None;
        }
    }
}
using System;
using BugsFarm.AnimationsSystem;
using BugsFarm.UnitSystem;
using BugsFarm.UnitSystem.Obsolete;
using AntAnimator = BugsFarm.UnitSystem.Obsolete.AntAnimator;

public enum UnitDeathState
{
    None,

    Animation,
    Wait,
    Fade,
    Done
}

[Serializable]
public class SM_UnitDeath : AStateMachine<UnitDeathState>
{
    [NonSerialized] MB_Unit _mb_Unit;
    [NonSerialized] AntAnimator _antAnimator;

    private Timer _timer = new Timer();

    public void Init(MB_Unit mb_Unit, AntAnimator antAnimator)
    {
        _mb_Unit = mb_Unit;
        _antAnimator = antAnimator;
    }

    public override bool TaskStart()
    {
        Transition(UnitDeathState.Animation);
        return true;
    }

    public override void Update()
    {
        switch (State)
        {
            case UnitDeathState.Animation:
                if (_antAnimator.IsAnimComplete)
                    Transition(UnitDeathState.Wait);
                break;

            case UnitDeathState.Wait:
                if (_timer.IsReady)
                    Transition(UnitDeathState.Fade);
                break;

            case UnitDeathState.Fade:
                if (_antAnimator.SkeletonCurrent != null)
                    _antAnimator.SkeletonCurrent.skeleton.A = 1 - _timer.Progress;

                if (_timer.IsReady)
                    Transition(UnitDeathState.Done);
                break;
        }
    }

    protected override void OnEnter()
    {
        switch (State)
        {
            case UnitDeathState.Animation:
                _antAnimator.SetAnim(AnimKey.Death);
                break;
            case UnitDeathState.Wait:
                _timer.Set(1);
                break;
            case UnitDeathState.Fade:
                _timer.Set(1);
                break;
            case UnitDeathState.Done:
                _mb_Unit.gameObject.SetActive(false);
                break;
        }
    }
}
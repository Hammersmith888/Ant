using System;
using BugsFarm.AnimationsSystem;
using BugsFarm.AudioSystem;
using BugsFarm.AudioSystem.Obsolete;
using BugsFarm.UnitSystem;
using BugsFarm.UnitSystem.Obsolete;
using UnityEngine;
using AntAnimator = BugsFarm.UnitSystem.Obsolete.AntAnimator;
using Random = UnityEngine.Random;

public enum ArcherState
{
    None,

    Attack,
    CoolDown,
}


[Serializable]
public class SmLru_Attack : AStateMachine<ArcherState>
{
    [NonSerialized] AntAnimator _antAnimator;
    [NonSerialized] MB_Unit _mb_Unit;
    [NonSerialized] float _attack;
    [NonSerialized] float _сoolDown;

    private Timer _timerCoolDown = new Timer(TimerType.Scaled);
    private Unit _target;

    public void Init(MB_Unit mb_Unit, AntAnimator antAnimator, int level)
    {
        _mb_Unit = mb_Unit;
        _antAnimator = antAnimator;

        CfgUnit cfg = Data_Fight.Instance.units[mb_Unit.Unit.AntType];
        _attack = cfg.Attack(level);
        _сoolDown = cfg.Cooldown;

        _antAnimator.OnSpineAnimationEvent += HandleSpineEvent;
    }

    public override bool TaskStart()
    {
        Transition(ArcherState.CoolDown);
        _timerCoolDown.Set(_сoolDown * Random.value);
        return true;
    }

    public override void Update()
    {
        if (State != ArcherState.None)
        {
            SetLookDir();
        }

        switch (State)
        {
            case ArcherState.Attack:
                if (_antAnimator.IsAnimComplete)
                    Transition(ArcherState.CoolDown);
                break;

            case ArcherState.CoolDown:
                if (_timerCoolDown.IsReady)
                    Transition(ArcherState.Attack);
                break;
        }
    }

    protected override void OnEnter()
    {
        switch (State)
        {
            case ArcherState.Attack:
                OnEnter_Attack();
                break;
            case ArcherState.CoolDown:
                OnEnter_CoolDown();
                break;
        }
    }

    private void OnEnter_Attack()
    {
        _antAnimator.SetAnim(AnimKey.Attack);

        _timerCoolDown.Set(_сoolDown);

        Sounds.Play(Tools.RandomItem(Sound.Archer_BowString1, Sound.Archer_BowString2));
    }

    private void OnEnter_CoolDown()
    {
        _antAnimator.SetAnim(AnimKey.Idle);
    }

    private void SetLookDir()
    {
        float target_X = _target?.MB_Unit.transform.position.x ?? 0;
        bool left = target_X < _mb_Unit.transform.position.x;

        _mb_Unit.SetLookDir(left);
    }

    private void HandleSpineEvent(Spine.Event e)
    {
        if (e.Data.Name == "Shoot")
            Shoot();
    }

    private void Shoot()
    {
        Sounds.Play(Tools.RandomItem(Sound.Archer_Shoot1, Sound.Archer_Shoot2));
    }

    private float Dist(Unit unit)
    {
        return Mathf.Abs(
            _mb_Unit.transform.position.x -
            unit.MB_Unit.transform.position.x
        );
    }
}
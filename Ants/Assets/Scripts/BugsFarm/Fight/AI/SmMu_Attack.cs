using System;
using BugsFarm.AnimationsSystem;
using BugsFarm.AudioSystem;
using BugsFarm.AudioSystem.Obsolete;
using BugsFarm.UnitSystem;
using BugsFarm.UnitSystem.Obsolete;
using Spine;
using AntAnimator = BugsFarm.UnitSystem.Obsolete.AntAnimator;

public enum MeleeAttackState
{
    None,
    Attack,
}

[Serializable]
public class SmMu_Attack : AStateMachine<MeleeAttackState>
{
    [NonSerialized] AntAnimator _antAnimator;
    [NonSerialized] float _attack;
    [NonSerialized] float _coolDown;
    [NonSerialized] AnimKey _curAttackAnim;

    private AI_Unit _ai_Unit;
    private Timer _timerCoolDown = new Timer(TimerType.Scaled);

    public SmMu_Attack(AI_Unit ai_Unit)
    {
        _ai_Unit = ai_Unit;
    }

    public void Init(AntAnimator antAnimator, int level)
    {
        _antAnimator = antAnimator;

        CfgUnit cfg = Data_Fight.Instance.units[_ai_Unit.Type];
        _attack = cfg.Attack(level);
        _coolDown = cfg.Cooldown;

        _antAnimator.OnSpineAnimationEvent += HandleSpineEvent;
    }

    public override bool TaskStart()
    {
        Transition(MeleeAttackState.Attack);
        return true;
    }

    public override void Update()
    {
        switch (State)
        {
            case MeleeAttackState.Attack:
                if (
                    _antAnimator.IsAnimComplete &&
                    _timerCoolDown.IsReady
                )
                {
                    if (State == MeleeAttackState.Attack) // TODO: refact
                        Transition(MeleeAttackState.Attack);
                }

                break;
        }
    }

    protected override void OnEnter()
    {
        switch (State)
        {
            case MeleeAttackState.Attack:
                OnEnter_Attack();
                break;
        }
    }

    private void OnEnter_Attack()
    {
        _curAttackAnim = GetAttackAnim();
        _antAnimator.SetAnim(_curAttackAnim);
        _timerCoolDown.Set(_coolDown);
        Sounds.Play(GetOnAttackSound());
    }

    private void HandleSpineEvent(Event e)
    {
        if (
            e.Data.Name == "hit" &&
            State == MeleeAttackState.Attack &&
            _ai_Unit.Target != null
        )
        {
            _ai_Unit.Target.TakeDamage(_attack);

            Sounds.Play(GetOnHitSound());
        }
    }

    private AnimKey GetAttackAnim()
    {
        switch (_ai_Unit.Type)
        {
            case AntType.Worker: return Tools.RandomBool() ? AnimKey.FightShovel1 : AnimKey.FightShovel2;
            case AntType.Pikeman: return Tools.RandomBool() ? AnimKey.Attack : AnimKey.Attack2;
            case AntType.Snail: return AnimKey.None;

            default: return AnimKey.Attack;
        }
    }

    private bool IsPikemanAttack1 =>
        _ai_Unit.Type == AntType.Pikeman &&
        _curAttackAnim == AnimKey.Attack;

    private Sound GetOnAttackSound()
    {
        if (IsPikemanAttack1)
            return Sound.Pikeman_Attack1;

        if (_ai_Unit.Type == AntType.Spider)
            return Tools.RandomItem(Sound.Spider_Attack1, Sound.Spider_Attack2);

        return Sound.None;
    }

    private Sound GetOnHitSound()
    {
        if (IsPikemanAttack1)
            return Sound.None;

        switch (_ai_Unit.Type)
        {
            case AntType.Pikeman: return Sound.Pikeman_Attack2;
            case AntType.Worker: return Tools.RandomItem(Sound.Worker_Attack1, Sound.Worker_Attack2);

            case AntType.Cockroach: return Tools.RandomItem(Sound.Cockroach_Attack1, Sound.Cockroach_Attack2);
            case AntType.Worm: return Tools.RandomItem(Sound.Worm_Attack1, Sound.Worm_Attack2);
            case AntType.PotatoBug: return Tools.RandomItem(Sound.PotatoBug_Attack1, Sound.PotatoBug_Attack2);

            default: return Sound.None;
        }
    }
}
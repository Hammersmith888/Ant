using System;
using BugsFarm.UnitSystem;
using BugsFarm.UnitSystem.Obsolete;

[Serializable]
public class AI_LongRangeUnit : AI_Unit
{
    SmLru_Attack _sm_Attack = new SmLru_Attack();

    public AI_LongRangeUnit(Unit unit)
        : base(unit)
    {
    }

    public override void Init(MB_Unit mb_Unit)
    {
        base.Init(mb_Unit);

        _sm_Attack.Init(mb_Unit, _antAnimator, _unit.Level);
    }

    public override void Update()
    {
        if (State == UnitState.SelectTarget)
            Transition(UnitState.Attack);

        base.Update();
    }

    protected override ATask GetTask()
    {
        switch (State)
        {
            case UnitState.Attack: return _sm_Attack;
            default: return base.GetTask();
        }
    }
}
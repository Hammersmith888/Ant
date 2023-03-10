using BugsFarm.SimulationSystem.Obsolete;

namespace BugsFarm.UnitSystem.Obsolete.Components
{
    public class AiStats
    {
        public double Age => IsAlive ? SimulationOld.GetAge(TimeBorn) : (TimeDied - TimeBorn);
        public double TimeBorn { get; }
        public double TimeDied { get; private set; }

        public bool IsAlive { get; private set; } = true;

        public bool ShouldSleep { get; private set; }
        public float AwakeLeft { get; private set; }
        public bool IsSleepy { get; private set; }
        public float TimeSleepy { get; private set; }

    #region Consuming States
        public bool isConsumingFood { get; private set; }
        public bool isConsumingWater { get; private set; }
        public float DrinkNeed { get; private set; }
        public float EatNeed { get; private set; }
        public bool IsHungry => IsHungryFood || IsHungryWater;
        public bool IsHungryWater { get; private set; }
        public bool IsHungryFood { get; private set; }
        public bool IsWaterHunger { get; private set; }
        public float TimeWaterHunger { get; private set; }
        public bool IsFoodHunger { get; private set; }
        public float TimeFoodHunger { get; private set; }
        public bool IsDeadlyHunger => IsWaterHunger || IsFoodHunger;
    #endregion
        
        public float? CurrentTaskTime => _aiRoot.CurrentTaskTime;
        public IAiControlable CurrentAi => _aiRoot.CurrentAi;
        
        private readonly SM_AntRoot _aiRoot;

        public AiStats(SM_AntRoot root)
        {
            TimeBorn = root.TimeBorn;
            _aiRoot = root;
        }
        public void Update(SM_Sleep sleep)
        {
            ShouldSleep = sleep.ShouldSleep;
            AwakeLeft = sleep.AwakeLeft;
            IsSleepy = sleep.IsSleepy;
            TimeSleepy = sleep.TimeSleepy;
        }
        public void Update(SM_Drink drink)
        {
            DrinkNeed = drink.TimeNeed;
            IsHungryWater = drink.IsHungry;
            IsWaterHunger = drink.IsHunger;
            isConsumingFood = drink.State == ConsumeState.Consume;
            TimeWaterHunger = drink.TimeHunger;
        }
        public void Update(SM_Eat eat)
        {
            EatNeed = eat.TimeNeed;
            IsHungryFood = eat.IsHungry;
            IsFoodHunger = eat.IsHunger;
            isConsumingWater = eat.State == ConsumeState.Consume;
            TimeFoodHunger = eat.TimeHunger;
        }
        public void Update(SM_Death death)
        {
            IsAlive = death.IsAlive;
            if(!IsAlive)
            {
                TimeDied = !IsAlive ? SimulationOld.GameAge : 0;
            }
            else
            {
                TimeDied = 0;
            }
        }

        public IAiControlable SelectedAi { get; private set; }
        public void SelectNextAi()
        {
            SelectedAi = _aiRoot.SelectByCycles(SelectedAi ?? CurrentAi, false);
        }
        public void SelectPreviousAi()
        {
            SelectedAi = _aiRoot.SelectByCycles(SelectedAi ?? CurrentAi, true);
        }
        public int CountAvailableAi()
        {
            return _aiRoot.CountAvailable();
        }
    }
}
using System;
using BugsFarm.AnimationsSystem;

namespace BugsFarm.UnitSystem.Obsolete
{
    [Serializable]
    public class SM_Drink : SM_AConsumer
    {
        protected override float ConsumptionTime => Constants.DrinkTime;
        public override string ToString() //TODO : Добавить ключ локализации
        {
            return "попить воды";
        }
        public SM_Drink(int priority, StateAnt aiType) : base(priority,aiType) { }
        protected override void Setup()
        {
            ConsumptionMax = Data.WaterConsumption;
            Consumer.Init(Data.WaterNeedTime, Data.DaysWithoutFoodAndWater, false);
        }
        public override bool TryStart()
        {
            return AvailableTask() && TryOccupyConsumable(Keeper.GetObjects<BowlPresenter>(), out var anyFood)&& 
                   AiRoot.CanReachTarget(anyFood.GetRandomPosition(),anyFood);
        }
        public override bool TaskStart()
        {
            if (!AvailableTask())
            {
                SetStatus(TaskStatus.NotAvailable);
                return false;
            }
            
            if ( TryOccupyConsumable(Keeper.GetObjects<BowlPresenter>(), out var bowl) && OccupyConsumable(bowl))
            {
                SetStatus(TaskStatus.NotReached);
                return base.TaskStart();
            }
            
            SetStatus(TaskStatus.NotAvailable);
            return false;
        }
        protected override void OnStatsUpdate()
        {
            base.OnStatsUpdate();
            AiStats.Update(this);

            if (TryStart())
            {
                AiRoot.AutoTransition(this);
            }
        }
        protected override AnimKey GetAnim()
        {
            return State == ConsumeState.Consume ? AnimKey.Drink : base.GetAnim();
        }
    }
}

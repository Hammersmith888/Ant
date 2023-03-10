using System;
using BugsFarm.AnimationsSystem;
using BugsFarm.Managers;

namespace BugsFarm.UnitSystem.Obsolete
{
    [Serializable]
    public class SM_Eat : SM_AConsumer
    {
        protected override float ConsumptionTime => Constants.EatTime;

        public override string ToString() //TODO : Добавить ключ локализации
        {
            return "покушать";
        }
        public SM_Eat(int priority, StateAnt aiType) : base(priority, aiType){}
        public override bool TryStart()
        {
            return AvailableTask() && 
                   (TryOccupyFresh(out var anyFood) || TryOccupyOther(out anyFood)) &&
                   AiRoot.CanReachTarget(anyFood.GetRandomPosition(),anyFood);
        }
        public override bool TaskStart()
        {
            if (!AvailableTask())
            {
                SetStatus(TaskStatus.NotAvailable);
                return false;
            }

            if (OccupyFresh() || OccupyOther())
            {
                SetStatus(TaskStatus.NotReached);
                return base.TaskStart();
            }
            
            SetStatus(TaskStatus.NotAvailable);
            return false;
        }
        protected override void Setup()
        {
            ConsumptionMax = Data.FoodConsumption;
            Consumer.Init(Data.FoodNeedTime, Data.DaysWithoutFoodAndWater, true);
        }

        private bool TryOccupyFresh(out AConsumable freshFood)
        {
            freshFood = default;
            if (!FreshFood.TryGetFreshFoods(out var freshFoods) || !TryOccupyConsumable(freshFoods, out freshFood))
            {
                return false;
            }

            // Еда должна быть в наличии и не должна находится в строительстве.
            return !freshFood.IsDepleted && freshFood.IsReady;
        }
        private bool OccupyFresh()
        {
            return TryOccupyFresh(out var freshFood) && OccupyConsumable(freshFood);
        }
        private bool TryOccupyOther(out AConsumable consumable)
        {
            return TryOccupyConsumable(FindFood.ForEat1(), out consumable) ||
                   TryOccupyConsumable(FindFood.ForEat2(), out consumable) ||
                   TryOccupyConsumable(FindFood.ForEat3(), out consumable);
        }
        private bool OccupyOther()
        {
            return TryOccupyOther(out var consumable) && OccupyConsumable(consumable);
        }
        protected override AnimKey GetAnim()
        {
            return State == ConsumeState.Consume ? AnimKey.Eat : base.GetAnim();
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
        protected override void OnNone()
        {
            base.OnNone();
            Keeper.Stats.cb_FoodEaten(Consumed);
        }
    }
}


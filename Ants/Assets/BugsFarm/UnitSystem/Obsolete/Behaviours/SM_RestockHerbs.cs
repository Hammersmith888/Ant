using System;
using BugsFarm.AstarGraph;
using BugsFarm.Objects.Stock.Utils;
using BugsFarm.SimulationSystem.Obsolete;

namespace BugsFarm.UnitSystem.Obsolete
{
    [Serializable]
    public class SM_RestockHerbs : SM_ARestock<HerbsDummy>
    {
        private readonly int _grassMask = 1 << NodeUtils.GetTagID("Grass");
        protected override ObjType RestockType => ObjType.HerbsStock;
        protected override int RestockSubType => 0;
        [field:NonSerialized] protected override HerbsDummy FromRestock { get; set; }

        public SM_RestockHerbs(int priority, StateAnt aiType) : base(priority, aiType){}
        public override void Setup(SM_AntRoot root)
        {
            base.Setup(root);
            FromRestock = new HerbsDummy();
        }
        protected override bool OccupyRestock()
        {
            if (!TryOccupyStock(out var stock))
            {
                return false;
            }
            Restock = stock;
            Restock.SetSubscriber(this, true);
            return true;
        }
        protected override void FreeFromRestock(){}
        protected override void OnGoToFrom()
        {
            if (AnyCompleted())
            {
                Transition(RestockState.None);
                return;
            }
            SetStatus(TaskStatus.InProcess);
            PointFrom = FromRestock.GetRandomPosition(AiRoot.AntPresenter.AntType, _grassMask);
            MoveToTarget(PointFrom);
            SetAnim();
        }
        protected override void OnTakeInstant()
        {
            if (AnyCompleted())
            {
                Transition(RestockState.None);
                return;
            }
            SetStatus(TaskStatus.InProcess);
            CalcAmmoutCarry(FromRestock.QuantityMax); // посчитать сколько можно взять 
            if (TimerTask.IsReady) // если мы потянулись за едой но время задачи закончилось, то это последняя итерация.
            {
                IsLastCycle = true;
            }
            else
            {
                TimeCycleBgn = SimulationOld.GameAge;
            }
            Transition(RestockState.GoToStock);
        }
        protected override bool IsCompleteFrom(StockCheck check, HerbsDummy from = null)
        {
            return from.IsNullOrDefault();
        }
    }
}
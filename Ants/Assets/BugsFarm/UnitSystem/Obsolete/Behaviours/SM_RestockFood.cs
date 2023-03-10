using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.Objects.Stock.Utils;
using BugsFarm.SimulationSystem.Obsolete;

namespace BugsFarm.UnitSystem.Obsolete
{
    [Serializable]
    public class SM_RestockFood : SM_ARestock<Food>
    {
        protected override ObjType RestockType => ObjType.Food;
        protected override int RestockSubType => (int)FoodType.FoodStock;
        protected override Food FromRestock { get; set; }
        
        public override string ToString() //TODO : Добавить ключ локализации
        {
            return "складывать запасы";
        }
        public SM_RestockFood(int priority, StateAnt aiType) : base(priority,aiType){}
        public override void ForceHardFinish()
        {
            switch (State)
            {
                case RestockState.GoToStock:
                case RestockState.PutToStock:
                    FromRestock?.Add(AmountCarry);
                    break; 
            }
            base.ForceHardFinish();
        }
        public override bool TryStart()
        {
            //!Tutorial.Instance.IsRestockAllowed
            return base.TryStart() && TryOccupyFood(out _);
        }
        protected bool TryOccupyFood(IEnumerable<Food> consumables, out Food consumable)
        {
            consumable =  Tools.Shuffle_FisherYates(consumables).FirstOrDefault(x => x.TryOccupyConsumable());
            return IsCompleteFrom(StockCheck.Depleted, consumable);
        }
        protected bool TryOccupyFood(out Food food)
        {
            return TryOccupyFood(FindFood.ForEat1(), out food) || TryOccupyFood(FindFood.ForEat2(), out food);
        }
        protected override bool OccupyRestock()
        {
            if (!TryOccupyStock(out var stock) || !TryOccupyFood(out var food))
            {
                return false;
            }

            FromRestock = food;
            Restock = stock;

            FromRestock.SetSubscriber(this, true);
            Restock.SetSubscriber(this, true);
            return true;
        }
        protected override void OnTakeInstant()
        {
            if (AnyCompleted())
            {
                Transition(RestockState.None);
                return;
            }
            SetStatus(TaskStatus.InProcess);
            CalcAmmoutCarry(FromRestock.QuantityCur); // посчитать сколько можно взять 
            if (TimerTask.IsReady)            // если мы потянулись за едой но время задачи закончилось, то это последняя итерация.
            {
                IsLastCycle = true;
            }
            else
            {
                TimeCycleBgn = SimulationOld.GameAge;
            }
            FromRestock.Add(AmountCarry * -1);
            Transition(RestockState.GoToStock);
        }
        protected override bool IsCompleteFrom(StockCheck check, Food from = null)
        {
            if (from.IsNullOrDefault())
            {
                return false;
            }

            switch (check)
            {
                case StockCheck.Full:     return from.IsFull;
                case StockCheck.Depleted: return from.IsDepleted;
                default:                  return false;
            }
        }
    }
    // [Serializable]
    // public class SM_FoodRestock : AStateMachine<RestockState>, IAiControlable
    // {
    //     public bool IsInterruptible => true;
    //     public StateAnt AiType { get; }
    //     public int Priority { get; }
    //     public ATask Task => this;
    //
    //     [NonSerialized] protected AIMover AiMover;
    //     [NonSerialized] protected AntAnimator Animator;
    //     [NonSerialized] protected SM_AntRoot AiRoot;
    //     [NonSerialized] protected AiStats AiStats;
    //     [NonSerialized] private float _restockAmount;
    //     [NonSerialized] private IPosSide _pointFrom;
    //     [NonSerialized] private IPosSide _pointStock;
    //     
    //     protected virtual ObjType Type { get; } = ObjType.Food;
    //     protected virtual int SubType { get; } = (int)FoodType.FoodStock;
    //     
    //     protected Food Food;
    //     protected IStock StockObject;
    //     protected IPosSide SPointFrom;
    //     protected IPosSide SPointStock;
    //     protected double TimeCycleBgn;
    //     protected float AmountCarry;
    //
    //     internal override void OnSerializeMethod(StreamingContext ctx)
    //     {
    //         base.OnSerializeMethod(ctx);
    //         SPointFrom = _pointFrom.ToSPosSide();
    //         SPointStock = _pointStock.ToSPosSide();
    //     }
    //     internal override void OnDeserializeMethod(StreamingContext ctx)
    //     {
    //         base.OnDeserializeMethod(ctx);
    //         _pointFrom = SPointFrom;
    //         _pointStock = SPointStock;
    //     }
    //     public override string ToString() //TODO : Добавить ключ локализации
    //     {
    //         return "складывать запасы";
    //     }
    //     public SM_FoodRestock(int priority, StateAnt aiType)
    //     {
    //         Priority = priority;
    //         AiType = aiType;
    //     }
    //     public void Setup(SM_AntRoot root)
    //     {
    //         AiRoot = root;
    //         AiMover = root.AiMover;
    //         Animator = root.Animator;
    //         AiStats = root.AiStats;
    //         _restockAmount = Data_Ants.Instance.GetData(AntType.Worker).other[OtherAntParams.RestockAmmout];
    //     }
    //     public void PostLoadRestore()
    //     {
    //         switch (State)
    //         {
    //             case RestockState.GoToFrom:
    //             case RestockState.GoToStock:
    //                 AiMover.OnComplete += OnDestenationComplete;
    //                 break;
    //         }
    //         SetAnim();
    //     }
    //     public void Dispose()
    //     {
    //         if (State == RestockState.None)
    //             return;
    //         Transition(RestockState.None);
    //     }
    //     public override void ForceHardFinish()
    //     {
    //         switch (State)
    //         {
    //             case RestockState.GoToStock:
    //             case RestockState.PutToStock:
    //                 Food?.Add(AmountCarry);
    //                 break; 
    //         }
    //         base.ForceHardFinish();
    //     }
    //
    //     public virtual bool TryStart()
    //     {
    //         //!Tutorial.Instance.IsRestockAllowed
    //         return !Status.IsCurrent() && !AiStats.IsHungry && TryOccupyStock(out _) && TryOccupyFood(out _);
    //     }
    //     public override bool TaskStart()
    //     {
    //         if (AiStats.IsHungry || !OccupyRestock())
    //         {
    //             SetStatus(TaskStatus.NotAvailable);
    //             return false;
    //         }
    //
    //         SetStatus(TaskStatus.NotReached);
    //         IsLastCycle = false;
    //         TimeCycleBgn = Simulation.GameAge;
    //         SetTaskTimer();
    //         Transition(RestockState.GoToFrom);
    //         return true;
    //     }
    //     public override void Update()
    //     {
    //         if (!State.IsNullOrDefault())
    //         {
    //             if (Animator.IsAnimComplete)
    //             {
    //                 switch (State)
    //                 {
    //                     case RestockState.TakeFrom:
    //                         Transition(RestockState.TakeInstant);
    //                         break;
    //                     case RestockState.PutToStock:
    //                         StockObject.Add(AmountCarry);
    //                         if (IsLastCycle)
    //                         {
    //                             Transition(RestockState.None);
    //                             return;
    //                         }
    //
    //                         Transition(RestockState.GoToFrom);
    //                         break;
    //                 }
    //             }
    //         }
    //     }
    //
    //     protected void FreeFood()
    //     {
    //         Food?.SetSubscriber(this, false);
    //         Food = null;
    //     }
    //     protected void FreeStock()
    //     {
    //         StockObject?.SetSubscriber(this, false);
    //         StockObject = null;
    //     }
    //     protected bool TryOccupyConsumable<T>(IEnumerable<T> consumables, out T consumable) where  T: AConsumable
    //     {
    //         consumable =  Tools.Shuffle_FisherYates(consumables).FirstOrDefault(x => x.TryOccupyConsumable());
    //         return IsAvailable(StockCheck.Depleted, consumable);
    //     }
    //     protected virtual bool TryOccupyStock(out IStock stock)
    //     {
    //         stock = Stock.FindMore(Stock.Find(Type,SubType),StockCheck.Depleted);
    //
    //         if (stock.IsNullOrDefault()) // Stock не найден
    //         {
    //             if (!Stock.TrySpawn(Type,SubType, out stock)) // Попытаться создать Stock
    //             {
    //                 return false;
    //             }
    //         }
    //
    //         return IsAvailable(StockCheck.Full, (AConsumable)stock);
    //     }
    //     protected bool TryOccupyFood(out Food food)
    //     {
    //         if (TryOccupyConsumable(FindFood.ForEat1(), out food) || TryOccupyConsumable(FindFood.ForEat2(), out food))
    //         {
    //             return IsAvailable(StockCheck.Depleted , food);
    //         }
    //         return false;
    //     }
    //     protected bool OccupyRestock()
    //     {
    //         if (!TryOccupyStock(out var stock) || !TryOccupyFood(out var food))
    //         {
    //             return false;
    //         }
    //
    //         Food = food;
    //         StockObject = stock;
    //
    //         Food.SetSubscriber(this, true);
    //         StockObject.SetSubscriber(this, true);
    //         return true;
    //     }
    //
    //     protected override void HandleObjectEvent(APublisher publisher, ObjEvent objEvent)
    //     {
    //         if (publisher == Food) // Food events
    //         {
    //             switch (objEvent)
    //             {
    //                 case ObjEvent.Moved:
    //                     _pointFrom = Food.GetRandomPosition();
    //
    //                     if (State == RestockState.GoToFrom)
    //                     {
    //                         MoveToTarget(_pointFrom);
    //                     }
    //                     break;
    //
    //                 case ObjEvent.Destroyed:
    //                 case ObjEvent.IsDepleted:
    //                 case ObjEvent.BuildUpgradeBgn: 
    //                     Transition(RestockState.None);
    //                     break;
    //             }
    //         }
    //         else if (publisher == StockObject) // Stock events
    //         {
    //             switch (objEvent)
    //             {
    //                 case ObjEvent.Moved:
    //                     _pointStock = StockObject.GetRandomPosition();
    //
    //                     if (State == RestockState.GoToStock)
    //                     {
    //                         MoveToTarget(_pointStock);
    //                     }
    //                     break;
    //                 case ObjEvent.Destroyed:
    //                 case ObjEvent.IsDepleted: 
    //                     Transition(RestockState.None);
    //                     break;
    //             }
    //         }
    //     }
    //     protected override void OnEnter()
    //     {
    //         switch (State)
    //         {
    //             case RestockState.GoToFrom:        OnGoToFood(); break;
    //             case RestockState.TakeFrom:        OnTakeFood(); break;
    //             case RestockState.TakeInstant: OnTakeFoodInstant(); break;
    //             case RestockState.GoToStock:       OnGotoStock(); break;
    //             case RestockState.PutToStock:      OnPutToStock(); break;
    //             case RestockState.None:            OnNone(); break;
    //         }
    //     }
    //
    //     private void OnGoToFood()
    //     {
    //         if (!CheckWip())
    //         {
    //             Transition(RestockState.None);
    //             return;
    //         }
    //         SetStatus(TaskStatus.InProcess);
    //         _pointFrom = Food.GetRandomPosition();
    //         MoveToTarget(_pointFrom);
    //         SetAnim();
    //     }
    //     private void OnTakeFood()
    //     {
    //         if (!CheckWip())
    //         {
    //             Transition(RestockState.None);
    //             return;
    //         }
    //         SetStatus(TaskStatus.InProcess);
    //         SetLookDir(_pointFrom);
    //         SetAnim();
    //     }
    //     protected void OnTakeFoodInstant()
    //     {
    //         if (!CheckWip())
    //         {
    //             Transition(RestockState.None);
    //             return;
    //         }
    //         SetStatus(TaskStatus.InProcess);
    //         CalcAmmoutCarry(ref AmountCarry); // посчитать сколько можно взять 
    //         if (TimerTask.IsReady)            // если мы потянулись за едой но время задачи закончилось, то это последняя итерация.
    //         {
    //             IsLastCycle = true;
    //         }
    //         else
    //         {
    //             TimeCycleBgn = Simulation.GameAge;
    //         }
    //         Food.Add(AmountCarry * -1);
    //         Transition(RestockState.GoToStock);
    //     }
    //     private void OnGotoStock()
    //     {
    //         if (!CheckWip())
    //         {
    //             Transition(RestockState.None);
    //             return;
    //         }
    //         SetStatus(TaskStatus.InProcess);
    //         _pointStock = StockObject.GetRandomPosition();
    //         MoveToTarget(_pointStock);
    //         SetAnim();
    //     }
    //     private void OnPutToStock()
    //     {
    //         SetStatus(TaskStatus.InProcess);
    //         SetLookDir(_pointStock);
    //         SetAnim();
    //     }
    //     private void OnNone()
    //     {
    //         AiMover.OnComplete -= OnDestenationComplete;
    //         SetStatus(TaskStatus.Completed);
    //         FreeFood();
    //         FreeStock();
    //
    //         AmountCarry = 0;
    //         TimeCycleBgn = 0;
    //         IsLastCycle = false;
    //     }
    //     protected virtual void OnDestenationComplete()
    //     {
    //         switch (State)
    //         {
    //             case RestockState.GoToFrom:  Transition(RestockState.TakeFrom); break;
    //             case RestockState.GoToStock: Transition(RestockState.PutToStock); break;
    //         }
    //         AiMover.OnComplete -= OnDestenationComplete;
    //     }
    //     protected void CalcAmmoutCarry(ref float amountCarry)
    //     {
    //         var timeCycleEnd = Math.Min(Simulation.GameAge, TimerTask.End);
    //         amountCarry = (float)((timeCycleEnd - TimeCycleBgn) / TimerTask.Duration) * _restockAmount;
    //         amountCarry = Mathf.Clamp(amountCarry, 0, Food.QuantityCur);
    //     }
    //
    //     protected bool IsAvailable(StockCheck check, AConsumable consumable = null)
    //     {
    //         if (consumable.IsNullOrDefault())
    //         {
    //             return false;
    //         }
    //
    //         switch (check)
    //         {
    //             case StockCheck.Full:     return !consumable.IsFull;
    //             case StockCheck.Depleted: return !consumable.IsDepleted;
    //             default:                       return false;
    //         }
    //     }
    //     private void MoveToTarget(IPosSide point)
    //     {
    //         AiMover.OnComplete += OnDestenationComplete;
    //         AiMover.GoTarget(point.Position);
    //     }
    //     private void SetLookDir(IPosSide point)
    //     {
    //         AiMover.SetLook(point.LookLeft);
    //     }
    //     private void SetAnim()
    //     {
    //         Animator.SetAnim(GetAnim());
    //     }
    //     private AntAnim GetAnim()
    //     {
    //         switch (State)
    //         {
    //             case RestockState.GoToFrom:  return AntAnim.Walk;
    //             case RestockState.TakeFrom:
    //                 return AnimRefs_Ant.GetTakeAnim(Food.FoodType);
    //             case RestockState.GoToStock: return AntAnim.FeedCarry;
    //             case RestockState.PutToStock:
    //                 return AntAnim.Put;
    //             default: return AntAnim.Breath;
    //         }
    //     }
    //     private bool CheckWip()
    //     {
    //         return !State.IsNullOrDefault() &&
    //                (IsAvailable(StockCheck.Full, (AConsumable)StockObject) || IsAvailable(StockCheck.Depleted, Food));
    //     }
    // }
}
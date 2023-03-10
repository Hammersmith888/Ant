using System;
using System.Runtime.Serialization;
using BugsFarm.AnimationsSystem;
using BugsFarm.BuildingSystem;
using BugsFarm.Objects.Stock.Utils;
using BugsFarm.SimulationSystem.Obsolete;
using BugsFarm.UnitSystem.Obsolete.Components;
using UnityEngine;

namespace BugsFarm.UnitSystem.Obsolete
{
    public enum RestockState
    {
        None,

        GoToFrom,
        TakeFrom,
        TakeInstant,
        GoToStock,
        PutToStock
    }
    [Serializable]
    public abstract class SM_ARestock<TPlacable> : AStateMachine<RestockState>, IAiControlable where TPlacable : APlaceable
    {
        public bool IsInterruptible => true;
        public StateAnt AiType { get; }
        public int Priority { get; }
        public ATask Task => this;
    
        [NonSerialized] protected AIMover AiMover;
        [NonSerialized] protected AntAnimator Animator;
        [NonSerialized] protected SM_AntRoot AiRoot;
        [NonSerialized] protected AiStats AiStats;
        [NonSerialized] protected IPosSide PointFrom;
        [NonSerialized] protected IPosSide PointStock;
        [NonSerialized] protected float RestockAmount;
        protected abstract ObjType RestockType { get;}
        protected abstract int RestockSubType { get;}
        protected abstract TPlacable FromRestock { get; set; }
        
        protected IStock Restock;
        protected IPosSide SPointFrom;
        protected IPosSide SPointStock;
        protected double TimeCycleBgn;
        protected float AmountCarry;

        internal override void OnSerializeMethod(StreamingContext ctx)
        {
            base.OnSerializeMethod(ctx);
            //SPointFrom = PointFrom.ToSPosSide();
            //SPointStock = PointStock.ToSPosSide();
        }
        internal override void OnDeserializeMethod(StreamingContext ctx)
        {
            base.OnDeserializeMethod(ctx);
            PointFrom = SPointFrom;
            PointStock = SPointStock;
        }
        public SM_ARestock(int priority, StateAnt aiType)
        {
            Priority = priority;
            AiType = aiType;
        }
        public virtual void Setup(SM_AntRoot root)
        {
            AiRoot = root;
            AiMover = root.AiMover;
            Animator = root.Animator;
            AiStats = root.AiStats;
            RestockAmount = Data_Ants.Instance.GetData(AiRoot.AntPresenter.AntType).other[OtherAntParams.AmmountCarry];
        }
        public virtual void PostLoadRestore()
        {
            switch (State)
            {
                case RestockState.GoToFrom:
                case RestockState.GoToStock:
                    AiMover.OnComplete += OnDestenationComplete;
                    break;
            }
            SetAnim();
        }
        public virtual void Dispose()
        {
            AiMover.OnComplete -= OnDestenationComplete;
        }

        public virtual bool TryStart()
        {
            return !Status.IsCurrent() && !AiStats.IsHungry && TryOccupyStock(out _);
        }
        public override bool TaskStart()
        {
            if (AiStats.IsHungry || !OccupyRestock())
            {
                SetStatus(TaskStatus.NotAvailable);
                return false;
            }

            SetStatus(TaskStatus.NotReached);
            IsLastCycle = false;
            TimeCycleBgn = SimulationOld.GameAge;
            SetTaskTimer();
            Transition(RestockState.GoToFrom);
            return true;
        }
        public override void Update()
        {
            if (!State.IsNullOrDefault())
            {
                if (Animator.IsAnimComplete)
                {
                    switch (State)
                    {
                        case RestockState.TakeFrom:
                            Transition(RestockState.TakeInstant);
                            break;
                        case RestockState.PutToStock:
                            Restock.Add(AmountCarry);
                            if (IsLastCycle)
                            {
                                Transition(RestockState.None);
                                return;
                            }

                            Transition(RestockState.GoToFrom);
                            break;
                    }
                }
            }
        }

        protected virtual void FreeFromRestock()
        {
            FromRestock?.SetSubscriber(this, false);
            FromRestock = null;
        }
        protected virtual void FreeStock()
        {
            Restock?.SetSubscriber(this, false);
            Restock = null;
        }
        protected virtual bool TryOccupyStock(out IStock stock)
        {
            stock = Stock.Find(RestockType,RestockSubType).FindMore(StockCheck.Depleted);

            if (stock.IsNullOrDefault()) // Stock не найден
            {
                if (!Stock.TrySpawn(RestockType,RestockSubType, out stock)) // Попытаться создать Stock
                {
                    return false;
                }
            }
            
            return !IsCompleteStock(StockCheck.Full, stock);
        }
        protected abstract bool OccupyRestock();
        
        protected override void HandleObjectEvent(APublisher publisher, ObjEvent objEvent)
        {
            if (publisher == FromRestock) // Food events
            {
                switch (objEvent)
                {
                    case ObjEvent.Moved:
                        PointFrom = FromRestock.GetRandomPosition();

                        if (State == RestockState.GoToFrom)
                        {
                            MoveToTarget(PointFrom);
                        }
                        break;

                    case ObjEvent.Destroyed:
                    case ObjEvent.IsDepleted:
                    case ObjEvent.BuildUpgradeBgn: 
                        Transition(RestockState.None);
                        break;
                }
            }
            else if (publisher == Restock) // Stock events
            {
                switch (objEvent)
                {
                    case ObjEvent.Moved:
                        PointStock = Restock.GetRandomPosition();

                        if (State == RestockState.GoToStock)
                        {
                            MoveToTarget(PointStock);
                        }
                        break;
                    case ObjEvent.Destroyed:
                    case ObjEvent.IsDepleted: 
                        Transition(RestockState.None);
                        break;
                }
            }
        }
        protected override void OnEnter()
        {
            switch (State)
            {
                case RestockState.GoToFrom:    OnGoToFrom(); break;
                case RestockState.TakeFrom:    OnTakeFrom(); break;
                case RestockState.TakeInstant: OnTakeInstant(); break;
                case RestockState.GoToStock:   OnGotoStock(); break;
                case RestockState.PutToStock:  OnPutToStock(); break;
                case RestockState.None:        OnNone(); break;
            }
        }
    
        protected virtual void OnGoToFrom()
        {
            if (AnyCompleted())
            {
                Transition(RestockState.None);
                return;
            }
            SetStatus(TaskStatus.InProcess);
            PointFrom = FromRestock.GetRandomPosition();
            MoveToTarget(PointFrom);
            SetAnim();
        }
        protected virtual void OnTakeFrom()
        {
            if (AnyCompleted())
            {
                Transition(RestockState.None);
                return;
            }
            SetStatus(TaskStatus.InProcess);
            SetLookDir(PointFrom);
            SetAnim();
        }
        protected abstract void OnTakeInstant();
        protected virtual void OnGotoStock()
        {
            if (AnyCompleted())
            {
                Transition(RestockState.None);
                return;
            }
            SetStatus(TaskStatus.InProcess);
            PointStock = Restock.GetRandomPosition();
            MoveToTarget(PointStock);
            SetAnim();
        }
        protected virtual void OnPutToStock()
        {
            SetStatus(TaskStatus.InProcess);
            SetLookDir(PointStock);
            SetAnim();
        }
        protected virtual void OnNone()
        {
            AiMover.OnComplete -= OnDestenationComplete;
            SetStatus(TaskStatus.Completed);
            FreeFromRestock();
            FreeStock();

            AmountCarry = 0;
            TimeCycleBgn = 0;
            IsLastCycle = false;
        }
        protected virtual void OnDestenationComplete()
        {
            switch (State)
            {
                case RestockState.GoToFrom:  Transition(RestockState.TakeFrom); break;
                case RestockState.GoToStock: Transition(RestockState.PutToStock); break;
            }
            AiMover.OnComplete -= OnDestenationComplete;
        }
        protected void MoveToTarget(IPosSide point)
        {
            AiMover.OnComplete += OnDestenationComplete;
            AiMover.GoTarget(point.Position);
        }
        protected void SetLookDir(IPosSide point)
        {
            AiMover.SetLook(point.LookLeft);
        }
        protected void SetAnim()
        {
            Animator.SetAnim(GetAnim());
        }
        protected virtual AnimKey GetAnim()
        {
            switch (State)
            {
                case RestockState.GoToFrom: return AnimKey.Walk;
                case RestockState.TakeFrom:
                    return FromRestock.Type == ObjType.Food
                        ? AnimRefs_Ant.GetTakeAnim((FoodType) FromRestock.SubType)
                        : AnimKey.Put;
                case RestockState.GoToStock:  return AnimKey.WalkFood;
                case RestockState.PutToStock: return AnimKey.Put;
                default:                      return AnimKey.Idle;
            }
        }
        protected virtual bool AnyCompleted()
        {
            return State == RestockState.None ||
                   (IsCompleteStock(StockCheck.Full, Restock) || IsCompleteFrom(StockCheck.Depleted, FromRestock));
        }
        protected virtual bool IsCompleteStock(StockCheck check, IStock stock = null)
        {
            if (stock.IsNullOrDefault())
            {
                return false;
            }

            switch (check)
            {
                case StockCheck.Full:     return stock.QuantityCur >= stock.QuantityMax;
                case StockCheck.Depleted: return stock.QuantityCur <= 0;
                default:                  return false;
            }
        }
        protected abstract bool IsCompleteFrom(StockCheck check, TPlacable from = null);
        protected virtual void CalcAmmoutCarry(float fromRestockQuanitity)
        {
            var timeCycleEnd = Math.Min(SimulationOld.GameAge, TimerTask.End);
            AmountCarry = (float)((timeCycleEnd - TimeCycleBgn) / TimerTask.Duration) * RestockAmount;
            AmountCarry = Mathf.Clamp(AmountCarry, 0, fromRestockQuanitity);
        }
    }
}
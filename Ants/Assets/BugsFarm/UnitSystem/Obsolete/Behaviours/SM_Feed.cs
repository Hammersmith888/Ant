using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using BugsFarm.AnimationsSystem;
using BugsFarm.BuildingSystem;
using BugsFarm.UnitSystem.Obsolete.Components;
using Zenject;

namespace BugsFarm.UnitSystem.Obsolete
{
    public enum FeedState
    {
        None,

        GotoFood,
        TakeFood,
        TakeFoodInstant,
        GotoQueen,
        GiveToQueen
    }

    [Serializable]
    public class SM_Feed : AStateMachine<FeedState>, IAiControlable
    {
        public bool IsInterruptible => false;
        public StateAnt AiType { get; }
        public int Priority { get; }

        public ATask Task => this;

        [NonSerialized] private AIMover _aiMover;
        [NonSerialized] private AntAnimator _animator;
        [NonSerialized] private AiStats _aiStats;
        [NonSerialized] private SM_AntRoot _aiRoot;
        [NonSerialized] private FB_CfgAntConsumption _queenConsumption;
        [NonSerialized] private IPosSide _point;
        [NonSerialized] private Data_Other _dataOther;
        
        private QueenPressenter _queen;      
        private Food _food;
        private IPosSide _sPoint;
        
        internal override void OnSerializeMethod(StreamingContext ctx)
        {
            base.OnSerializeMethod(ctx);
            //_sPoint = _point?.ToSPosSide();
        }
        internal override void OnDeserializeMethod(StreamingContext ctx)
        {
            base.OnDeserializeMethod(ctx);
            _point = _sPoint;
        }

        public override string ToString() //TODO : Добавить ключ локализации
        {
            return "еду королеве";
        }
        public SM_Feed(int priority, StateAnt aiType)
        {
            Priority = priority;
            AiType = aiType;
        }
        [Inject]
        private void Inject(Data_Other dataOther)
        {
            _dataOther = dataOther;
        }
        public void Setup(SM_AntRoot root)
        {
            _aiMover = root.AiMover;
            _animator = root.Animator;
            _aiStats = root.AiStats;
            _aiRoot = root;
            _queenConsumption = _dataOther.Other.Queen.Consumption;
            root.OnAiStatsUpdate += OnStatsUpdate;
        }
        public void PostLoadRestore()
        {
            SetAnim();
            switch (State)
            {
                case FeedState.GotoFood:
                case FeedState.GotoQueen:
                    _aiMover.OnComplete += OnDestenationComplete;
                    break;
            }
        }
        public void Dispose()
        {
            FreeFood();
            FreeQueen();
            _aiRoot.OnAiStatsUpdate -= OnStatsUpdate;
            _aiMover.OnComplete -= OnDestenationComplete;
        }
        public bool TryStart()
        {
            if (!AvailableTask())
            {
                return false;
            }

            if (_queen.IsNullOrDefault())
            {
                _queen = Keeper.GetObjects<QueenPressenter>().FirstOrDefault();
            }

            if (!_queen.IsNullOrDefault() && !_queen.IsOccupied && _queen.NeedsFeeder)
            {
                return TryOccupyConsumable(FindFood.ForEat1(), out _) ||
                       TryOccupyConsumable(FindFood.ForEat2(), out _) ||
                       TryOccupyConsumable(FindFood.ForEat3(), out _);
            }

            return false;
        }
        public override bool TaskStart()
        {
            if (!AvailableTask())
            {
                return false;
            }
            
            if (OccupyFood(FindFood.ForEat1()) ||
                OccupyFood(FindFood.ForEat2()) ||
                OccupyFood(FindFood.ForEat3()))
            {
                _queen.Occupy(this);
                _queen.SetSubscriber(this, true);
                _food.SetSubscriber(this, true);
            }
            else
            {
                SetStatus(TaskStatus.NotAvailable);
                return false;
            }
            
            SetStatus(TaskStatus.NotReached);
            Transition(FeedState.GotoFood);
            return true;
        }
        public override void Update()
        { 
            if(!State.IsNullOrDefault() && _animator.IsAnimComplete)
            {
                switch (State)
                {
                    case FeedState.TakeFood:
                        Transition(FeedState.TakeFoodInstant);
                        break;
                    case FeedState.GiveToQueen:
                        _queen.Eat();
                        Transition(FeedState.None);
                        break;
                }
            }
        }

        private void FreeFood()
        {
            _food?.FreeOccupy(this);
            _food?.SetSubscriber(this, false);
            _food = null;
        }
        private void FreeQueen()
        {
            _queen?.Free();
            _queen?.SetSubscriber(this, false);
        }
        private bool OccupyFood(IEnumerable<Food> consumables)
        {
            if (TryOccupyConsumable(consumables, out var consumable) && consumable.OccupyConsumable(this, out var point))
            {
                _food = consumable;
                _point = point;
                return true;
            }
            return false;
        }
        protected bool TryOccupyConsumable<T>(IEnumerable<T> consumables, out T consumable) where T : AConsumable
        {
            consumable = consumables.FirstOrDefault(x => x.TryOccupyConsumable(false));
            return !consumable.IsNullOrDefault();
        }
        
        protected override void HandleObjectEvent(APublisher publisher, ObjEvent objEvent)
        {
            if (publisher == _food) // Food events
                switch (objEvent)
                {
                    case ObjEvent.Moved:
                        if (State == FeedState.GotoFood)
                            GoTo(_food?.GetPointSide(this));
                        break;

                    case ObjEvent.IsDepleted:
                    case ObjEvent.Destroyed:
                    case ObjEvent.BuildUpgradeBgn:
                        Transition(FeedState.None);
                        break;
                }
            else if (publisher == _queen) // Queen events
                switch (objEvent)
                {
                    case ObjEvent.Moved:
                        if (State == FeedState.GotoQueen)
                            GoTo(_queen.Mb.Point);
                        break;

                    case ObjEvent.Destroyed:
                        switch (State)
                        {
                            case FeedState.GotoQueen:
                            case FeedState.TakeFoodInstant:
                                _food?.Add(_queenConsumption.FoodConsumption);
                                goto default;
                            default: Transition(FeedState.None);
                                break;
                        }
                        break;
                }
        }
        protected override void OnEnter()
        {
            switch (State)
            {
                case FeedState.GotoFood:
                    OnGotoFood();
                    break;
                case FeedState.TakeFood:
                    OnFood();
                    break;
                case FeedState.TakeFoodInstant:
                    OnTakeFoodInstant();
                    break;
                case FeedState.GotoQueen:
                    OnGotoQueen();
                    break;
                case FeedState.GiveToQueen:
                    OnGiveToQueen();
                    break;
                case FeedState.None:
                    OnNone();
                    break;
            }
        }
        private void OnGotoFood()
        {
            SetStatus(TaskStatus.InProcess);
            SetAnim();
            GoTo(_point);
        }
        private void OnFood()
        {
            SetStatus(TaskStatus.InProcess);
            _aiMover.SetLook(_point.LookLeft);
            SetAnim();
        }
        private void OnTakeFoodInstant()
        {
            SetStatus(TaskStatus.InProcess);
            float dummy = 0;
            _food.Consume(_queenConsumption.FoodConsumption, _queenConsumption.FoodConsumption, ref dummy);
            FreeFood();

            Transition(FeedState.GotoQueen);
        }
        private void OnGotoQueen()
        {
            SetStatus(TaskStatus.InProcess);
            GoTo(_queen.Mb.Point);
            SetAnim();
        }
        private void OnGiveToQueen()
        {
            SetStatus(TaskStatus.InProcess);
            _aiMover.SetLook(_point.LookLeft);
            SetAnim();
        }
        private void OnNone()
        {
            FreeFood();
            FreeQueen();
            _aiMover.OnComplete -= OnDestenationComplete;
            SetStatus(TaskStatus.Completed);
        }
        private void OnDestenationComplete()
        {
            _aiMover.OnComplete -= OnDestenationComplete;
            switch (State)
            {
                case FeedState.GotoFood:
                    Transition(FeedState.TakeFood);
                    break;
                case FeedState.GotoQueen:
                    Transition(FeedState.GiveToQueen);
                    break;
            }
        }
        private void OnStatsUpdate()
        {
            if (TryStart())
            {
                _aiRoot.AutoTransition(this);
            }
        }
        
        private void GoTo(IPosSide point)
        {
            _point = point;
            _aiMover.OnComplete += OnDestenationComplete;
            _aiMover.GoTarget(point.Position);
        }
        private void SetAnim()
        {
            _animator.SetAnim(GetAnim());
        }
        private AnimKey GetAnim()
        {
            switch (State)
            {
                case FeedState.GotoFood:  return AnimKey.Walk;
                case FeedState.TakeFood:  return AnimRefs_Ant.GetTakeAnim(_food.FoodType);
                case FeedState.GotoQueen: return AnimKey.WalkFood;
                case FeedState.GiveToQueen: return AnimKey.Put;
                default: return AnimKey.None;
            }
        }
        protected virtual bool AvailableTask()
        {
            return !Status.IsCurrent() && !_aiStats.IsHungry;
        }
    }
}
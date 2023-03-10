using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using BugsFarm.AnimationsSystem;
using BugsFarm.BuildingSystem;
using BugsFarm.Objects.Stock.Utils;
using BugsFarm.SimulationSystem.Obsolete;
using BugsFarm.UnitSystem.Obsolete.Components;

namespace BugsFarm.UnitSystem.Obsolete
{
    public enum CleanState
    {
        None,

        GotoGarbage,
        TakeGarbage,
        TakeGarbageInstant,
        GotoDumpster,
        PutToDumpster
    }

    [Serializable]
    public class SM_Clean : AStateMachine<CleanState>, IAiControlable
    {
        public bool IsInterruptible => true;
        public StateAnt AiType { get; }
        public int Priority { get; }
        public ATask Task => this;
        
        [NonSerialized] private AIMover _aiMover;
        [NonSerialized] private AntAnimator _animator;
        [NonSerialized] private AiStats _aiStats;
        [NonSerialized] private List<Food> _garbages;
        [NonSerialized] private IPosSide _pointGarbage;
        [NonSerialized] private IPosSide _pointDumpster;

        private Food _garbage;
        private Food _dumpster;
        private IPosSide _sPointGarbage;
        private IPosSide _sPointDumpster;
        private double _timeCycleBgn;
        private float _amountCarry;

        internal override void OnSerializeMethod(StreamingContext ctx)
        {
            base.OnSerializeMethod(ctx);
            //_sPointGarbage = _pointGarbage.ToSPosSide();
           // _sPointDumpster = _pointDumpster.ToSPosSide();
        }
        internal override void OnDeserializeMethod(StreamingContext ctx)
        {
            base.OnDeserializeMethod(ctx);
            _pointGarbage = _sPointGarbage;
            _pointDumpster = _sPointDumpster;
        }
        
        public override string ToString() //TODO : Добавить ключ локализации
        {
            return "уборка";
        }
        public SM_Clean(int priority,StateAnt aiType)
        {
            Priority = priority;
            AiType = aiType;
        }
        public void Setup(SM_AntRoot root)
        {
            _aiMover = root.AiMover;
            _animator = root.Animator;
            _aiStats = root.AiStats;
            _garbages = new List<Food>();
        }
        public override void ForceHardFinish()
        {
            switch (State)
            {
                case CleanState.GotoDumpster:
                case CleanState.PutToDumpster:
                    _garbage?.Add(_amountCarry);
                    break;
            }
            
            base.ForceHardFinish();
        }
        public void PostLoadRestore()
        {
            SetAnim();
            switch (State)
            {
                case CleanState.GotoGarbage:
                case CleanState.GotoDumpster:
                    _aiMover.OnComplete += OnDestenationComplete;
                    break;
            }
        }
        public void Dispose()
        {
            FreeOccupy();
        }
        public bool TryStart()
        {
            return !Status.IsCurrent() && !_aiStats.IsHungry && FindAllGarbages() &&
                   (TryOccupyDumpster(out _dumpster) && TryOccupyGarbage(out _garbage)) || // попытаться окупировать этап1
                   (TryOccupyPileAsDumpster(out _dumpster) && TryOccupyGarbage(out _garbage, true)); // попытаться окупировать этап2
        }
        public override bool TaskStart()
        {
            if (TryOccupyDumpster(out _dumpster) && TryOccupyGarbage(out _garbage))
            {
                return OccupyCleaning();
            }
            
            if (TryOccupyPileAsDumpster(out _dumpster) && TryOccupyGarbage(out _garbage, true))
            {
                return OccupyCleaning();
            }
            
            SetStatus(TaskStatus.NotAvailable);
            return false;
        }
        public override void Update()
        {
            if (!State.IsNullOrDefault())
            {
                if (TimerTask.IsReady)
                {
                    Transition(CleanState.None);
                    return;
                }
                if (_animator.IsAnimComplete)
                {
                    switch (State)
                    {
                        case CleanState.TakeGarbage:
                            Transition(CleanState.TakeGarbageInstant);
                            break;
                        case CleanState.PutToDumpster:
                            _dumpster.Add(_amountCarry);
                            Transition(IsLastCycle ? CleanState.None : CleanState.GotoGarbage);
                            break;
                    }
                }
            }
        }

        private bool OccupyCleaning()
        {
            if (OccupyDumpster(_dumpster) && OccupyGarbage(_garbage))
            {
                _garbage.SetSubscriber(this, true);
                _dumpster.SetSubscriber(this, true);
                _garbages.Clear();
            
                IsLastCycle = false;
                _timeCycleBgn = SimulationOld.GameAge;
                SetTaskTimer();
                SetStatus(TaskStatus.NotReached);
                Transition(CleanState.GotoGarbage);
                return true;
            }
            SetStatus(TaskStatus.NotAvailable);
            return false;
        }
        private void FreeOccupy()
        {
            _dumpster?.FreeOccupy(this);
            _garbage?.FreeOccupy(this);
            _garbage?.SetSubscriber(this, false);
            _dumpster?.SetSubscriber(this, false);
            IsLastCycle = false;
            _garbage = null;
            _dumpster = null;
        }

    #region Найти весь мусор в игре
        private bool FindAllGarbages()
        {
            _garbages.Clear();
            _garbages.AddRange(FindFood.Garbage());
            _garbages.Sort((a, b) => a.QuantityCur.CompareTo(b.QuantityCur)); // sort: smaller garbage first
            return _garbages.Any();
        }
    #endregion 
    #region Окупировать мусорный контейнер
        private bool TryOccupyDumpster(out Food dumpster)
        {
            dumpster = FindFood.Dumpster();
            
            if (dumpster.IsNullOrDefault() || dumpster.IsFull)
            {
                return false;
            }

            return dumpster.TryOccupyConsumable(true);
        }
        private bool OccupyDumpster(Food dumpster)
        {
            return !dumpster.IsNullOrDefault() && dumpster.OccupyConsumable(this, out _pointDumpster);
        }
    #endregion
    #region Окупировать мусор
        private bool TryOccupyGarbage(out Food grabage, bool excludePile = false)
        {
            grabage = _garbages?.FirstOrDefault(x =>
            {
                if (x.FoodType == FoodType.PileStock && excludePile)
                {
                    return false;
                }

                return x.TryOccupyConsumable();
            });
            return !grabage.IsNullOrDefault();
        }
        private bool OccupyGarbage(Food grabage)
        {
            return !grabage.IsNullOrDefault() && grabage.OccupyConsumable(this, out _pointGarbage);
        }
    #endregion
    #region Окупировать кучу мусора как мусорный контейнер
        private bool TryOccupyPileAsDumpster(out Food dumpster)
        {
            dumpster = _garbages.Where(x =>  x.FoodType == FoodType.PileStock && !x.IsFull)
                       .OrderByDescending(x => x.QuantityCur)
                       .FirstOrDefault();

            if (dumpster.IsNullOrDefault())
            {
                if (Stock.TrySpawn(ObjType.Food,(int)FoodType.PileStock, out var dumpsterStock))
                {
                    dumpster = (Food)dumpsterStock;
                }
                else
                {
                    return false;
                }
            }

            return dumpster?.TryOccupyConsumable(true) ?? false;
        }
    #endregion
        protected override void HandleObjectEvent(APublisher publisher, ObjEvent objEvent)
        {
            if (publisher == _garbage) // Garbage events
            {
                switch (objEvent)
                {
                    case ObjEvent.Moved:
                        _pointGarbage = _garbage.GetPointSide(this);

                        if (State == CleanState.GotoGarbage)
                        {
                            GoTo(_pointGarbage);
                        }
                        break;

                    case ObjEvent.IsDepleted:
                    case ObjEvent.Destroyed:
                        Transition(CleanState.None);
                        break;
                }
            }
            else if (publisher == _dumpster) // Dumpster events
            {
                switch (objEvent)
                {
                    case ObjEvent.Moved:
                        _pointDumpster = _dumpster.GetPointSide(this);

                        if (State == CleanState.GotoDumpster)
                        {
                            GoTo(_pointDumpster);
                        }

                        break;

                    case ObjEvent.Destroyed:
                    case ObjEvent.BuildUpgradeBgn:
                        Transition(CleanState.None);
                        break;
                }
            }
        }
        protected override void OnEnter()
        {
            switch (State)
            {
                case CleanState.GotoGarbage:
                    OnGotoGarbage();
                    break;
                case CleanState.TakeGarbage:
                    OnTakeGarbage();
                    break;
                case CleanState.TakeGarbageInstant:
                    OnTakeGarbageInstant();
                    break;
                case CleanState.GotoDumpster:
                    OnGotoDumpster();
                    break;
                case CleanState.PutToDumpster:
                    OnPutToDumpster();
                    break;
                case CleanState.None:
                    OnNone();
                    break;
            }
        }
        private void OnGotoGarbage()
        {
            GoTo(_pointGarbage);
            SetAnim();
        }
        private void OnTakeGarbage()
        {
            SetLookDir(_pointGarbage);
            SetAnim();
        }
        private void OnTakeGarbageInstant()
        {
            CalcAmmoutCarry(ref _amountCarry);
            var dummy = 0f;
            var totalTgt = _amountCarry * 2; // to prevent auto-free

            _garbage?.Consume(totalTgt, _amountCarry, ref dummy); // Depleted, Destroyed events possible
            _timeCycleBgn = SimulationOld.GameAge;
            Transition(CleanState.GotoDumpster);
        }
        private void OnGotoDumpster()
        {
            GoTo(_pointDumpster);
            SetAnim();
        }
        private void OnPutToDumpster()
        {
            SetLookDir(_pointDumpster);
            SetAnim();
        }
        private void OnNone()
        {
            FreeOccupy();
            SetStatus(TaskStatus.Completed);
        }
        private void OnDestenationComplete()
        {
            _aiMover.OnComplete -= OnDestenationComplete;
            switch (State)
            {
                case CleanState.GotoGarbage:
                    Transition(CleanState.TakeGarbage);
                    break;
                case CleanState.GotoDumpster:
                    Transition(CleanState.PutToDumpster);
                    break;
            }
        }
        
        protected void CalcAmmoutCarry(ref float amountCarry)
        {
            var timeCycleEnd = Math.Min(SimulationOld.GameAge, TimerTask.End);
            amountCarry = (float)((timeCycleEnd - _timeCycleBgn) / TimerTask.Duration) * Constants.GarbageQuantity;
        }
        private void GoTo(IPosSide point)
        {
            _aiMover.OnComplete += OnDestenationComplete;
            _aiMover.GoTarget(point.Position);
        }
        private void SetLookDir(IPosSide point)
        {
            _aiMover.SetLook(point.LookLeft);
        }
        private void SetAnim()
        {
            _animator.SetAnim(GetAnim(State));
        }
        private AnimKey GetAnim(CleanState state)
        {
            switch (state)
            {
                case CleanState.GotoGarbage:  return AnimKey.Walk;
                case CleanState.TakeGarbage:  return AnimKey.TakeGarbage;
                case CleanState.GotoDumpster: return AnimKey.WalkGarbage;
                case CleanState.PutToDumpster:
                    return _dumpster is DumpsterStockPresenter ? AnimKey.GarbageDropRecycler : AnimKey.GarbageDropPile;
                default: return AnimKey.None;
            }
        }
    }
}
using System;
using System.Runtime.Serialization;
using BugsFarm.AnimationsSystem;
using BugsFarm.AudioSystem.Obsolete;
using BugsFarm.BuildingSystem;
using BugsFarm.Game;
using BugsFarm.Objects.Stock.Utils;
using BugsFarm.SimulationSystem.Obsolete;
using BugsFarm.UnitSystem.Obsolete.Components;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BugsFarm.UnitSystem.Obsolete
{
    public enum BuildingVineState
    {
        None,
        
        Build,
        Carry,
        GoHerbs,
        TakeHerbs,
        TakeHerbsInstant
    }
    [Serializable]
    public class SM_VineBuild : AStateMachine<BuildingVineState>, IAiControlable, ISoundOccupant
    {
        private static readonly BuildingType[] _variants =
        {
            new BuildingType(AnimKey.Build1, new Vector2Int(10, 10), Sound.Hammer),
            new BuildingType(AnimKey.Build2, new Vector2Int(10, 10), Sound.Hammer),
            new BuildingType(AnimKey.Build3, new Vector2Int(10, 12), Sound.Saw)
        };
        private BuildingType variant => _variants[_variantIndex];
        public bool IsInterruptible => true;
        public StateAnt AiType { get; }
        public int Priority { get; }
        public ATask Task => this;
        public bool CanSoundPlay => true;
        [field:NonSerialized] public event Action<Sound> OnSoundChange;
        [field:NonSerialized] public event Action<ISoundOccupant> OnSoundFree;

        [NonSerialized] private AIMover _aiMover;
        [NonSerialized] private AntAnimator _animator;
        [NonSerialized] private AiStats _aiStats;
        [NonSerialized] private SM_AntRoot _aiRoot;
        [NonSerialized] private float _workAmount;
        [NonSerialized] private IPosSide _workPoint;
        [NonSerialized] private IPosSide _herbsPoint;
        
        private HerbsStockPresenter _herbsStock;
        private WorkPlace _workPlace;
        private IPosSide _sPointWorkPlace;
        private IPosSide _sPointHerbs;
        private double _timeCycleBgn;
        private float _amountCarry;
        private int _variantIndex;
        private int _nCycles;
        private int _cycle;

        internal override void OnSerializeMethod(StreamingContext ctx)
        {
            base.OnSerializeMethod(ctx);
            //_sPointWorkPlace = _workPoint.ToSPosSide();
            //_sPointHerbs = _herbsPoint.ToSPosSide();
        }
        internal override void OnDeserializeMethod(StreamingContext ctx)
        {
            base.OnDeserializeMethod(ctx);
            _workPoint = _sPointWorkPlace;
            _herbsPoint = _sPointHerbs;
        }
        public override string ToString() //TODO : Добавить ключ локализации
        {
            return "плетение лозы";
        }
        public SM_VineBuild(int priority,StateAnt aiType)
        {
            Priority = priority;
            AiType = aiType;
        }
        public void Setup(SM_AntRoot root)
        {
            _animator = root.Animator;
            _aiMover = root.AiMover;
            _aiStats = root.AiStats;
            _aiRoot = root;
            _workAmount = Data_Ants.Instance.GetData(root.AntPresenter.AntType).other[OtherAntParams.AmmountCarry];
        }
        public void PostLoadRestore()
        {
            switch (State)
            {
                case BuildingVineState.Carry:
                    _aiMover.OnComplete += OnDestenationComplete;
                    break;
                case BuildingVineState.Build: SetSound();
                    break;
            }
            SetAnim();
        }
        public override void Update()
        {
            if(State.IsNullOrDefault()) return;
            
            if (TimerTask.IsReady || _workPlace.IsNullOrDefault() || _workPlace.IsCompleted)
            {
                if (TimerTask.IsReady && !IsLastCycle)
                {
                    IsLastCycle = true;
                }
                else
                {
                    Transition(BuildingVineState.None);
                    return;     
                }
            }
            if (_animator.IsAnimComplete)
            {
                switch (State)
                {
                    case BuildingVineState.Build:
                        if (++_cycle >= _nCycles)
                        {
                            _workPlace?.Process(Mathf.RoundToInt(_workAmount));
                            Transition(BuildingVineState.GoHerbs);
                        }
                        else
                        {
                            SetAnim();
                            SetSound();
                        }

                        break;
                    case BuildingVineState.TakeHerbs:
                        Transition(BuildingVineState.TakeHerbsInstant);
                        break;
                }
            }
        }
        public void Dispose()
        {
            if (State.IsNullOrDefault()) return;
            _aiMover.OnComplete -= OnDestenationComplete;
            FreeOccupy();
            OnSoundFree?.Invoke(this);
        }
        public bool TryStart()
        {
            return !Status.IsCurrent() && 
                   !_aiStats.IsHungry && 
                   FindHerbsStock() && 
                   FindWorkPlace() &&
                   _workPlace.TryOccupy();
        }
        public override bool TaskStart()
        {
            if (!TryStart())
            {
                SetStatus(TaskStatus.NotAvailable);
                return false;
            }

            _workPoint = _workPlace.Occupy();
            _workPlace.SetSubscriber(this, true);

            _herbsStock.SetSubscriber(this, true);
            _herbsPoint = _herbsStock.GetRandomPosition();
            SetStatus(TaskStatus.NotReached);

            SetTaskTimer();
            _timeCycleBgn = SimulationOld.GameAge;
            Transition(BuildingVineState.GoHerbs);
            return true;
        }

        private bool FindHerbsStock()
        {
            const float takeToWorkPercent = 1f;
            _herbsStock = Stock.Find<HerbsStockPresenter>().FindMore(StockCheck.Full);
            if (!_herbsStock.IsNullOrDefault())
            {
                return (_herbsStock.QuantityCur / _herbsStock.QuantityMax) * 100f >= takeToWorkPercent;
            }
            
            return false;
        }
        private bool FindWorkPlace()
        {
            if(_workPlace.IsNullOrDefault() || _workPlace.IsCompleted)
            {
                //_workPlace = GameInit.FarmServices.Rooms.CurrentWork;
            }

            return _workPlace is BuildVinePlace;
        }
        protected override void HandleObjectEvent(APublisher publisher, ObjEvent objEvent)
        {
            if (publisher == _herbsStock) // HerbsStock events
            {
                switch (objEvent)
                {
                    case ObjEvent.Moved:
                    case ObjEvent.Destroyed:
                    case ObjEvent.IsDepleted:
                        if (FindHerbsStock())
                        {
                            _herbsPoint = _herbsStock.GetRandomPosition();
                            if(State == BuildingVineState.GoHerbs)
                            {
                                _aiMover.GoTarget(_herbsPoint.Position);
                            }
                        }
                        break;
                }
            }
            else if (publisher == _workPlace) // WorkPlace events
            {
                if (objEvent != ObjEvent.RoomOpened)
                {
                    return;
                }

                Transition(BuildingVineState.None);
            }
        }
        protected override void OnEnter()
        {
            switch (State)
            {
                case BuildingVineState.GoHerbs:          OnGoHerbs();   break;
                case BuildingVineState.TakeHerbs:        OnTakeHerbs(); break;
                case BuildingVineState.TakeHerbsInstant: OnTakeHerbsInstant(); break;
                case BuildingVineState.Carry:            OnCarry(); break;
                case BuildingVineState.Build:            OnBuild(); break;
                case BuildingVineState.None:             OnNone();  break;
            }
        }
        private void OnGoHerbs()
        {
            SetStatus(TaskStatus.InProcess);
            _aiMover.OnComplete += OnDestenationComplete;
            _aiMover.GoTarget(_herbsPoint.Position);
            OnSoundFree?.Invoke(this);
            SetAnim();
        }
        private void OnTakeHerbs()
        {
            SetStatus(TaskStatus.InProcess);
            _aiMover.SetLook(_herbsPoint.LookLeft);
            SetAnim();
        }
        private void OnTakeHerbsInstant()
        {
            SetStatus(TaskStatus.InProcess);
            CalcAmmoutCarry(_herbsStock.QuantityCur); // посчитать сколько можно взять 
            _timeCycleBgn = SimulationOld.GameAge;
            _herbsStock.Remove(_amountCarry);
            Transition(BuildingVineState.Carry);
        }
        private void OnCarry()
        {
            SetStatus(TaskStatus.InProcess);
            _aiMover.OnComplete += OnDestenationComplete;
            _aiMover.GoTarget(_workPoint.Position);
            OnSoundFree?.Invoke(this);
            SetAnim();
        }
        private void OnBuild()
        {
            SetStatus(TaskStatus.InProcess);
            _variantIndex = Random.Range(0, _variants.Length);
            _nCycles = Random.Range(variant.CyclesRange.x, variant.CyclesRange.y + 1);
            _cycle = 0;
            _aiMover.SetLook(_workPoint.LookLeft);
            SetAnim();
            SetSound();
        }
        private void OnNone()
        {
            FreeOccupy();
            OnSoundFree?.Invoke(this);
            _aiMover.OnComplete -= OnDestenationComplete;
            SetStatus(TaskStatus.Completed);
            _timeCycleBgn = 0;
        }
        private void OnDestenationComplete()
        {
            switch (State)
            {
                case BuildingVineState.Carry:
                    Transition(BuildingVineState.Build);
                    break;
                case BuildingVineState.GoHerbs:
                    Transition(BuildingVineState.TakeHerbs);
                    break;
            }
            _aiMover.OnComplete -= OnDestenationComplete;
        }
    
        private void SetAnim()
        {
            _animator.SetAnim(GetAnim());
        }
        private AnimKey GetAnim()
        {
            switch (State)
            {
                case BuildingVineState.Carry:     return AnimKey.WalkFood;
                case BuildingVineState.Build:     return variant.Anim;
                case BuildingVineState.GoHerbs:   return AnimKey.Walk;
                case BuildingVineState.TakeHerbs: return AnimKey.Put;
                default:                          return AnimKey.None;
            }
        }
        private void FreeOccupy()
        {
            _workPlace?.FreeOccupy(_workPoint);
            _herbsStock?.SetSubscriber(this, false);
            _workPlace?.SetSubscriber(this, false);
            _herbsStock = null;
            _workPlace = null;
        }
        private void SetSound()
        {
            if(SimulationOld.Any) return;
            GameEvents.OnSoundQueue?.Invoke(this);
            OnSoundChange?.Invoke(variant.Sound);
        }
        protected virtual void CalcAmmoutCarry(float fromStockQuanitity)
        {
            var timeCycleEnd = Math.Min(SimulationOld.GameAge, TimerTask.End);
            _amountCarry = (float)((timeCycleEnd - _timeCycleBgn) / TimerTask.Duration) * _workAmount;
            _amountCarry = Mathf.Clamp(_amountCarry, 0, fromStockQuanitity);
        }
    }
}
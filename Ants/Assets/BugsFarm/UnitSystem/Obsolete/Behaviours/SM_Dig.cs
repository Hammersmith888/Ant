using System;
using System.Linq;
using System.Runtime.Serialization;
using BugsFarm.AnimationsSystem;
using BugsFarm.AudioSystem.Obsolete;
using BugsFarm.BuildingSystem;
using BugsFarm.Game;
using BugsFarm.UnitSystem.Obsolete.Components;
using UnityEngine;

namespace BugsFarm.UnitSystem.Obsolete
{
    public enum DigState
    {
        None,

        GoDig,
        Dig,
        Carry,
        Throw,
    }


    [Serializable]
    public class SM_Dig : AStateMachine<DigState>, IAiControlable, ISoundOccupant
    {
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
        [NonSerialized] private float _digAmount;
        [NonSerialized] private IPosSide _digPoint;

        private readonly Timer _timerDigging = new Timer();
        private WorkPlace _digPlace;
        private DigGroundStock digGroundStock;
        private IPosSide _sPoint;

        internal override void OnSerializeMethod(StreamingContext ctx)
        {
            base.OnSerializeMethod(ctx);
            //_sPoint = _digPoint.ToSPosSide();
        }
        internal override void OnDeserializeMethod(StreamingContext ctx)
        {
            base.OnDeserializeMethod(ctx);
            _digPoint = _sPoint;
        }
        public override string ToString() //TODO : Добавить ключ локализации
        {
            return "раскопки";
        }
        public SM_Dig(int priority,StateAnt aiType)
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
            _digAmount = Data_Ants.Instance.GetData(root.AntPresenter.AntType).other[OtherAntParams.WorkAmount];
        }
        public void PostLoadRestore()
        {
            switch (State)
            {
                case DigState.GoDig:
                case DigState.Carry:
                    _aiMover.OnComplete += OnDestenationComplete;
                    break;
                case DigState.Dig:
                case DigState.Throw: SetSound();
                    break;
            }
            SetAnim();
        }
        public override void Update()
        {
            if(State.IsNullOrDefault()) return;
            
            if(State != DigState.Carry && State != DigState.Throw)
            {
                if (TimerTask.IsReady || _digPlace.IsNullOrDefault() || _digPlace.IsCompleted)
                {
                    Transition(DigState.None);
                    return;  
                }
            }

            if (_animator.IsAnimComplete)
            {
                switch (State)
                {
                    case DigState.Dig:
                        if (!_timerDigging.IsReady)
                        {
                            SetAnim();
                            SetSound();
                        }
                        else
                        {
                            _digPlace?.Process(Mathf.RoundToInt(_digAmount));
                            Transition(DigState.Carry);
                        }

                        break;
                    case DigState.Throw:
                        digGroundStock?.Add(Mathf.RoundToInt(_digAmount));
                        Transition(DigState.GoDig);
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
                   FindDigGround() && 
                   FindDigPlace() &&
                   _digPlace.TryOccupy();
        }
        public override bool TaskStart()
        {
            if (!TryStart())
            {
                SetStatus(TaskStatus.NotAvailable);
                return false;
            }

            _digPoint = _digPlace.Occupy();
            _digPlace.SetSubscriber(this, true);

            digGroundStock?.SetSubscriber(this, true);
            SetStatus(TaskStatus.NotReached);

            SetTaskTimer();
            Transition(DigState.GoDig);
            return true;
        }

        private bool FindDigGround()
        {
            if(digGroundStock.IsNullOrDefault())
                digGroundStock = Keeper.GetObjects<DigGroundStock>().FirstOrDefault();

            return !digGroundStock.IsNullOrDefault();
        }
        private bool FindDigPlace()
        {
            //if(_digPlace.IsNullOrDefault() || _digPlace.IsCompleted)
            //    _digPlace = GameInit.FarmServices.Rooms.CurrentWork;

            return _digPlace is DigPlace;
        }
        protected override void HandleObjectEvent(APublisher publisher, ObjEvent objEvent)
        {
            if (publisher == digGroundStock) // DigGround events
            {
                switch (objEvent)
                {
                    case ObjEvent.Moved:
                    case ObjEvent.Destroyed:
                        FindDigGround();
                        if (State == DigState.Carry)
                        {
                            _aiMover.GoTarget(digGroundStock.Mb.Point.Position);
                        }
                        break;
                }
            }
            else if (publisher == _digPlace) // DigPlace events
            {
                if (objEvent != ObjEvent.RoomOpened)
                {
                    return;
                }

                switch (State)
                {
                    case DigState.GoDig:
                        Transition(DigState.None);
                        break;

                    case DigState.Dig:
                        Transition(DigState.Carry);
                        break;
                }
            }
        }
        protected override void OnEnter()
        {
            switch (State)
            {
                case DigState.GoDig: OnGoDig(); break;
                case DigState.Dig:   OnDig();   break;
                case DigState.Carry: OnCarry(); break;
                case DigState.Throw: OnThrow(); break;
                case DigState.None:  OnNone();  break;
            }
        }
        private void OnGoDig()
        {
            SetStatus(TaskStatus.InProcess);
            _aiMover.OnComplete += OnDestenationComplete;
            _aiMover.GoTarget(_digPoint.Position);
            OnSoundFree?.Invoke(this);
            SetAnim();
        }
        private void OnDig()
        {
            SetStatus(TaskStatus.InProcess);
            _timerDigging.Set(Tools.RandomRange(Constants.Dig_DigTime));
            _aiMover.SetLook(_digPoint.LookLeft);
            SetAnim();
            SetSound();
        }
        private void OnCarry()
        {
            SetStatus(TaskStatus.InProcess);
            _aiMover.OnComplete += OnDestenationComplete;
            var point = digGroundStock.Point;
            _aiMover.GoTarget(point.Position);
            SetAnim();
            OnSoundFree?.Invoke(this);
        }
        private void OnThrow()
        {
            SetStatus(TaskStatus.InProcess);
            SetAnim();
            SetSound();
        }
        private void OnNone()
        {
            FreeOccupy();
            OnSoundFree?.Invoke(this);
            _aiMover.OnComplete -= OnDestenationComplete;
            SetStatus(TaskStatus.Completed);
        }
        private void OnDestenationComplete()
        {
            switch (State)
            {
                case DigState.Carry:
                    var point = digGroundStock.Mb.Point;
                    _aiMover.SetLook(point.LookLeft);
                    Transition(DigState.Throw);
                    break;
                case DigState.GoDig:
                    Transition(DigState.Dig);
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
                case DigState.GoDig: return AnimKey.Walk;
                case DigState.Dig: return _digPlace.IsWorkDown ? AnimKey.DigLow : AnimKey.DigMidle;
                case DigState.Carry: return AnimKey.WalkMud;
                case DigState.Throw: return AnimKey.DropMud;
                default: return AnimKey.None;
            }
        }
        private void FreeOccupy()
        {
            _digPlace?.FreeOccupy(_digPoint);
            digGroundStock?.SetSubscriber(this, false);
            _digPlace?.SetSubscriber(this, false);
            digGroundStock = null;
            _digPlace = null;
        }
        private void SetSound()
        {
            GameEvents.OnSoundQueue?.Invoke(this);
            if (!CanSoundPlay) 
                return;

            switch (State)
            {
                case DigState.Dig:
                    OnSoundChange?.Invoke(Tools.RandomItem(Sound.Dig_1, Sound.Dig_2, Sound.Dig_3));
                    break;
                case DigState.Throw:
                    OnSoundChange?.Invoke(Tools.RandomItem(Sound.GroundToPile_1, Sound.GroundToPile_2));
                    break;
            }
        }
    }
}
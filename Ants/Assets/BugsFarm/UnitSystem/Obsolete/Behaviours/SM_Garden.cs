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
    public enum GardenState
    {
        None,

        GoGarden,
        Garden,
    }

    public struct GardeningType
    {
        public readonly AnimKey Anim;
        public Vector2Int CyclesRange;

        public GardeningType(AnimKey anim, Vector2Int cyclesRange)
        {
            Anim = anim;
            CyclesRange = cyclesRange;
        }
    }

    [Serializable]
    public class SM_Garden : AStateMachine<GardenState>, IAiControlable, ISoundOccupant
    {
        public bool IsInterruptible => true;
        public StateAnt AiType { get; }
        public int Priority { get; }
        public ATask Task => this;
        public bool CanSoundPlay => _soundCycle % 3 == 1;
        [field:NonSerialized] public event Action<Sound> OnSoundChange;
        [field:NonSerialized] public event Action<ISoundOccupant> OnSoundFree;
        
        private static readonly GardeningType ver1 = new GardeningType(AnimKey.Garden1, new Vector2Int(5, 8));
        private static readonly GardeningType ver2 = new GardeningType(AnimKey.Garden2, new Vector2Int(2, 3));

        [NonSerialized] private AIMover _aiMover;
        [NonSerialized] private AntAnimator _animator;
        [NonSerialized] private AiStats _aiStats;
        [NonSerialized] private IPosSide _point;
        
        private int _soundCycle;
        private int _cycle;
        private GardenPresenter _garden;
        private bool _gardeningStarted;
        private int _nCycles;
        private IPosSide _sPoint;
        private bool _ver1;
        
        internal override void OnSerializeMethod(StreamingContext ctx)
        {
            base.OnSerializeMethod(ctx);
            //_sPoint = _point.ToSPosSide();
        }
        internal override void OnDeserializeMethod(StreamingContext ctx)
        {
            base.OnDeserializeMethod(ctx);
            _point = _sPoint;
        }
        
        public override string ToString() //TODO : Добавить ключ локализации
        {
            return "огород";
        }
        public SM_Garden(int priority,StateAnt aiType)
        {
            Priority = priority;
            AiType = aiType;
        }
        public void Setup(SM_AntRoot root)
        {
            _aiMover = root.AiMover;
            _animator = root.Animator;
            _aiStats = root.AiStats;
        }
        public void PostLoadRestore()
        {
            switch (State)
            { 
                case GardenState.GoGarden:
                    _aiMover.OnComplete += OnDestenationComplete;
                    SetAnim();
                    break;
            }
        }
        public void Dispose()
        {
            FreeOccupy();
            OnSoundFree?.Invoke(this);
            _aiMover.OnComplete -= OnDestenationComplete;
        }
        public bool TryStart()
        {
            return !Status.IsCurrent() && !_aiStats.IsHungry && TryOccupyGarden(out _);
        }
        public override bool TaskStart()
        {
            if (_aiStats.IsHungry || !OccupyGarden())
            {
                SetStatus(TaskStatus.NotAvailable);
                return false;
            }

            _ver1 = Tools.RandomBool();
            _gardeningStarted = false;

            SetTaskTimer();
            SetStatus(TaskStatus.NotReached);
            Transition(GardenState.GoGarden);

            return true;
        }
        public override void Update()
        {
            if (TimerTask.IsReady)
            {
                Transition(GardenState.None);
                return;
            }

            if (State == GardenState.Garden && _animator.IsAnimComplete)
            {
                _cycle++;
                if (_cycle >= _nCycles)
                {
                    Transition(GardenState.GoGarden);
                }
                else
                {
                    SetAnim();
                    SetSound();
                }
            }
        }

        private bool TryOccupyGarden(out GardenPresenter garden)
        {
            garden = FindFood.Gardening().FirstOrDefault(x => x.TryOccupyBuilding());
            return !garden.IsNullOrDefault();
        }
        private bool OccupyGarden()
        {
            if (TryOccupyGarden(out var garden) && garden.OccupyBuilding())
            {
                _garden = garden;
                _garden.SetSubscriber(this, true);
                return true;
            }
            return false;
        }
        private void FreeOccupy()
        {
            _garden?.FreeBuilding();
            _garden?.SetSubscriber(this, false);
            _garden = null;
        }
        
        protected override void HandleObjectEvent(APublisher publisher, ObjEvent objEvent)
        {
            switch (objEvent)
            {
                case ObjEvent.Moved:
                    Transition(GardenState.GoGarden);
                    break;

                case ObjEvent.Destroyed:
                case ObjEvent.BuildUpgradeBgn:
                    Transition(GardenState.None);
                    break;
            }
        }
        protected override void OnEnter()
        {
            switch (State)
            {
                case GardenState.GoGarden:
                    OnGoGarden();
                    break;
                case GardenState.Garden:
                    OnGarden();
                    break;
                case GardenState.None:
                    OnNone();
                    break;
            }
        }
        private void OnGoGarden()
        {
            SetStatus(TaskStatus.InProcess);
            SetAnim();
            OnSoundFree?.Invoke(this);
            _soundCycle++;
            _point = _garden.GetRandomPosition(_point);
            _aiMover.OnComplete += OnDestenationComplete;
            _aiMover.GoTarget(_point.Position);
        }
        private void OnGarden()
        {
            SetStatus(TaskStatus.InProcess);
            _gardeningStarted = true;
            _nCycles = Tools.RandomRange(Getver().CyclesRange);
            _cycle = 0;

            _aiMover.SetLook(_point.LookLeft);

            SetAnim();
            SetSound();
        }
        private void OnNone()
        {
            if (_gardeningStarted)
            {
                _garden?.SetTimer();
            }

            FreeOccupy();
            OnSoundFree?.Invoke(this);
            SetStatus(TaskStatus.Completed);
            _aiMover.OnComplete -= OnDestenationComplete;
        }
        private void OnDestenationComplete()
        {
            _aiMover.OnComplete -= OnDestenationComplete;
            if (State == GardenState.GoGarden)
            {
                Transition(GardenState.Garden);
            }
        }
        
        private void SetAnim()
        {
            _animator.SetAnim(GetAnim());
        }
        private AnimKey GetAnim()
        {
            switch (State)
            {
                case GardenState.GoGarden: return AnimKey.Walk;
                case GardenState.Garden: return Getver().Anim;
                default: return AnimKey.None;
            }
        }
        private GardeningType Getver()
        {
            return _ver1 ? ver1 : ver2;
        }
        private void SetSound()
        {
            GameEvents.OnSoundQueue?.Invoke(this);
            OnSoundChange?.Invoke(Tools.RandomItem(Sound.Gardening_1, Sound.Gardening_2));
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using BugsFarm.AnimationsSystem;
using BugsFarm.AudioSystem.Obsolete;
using BugsFarm.BuildingSystem;
using BugsFarm.Game;
using BugsFarm.SimulationSystem.Obsolete;
using BugsFarm.UnitSystem.Obsolete.Components;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BugsFarm.UnitSystem.Obsolete
{
    public enum BuildState
    {
        None,

        GoBuild,
        Build,
    }

    public struct BuildingType
    {
        public readonly AnimKey Anim;
        public Vector2Int CyclesRange;
        public readonly Sound Sound;

        public BuildingType( AnimKey anim, Vector2Int cyclesRange, Sound sound)
        {
            Anim = anim;
            CyclesRange = cyclesRange;
            Sound = sound;
        }
    }

    [Serializable]
    public class SM_Build : AStateMachine<BuildState>, IAiControlable, ISoundOccupant
    {
        private static readonly BuildingType[] _variants =
        {
            new BuildingType(AnimKey.Build1, new Vector2Int(10, 10), Sound.Hammer),
            new BuildingType(AnimKey.Build2, new Vector2Int(10, 10), Sound.Hammer),
            new BuildingType(AnimKey.Build3, new Vector2Int(10, 12), Sound.Saw)
        };
        public bool IsInterruptible => true;
        public StateAnt AiType { get; }
        public int Priority { get; }
        public ATask Task => this;
        public bool CanSoundPlay => _soundCycle % 3 == 1;
        [field:NonSerialized] public event Action<Sound> OnSoundChange;
        [field:NonSerialized] public event Action<ISoundOccupant> OnSoundFree;

        [NonSerialized] private AIMover _aiMover;
        [NonSerialized] private AntAnimator _animator;
        [NonSerialized] private AiStats _aiStats;
        [NonSerialized] private SM_AntRoot _aiRoot;
        [NonSerialized] private IPosSide _point;
        // Нужно, чтобы весь муравейник не реагировал на доступное место ремонта.
        [NonSerialized] private static List<APlaceable> _preOccupied;
        private BuildingType variant => _variants[_variantIndex];
        
        private APlaceable _building;
        private IPosSide _sPoint;
        private int _variantIndex;
        private int _nCycles;
        private int _cycle;
        private static int _soundCycle;

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
            return "строительство";
        }
        public SM_Build(int priority,StateAnt aiType)
        {
            Priority = priority;
            AiType = aiType;
        }
        public void Setup(SM_AntRoot root)
        {
            _aiMover = root.AiMover;
            _animator = root.Animator;
            _aiStats = root.AiStats;
            _aiRoot = root;
            GameEvents.OnObjectBought += OnObjectsActions;
            GameEvents.OnObjectUpgradeBgn += OnObjectsActions;
            if (_preOccupied.IsNullOrDefault())
            {
                _preOccupied = new List<APlaceable>();
            }
        }
        private void OnObjectsActions(APlaceable placeable)
        {
            if(placeable.IsNullOrDefault()) return;
            if(!AvailableTask()) return;
            // так как порядок вызовов не корректный, нужно подождать, чтобы все инициализации были выполненны.
            // в противном случае занимаемые позиции будут в 0 0 0 координатах.
            Observable.Timer(TimeSpan.FromSeconds(Time.deltaTime)).Subscribe(xs => ForceTransition());
        }
        private void ForceTransition()
        {
            if (_building.IsNullOrDefault() && TryStart())
            {
                if (!_aiRoot.AutoTransition(this))
                {
                    FreeOccupy();
                    SetStatus(TaskStatus.NotAvailable);
                }
            }
        }
        
        public void Dispose()
        {
            FreeOccupy();
        }
        public void PostLoadRestore()
        {
            SetAnim();
            if (State == BuildState.GoBuild)
            {
                _aiMover.OnComplete += OnDestenationComplete;
                return;
            }
            if (!_building.IsNullOrDefault() && !_preOccupied.Contains(_building))
            {
                _preOccupied.Add(_building);
            }
            SetSound();
        }
        public bool TryStart()
        {
            if (AvailableTask())
            {
                // Если ранее заняли, значит мы все еще можем начать работу.
                if (!_building.IsNullOrDefault())
                {
                    return true;
                }
                // Попытаемся временно занять работу.
                if (TryOccupy(out _building))
                {
                    _preOccupied.Add(_building);
                    // Если в течении времени работа не была занята, то освободить.
                    Observable.Timer(TimeSpan.FromSeconds(SimulationOld.DeltaTime)).Subscribe(xs =>
                    {
                        // Уже в работе
                        if(!AvailableTask()) return;
                        // Никто не занял и мы в том чесле, освободить стройку.
                        FreeOccupy();
                    });
                
                    return true;
                }
            }
            return false;
        }
        public override bool TaskStart()
        {
            if (!TryOccupy(out _building, true) || !_building.OccupyBuilding())
            {
                SetStatus(TaskStatus.NotAvailable);
                FreeOccupy();
                return false;
            }

            _building.SetSubscriber(this, true);
            
            SetTaskTimer();
            SetStatus(TaskStatus.NotReached);
            Transition(BuildState.GoBuild);
            return true;
        }
        public override void Update()
        {
            if (TimerTask.IsReady)
            {
                Transition(BuildState.None);
                return;
            }
            if (_animator.IsAnimComplete && State == BuildState.Build)
            {
                if (++_cycle >= _nCycles)
                {
                    Transition(BuildState.GoBuild);
                }
                else
                {
                    SetAnim();
                    SetSound();
                }
            }
        }

        private bool TryOccupy(out APlaceable building, bool preOccupyAllowed = false)
        {
            building = Keeper.Buildings.FirstOrDefault(x => !x.IsReady && x.TryOccupyBuilding() &&
                                                            (preOccupyAllowed || !_preOccupied.Contains(x)));
            return !building.IsNullOrDefault();
        }
        private void FreeOccupy()
        {
            _building?.FreeBuilding();
            _building?.SetSubscriber(this, false);
            
            _preOccupied.Remove(_building);
            _building = null;
        }

        protected override void HandleObjectEvent(APublisher publisher, ObjEvent objEvent)
        {
            switch (objEvent)
            {
                case ObjEvent.Moved:
                    _building.BuildUpgradeTimer.Pause();
                    Transition(BuildState.GoBuild);
                    break;

                case ObjEvent.Destroyed:
                    Transition(BuildState.None);
                    break;

                case ObjEvent.BuildUpgradeEnd:
                    Transition(BuildState.None);
                    break;
            }
        }
        protected override void OnEnter()
        {
            switch (State)
            {
                case BuildState.GoBuild:
                    OnGoBuild();
                    break;
                case BuildState.Build:
                    OnBuild();
                    break;
                case BuildState.None:
                    OnNone();
                    break;
            }
        }
        private void OnGoBuild()
        {
            SetStatus(TaskStatus.InProcess);
           _soundCycle++;
           _aiMover.OnComplete += OnDestenationComplete;
           _point = _building.GetRandomPosition();
           _aiMover.GoTarget(_point.Position);
           SetAnim();
           OnSoundFree?.Invoke(this);
        }
        private void OnBuild()
        {
            SetStatus(TaskStatus.InProcess);
            _variantIndex = Random.Range(0, _variants.Length);
            _nCycles = Random.Range(variant.CyclesRange.x, variant.CyclesRange.y + 1);
            _cycle = 0;

            _aiMover.SetLook(_point.LookLeft);
            _building.BuildUpgradeTimer.Unpause();
            SetAnim();
            SetSound();
        }
        private void OnNone()
        {
            FreeOccupy();
            OnSoundFree?.Invoke(this);
            SetStatus(TaskStatus.Completed);
        }
        private void OnDestenationComplete()
        {
            _aiMover.OnComplete -= OnDestenationComplete;
            if (State == BuildState.GoBuild)
            {
                Transition(BuildState.Build);
            }
        }

        private void SetSound()
        {
            if(SimulationOld.Any) return;
            GameEvents.OnSoundQueue?.Invoke(this);
            OnSoundChange?.Invoke(variant.Sound);
        }
        private void SetAnim()
        {
            _animator.SetAnim(GetAnim());
        }
        private AnimKey GetAnim()
        {
            switch (State)
            {
                case BuildState.GoBuild: return AnimKey.Walk;
                case BuildState.Build:   return variant.Anim;
                default: return AnimKey.None;
            }
        }
        protected virtual bool AvailableTask()
        {
            return !Status.IsCurrent() && !_aiStats.IsHungry;
        }
    }
}
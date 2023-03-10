using System;
using System.Linq;
using BugsFarm.AnimationsSystem;
using BugsFarm.AudioSystem.Obsolete;
using BugsFarm.BuildingSystem;
using BugsFarm.Game;
using BugsFarm.SimulationSystem.Obsolete;
using BugsFarm.UnitSystem.Obsolete.Components;
using UnityEngine;
using Event = Spine.Event;

namespace BugsFarm.UnitSystem.Obsolete
{
    public enum MiningState
    {
        None,

        GoLever,
        Lever,
        GoRock,
        WaitLever,
        Rock,

        TaskPaused
    }

    [Serializable]
    public class SM_Goldmine : AStateMachine<MiningState>, IAiControlable, ISoundOccupant
    {
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

        private Timer _timerMining = new Timer();
        private bool _mineReached;
        private int _minedTotal;
        private int _hits;
        private GoldminePresenter _goldmine;
        private int _soundCycle;

        public override string ToString() //TODO : Добавить ключ локализации
        {
            return "добыча золота";
        }
        public SM_Goldmine(int priority, StateAnt aiType)
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
        public override void ForceHardFinish()
        {
            if (State.IsNullOrDefault())
            {
                return; // Normal task end
            }
            
            // Task paused >>>
            TimerTask.Pause();
            _timerMining.Pause();
            Transition(MiningState.TaskPaused);
            OnNone();
            base.ForceHardFinish();
        }

        public void PostLoadRestore()
        {
            SetAnim();
            switch (State)
            {
                case MiningState.Rock:
                    SetSound();
                    break;
                case MiningState.GoLever:
                case MiningState.GoRock:
                    _aiMover.OnComplete += OnDestenationComplete;
                    break;
                case MiningState.WaitLever :
                case MiningState.Lever :
                    Transition(MiningState.Rock);
                    break;
            }
        }
        public void Dispose()
        {
            FreeGoldmine();
        }
        public bool TryStart()
        {
            return !Status.IsCurrent() && !_aiStats.IsHungry && TryOccupyGoldmine(out _);
        }
        public override bool TaskStart()
        {
            
            if (_aiStats.IsHungry || !OccupyGoldmine())
            {
                SetStatus(TaskStatus.NotAvailable);
                return false;
            }

            switch (State)
            {
                case MiningState.None: // Task start
                    _minedTotal = 0;
                    _mineReached = false;
                    IsLastCycle = false;
                    SetTaskTimer();

                    break;

                case MiningState.TaskPaused: // Task resume
                    TimerTask.Unpause();
                    _timerMining.Unpause();
                    break;
            }

            SetStatus(TaskStatus.NotReached);
            Transition(MiningState.GoLever);
            return true;
        }
        public override void Update()
        {
            if (!_goldmine.IsNullOrDefault() && _goldmine.MbGoldmine.IsAnimComplete)
            {
                if (State == MiningState.WaitLever)
                {
                    Transition(MiningState.Rock);
                }
            }
            if (_animator.IsAnimComplete)
            {
                switch (State)
                {
                    case MiningState.Lever:
                        Transition(MiningState.GoRock);
                        break;
                    case MiningState.Rock:
                        RockProcess();
                        break;
                }
            }
        }
        protected bool TryOccupyGoldmine(out GoldminePresenter goldmine)
        {
            goldmine = Keeper.GetObjects<GoldminePresenter>().FirstOrDefault(x => x.TryOccupyGoldmine());
            return !goldmine.IsNullOrDefault();
        }
        private bool OccupyGoldmine()
        {
            if (TryOccupyGoldmine(out var goldmine))
            {
                if (goldmine.OccupyGoldmine())
                {
                    _goldmine = goldmine;
                    _goldmine.SetSubscriber(this, true);
                    return true;
                }
            }
            return false;
        }
        private void FreeGoldmine()
        {
            if (!_goldmine.IsNullOrDefault())
            {
                _goldmine.MbGoldmine.OnSpineEvent -= OnSpineEvent;
                _goldmine.OccupyFree();
                _goldmine.SetSubscriber(this, false);
                _goldmine = null;
            }
        }
        
        protected override void HandleObjectEvent(APublisher publisher, ObjEvent objEvent)
        {
            switch (objEvent)
            {
                case ObjEvent.Destroyed:
                case ObjEvent.BuildUpgradeBgn:
                    Transition(MiningState.None);
                    break;

                case ObjEvent.Moved:
                    Transition(MiningState.GoLever);
                    break;
            }
        }
        protected override void OnEnter()
        {
            switch (State)
            {
                case MiningState.GoLever:
                    OnGoLever();
                    break;
                case MiningState.Lever:
                    OnLever();
                    break;
                case MiningState.GoRock:
                    OnGoRock();
                    break;
                case MiningState.WaitLever:
                    OnWaitLever();
                    break;
                case MiningState.Rock:
                    OnRock();
                    break;
                case MiningState.None:
                    OnNone();
                    break;
            }
        }
        private void OnGoLever()
        {
            _soundCycle++;
            GoTo(_goldmine.MbGoldmine.PointLever);
            SetAnim();
        }
        private void OnLever()
        {
            if (!_mineReached)
            {
                _mineReached = true;

                _timerMining.Set((float) (TimerTask.End - SimulationOld.GameAge));
            }

            SetAnim();
            _aiMover.SetLook(_goldmine.MbGoldmine.PointLever.LookLeft);
            _goldmine.MbGoldmine.Lever();
            _goldmine.MbGoldmine.OnSpineEvent += OnSpineEvent;
        }
        private void OnGoRock()
        {
            SetAnim();
            GoTo(_goldmine.MbGoldmine.PointRock);
        }
        private void OnWaitLever()
        {
            SetAnim();
        }
        private void OnRock()
        {
            _hits = 0;

            _goldmine.MbGoldmine.Idle();
            
            _aiMover.SetLook(_goldmine.MbGoldmine.PointRock.LookLeft);
            CrumbleRock();
        }
        private void OnNone()
        {
            OnSoundFree?.Invoke(this);
            FreeGoldmine();
            SetStatus(TaskStatus.Completed);
            _aiMover.OnComplete -= OnDestenationComplete;
        }
        
        private void OnSpineEvent(Event spineEvent)
        {
            if (!spineEvent.IsNullOrDefault() && spineEvent.Data.Name == "Sound")
            {
                SetSound(Sound.GoldmineRockLift);
                if(!_goldmine.IsNullOrDefault())
                    _goldmine.MbGoldmine.OnSpineEvent -= OnSpineEvent;
            }
        }
        private void OnDestenationComplete()
        {
            _aiMover.OnComplete -= OnDestenationComplete;
            switch (State)
            {
                case MiningState.GoLever:
                    Transition(MiningState.Lever);
                    break;
                case MiningState.GoRock: Transition(MiningState.WaitLever);
                    break;
            }
        }

        private void CrumbleRock()
        {
            SetAnim();
            SetSound();
        }
        private void SetSound(Sound? sound = null)
        {
            GameEvents.OnSoundQueue?.Invoke(this);
            sound = sound ?? Tools.RandomItem(Sound.Mattock_1, Sound.Mattock_3);
            OnSoundChange?.Invoke(sound.Value);
        }
        private void RockProcess()
        {
            if (++_hits >= cfg_Goldmine.hits_Max)
            {
                var goldPerCycle = _goldmine.GoldPerCycle;
                var minedTotal = Mathf.FloorToInt(_timerMining.Progress * goldPerCycle);
                var mined = Mathf.Clamp(minedTotal - _minedTotal, 0, goldPerCycle); // To prevent strange numbers after config changes
                _minedTotal = minedTotal;

                var done =
                        _goldmine.AddGold(mined) || // (!) Must be first! First - put gold to the goldmine.
                        IsLastCycle ||
                        _timerMining.IsReady ;

                Transition(done ? MiningState.None : MiningState.GoLever);
            }
            else
            {
                CrumbleRock();
            }
        }
        private void GoTo(IPosSide point)
        {
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
                case MiningState.Lever:  return AnimKey.MineLever;
                case MiningState.GoLever:
                case MiningState.GoRock: return AnimKey.Walk;
                case MiningState.Rock:   return AnimKey.MineRock;
                case MiningState.WaitLever: return AnimKey.Idle;
                default: return AnimKey.None;
            }
        }
    }
}
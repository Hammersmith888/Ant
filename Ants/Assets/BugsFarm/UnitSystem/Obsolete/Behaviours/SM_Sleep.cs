using System;
using BugsFarm.AnimationsSystem;
using BugsFarm.SimulationSystem.Obsolete;
using BugsFarm.UnitSystem.Obsolete.Components;

namespace BugsFarm.UnitSystem.Obsolete
{
    public enum SleepState
    {
        None,

        GoSleep,
        Sleep,
        Awake,

        TaskPaused,
    }


    [Serializable]
    public class SM_Sleep : AStateMachine<SleepState>, IAiControlable
    {
        /// <summary>
        /// Когда жучек должен спать
        /// </summary>
        public bool ShouldSleep => _timerAwake.IsReady && State == SleepState.None;
        /// <summary>
        /// Отсчет времени когда бодорствование прикратиться.
        /// </summary>
        public float AwakeLeft => _timerAwake.Left;
        /// <summary>
        /// Когда жучек сонный
        /// </summary>
        public bool IsSleepy => _timerAwake.IsReady;
        /// <summary>
        /// Сколько времени жучек сонный
        /// </summary>
        public float TimeSleepy => (float)(SimulationOld.GameAge - _timerAwake.End);
        public bool IsInterruptible => true;
        public StateAnt AiType { get; }
        public int Priority { get; }
        public ATask Task => this;

        [NonSerialized] private AIMover _aiMover;
        [NonSerialized] private AntAnimator _animator;
        [NonSerialized] private AiStats _aiStats;
        [NonSerialized] private SM_AntRoot _aiRoot;

        private readonly Timer _timerAwake = new Timer();
        public override string ToString() //TODO : Добавить ключ локализации
        {
            return "спать";
        }
        public SM_Sleep(int aiPriority, StateAnt aiType)
        {
            Priority = aiPriority;
            AiType = aiType;
        }
        public void Setup(SM_AntRoot rootAi)
        {
            _aiRoot = rootAi;
            _aiMover = rootAi.AiMover;
            _animator = rootAi.Animator;
            _aiStats = rootAi.AiStats;
            _aiRoot.OnAiStatsUpdate += OnStatsUpdate;
            _aiRoot.OnPostSpawnInit += OnPostSpawnInit;
        }
        public override void ForceHardFinish()
        {
            if (State == SleepState.Sleep && _animator.HasAnim(AnimKey.Awake))
            {
                Transition(SleepState.Awake);
                return;
            }
            base.ForceHardFinish();
        }

        public void PostLoadRestore()
        {
            SetAnim();
            if (State == SleepState.GoSleep)
            {
                _aiMover.OnComplete += OnDestenationComplete;
            }
        }
        public void Dispose() 
        {
            _aiRoot.OnAiStatsUpdate -= OnStatsUpdate;
            _aiRoot.OnPostSpawnInit -= OnPostSpawnInit;
        }
        public bool TryStart()
        {
            return !Status.IsCurrent() && !_aiStats.IsHungry && ShouldSleep;
        }
        public override bool TaskStart()
        {
            if(!TryStart())
            {
                SetStatus(TaskStatus.NotAvailable);
                return false;
            }
        
            switch (State)
            {
                case SleepState.None:
                    SetTaskTimer(); // Task start
                    break;

                case SleepState.TaskPaused:
                    TimerTask.Unpause(); // Task resume
                    break;
            }
            SetStatus(TaskStatus.NotReached);
            Transition(SleepState.GoSleep);
            return true;
        }
        public override void Update()
        {
            switch (State)
            {
                case SleepState.Awake:
                    if(_animator.IsAnimComplete)
                        Transition(SleepState.None);
                    break;
                case SleepState.Sleep :
                    if (TimerTask.IsReady)
                    {
                        Transition(SleepState.Awake);
                    }
                    break;
            }
        }

        protected override void OnEnter()
        {
            switch (State)
            {
                case SleepState.GoSleep: OnGoSleep(); break;
                case SleepState.Sleep:   OnSleep(); break;
                case SleepState.Awake:   OnAwake(); break;
                case SleepState.None:    OnNone(); break;
            }
        }
        private void OnGoSleep()
        {
            SetStatus(TaskStatus.InProcess);
            _aiMover.OnComplete += OnDestenationComplete;
            _aiMover.GoToRandom();
            SetAnim();
        }
        private void OnSleep()
        {
            SetStatus(TaskStatus.InProcess);
            _aiMover.SetLookRandom();
            SetAnim();
        }
        private void OnAwake()
        {
            if (_animator.HasAnim(AnimKey.Awake))
            {
                SetAnim();
            }
            else
            {
                Transition(SleepState.None);
            }
        }
        private void OnNone()
        {
            SetStatus(TaskStatus.Completed);
            _aiMover.OnComplete -= OnDestenationComplete;
            if (!Pause())
            {
                SetAwakeTimer(false);
            }
        }
        
        private void OnDestenationComplete()
        {
            _aiMover.OnComplete -= OnDestenationComplete;
            if (State == SleepState.GoSleep)
            {
                Transition(SleepState.Sleep);
            }
        }
        private void OnPostSpawnInit()
        {
            SetAwakeTimer(true);
        }
        private void OnStatsUpdate()
        {
            _aiStats.Update(this);
            if (TryStart())
            {
                _aiRoot.AutoTransition(this);
            }
        }
        private void SetAwakeTimer(bool randomForward)
        {
            GameTools.SetAwakeTimer(_timerAwake, TaskTime / 60, randomForward);
        }
        private void SetAnim()
        {
            _animator.SetAnim(GetAnim());
        }
        private bool Pause()
        {
            if (TimerTask.IsReady)
            {
                return false;
            }

            _aiMover.OnComplete -= OnDestenationComplete;
            TimerTask.Pause();
            SetStatus(TaskStatus.Completed);
            Transition(SleepState.TaskPaused);
            return true;
        }
        private AnimKey GetAnim()
        {
            switch (State)
            {
                case SleepState.GoSleep: return AnimKey.Walk;
                case SleepState.Sleep:   return AnimKey.Sleep;
                case SleepState.Awake:   return AnimKey.Awake;
                default:                 return AnimKey.None;
            }
        }
    }
}
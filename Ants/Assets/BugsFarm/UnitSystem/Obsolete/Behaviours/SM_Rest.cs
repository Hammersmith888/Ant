using System;
using BugsFarm.AnimationsSystem;
using BugsFarm.UnitSystem.Obsolete.Components;

namespace BugsFarm.UnitSystem.Obsolete
{
    public enum RestState
    {
        None,

        Walk,
        Rest,

        TaskPaused,
    }


    [Serializable]
    public class SM_Rest : AStateMachine<RestState>, IAiControlable
    {
        public bool IsInterruptible => true;
        public StateAnt AiType { get; }
        public int Priority { get; }
        public ATask Task => this;
    
        [NonSerialized] protected AIMover AIMover;
        [NonSerialized] protected AntAnimator Animator;
        [NonSerialized] protected AiStats AIStats;
        public override string ToString() //TODO : Добавить ключ локализации
        {
            return "отдых";
        }
        public SM_Rest(int priority,StateAnt aiType)
        {
            Priority = priority;
            AiType = aiType;
        }
        public void Setup(SM_AntRoot root)
        {
            AIMover = root.AiMover;
            Animator = root.Animator;
            AIStats = root.AiStats;
        }
        public void PostLoadRestore()
        {
            SetAnim();
            switch (State)
            {
                case RestState.Walk:
                    AIMover.OnComplete += OnDestenationComplete;
                    break;
            }
        }
        public void Dispose()
        {
            AIMover.OnComplete -= OnDestenationComplete;
        }
        public bool TryStart()
        {
            return !Status.IsCurrent();
        }
        public override bool TaskStart()
        {
            if(State == RestState.TaskPaused)
            {
                TimerTask.Unpause();
            }
            else
            {
                SetTaskTimer();
            }
            SetStatus(TaskStatus.NotReached);
            Transition(RestState.Walk);
            return true;
        }
        public override void Update()
        {
            if (TimerTask.IsReady)
            {
                Transition(RestState.None);
            }
        }
    
        protected override void OnEnter()
        {
            switch (State)
            {
                case RestState.Walk: OnWalk(); break;
                case RestState.Rest: OnRest(); break;
                case RestState.None: OnNone(); break;
            }
        }
        private void OnWalk()
        {
            SetStatus(TaskStatus.InProcess);
            AIMover.OnComplete += OnDestenationComplete;
            AIMover.GoToRandom();
            SetAnim();
        }
        private void OnRest()
        {
            SetStatus(TaskStatus.InProcess);
            SetAnim();
            AIMover.SetLookRandom();
        }
        private void OnNone()
        {
            SetStatus(TaskStatus.Completed);
            AIMover.OnComplete -= OnDestenationComplete;
        
            if (TimerTask.IsReady) 
                return;
        
            TimerTask.Pause();
            Transition(RestState.TaskPaused);
        }
        private void OnDestenationComplete()
        {
            AIMover.OnComplete -= OnDestenationComplete;
            if (State == RestState.Walk)
            {
                Transition(RestState.Rest);
            }
        }

        protected void SetAnim()
        {
            Animator.SetAnim(GetAnim());
        }
        protected virtual AnimKey GetAnim()
        {
            switch (State)
            {
                case RestState.Walk: return AnimKey.Walk;
                case RestState.Rest: return AnimKey.Idle;
            }
            return default;
        }

    }
}
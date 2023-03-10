using System;
using BugsFarm.AnimationsSystem;
using BugsFarm.SimulationSystem.Obsolete;
using BugsFarm.UnitSystem.Obsolete.Components;

namespace BugsFarm.UnitSystem.Obsolete
{
    [Serializable]
    public class SM_Walk : AStateMachine<WalkState>, IAiControlable
    {
        [NonSerialized] protected AIMover AiMover;
        [NonSerialized] protected AntAnimator Animator;
        [NonSerialized] protected AiStats AiStats;

        protected readonly Timer TimerStay = new Timer();
        public virtual bool IsInterruptible => true;
        public StateAnt AiType { get; }
        public int Priority { get; }
        public ATask Task => this;

        public SM_Walk(int priority,StateAnt aiType)
        {
            Priority = priority;
            AiType = aiType;
        }

        public override string ToString() // TODO : добавить ключи локализации
        {
            return "прогулка";
        }

        public virtual void Setup(SM_AntRoot root)
        {
            AiMover = root.AiMover;
            Animator = root.Animator;
            AiStats = root.AiStats;
        }
        public virtual void PostLoadRestore()
        {
            SetAnim();
            if (State == WalkState.Walk)
            {
                AiMover.OnComplete += OnDestenationComplete;
            }
        }
        public void Dispose()
        {
            AiMover.OnComplete -= OnDestenationComplete;
        }
        public virtual bool TryStart()
        {
            return !Status.IsCurrent() && !AiStats.IsHungry && Tools.RandomBool();
        }
        public override bool TaskStart()
        {
            if(AiStats.IsHungry)
            {
                SetStatus(TaskStatus.NotAvailable);
                return false;
            }
            switch (State)
            {
                case WalkState.TaskPaused: 
                    TimerTask.Unpause(); 
                    break;
                default: 
                    SetTaskTimer(); 
                    break;
            }
        
            SetStatus(TaskStatus.NotReached);
            var state = SimulationOld.Raw ? WalkState.Stay : WalkState.Walk;
            Transition(state);
            return true;
        }
        public override void Update()
        {
            if (State == WalkState.Stay)
            {
                if (TimerStay.IsReady)
                {
                    if (TimerTask.IsReady)
                    {
                        Transition(WalkState.None);
                    }
                    else
                    {
                        Transition(WalkState.Walk);
                    }
                }
            }
        }
    
        protected override void OnEnter()
        {
            switch (State)
            {
                case WalkState.Walk:       OnWalk(); break;
                case WalkState.Stay:       OnStay(); break;
                case WalkState.None:       OnNone(); break;
                case WalkState.TaskPaused: OnPaused(); break;
            }
        }
        protected virtual void OnWalk()
        {
            SetStatus(TaskStatus.InProcess);
            AiMover.OnComplete += OnDestenationComplete;
            AiMover.GoToRandom();
            SetAnim();
        }
        protected virtual void OnStay()
        {
            SetStatus(TaskStatus.InProcess);
            SetAnim();
            AiMover.Stay();
            TimerStay.Set(Tools.RandomRange(Constants.PatrolStayTime));
            AiMover.SetLookRandom();
        }
        protected virtual void OnPaused() 
        {
            SetStatus(TaskStatus.Completed);
            AiMover.OnComplete -= OnDestenationComplete;
        }
        protected virtual void OnNone()
        {
            SetStatus(TaskStatus.Completed);
            AiMover.OnComplete -= OnDestenationComplete;
            if (!TimerTask.IsReady)
            {
                TimerTask.Pause();
                Transition(WalkState.TaskPaused);
            }
        }
        protected void OnDestenationComplete()
        {
            AiMover.OnComplete -= OnDestenationComplete;
            if (State == WalkState.Walk)
            {
                Transition(WalkState.Stay);
            }
        }
        protected void SetAnim()
        {
            Animator.SetAnim(GetAnim());
        }
        protected AnimKey GetAnim()
        {
            switch (State)
            {
                case WalkState.Walk: return AnimKey.Walk;
                default:             return AnimKey.Idle;
            }
        }
    }
}
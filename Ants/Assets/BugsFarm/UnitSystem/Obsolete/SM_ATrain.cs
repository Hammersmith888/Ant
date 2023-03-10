using System;
using System.Linq;
using BugsFarm.AnimationsSystem;
using BugsFarm.UnitSystem.Obsolete.Components;

namespace BugsFarm.UnitSystem.Obsolete
{
    public enum TrainState
    {
        None,

        GoTrain,
        Train,
        Rest
    }
    [Serializable]
    public abstract class SM_ATrain : AStateMachine<TrainState>, IAiControlable
    {
        public bool IsInterruptible => true;
        public StateAnt AiType { get; }
        public int Priority { get; }
        public ATask Task => this;

        [NonSerialized] protected AIMover AiMover;
        [NonSerialized] protected AntAnimator Animator;
        [NonSerialized] protected AntPresenter AntPresenter;
        [NonSerialized] protected AiStats AiStats;
    
        protected abstract ObjType TrainObjectType { get; }
        protected TrainingEquipment Equipment;
        protected readonly Timer TimerXp = new Timer();

        public override string ToString() //TODO : Добавить ключ локализации
        {
            return "тренировка";
        }
        protected SM_ATrain(int priority,StateAnt aiType)
        {
            Priority = priority;
            AiType = aiType;
        }
        public virtual void PostLoadRestore()
        {
            SetAnim();
            switch (State)
            {
                case TrainState.GoTrain:
                    AiMover.OnComplete += OnDestenationComplete;
                    break;
            }
        }
        public virtual void Setup(SM_AntRoot root)
        {
            AiMover = root.AiMover;
            Animator = root.Animator;
            AntPresenter = root.AntPresenter;
            AiStats = root.AiStats;
        }
        public virtual void Dispose()
        {
            AiMover.OnComplete -= OnDestenationComplete;
        }
        public bool TryStart()
        {
            return !Status.IsCurrent() && !AiStats.IsHungry && 
                   Keeper.GetObjects<TrainingEquipment>().Any(x => x.Type == TrainObjectType && x.TryOccupy());
        }
        public override bool TaskStart()
        {
            if(AiStats.IsHungry)
            {
                SetStatus(TaskStatus.NotAvailable);
                return false;
            }
            
            foreach (var equipment in Keeper.GetObjects(TrainObjectType).OfType<TrainingEquipment>())
            {
                if (equipment.IsNullOrDefault() || !equipment.Occupy(AntPresenter))
                {
                    continue;
                }

                Equipment = equipment;
                equipment.SetSubscriber(this, true);
                SetTaskTimer();
                SetXpTimer();
                
                SetStatus(TaskStatus.NotReached);
                Transition(TrainState.GoTrain);
                return true;
            }
            SetStatus(TaskStatus.NotAvailable);
            return false;
        }
        public override void Update()
        {
            if (TimerTask.IsReady)
            {
                Transition(TrainState.None);
                return;
            }

            if (State == TrainState.Train)
            {
                if (TimerXp.IsReady)
                {
                    AddXp();
                    SetXpTimer();
                }
            }
            if (Animator.IsAnimComplete)
            {
                switch (State)
                {
                    case TrainState.Train:
                        Transition(TrainState.Train);
                        break;
                    case TrainState.Rest:
                        Transition(TrainState.Rest);
                        break;
                }
            }
        }

        protected void FreeOccupy()
        {
            Equipment?.Free(AntPresenter);
            Equipment?.SetSubscriber(this, false);
            Equipment = null;
        }
        protected override void HandleObjectEvent(APublisher publisher, ObjEvent objEvent)
        {
            switch (objEvent)
            {
                case ObjEvent.Moved:
                    Transition(TrainState.GoTrain);
                    break;

                case ObjEvent.Destroyed:
                case ObjEvent.BuildUpgradeBgn:
                    Transition(TrainState.None);
                    break;
            }
        }
        protected override void OnEnter()
        {
            switch (State)
            {
                case TrainState.GoTrain: OnGoTrain(); break;
                case TrainState.Train:   OnTrain(); break;
                case TrainState.Rest:    OnRest(); break;
                case TrainState.None:    OnNone(); break;
            }
        }
        protected virtual void OnGoTrain()
        {
            SetStatus(TaskStatus.InProcess);
            TimerXp.Pause();
            var point = Equipment.GetPoint(AntPresenter);
            AiMover.OnComplete += OnDestenationComplete;
            AiMover.GoTarget(point.Position);
            SetAnim();
        }
        protected virtual void OnTrain()
        {
            TimerXp.Unpause();
            var point = Equipment.GetPoint(AntPresenter);
            AiMover.SetLook(point.LookLeft);
            SetAnim();
        }
        protected virtual void OnRest()
        {
            SetAnim();
            TimerXp.Pause();
        }
        protected virtual void OnNone()
        {
            if (!TimerXp.IsReady)
            {
                TimerXp.Pause();
            }
            FreeOccupy();
            AiMover.OnComplete -= OnDestenationComplete;
            SetStatus(TaskStatus.Completed);
        }
        protected abstract AnimKey GetAnim();
        protected virtual void SetAnim()
        {
            Animator.SetAnim(GetAnim());
        }
        protected virtual void SetXpTimer()
        {
            TimerXp.Set(10 * 60); // 10 min
        }
        protected virtual void AddXp()
        {
            if (Equipment == null)
            {
                return;
            }

            var xp = Equipment.UpgradeLevelCur.param1 * TimerXp.Progress;

            AntPresenter.AddXp(xp);
        }
        
        protected virtual void OnDestenationComplete()
        {
            AiMover.OnComplete -= OnDestenationComplete;
        }
    }
}
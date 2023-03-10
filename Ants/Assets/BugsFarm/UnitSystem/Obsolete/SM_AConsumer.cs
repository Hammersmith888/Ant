using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using BugsFarm.AnimationsSystem;
using BugsFarm.BuildingSystem;
using BugsFarm.SimulationSystem.Obsolete;
using BugsFarm.UnitSystem.Obsolete.Components;

namespace BugsFarm.UnitSystem.Obsolete
{
    public enum ConsumeState
    {
        None,
    
        GoConsume,
        Consume
    }

    [Serializable]
    public abstract class SM_AConsumer : AStateMachine<ConsumeState>, IAiControlable
    {
        public virtual bool IsInterruptible => false;
        public StateAnt AiType { get; }
        public int Priority { get; }
        public ATask Task => this;
    
        [NonSerialized] protected AIMover AiMover;
        [NonSerialized] protected AntAnimator Animator;
        [NonSerialized] protected AiStats AiStats;
        [NonSerialized] protected SM_AntRoot AiRoot;
        [NonSerialized] protected AntPresenter AntPresenter;
        [NonSerialized] protected FB_CfgAntConsumption Data;
        [NonSerialized] protected IPosSide ConsumePoint;
    
        public float TimeNeed => Consumer.TimerNeed.Left;
        public bool IsHunger => IsHungry && Consumer.TimerHunger.IsReady;
        public float TimeHunger => Consumer.TimerHunger.Passed;
        public bool IsHungry => Consumer.TimerNeed.IsReady;
        protected abstract float ConsumptionTime { get; } 
        protected float ConsumptionMax { get; set; }
        protected Consumer Consumer = new Consumer();
        protected AConsumable Consumable;
        protected IPosSide ConsumeSPoint;
        protected float Consumed;


        internal override void OnSerializeMethod(StreamingContext ctx)
        {
            //ConsumeSPoint = ConsumePoint.ToSPosSide();
        }
        internal override void OnDeserializeMethod(StreamingContext ctx)
        {
            ConsumePoint = ConsumeSPoint;
        }

        protected SM_AConsumer(int priority, StateAnt aiType)
        {
            Priority = priority;
            AiType = aiType;
        }
        public void Setup(SM_AntRoot rootAi)
        {
            AiRoot = rootAi;
            AiStats = rootAi.AiStats;
            AiMover = rootAi.AiMover;
            Animator = rootAi.Animator;
            AntPresenter = rootAi.AntPresenter;
            Data = Data_Ants.Instance.GetData(AntPresenter.AntType).consumption;
            Setup();
            AiRoot.OnPostSpawnInit += OnPostSpawnInit;
            AiRoot.OnAiStatsUpdate += OnStatsUpdate;
        }
        public void PostLoadRestore()
        {
            if(State == ConsumeState.GoConsume)
            {
                AiMover.OnComplete += OnDestenationComplete;
            }
            SetAnim();
        }
        public void Dispose()
        {
            FreeOccupy();
            AiRoot.OnPostSpawnInit -= OnPostSpawnInit;
            AiRoot.OnAiStatsUpdate -= OnStatsUpdate;
        }
        public override void Update()
        {
            if (State == ConsumeState.Consume)
            {
                if (Consume())
                {
                    Transition(ConsumeState.None);
                }
            }
        }
        public abstract bool TryStart();
        public override bool TaskStart()
        {
            Consumed = 0;
            return true;
        }

        protected abstract void Setup();
        protected bool TryOccupyConsumable<T>(IEnumerable<T> consumables, out T consumable) where  T : AConsumable
        {
            var filtred = consumables.Where(x => !AiRoot.BlackList.HasObjects(x));
            consumable = filtred.FirstOrDefault(x => x.TryOccupyConsumable());
            return !consumable.IsNullOrDefault();
        }
        protected bool OccupyConsumable<T>(T consumable) where T: AConsumable
        {
            if (consumable.IsNullOrDefault())
            {
                return false;
            }

            if (!consumable.OccupyConsumable(this, out var point)) 
                return false;
        
            ConsumePoint = point;
            SetCosumable(consumable);
            Transition(ConsumeState.GoConsume);
        
            return true;
        }
        private void FreeOccupy()
        {
            Consumable?.FreeOccupy(this);
            Consumable?.SetSubscriber(this, false);
            Consumable = null;
        }
    
        protected override void HandleObjectEvent(APublisher publisher, ObjEvent objEvent)
        {
            switch (objEvent)
            {
                case ObjEvent.Moved:
                    switch (State)
                    {
                        case ConsumeState.GoConsume:                   
                        case ConsumeState.Consume: Transition(ConsumeState.GoConsume);
                            break;
                    }
                    break;

                case ObjEvent.Destroyed:
                case ObjEvent.IsDepleted:
                case ObjEvent.BuildUpgradeBgn: Transition(ConsumeState.None);
                    break;
            }
        }
        protected override void OnEnter()
        {
            switch (State)
            {
                case ConsumeState.GoConsume: OnGoConsume(); break;
                case ConsumeState.Consume:   OnConsume(); break;
                case ConsumeState.None:      OnNone(); break;
            }
        }
        private void OnGoConsume()
        {
            SetStatus(TaskStatus.InProcess);
            AiMover.OnComplete += OnDestenationComplete;
            AiMover.GoTarget(ConsumePoint.Position);
            SetAnim();
        }
        protected virtual void OnConsume()
        {
            SetStatus(TaskStatus.InProcess);
            Consumer.ConsumptionStart();
            AiMover.SetLook(ConsumePoint.LookLeft);
            SetAnim();
        }
        protected virtual void OnNone()
        {
            Consumer.ConsumptionEnd(Consumed, ConsumptionMax);
            AiMover.OnComplete -= OnDestenationComplete;
            FreeOccupy();
            SetStatus(TaskStatus.Completed);
        }
        private void OnPostSpawnInit()
        {
            Consumer.PostSpawnInit();
        }
        private void OnDestenationComplete()
        {            
            AiMover.OnComplete -= OnDestenationComplete;
            if(State == ConsumeState.GoConsume)
            {
                Transition(ConsumeState.Consume);
            }
        }
        protected virtual void OnStatsUpdate()
        {
            if (State != ConsumeState.Consume)
            {
                Consumer.Update();
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
                case ConsumeState.GoConsume: return AnimKey.Walk;
                default:                     return AnimKey.None;
            }
        }
        protected virtual bool AvailableTask()
        {
            return !Status.IsCurrent() && IsHungry;
        }
        private void SetCosumable(AConsumable consumable)
        {
            if (consumable.IsNullOrDefault())
            {
                return;
            }
            Consumable = consumable;
            consumable.SetSubscriber(this, true);
        }
        private bool Consume()
        {
            var singlePassConsume = ConsumptionMax * SimulationOld.DeltaTime / ConsumptionTime;
            return Consumable.Consume(ConsumptionMax, singlePassConsume, ref Consumed);
        }
    }
}
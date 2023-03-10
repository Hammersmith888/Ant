using System;
using BugsFarm.AnimationsSystem;
using BugsFarm.SimulationSystem.Obsolete;
using BugsFarm.UnitSystem.Obsolete.Components;
using UnityEngine;

namespace BugsFarm.UnitSystem.Obsolete
{
    public enum DeathState
    {
        None,

        Walk,
        Fall,
        Fade,
        RIP
    }


    [Serializable]
    public class SM_Death : AStateMachine<DeathState>, IAiControlable
    {
        public ATask Task => this;
        public bool IsInterruptible => false;
        public StateAnt AiType { get; }
        public int Priority { get; }
        public bool IsAlive { get; private set; } = true;

        [NonSerialized] private AIMover _aiMover;
        [NonSerialized] private AntAnimator _animator;
        [NonSerialized] private AUnitView unitView;
        [NonSerialized] private UnitRipSceneObject _unitRipSceneObject;
        [NonSerialized] private SM_AntRoot _aiRoot;
        [NonSerialized] private AiStats _aiStats;

        private readonly Timer _timerFade = new Timer();

        public override string ToString() //TODO : Добавить ключ локализации
        {
            return "погибает";
        }
        public SM_Death(int priority,StateAnt aiType)
        {
            Priority = priority;
            AiType = aiType;
        }
        public void Setup(SM_AntRoot root)
        {
            _aiRoot = root;
            _aiMover = root.AiMover;
            _animator = root.Animator;
            unitView = root.UnitView;
            _aiStats = root.AiStats;
            _unitRipSceneObject = root.UnitRipSceneObject;
            root.OnAiStatsUpdate += OnStatsUpdate;
        }
        public void PostLoadRestore()
        {
            SetColliders();
            SetAnim();
            _animator.Update(false,false); // исключаем возможность проигрывания анимации лестница, полет.
            if (SimulationOld.Any)
            {
                _animator.FastForward();
            }
            switch (State)
            {
                case DeathState.Walk:
                    _aiMover.OnComplete += OnDestenationComplete;
                    break;
                case DeathState.RIP:
                    Debug.LogError($"{this} : DeathState.RIP");
                    SetRipNormal();
                    SetRipAlpha(1);
                    break;
            }
        }
        public void Dispose()
        {
            _aiMover.OnComplete -= OnDestenationComplete;
        }
        public override void Update()
        {
            if (!State.IsNullOrDefault())
            {
                switch (State)
                {
                    case DeathState.Fall:
                        if(_animator.IsAnimComplete)
                            Transition(DeathState.Fade);
                        break;
                    case DeathState.Fade:
                        if (!_timerFade.IsReady)
                        {
                            SetRipAlpha();
                        }
                        else
                        {
                            SetRipAlpha(1);
                            Transition(DeathState.RIP);
                        }
                        break;
                }
            } 
        }

        public bool TryStart()
        {
            return _aiStats.IsDeadlyHunger;
        }
        public override bool TaskStart()
        {
            if (TryStart())
            {
                SetStatus(TaskStatus.NotReached);
                Transition(DeathState.Walk);
                return true;
            }
            SetStatus(TaskStatus.NotAvailable);
            return false;
        }
        protected override void OnEnter()
        {
            switch (State)
            {
                case DeathState.Walk: OnWalk(); break;
                case DeathState.Fall: OnFall(); break;
                case DeathState.Fade: OnFade(); break;
                case DeathState.RIP:  OnRip(); break;
            }
        }
        private void OnWalk()
        {
            SetStatus(TaskStatus.InProcess);
            if (SimulationOld.Raw)
            {
                _aiMover.TeleportRandom();
                Transition(DeathState.Fall);
                return;
            }
        
            _aiMover.OnComplete += OnDestenationComplete;
            _aiMover.GoToRandom();
            SetColliders();
            SetAnim();
        }
        private void OnFall()
        {
            _aiMover.Stay();
            _animator.Update(false,false); // исключаем возможность проигрывания анимации лестница, полет.
            SetColliders();
            SetAnim();
        }
        private void OnFade()
        {
            _timerFade.Set(1);
            SetRipNormal();
        }
        private void OnRip()
        {
            IsAlive = false;
            // var aboutDeath = DeathReason.None;
            // if (_aiStats.IsFoodHunger)
            // {
            //     aboutDeath = DeathReason.NoFood;
            // }
            // else if (_aiStats.IsWaterHunger)
            // {
            //     aboutDeath = DeathReason.NoWater;
            // }
            //anSetStatus(TaskStatus.Completed); 
            // GameEvents.OnAntDied?.Invoke(_aiRoot.AntPresenter, aboutDeath);
            // Debug.LogError("Помер от : " + aboutDeath);
        }
        private void OnStatsUpdate()
        {
            _aiStats.Update(this);
        }
        private void OnDestenationComplete()
        {
            Transition(DeathState.Fall);
            _aiMover.OnComplete -= OnDestenationComplete;
        }
    
        private void SetColliders()
        {
            unitView.RefreshColliders();
        }
        private void SetAnim()
        {
            _animator.SetAnim(GetAnim());
        }
        private AnimKey GetAnim()
        {
            switch (State)
            {
                case DeathState.Walk: return AnimKey.Walk;
                case DeathState.Fall: return AnimKey.Death;
                default:              return AnimKey.None;
            }
        }
        private void SetRipNormal()
        {
            Debug.LogError($"{this} : Does not normal");
            _unitRipSceneObject.SetAngle(Vector2.up);//_aiMover.Transform.up);
        }
        private void SetRipAlpha(float? alpha = null)
        {
            alpha = alpha ?? _timerFade.Progress;
            _unitRipSceneObject.SetAlpha(alpha.Value);
        }
    }
}
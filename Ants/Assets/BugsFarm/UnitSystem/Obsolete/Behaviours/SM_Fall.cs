using System;
using BugsFarm.AnimationsSystem;
using BugsFarm.UnitSystem.Obsolete.Components;
using UnityEngine;

namespace BugsFarm.UnitSystem.Obsolete
{
    public enum FallState
    {
        None,

        Fall
    }


    [Serializable]
    public class SM_Fall : AStateMachine<FallState>, IAiControlable
    {
        public bool IsInterruptible => false;
        public StateAnt AiType { get; }
        public int Priority { get; }
        public ATask Task => this;
    
        [NonSerialized] private AIMover _aiMover;
        [NonSerialized] private AntAnimator _animator;
        [NonSerialized] private SM_AntRoot _aiRoot;
    
        [NonSerialized] private readonly Vector2 _targetPosition = new Vector2(-3.35f, 6.4f);
        [NonSerialized] private Vector2 _beginPosition;
        private bool _postSpawnFall = true;
    
        public override string ToString() //TODO : Добавить ключ локализации
        {
            return "падает";
        }
        public SM_Fall(int priority, StateAnt aiType)
        {
            Priority = priority;
            AiType = aiType;
        }
        public void Setup(SM_AntRoot root)
        {
            _beginPosition = new Vector2(_targetPosition.x, 10f);
        
            _aiMover = root.AiMover;
            _animator = root.Animator;
            _aiRoot = root;
            _aiRoot.OnAiStatsUpdate += OnStatsUpdate;
        }
        public void PostLoadRestore()
        {
            SetAnim();
            switch (State)
            {
                case FallState.Fall:
                    _aiMover.OnComplete += OnDestenationComplete;
                    break;
            }
        }
        public void Dispose()
        {
            _aiMover.OnComplete -= OnDestenationComplete;
        }
        public bool TryStart()
        {
            return !Status.IsCurrent() && _postSpawnFall;// || (!_aiMover.Current.IsNull() && !_aiMover.Current.Walkable);
        }
        public override bool TaskStart()
        {
            if (!TryStart())
            {
                SetStatus(TaskStatus.NotAvailable);
                return false;
            }
            SetStatus(TaskStatus.NotReached);
            Transition(FallState.Fall);
            return true;
        }
        private void OnStatsUpdate()
        {
            if (TryStart())
            {
                _aiRoot.AutoTransition(this);
            }
        }
    
        protected override void OnEnter()
        {
            switch (State)
            {
                case FallState.Fall: OnFall(); break;
                case FallState.None: OnNone(); break;
            }
        }
        private void OnFall() // TODO : сделать поиск места куда можно упасть, сейчас падаем всегда как после спавна
        {
            if(_postSpawnFall)
            {
                _postSpawnFall = false;
                _aiMover.SetPosition(_beginPosition);
            }

            SetStatus(TaskStatus.InProcess);
            SetAnim();
            // _aiMover.OnComplete += OnDestenationComplete;
            // _aiMover.GoTarget(_targetPosition, Constants.Ant_FallSpeed); 
            _aiMover.SetPositionAtClosedNode(_targetPosition);
            Transition(FallState.None);
        }
        private void OnNone()
        {
            //_aiMover.OnComplete -= OnDestenationComplete;
            SetStatus(TaskStatus.Completed);
        }
        private void OnDestenationComplete()
        {
            _aiMover.OnComplete -= OnDestenationComplete;
            Transition(FallState.None);
        }
        private void SetAnim()
        {
            _animator.SetAnim(AnimKey.Idle);
        }
    }
}
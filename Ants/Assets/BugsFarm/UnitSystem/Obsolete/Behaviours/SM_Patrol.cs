using System;
using System.Collections.Generic;
using BugsFarm.SimulationSystem.Obsolete;

namespace BugsFarm.UnitSystem.Obsolete
{
    public enum WalkState
    {
        None,

        Walk,
        Stay,

        TaskPaused,
    }


    [Serializable]
    public class SM_Patrol : SM_Walk
    {
        private struct Patrol
        {
            public int Max;
            public const int Min = 1;
            public int Patrolling;
        }

        [NonSerialized] private static Dictionary<AntType, Patrol> _patrolls;
        [NonSerialized] private AntPresenter _antPresenter;
        private bool _isPatrolling;
    
        public override string ToString() //TODO : Добавить ключ локализации
        {
            return "патрулирование";
        }
        public SM_Patrol(int priority, StateAnt aiType) : base(priority,aiType) { }
        public override void Setup(SM_AntRoot root)
        {
            base.Setup(root);
            _antPresenter = root.AntPresenter;
            if (_patrolls.IsNullOrDefault())
            {
                _patrolls = new Dictionary<AntType, Patrol>();
            }
            if(!_patrolls.ContainsKey(_antPresenter.AntType))
            {
                _patrolls.Add(_antPresenter.AntType, new Patrol());
            }

            var patrol = _patrolls[_antPresenter.AntType];
            patrol.Max++;
            _patrolls[_antPresenter.AntType] = patrol;
        }
        public override void PostLoadRestore()
        {
            base.PostLoadRestore();
            if(_isPatrolling && _patrolls.TryGetValue(_antPresenter.AntType, out var patrol))
            {
                patrol.Patrolling++;
                _patrolls[_antPresenter.AntType] = patrol;
            }

            if (SimulationOld.Any)
            {
                AiMover.TeleportRandom();
            }
        }
        public override bool TryStart()
        {
            return !Status.IsCurrent() && !AiStats.IsHungry && Tools.RandomBool() && TryPatrolling();
        }
        public override bool TaskStart()
        {
            if (!TryPatrolling() || !Patrolling())
            {
                SetStatus(TaskStatus.NotAvailable);
                return false;
            }
            switch (State)
            {
                case WalkState.TaskPaused: TimerTask.Unpause(); break;
                default:                   SetTaskTimer(); break;
            }


            SetStatus(TaskStatus.NotReached);
            var state = SimulationOld.Raw ? WalkState.Stay : WalkState.Walk;
            Transition(state);
            return true;
        }
        public override void Update()
        {
            if(State == WalkState.Stay)
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
                case WalkState.Walk: OnWalk(); break;
                case WalkState.Stay: OnStay(); break;
                case WalkState.None: OnNone(); break;
            }
        }
        protected override void OnPaused()
        {
            base.OnPaused();
            FreePatrol();
        }
        protected override void OnNone()
        {
            base.OnNone();
            FreePatrol();
        }

        private bool TryPatrolling()
        {
            if (_patrolls.TryGetValue(_antPresenter.AntType, out var currentPatrol))
            {
                var availableTask = !AiStats.IsHungry || currentPatrol.Patrolling > Patrol.Min;
                var lessFiftyPercent = ((currentPatrol.Patrolling * 2) + 1) < currentPatrol.Max;
                return availableTask && lessFiftyPercent;
            }
            return false;
        }
        private bool Patrolling()
        {
            if (TryPatrolling())
            {
                var currentPatrol = _patrolls[_antPresenter.AntType];
                currentPatrol.Patrolling++;
                _patrolls[_antPresenter.AntType] = currentPatrol;
                return _isPatrolling = true;
            }
        
            return _isPatrolling = false;
        }
        private void FreePatrol()
        {
            if (!_patrolls.ContainsKey(_antPresenter.AntType)) return;
            var patrol = _patrolls[_antPresenter.AntType];
            if(patrol.Patrolling > 0)
            {
                patrol.Patrolling--;
            }
            _patrolls[_antPresenter.AntType] = patrol;
            _isPatrolling = false;
        }
    }
}
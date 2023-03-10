using System;
using BugsFarm.SimulationSystem;
using Spine;
using Zenject;
using TickableManager = Zenject.TickableManager;

namespace BugsFarm.AnimationsSystem
{
    public abstract class BasePlayer : ITickable, IDisposable, ISpinePlayer
    {
        public event Action OnComplete;
        public abstract float TimeScale { get; set; }
        protected abstract AnimationState AnimationState { get; }

        private ISimulationSystem _simulationSystem;
        private ITickableManager _tickableManager;

        protected SimulationTime SimulationTime;
        protected Animation Current;
        protected bool Loop;
        protected bool Finalized;
        protected bool Completed;

        [Inject]
        private void Inject(ISimulationSystem simulationSystem,
                            ITickableManager tickableManager)
        {
            _simulationSystem = simulationSystem;
            _tickableManager = tickableManager;
            SimulationTime = new SimulationTime();
            simulationSystem.OnSimulationEnd += OnSimulationEnded;
            tickableManager.Add(this);
        }

        public void Play(Animation animation, bool loop)
        {
            if (Finalized) return;
            Completed = false;
            Current = animation;
            Loop = loop;
            if (_simulationSystem.Simulation)
            {
                SimulationTick(false);
            }
            else
            {
                SetAnimation();
            }
        }

        public void Tick()
        {
            if (Finalized) return;
            if (_simulationSystem.Simulation)
            {
                SimulationTick(true);
            }
            else
            {
                Update(_simulationSystem.OrigDeltaTime);
            }
        }

        public void Dispose()
        {
            if (Finalized) return;
            _tickableManager.Remove(this);
            _simulationSystem.OnSimulationEnd -= OnSimulationEnded;
            Finalized = true;
        }

        protected abstract void Update(float deltaTime);

        private void SimulationTick(bool nextTick)
        {
            if (Finalized) return;
            if (nextTick)
            {
                SimulationTime.ResetWith(_simulationSystem.DeltaTime);
            }

            if (!Loop && SimulationTime.Available() && Current != null && !Completed)
            {
                if (SimulationTime.Apply(Current.Duration / TimeScale))
                {
                    OnCompleted();
                }
            }
        }

        private void SetAnimation()
        {
            if (Current == null) return;
            var entry = AnimationState.SetAnimation(0, Current, Loop);
            if (Loop) return;
            entry.Complete += _ => OnCompleted();
        }

        private void OnCompleted()
        {
            if (Loop || Completed) return;
            Completed = true;
            OnComplete?.Invoke();
        }

        private void OnSimulationEnded()
        {
            if (Finalized || Completed || (!Loop && Completed)) return;
            SetAnimation();
        }
    }
}
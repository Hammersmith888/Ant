using BugsFarm.SimulationSystem;
using BugsFarm.TaskSystem;
using UnityEngine;
using Zenject;

namespace BugsFarm.UnitSystem
{
    public class FadeInOutUnitTask : BaseTask, ITickable
    {
        private ISimulationSystem _simulationSystem;
        private ITickableManager _tickableManager;
        private UnitSceneObjectStorage _sceneObjectStorage;
        protected UnitSceneObject View;
        protected string UnitId;
        protected bool ToFade;
        private float _duration;
        private float _timer;


        [Inject]
        private void Inject(UnitSceneObjectStorage sceneObjectStorage,
                            ISimulationSystem simulationSystem,
                            ITickableManager tickableManager,
                            bool fade = true,
                            float duration = 1f)
        {
            ToFade = fade;
            _duration = duration;
            _timer = duration;
            _simulationSystem = simulationSystem;
            _tickableManager = tickableManager;
            _sceneObjectStorage = sceneObjectStorage;
        }

        public override void Execute(params object[] args)
        {
            if (IsRunned) return;

            UnitId = (string)args[0];
            View = _sceneObjectStorage.Get(UnitId);
            if (_simulationSystem.Simulation)
            {
                View.SetAlpha(ToFade ? 0 : 1);
                Completed();
                return;
            }
            
            base.Execute(args);
            _tickableManager.Add(this);
        }
        public void Tick()
        {
            if(!IsRunned) return;
            
            _timer = Mathf.Max(0, _timer - _simulationSystem.DeltaTime);
            var alpha01 = _timer / _duration;
            View.SetAlpha(ToFade ? alpha01 : 1f - alpha01);
            if (_timer == 0)
            {
                Completed();
            }
        }

        protected override void OnForceCompleted()
        {
            if (IsExecuted)
            {
                View.SetAlpha(ToFade ? 0 : 1);
            }
            base.OnForceCompleted();
        }

        protected override void OnDisposed()
        {
            if (IsExecuted)
            {
                _tickableManager.Remove(this);
                View.SetAlpha(ToFade ? 0 : 1);
            }
            base.OnDisposed();
        }
    }
}
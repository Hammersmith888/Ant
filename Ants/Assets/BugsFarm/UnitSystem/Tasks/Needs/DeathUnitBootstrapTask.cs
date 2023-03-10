using System;
using BugsFarm.TaskSystem;
using UnityEngine;
using Zenject;

namespace BugsFarm.UnitSystem
{
    public class DeathUnitBootstrapTask : BaseTask
    {
        public override bool Interruptible => false;
        private readonly IInstantiator _instantiator;
        private readonly UnitSceneObjectStorage _sceneObjectStorage;
        private readonly Vector2 _position;
        private readonly string _unitGuid;
        private readonly Action _startFadeAction;
        private ITask _taskProcessed;

        public DeathUnitBootstrapTask(IInstantiator instantiator,
                                      UnitSceneObjectStorage sceneObjectStorage,
                                      Vector2 position,
                                      Action startFadeAction)
        {
            _instantiator = instantiator;
            _sceneObjectStorage = sceneObjectStorage;
            _position = position;
            _startFadeAction = startFadeAction;
        }

        public override void Execute(params object[] args)
        {
            if (IsRunned)
            {
                return;
            }

            base.Execute(args);
            var unitGuid = (string) args[0];
            var unitView = _sceneObjectStorage.Get(unitGuid);
            unitView.SetInteraction(false);
            var tasks = new ITask[]
            {
                _instantiator.Instantiate<MoveToScenePointTask>(new object[] {_position}),
                _instantiator.Instantiate<DeathUnitTask>(new object[] {_startFadeAction}),
            };
            _taskProcessed = _instantiator.Instantiate<BootstrapTask>(new object[] {tasks});
            _taskProcessed.OnComplete += _ => Completed();
            _taskProcessed.OnInterrupt += _ => Interrupt();
            _taskProcessed.Execute(args);
        }

        public override Vector2[] GetPositions()
        {
            return new[] {_position};
        }

        protected override void OnForceCompleted()
        {
            _taskProcessed?.ForceComplete();
            _taskProcessed = null;
        }

        protected override void OnInterrupted()
        {
            _taskProcessed?.Interrupt();
            _taskProcessed = null;
        }

        protected override void OnCompleted()
        {
            _taskProcessed = null;
        }
    }
}
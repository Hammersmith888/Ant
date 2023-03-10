using BugsFarm.SimulatingSystem;
using BugsFarm.TaskSystem;
using UnityEngine;
using Zenject;

namespace BugsFarm.UnitSystem
{
    public class SleepUnitBootstrapTask : BaseTask
    {
        private readonly IInstantiator _instantiator;
        private readonly NeedStatController _needController;
        private readonly Vector2 _position;
        private ITask _taskProcessed;

        public SleepUnitBootstrapTask(Vector2 position,
                                      NeedStatController needController,
                                      IInstantiator instantiator)
        {
            _instantiator = instantiator;
            _needController = needController;
            _position = position;
        }

        public override void Execute(params object[] args)
        {
            if (IsRunned)
            {
                return;
            }
            base.Execute(args);

            ITask[] tasks;
            tasks = new ITask[]
            {
                _instantiator.Instantiate<MoveToScenePointTask>(new object[] {_position}),
                _instantiator.Instantiate<RotateToTask>(new object[] {Tools.RandomBool()}),
                _instantiator.Instantiate<SleepTask>(new object[]{_needController})
            };
            

            _taskProcessed = _instantiator.Instantiate<BootstrapTask>(new object[]{tasks});
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
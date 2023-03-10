using System.Linq;
using BugsFarm.TaskSystem;
using BugsFarm.UnitSystem;
using UnityEngine;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public abstract class TrainingBootstrapTask<T> : BaseTask where T : ITask
    {
        private IInstantiator _instantiator;
        private IPosSide _trainingPoint;
        private ITask _taskProcessed;
        private string _buildingGuid;
        
        [Inject]
        private void Inject(string buildinGuid,
                            IPosSide trainingPoint,
                            IInstantiator instantiator)
        {
            _instantiator = instantiator;
            _trainingPoint = trainingPoint;
            _buildingGuid = buildinGuid;
        }

        public override void Execute(params object[] args)
        {
            if (IsRunned)
            {
                return;
            }
            args = args.Append(_buildingGuid).ToArray();
            
            base.Execute(args);
            var tasks = new ITask[]
            {
                _instantiator.Instantiate<MoveToSceneItemTask>(new object[] {_buildingGuid,_trainingPoint}),
                _instantiator.Instantiate<T>()
            };
            
            _taskProcessed = _instantiator.Instantiate<BootstrapTask>(new []{tasks});
            _taskProcessed.OnComplete  += StartComplete;
            _taskProcessed.OnInterrupt += StartInterrupt;
            _taskProcessed.Execute(args);
        }
        

        private void StartInterrupt(ITask task)
        {
            Interrupt();
        }
        private void StartComplete(ITask task)
        {
            Completed();
        }
        public override Vector2[] GetPositions()
        {
            return _trainingPoint?.ToPositions() ?? new Vector2[0];
        }

        protected override void OnForceCompleted()
        {
            _taskProcessed?.ForceComplete();
            _instantiator = null;
            _trainingPoint = null;
            _taskProcessed = null;
            _buildingGuid = null;
        }
        protected override void OnInterrupted()
        {
            _taskProcessed?.Interrupt();
            _instantiator = null;
            _trainingPoint = null;
            _taskProcessed = null;
            _buildingGuid = null;
        }
        protected override void OnCompleted()
        {
            _instantiator = null;
            _trainingPoint = null;
            _taskProcessed = null;
            _buildingGuid = null;
        }
    }
}
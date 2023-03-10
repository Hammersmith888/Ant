using System.Collections.Generic;
using System.Linq;
using BugsFarm.TaskSystem;
using BugsFarm.UnitSystem;
using UnityEngine;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class HospitalBootstrapTask : BaseTask
    {
        private readonly IInstantiator _instantiator;
        private readonly IEnumerable<IPosSide> _taskPoints;
        private readonly string _buildingId;
        private ITask _taskProcessed;

        public HospitalBootstrapTask(string buildingId,
                                     IEnumerable<IPosSide> taskPoints,
                                     IInstantiator instantiator)
        {
            _instantiator = instantiator;
            _buildingId = buildingId;
            _taskPoints = taskPoints;
        }

        public override void Execute(params object[] args)
        {
            if (IsRunned)
            {
                return;
            }

            base.Execute(args);
            var taskPoint = _taskPoints.First();
            var tasks = new ITask[]
            {
                _instantiator.Instantiate<MoveToSceneItemTask>(new object[] {_buildingId, taskPoint}),
                _instantiator.Instantiate<HospitalTask>(new object[] {_buildingId, _taskPoints}),
            };

            _taskProcessed = _instantiator.Instantiate<BootstrapTask>(new[] {tasks});
            _taskProcessed.OnComplete += _ => Completed();
            _taskProcessed.OnForceComplete += _ => ForceComplete();
            _taskProcessed.OnInterrupt += _ => Interrupt();
            _taskProcessed.Execute(args);
        }

        public override Vector2[] GetPositions()
        {
            return _taskPoints?.ToPositions() ?? new Vector2[0];
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
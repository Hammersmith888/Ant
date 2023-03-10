using System.Collections.Generic;
using System.Linq;
using BugsFarm.TaskSystem;
using BugsFarm.UnitSystem;
using UnityEngine;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class BuildingBootstrapTask : BaseTask
    {
        private readonly IInstantiator _instantiator;
        private readonly string _buildingGuid;
        private readonly IPosSide[] _buildingPoints;
        private ITask _taskProcessed;

        public BuildingBootstrapTask(IInstantiator instantiator, 
                                     string buildingGuid,
                                     IEnumerable<IPosSide> buildingPoints)
        {
            _instantiator = instantiator;
            _buildingGuid = buildingGuid;
            _buildingPoints = Tools.Shuffle_FisherYates(buildingPoints).ToArray();
        }

        public override void Execute(params object[] args)
        {
            if (IsRunned)
            {
                return;
            }
            base.Execute(args);
            var tasks = new ITask[]
            {
                _instantiator.Instantiate<MoveToSceneItemTask>(new object[]   {_buildingGuid, _buildingPoints[0]}),
                _instantiator.Instantiate<BuildingBuildSceneItemTask>(new object[] {_buildingGuid, _buildingPoints}),
            };
            _taskProcessed = _instantiator.Instantiate<BootstrapTask>(new object[]{tasks});
            _taskProcessed.OnComplete  += _ => Completed();
            _taskProcessed.OnInterrupt += _ => Interrupt();
            _taskProcessed.Execute(args);
        }
        public override Vector2[] GetPositions()
        {
            return _buildingPoints.ToPositions();
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
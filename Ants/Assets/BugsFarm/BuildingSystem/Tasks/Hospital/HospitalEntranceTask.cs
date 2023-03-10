using BugsFarm.TaskSystem;
using BugsFarm.UnitSystem;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class HospitalEntranceTask : FadeInOutUnitTask
    {
        private readonly IInstantiator _instantiator;
        private readonly IPosSide _taskPoint;
        private readonly string _buildingId;
        private ITask _moveTask;

        public HospitalEntranceTask(string buildingId,
                                    IPosSide taskPoint,
                                    IInstantiator instantiator)
        {
            _buildingId = buildingId;
            _taskPoint = taskPoint;
            _instantiator = instantiator;
        }

        public override void Execute(params object[] args)
        {
            if(IsRunned) return;
            base.Execute(args);
            if (ToFade)
            {
                var arg = new object[] {_buildingId, _taskPoint};
                _moveTask = _instantiator.Instantiate<MoveToSceneItemTask>(arg);
            }
            else
            {
                var arg = new object[] {_taskPoint.Position};
                _moveTask = _instantiator.Instantiate<MoveToScenePointTask>(arg);
            }
            _moveTask.OnComplete  += _=> ForceComplete();
            _moveTask.Execute(UnitId);
        }
        protected override void OnDisposed()
        {
            if (IsExecuted)
            {
                View.SetInteraction(!ToFade);
                View.SetActive(!ToFade);
                _moveTask?.ForceComplete();
                _moveTask = null;
            }
            base.OnDisposed();
        }
    }
}
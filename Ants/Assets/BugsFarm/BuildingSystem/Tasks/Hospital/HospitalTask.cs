using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.Services.StatsService;
using BugsFarm.TaskSystem;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class HospitalTask : BaseTask
    {
        private readonly string _buildingId;
        private readonly IInstantiator _instantiator;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private const string _helpTimeStatKey = "stat_helpTime";
        private const string _entrancedStatKey = "stat_entered";
        private readonly IPosSide[] _taskPoints;
        private string _unitId;
        private ITask _entranceTask;
        private ITask _taskTimer;
        private StatVital _enteredStat;
        private StatVital _helpTimeStatStat;

        public HospitalTask(string buildingId,
                            IEnumerable<IPosSide> taskPoints,
                            IInstantiator instantiator,
                            StatsCollectionStorage statsCollectionStorage)
        {
            _buildingId = buildingId;
            _instantiator = instantiator;
            _statsCollectionStorage = statsCollectionStorage;
            _taskPoints = taskPoints.ToArray();
        }

        public override void Execute(params object[] args)
        {
            if (IsRunned) return;
            base.Execute(args);

            _unitId = (string) args[0];

            if (!_statsCollectionStorage.HasEntity(_buildingId))
            {
                throw new InvalidOperationException();
            }
            
            var statCollection = _statsCollectionStorage.Get(_buildingId);
            var unitStatCollection = _statsCollectionStorage.Get(_unitId);
            _helpTimeStatStat = statCollection.Get<StatVital>(_helpTimeStatKey);
            _enteredStat = unitStatCollection.Get<StatVital>(_entrancedStatKey);
            
            _taskTimer = _instantiator.Instantiate<TimerFromStatKeyTask>(new object[]{TimeType.Minutes});
            _taskTimer.OnComplete += _ => ExitHospital();
            _taskTimer.Execute(_buildingId, _helpTimeStatKey);
            
            if (_entranceTask != null)
            {
                // выполняется выход
                return;
            }
            
            // Вход
            _entranceTask = GetEntranceTask(true);
            _entranceTask.OnForceComplete += _ => ForceComplete();
            _entranceTask.Execute(_unitId);
        }

        private void ExitHospital()
        {
            if (!IsExecuted)
            {
                return;
            }
            _entranceTask?.Interrupt();
            _entranceTask = GetEntranceTask(false);
            _entranceTask.OnComplete += _ => Completed();
            _entranceTask.OnForceComplete += _ => ForceComplete();
            _entranceTask.Execute(_unitId);
        }
        
        private ITask GetEntranceTask(bool enter)
        {
            if (!IsExecuted)
            {
                return default;
            }
            var taskPoint = enter ? _taskPoints[1] : _taskPoints[0];
            var arg = new object[] {_buildingId, taskPoint, enter};
            var entranceTask = _instantiator.Instantiate<HospitalEntranceTask>(arg);
            _enteredStat.CurrentValue = enter ? 1 : 0;
            return entranceTask;
        }

        protected override void OnDisposed()
        {
            if (IsExecuted)
            {
                _helpTimeStatStat.SetMax();
                _taskTimer.Interrupt();
                ExitHospital();
                _entranceTask?.ForceComplete();
            }
            _entranceTask = null;
            _taskTimer = null;
            _enteredStat = null;
            _helpTimeStatStat = null;  
            _unitId = null;
            base.OnDisposed();
        }
    }
}
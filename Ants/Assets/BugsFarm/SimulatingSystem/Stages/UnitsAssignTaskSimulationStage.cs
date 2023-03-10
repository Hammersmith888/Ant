using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.BuildingSystem;
using BugsFarm.Services.StatsService;
using BugsFarm.SimulatingSystem.AssignableTasks;
using BugsFarm.UnitSystem;
using UniRx;
using Zenject;

namespace BugsFarm.SimulatingSystem
{
    public class UnitsAssignTaskSimulationStage
    {
        private readonly UnitDtoStorage _unitDtoStorage;
        
        private double _simulatingTimeInSeconds;
        private double _pastGameAge;
        private readonly SimulatingUnitAssignableTaskModelStorage _assignableTaskModelStorage;
        private readonly SimulatingTaskAssigner _simulatingTaskAssigner;
        private readonly UnitSleepSystem _unitSleepSystem;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly SimulatingTeleporter _simulatingTeleporter;
        private readonly UnitTaskProcessorStorage _unitTaskProcessorStorage;
        private readonly UnitMoverStorage _unitMoverStorage;
        private readonly BuildingDtoStorage _buildingDtoStorage;

        private const string _noNeedSleep = "stat_noNeedTimeSleep";
        private const float _dayInSeconds = 86400.0f;
        
        public UnitsAssignTaskSimulationStage(UnitDtoStorage unitDtoStorage,
                                                SimulatingUnitAssignableTaskModelStorage simulatingUnitAssignableTaskModelStorage,
                                                UnitTaskProcessorStorage unitTaskProcessorStorage,
                                                StatsCollectionStorage statsCollectionStorage,
                                                SimulatingTeleporter simulatingTeleporter,
                                                BuildingDtoStorage buildingDtoStorage,
                                                UnitMoverStorage unitMoverStorage,
                                                UnitSleepSystem unitSleepSystem,
                                                SimulatingTaskAssigner simulatingTaskAssigner)
        {
            _unitSleepSystem = unitSleepSystem;
            _buildingDtoStorage = buildingDtoStorage;
            _unitTaskProcessorStorage = unitTaskProcessorStorage;
            _simulatingTaskAssigner = simulatingTaskAssigner;
            _unitMoverStorage = unitMoverStorage;
            _simulatingTeleporter = simulatingTeleporter;
            _statsCollectionStorage = statsCollectionStorage;
            _assignableTaskModelStorage = simulatingUnitAssignableTaskModelStorage;
            _unitDtoStorage = unitDtoStorage;
        }

        public void AssignTasksToUnits(double simulatingTimeInSeconds, double pastGameAge)
        {
            _simulatingTimeInSeconds = simulatingTimeInSeconds;
            _pastGameAge = pastGameAge;
            
            float currentDayTimeInMinutes = CalculateCurrentTimeInSeconds() / 60.0f;
            bool isNight = currentDayTimeInMinutes > 720 && currentDayTimeInMinutes < 1140;
            foreach (var unitDto in _unitDtoStorage.Get().OrderBy(x=>Guid.NewGuid().ToString()))
            {
                if (isNight)
                {
                    SetUnitsToSleep(unitDto);
                }
                else
                {
                    AssignTaskToUnit(unitDto);
                }
            }

            if (isNight)
            {
                SetQueenToSleep();
            }
        }

        private void SetQueenToSleep()
        {
            var queenDto = _buildingDtoStorage.Get().First(x => x.ModelID == "54");
            var statsCollection = _statsCollectionStorage.Get(queenDto.Guid);
            if (!statsCollection.HasEntity(_noNeedSleep))
                return;
            statsCollection.Get<StatVital>(_noNeedSleep).CurrentValue = 0;
            MessageBroker.Default.Publish(new SimulatingSleepProtocol());
        }

        private void AssignTaskToUnit(UnitDto unitDto)
        {
            var modelID = unitDto.ModelID;
            
            if(!_assignableTaskModelStorage.HasEntity(modelID))
                return;
            
            var unitAssignableTaskModel = _assignableTaskModelStorage.Get(modelID);
            var assignableTasks = unitAssignableTaskModel.AssignableTasks;
            var orderedTasks = assignableTasks.OrderBy(x => x.Time).Reverse();

            _simulatingTaskAssigner.ProcessTasks(unitDto.Guid, orderedTasks);
        }

        private void SetUnitsToSleep(UnitDto unitDto)
        {
                        
            if(!_assignableTaskModelStorage.HasEntity(unitDto.ModelID))
                return;
            
            var node = _simulatingTeleporter.TeleportToRandom(unitDto.Guid);
            var statsCollection = _statsCollectionStorage.Get(unitDto.Guid);
            if (!statsCollection.HasEntity(_noNeedSleep))
                return;
            statsCollection.Get<StatVital>(_noNeedSleep).CurrentValue = 0;
            var processor = _unitTaskProcessorStorage.Get(unitDto.Guid);
            processor.Interrupt(); 
         //   processor.SetTask(_instantiator.Instantiate<SleepTask>(new object[]{ _unitSleepSystem.GetController(unitDto.Guid)}));
            _unitSleepSystem.AddToSleepy(unitDto.Guid);
            _unitSleepSystem.Start(unitDto.Guid, node);
            _unitMoverStorage.Get(unitDto.Guid).Stay();
        }

        private float CalculateCurrentTimeInSeconds()
        {
            var currentDay = (int)((_pastGameAge + _simulatingTimeInSeconds) / _dayInSeconds) + 1;
            return (float)((_pastGameAge + _simulatingTimeInSeconds) - _dayInSeconds * (currentDay - 1));
        }
    }
    public struct SimulatingSleepProtocol{}
}
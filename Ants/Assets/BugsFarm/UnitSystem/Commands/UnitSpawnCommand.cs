using System;
using System.Linq;
using System.Threading.Tasks;
using BugsFarm.Services.CommandService;
using BugsFarm.TaskSystem;
using Zenject;

namespace BugsFarm.UnitSystem
{
    public class UnitSpawnCommand<T> : ICommand<UnitSpawnProtocol> where T : ITask
    {
        private readonly UnitTaskProcessorStorage _taskProcessorStorage;
        private readonly IActivitySystem _activitySystem;
        private readonly IInstantiator _instantiator;
        private readonly UnitsContainer _unitsContainer;
        private IUnitTaskProcessor _taskProcessor;
        private string _unitGuid;

        public UnitSpawnCommand(UnitTaskProcessorStorage taskProcessorStorage,
                                IActivitySystem activitySystem,
                                IInstantiator instantiator,
                                UnitsContainer unitsContainer)
        {
            _taskProcessorStorage = taskProcessorStorage;
            _activitySystem = activitySystem;
            _instantiator = instantiator;
            _unitsContainer = unitsContainer;
        }

        public Task Execute(UnitSpawnProtocol protocol)
        {
            if (!_taskProcessorStorage.HasEntity(protocol.Guid))
            {
                throw new InvalidOperationException();
            }

            if (!_activitySystem.HasEntity(protocol.Guid))
            {
                throw new InvalidOperationException();
            }

            _unitGuid = protocol.Guid;
            _taskProcessor = _taskProcessorStorage.Get(protocol.Guid);
            var spawnPoint = protocol.SpawnPoint ?? _unitsContainer.SpawnPoint;
            var args = protocol.Args.Append(spawnPoint);
            var spawnTask = _instantiator.Instantiate<T>(args);
            _taskProcessor.OnFree += OnTaskProcessorFree;
            _taskProcessor.SetTask(spawnTask);
            return Task.CompletedTask;
        }

        private void OnTaskProcessorFree(ITask taskEnd)
        {
            if (_taskProcessor != null)
            {
                _taskProcessor.OnFree -= OnTaskProcessorFree;
            }
            if (_activitySystem.HasEntity(_unitGuid) && 
                !_activitySystem.IsActive(_unitGuid))
            {
                _activitySystem.Activate(_unitGuid, true, true);
            }
        }
    }
}
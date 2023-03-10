using BugsFarm.BuildingSystem;
using BugsFarm.UnitSystem;
using Zenject;

namespace BugsFarm.UI
{
    public class PatrolAssignableTaskProcessor : IUnitAssignableTaskProcessor
    {
        private readonly UnitTaskProcessorStorage _unitTaskProcessorStorage;
        private readonly IInstantiator _instantiator;

        public PatrolAssignableTaskProcessor(IInstantiator instantiator,
            UnitTaskProcessorStorage unitTaskProcessorStorage)
        {
            _instantiator = instantiator;
            _unitTaskProcessorStorage = unitTaskProcessorStorage;
        }

        public bool CanExecute(string guid)
        {
            return true;
        }

        public void Execute(string guid)
        {
            var taskProcessor = _unitTaskProcessorStorage.Get(guid);
            var task = taskProcessor.GetCurrentTask();

            if (task is PatrolingTask)
            {
                return;
            }
            
            var restTask = _instantiator.Instantiate<PatrolingTask>();
            restTask.SetAction(taskProcessor.Update);
            taskProcessor.SetTask(restTask);
        }
        
        
    }
}
using System;
using System.Collections.Generic;
using BugsFarm.Services.StorageService;
using BugsFarm.UserSystem;
using UniRx;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class AntHillTaskHandler
    {

        private readonly Dictionary<string, BaseAntHillTaskProcessor> _sortedTasks;
        private readonly Storage<AntHillTaskModel> _antHillTaskModelStorage;
        private readonly Storage<AntHillTaskDto> _antHillTaskDtoStorage;
        private readonly IInstantiator _instantiator;
        private readonly IUser _user;

        private IDisposable _actionPerformedEvent;
        private IDisposable _userLevelChanged;


        public AntHillTaskHandler(AntHillTaskModelStorage antHillTaskModelStorage,
                                     AntHillTaskDtoStorage antHillTaskDtoStorage,
                                     IInstantiator instantiator,
                                     IUser user)
        {
            _user = user;
            _instantiator = instantiator;
            _antHillTaskDtoStorage = antHillTaskDtoStorage;
            _antHillTaskModelStorage = antHillTaskModelStorage;
            _sortedTasks = new Dictionary<string, BaseAntHillTaskProcessor>();
        }
        
        public void Initialize()
        {
            if (!_antHillTaskDtoStorage.Any())
            {
                FillNewTasks();
                
            }
            else
            {
                SortDtosByTargetTypes();                
            }
            _actionPerformedEvent = MessageBroker.Default.Receive<AntHillTaskActionCompletedProtocol>().Subscribe(OnActionPerformed);
            _userLevelChanged = MessageBroker.Default.Receive<UserLevelChangedProtocol>().Subscribe(OnUserLevelChanged);
        }

        public int GetAmountOfTasks()
        {
            return _antHillTaskDtoStorage.Count;
        }
        
        public int GetAmountOfCompletedTasks()
        {
            int counter = 0;
            foreach (var taskDto in _antHillTaskDtoStorage.Get())
            {
                if (taskDto.IsCompleted())
                {
                    counter++;
                }
            }

            return counter;
        }
        
        public bool IsAllCompleted()
        {
            return GetAmountOfTasks() == GetAmountOfCompletedTasks();
        }
        
        private void OnActionPerformed(AntHillTaskActionCompletedProtocol protocol)
        {
            var protocolText = protocol.EntityType.ToString();
            if (_sortedTasks.Count == 0 || !_sortedTasks.ContainsKey(protocolText))
                return;
            _sortedTasks[protocolText].UpdateTask(protocol);
        }

        private void SetActualTasksToDtoDatabase()
        {
            var userLevel = _user.GetLevel();
            foreach (var taskModel in _antHillTaskModelStorage.Get())
            {
                if(taskModel.Level != userLevel)
                    continue;

                var taskDto = new AntHillTaskDto();
                taskDto.CompletionGoal = taskModel.CompletionGoal;
                taskDto.ReferenceGroup = taskModel.ReferenceGroup;
                taskDto.ReferenceModelID = taskModel.ReferenceModelID;
                taskDto.TaskID = taskModel.TaskID;
                taskDto.TaskType = taskModel.TaskType;
                taskDto.CompletionGoal = taskModel.CompletionGoal;
                taskDto.ConditionName = taskModel.ConditionName;
                taskDto.ConditionValue = taskModel.ConditionValue;
                taskDto.ProgressWay = taskModel.ProgressWay;
                taskDto.CurrentValue = 0;
                _antHillTaskDtoStorage.Add(taskDto);
            }
        }
        private void OnUserLevelChanged(UserLevelChangedProtocol protocol)
        {
            ClearCompletedTasks();
            FillNewTasks();
        }
        
        private void FillNewTasks()
        {
            SetActualTasksToDtoDatabase();
            SortDtosByTargetTypes();
        }

        private void SortDtosByTargetTypes()
        {
            foreach (var taskDto in _antHillTaskDtoStorage.Get())
            {
                if(!_sortedTasks.ContainsKey(taskDto.ReferenceGroup))
                    _sortedTasks.Add(taskDto.ReferenceGroup, GetProcessor(taskDto.ReferenceGroup));
                _sortedTasks[taskDto.ReferenceGroup].Add(taskDto);
                _sortedTasks[taskDto.ReferenceGroup].RefreshAmount(taskDto);
            }
        }

        private void ClearCompletedTasks()
        {
            _antHillTaskDtoStorage.Clear();
            _sortedTasks.Clear();
        }
        public void Dispose()
        {
            _actionPerformedEvent.Dispose();
            _userLevelChanged.Dispose();
        }

        private BaseAntHillTaskProcessor GetProcessor(string referenceGroup)
        {
            switch(referenceGroup)
            {
                case AntHillTaskReferenceGroup.Building:
                    return _instantiator.Instantiate<BuildingAntHillTaskProcessor>();
                case AntHillTaskReferenceGroup.Unit:
                    return _instantiator.Instantiate<UnitAntHillTaskProcessor>();
                case AntHillTaskReferenceGroup.Room: 
                    return _instantiator.Instantiate<RoomAntHillTaskProcessor>();
            };
            new InvalidOperationException($"Can't return new BaseAntHillTaskProcessor for {referenceGroup}");
            return null;
        }

        public void UpdateAll()
        {
            foreach (var taskDto in _antHillTaskDtoStorage.Get())
            {
                _sortedTasks[taskDto.ReferenceGroup].RefreshAmount(taskDto);
            }
        }
    }
}



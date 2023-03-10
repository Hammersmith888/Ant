using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BugsFarm.AssetLoaderSystem;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.Quest;
using BugsFarm.Services.InputService;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.UIService;
using BugsFarm.SimulationSystem;
using BugsFarm.TaskSystem;
using BugsFarm.UnitSystem;
using BugsFarm.Utility;
using UniRx;
using UnityEngine;
using Zenject;

namespace BugsFarm.UI
{
    public class AntInteractionCommand : InteractionBaseCommand, ITickable
    {
        private readonly IUIService _uiService;
        private readonly IInputController<MainLayer> _inputController;
        private readonly UnitDtoStorage _unitDtoStorage;
        private readonly UnitModelStorage _unitModelStorage;
        private readonly UnitStatModelStorage _unitStatModelStorage;
        private readonly StateInfoStorage _stateInfoStorage;
        private readonly ITickableManager _tickableManager;
        private readonly IconLoader _iconLoader;
        private readonly TaskStorage _taskStorage;
        private readonly UnitTaskModelStorage _taskModelStorage;
        private readonly UnitMoverStorage _moverStorage;

        private UnitDto _dto;
        private UIBugInfoMenuWindow _window;

        private Dictionary<string, IUnitAssignableTaskProcessor> _taskProcessors;
        private List<UnitTaskAssignSlot> _assignSlots;
        private int _enumPointer;

        private const string _assignableTaskPrefix = "AssignableTask_";
        public AntInteractionCommand(IUIService uiService,
                                     IInputController<MainLayer> inputController,
                                     UnitDtoStorage unitDtoStorage,
                                     UnitModelStorage unitModelStorage,
                                     UnitStatModelStorage unitStatModelStorage,
                                     StateInfoStorage stateInfoStorage,
                                     ITickableManager tickableManager,
                                     IconLoader iconLoader,
                                     IInstantiator instantiator,
                                     TaskStorage taskStorage,
                                     UnitTaskModelStorage taskModelStorage,
                                     UnitMoverStorage moverStorage)
        {
            _uiService = uiService;
            _inputController = inputController;
            _unitDtoStorage = unitDtoStorage;
            _unitModelStorage = unitModelStorage;
            _unitStatModelStorage = unitStatModelStorage;
            _stateInfoStorage = stateInfoStorage;
            _tickableManager = tickableManager;
            _iconLoader = iconLoader;
            _taskStorage = taskStorage;
            _taskModelStorage = taskModelStorage;
            _moverStorage = moverStorage;
            _assignSlots = new List<UnitTaskAssignSlot>();

            _taskProcessors = new Dictionary<string, IUnitAssignableTaskProcessor>()
            {
                {UnitAssignTasks.WakeUp, instantiator.Instantiate<WakeUpAssignableTaskProcessor>()},
                {UnitAssignTasks.Goldmine, instantiator.Instantiate<GoldmineAssignableTaskProcessor>()},
                {UnitAssignTasks.Dig, instantiator.Instantiate<DigAssignableTaskProcessor>()},
                {UnitAssignTasks.Train, instantiator.Instantiate<TrainAssignableTaskProcessor>()},
                {UnitAssignTasks.Patrol, instantiator.Instantiate<PatrolAssignableTaskProcessor>()},
            };
        }

        public override Task Execute(InteractionProtocol protocol)
        {
            _inputController.Lock();
            _dto = _unitDtoStorage.Get(protocol.Guid);
            if (_dto == null)
            {
                return Task.CompletedTask;
            }

            var model = _unitModelStorage.Get(_dto.ModelID);

            var unitStatData = _unitStatModelStorage.Get(_dto.ModelID);
            var unitTypeName = GetName(_dto.ModelID);
            var unitName = LocalizationHelper.GetBugName(_dto.NameID, model.IsFemale);

            // show window
            _window = _uiService.Show<UIBugInfoMenuWindow>();

            _tickableManager.Add(this);

            _window.CloseEvent += CloseWindow;
   

            _window.ArrowRightEvent += NextTask;
            _window.ArrowLeftEvent += PreviousTask;
            _window.SetTaskEvent += SetTask;
            
            Debug.Log($"unit selected:{unitTypeName} with name {unitName}");

            var dStats = "";
            foreach (var stat in unitStatData.Stats)
            {
                dStats += "\n" + $"stat {stat.StatID} is value {stat.BaseValue}";
            }

            _window.SetHeader(unitName);
            _window.SetTypeName(unitTypeName);
            _window.SetIcon(_iconLoader.Load(model.TypeName));

            _window.SetActiveAssignTab(true);
            
            _enumPointer = 0;
            UpdateAssignableTasks();
            
            return Task.CompletedTask;
        }

        private void CloseWindow(object sender, EventArgs e)
        {
            _uiService.Hide<UIBugInfoMenuWindow>();
            _inputController.UnLock();
            _tickableManager.Remove(this);
        }

        
        private void SetTask(object sender, EventArgs e)
        {
            var task = _assignSlots[_enumPointer];
            _taskProcessors[task.TaskName].Execute(_dto.Guid);
            
            MessageBroker.Default.Publish(new QuestUpdateProtocol()
            {
                QuestType = QuestType.RedirectBug,
                ReferenceID = QuestType.Any,
                Value = 1
            });
            CloseWindow(null, null);
            Debug.Log(task.TaskName);
        }

        private void UpdateTasks()
        {
            var allTask = _taskStorage.GetAllInfo();
            var tasksModel = _taskModelStorage.Get(_dto.ModelID);

            if (!tasksModel.IsAssignable)
            {
                _window.SetActiveAssignTab(false);
                return;
            }
            
            foreach (var taskInfo in allTask)
            {
                foreach (var taskName in tasksModel.Tasks)
                {
                    if (taskName == taskInfo.TaskName)
                    {
                        var mover = _moverStorage.Get(_dto.Guid);
                        var task = _taskStorage.Get(taskInfo.TaskGuid);

                        if (mover.CanReachTarget(task.GetPositions()))
                        {
                            if (_assignSlots.All(x => x.TaskName != taskInfo.TaskName))
                            {
                                var assignSlot = new UnitTaskAssignSlot()
                                {
                                    TaskName = taskInfo.TaskName
                                };
                                _assignSlots.Add(assignSlot);

                            }
                        }
                    }
                }
            }
        }

        private void UpdateAssignableTasks()
        {
            var tasksModel = _taskModelStorage.Get(_dto.ModelID);
            _assignSlots.Clear();
            
            if (!tasksModel.IsAssignable)
            {
                _window.SetActiveAssignTab(false);
                return;
            }

            foreach (var assignTask in tasksModel.AssignTasks)
            {
                if(!_taskProcessors.ContainsKey(assignTask) || !_taskProcessors[assignTask].CanExecute(_dto.Guid))
                    continue;
                
                _assignSlots.Add(new UnitTaskAssignSlot()
                {
                    TaskName = assignTask
                });
            }

            if (_assignSlots.Count == 0)
            {
                _window.SetActiveAssignTab(false);
                return;
            }

            _enumPointer %= _assignSlots.Count;
            UpdateTab();
        }

        private void NextTask(object sender, EventArgs eventArgs)
        {
            _enumPointer = (_enumPointer + 1) % _assignSlots.Count;
            UpdateTab();
        }

        private void PreviousTask(object sender, EventArgs eventArgs)
        {
            _enumPointer = _enumPointer == 0 ? _assignSlots.Count - 1 : _enumPointer - 1;
            UpdateTab();
        }

        private void UpdateTab()
        {
            var task = _assignSlots[_enumPointer];
            _window.SetTaskLabel(LocalizationManager.Localize(_assignableTaskPrefix + task.TaskName));
        }

        private string GetName(string modelId)
        {
            var entityName = LocalizationHelper.GetBugTypeName(modelId);
            return entityName;
        }

        public void Tick()
        {
            if (_dto == null) return;

            var info = _stateInfoStorage.Get(_dto.Guid);
            if (info == null) return;

            info.Update();

            UpdateAssignableTasks();
            
            var txt = "";
            foreach (var text in info.Info)
            {
                if (string.IsNullOrEmpty(text)) continue;
                txt += text + "\n";
            }

            _window.SetStat(txt);
        }
    }

    public static class UnitAssignTasks
    {
        public const string WakeUp = "WakeUp";
        public const string Goldmine = "Goldmine";
        public const string Dig = "Dig";
        public const string Train = "Train";
        public const string Patrol = "Patrol";
    }
    
    public struct UnitTaskAssignSlot
    {
        public string TaskName;
    }
    
 
}
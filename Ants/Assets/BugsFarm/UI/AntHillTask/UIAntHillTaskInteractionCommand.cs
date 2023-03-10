using System;
using System.Collections.Generic;
using BugsFarm.AssetLoaderSystem;
using BugsFarm.BuildingSystem;
using BugsFarm.Services.InputService;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.StateMachine;
using BugsFarm.Services.UIService;
using Zenject;
using Object = UnityEngine.Object;

namespace BugsFarm.UI
{
    public class UIAntHillTaskInteractionCommand : State
    {
        public event Action OnCloseWindow;
        public override string Id => "AntHillTask";

        private readonly AntHillTaskModelStorage _antHillTaskModelStorage;
        private readonly IInputController<MainLayer> _inputController;
        private readonly AntHillTaskDtoStorage _antHillTaskDtoStorage;
        private readonly AntHillTaskHandler _antHillTaskHandler;
        private readonly IInstantiator _instantiator;
        private readonly IconLoader _iconLoader;
        private readonly IUIService _uiService;
        private List<UIAntHillTaskSlot> _slots;
        private UIAntHillTaskWindow _window;

        private const string _taskCompletedLocalizationKey = "UIAnyHillTasks_TaskCompleted";
        
        public UIAntHillTaskInteractionCommand(AntHillTaskHandler antHillTaskHandler, 
                                               IUIService uiService,
                                               IInstantiator instantiator,
                                               AntHillTaskDtoStorage antHillTaskDtoStorage,
                                               AntHillTaskModelStorage antHillTaskModelStorage,
                                               IInputController<MainLayer> inputController,
                                               IconLoader iconLoader)
        {
            _iconLoader = iconLoader;
            _inputController = inputController;
            _antHillTaskModelStorage = antHillTaskModelStorage;
            _antHillTaskDtoStorage = antHillTaskDtoStorage;
            _instantiator = instantiator;
            _uiService = uiService;
            _antHillTaskHandler = antHillTaskHandler;
        }

        public override void OnEnter(params object[] args)
        {
            _inputController.Lock();
            _slots = new List<UIAntHillTaskSlot>();
            _window = _uiService.Get<UIAntHillTaskWindow>();
            InstantiateSlots();
            _window = _uiService.Show<UIAntHillTaskWindow>();
            _window.OnWindowClose += NotifyClosingWindow;
        }


        public override void OnExit()
        {
            _window.OnWindowClose -= NotifyClosingWindow;
            _inputController.UnLock();
            DisposeSlots();
            _uiService.Hide<UIAntHillTaskWindow>();
        }

        private void NotifyClosingWindow()
        {
            OnCloseWindow?.Invoke();
        }
        
        private void DisposeSlots()
        {
            foreach (var slot in _slots.ToArray())
            {
                slot.Dispose();
                Object.Destroy(slot.gameObject);
            }
        }

        private void InstantiateSlots()
        {
            foreach (var taskDto in _antHillTaskDtoStorage.Get())
            {
                UIAntHillTaskSlot slot = _instantiator.InstantiatePrefabForComponent<UIAntHillTaskSlot>(_window.AntHillTaskSlot,
                    _window.SlotParentRectTransform);
                var model = _antHillTaskModelStorage.Get(taskDto.Id);
                slot.SetTitle( LocalizationManager.Localize(model.Localization));
                slot.SetIcon(_iconLoader.Load(model.TaskIcon));
                if (taskDto.IsCompleted())
                {
                    slot.SetCompletedTaskText(LocalizationManager.Localize(_taskCompletedLocalizationKey));
                }
                else
                {
                    slot.SetProgressText($"{taskDto.CurrentValue} / {taskDto.CompletionGoal}");
                    slot.SetProgress((float)taskDto.CurrentValue / taskDto.CompletionGoal);
                }
                _slots.Add(slot);
            }
        }
        
    }
}
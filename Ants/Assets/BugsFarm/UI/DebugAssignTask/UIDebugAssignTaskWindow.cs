using System;
using System.Linq;
using BugsFarm.BuildingSystem;
using BugsFarm.Services.UIService;
using BugsFarm.TaskSystem;
using BugsFarm.UnitSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace BugsFarm.UI
{
    public class UIDebugAssignTaskWindow : UISimpleWindow
    {
        public event Action OnButtonClicked;
        
        [SerializeField] private Button assignTaskButton;
        
        public override void Show()
        {
            base.Show();
            assignTaskButton.onClick.AddListener(NotifyKillingButtonPressed);
        }

        private void NotifyKillingButtonPressed()
        {
            OnButtonClicked?.Invoke();
        }

        public override void Hide()
        {
            base.Hide();
            assignTaskButton.onClick.RemoveListener(NotifyKillingButtonPressed);
        }
    }

    public class UIDebugAssignTaskInteractor
    {
        private readonly BuildingSceneObjectStorage _buildingSceneObjectStorage;
        private readonly UnitTaskProcessorStorage _unitTaskProcessorStorage;
        private readonly BuildingDtoStorage _buildingDtoStorage;
        private readonly UnitMoverStorage _unitMoverStorage;
        private readonly UnitSleepSystem _unitSleepSystem;
        private readonly UnitNeedSystem _unitNeedSystem;
        private readonly UnitDtoStorage _unitDtoStorage;
        private readonly IInstantiator _instantiator;
        private readonly TaskStorage _taskStorage;
        private readonly IUIService _uiService;

        private const string _modelID = "5";
        private const string _taskName = nameof(ConsumeUnitTask);
        
        public UIDebugAssignTaskInteractor(IUIService uiService,
                                            IInstantiator instantiator,
                                            UnitDtoStorage unitDtoStorage, 
                                            UnitMoverStorage unitMoverStorage,
                                            UnitSleepSystem unitSleepSystem,
                                            UnitEatSystem unitNeedSystem,
                                            UnitTaskProcessorStorage unitTaskProcessorStorage,
                                            BuildingSceneObjectStorage buildingSceneObjectStorage,
                                            BuildingDtoStorage buildingDtoStorage,
                                            TaskStorage taskStorage)
        {
            _taskStorage = taskStorage;
            _unitNeedSystem = unitNeedSystem;
            _unitSleepSystem = unitSleepSystem;
            _buildingSceneObjectStorage = buildingSceneObjectStorage;
            _instantiator = instantiator;
            _buildingDtoStorage = buildingDtoStorage;
            _unitTaskProcessorStorage = unitTaskProcessorStorage;
            _unitMoverStorage = unitMoverStorage;
            _unitDtoStorage = unitDtoStorage;
            _uiService = uiService;
        }

        public void Initialize()
        {
            _uiService.Show<UIDebugAssignTaskWindow>().OnButtonClicked += AssignTaskToUnit;
        }

        private void AssignTaskToUnit()
        {
            var unit = _unitDtoStorage.Get().First(x => x.ModelID == "8");
            var buildingDto = _buildingDtoStorage.Get().First(x => x.ModelID == _modelID);
            var buildingSceneObject = _buildingSceneObjectStorage.Get(buildingDto.Guid);
            if (!buildingSceneObject.TryGetComponent(out TasksPoints points))
                return;
            
            var positionSide = points.Points.First();
            var position = positionSide.Position;
            var mover = _unitMoverStorage.Get(unit.Guid);
            Debug.Log("Assigned");
            mover.SetPosition(position);
            var processor = _unitTaskProcessorStorage.Get(unit.Guid);
            processor.Interrupt();
            FeedBug(unit, positionSide);
            //SetUsualTask(processor, unit);
        }

        private void FeedBug(UnitDto unitDto, IPosSide position)
        {
            _unitNeedSystem.Start(unitDto.Guid);
        }

        private void SleepBug(UnitDto unitDto, IPosSide posSide)
        {
           // _unitSleepSystem.Start(unitDto.Guid, posSide.Position);
        }
        
        private void SetUsualTask(IUnitTaskProcessor processor, UnitDto unit)
        {
            var allTasks = _taskStorage.GetAllInfo().Where(x => x.TaskName == _taskName);
            foreach (var taskInfo in allTasks)
            {
                var task = _taskStorage.Get(taskInfo.TaskGuid);
                if (processor.CanExecute(task))
                {
                    _unitTaskProcessorStorage.Get(unit.Guid).SetTask(task);
                    return;
                }
            }
        }

        public void Dispose()
        {
            var window = _uiService.Get<UIDebugAssignTaskWindow>();
            window.OnButtonClicked -= AssignTaskToUnit;
            _uiService.Hide<UIDebugAssignTaskWindow>();
        }
    }
}
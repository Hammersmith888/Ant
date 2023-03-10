using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.AnimationsSystem;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.InventorySystem;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StatsService;
using BugsFarm.TaskSystem;
using BugsFarm.UnitSystem;
using UniRx;
using UnityEngine;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class HerbsStock : ISceneEntity, IInitializable
    {
        public string Id { get; }
        private string GetItemID => "5"; // предмет травы обработанной
        private string AddItemID => "1"; // предмет травы не обработанной
        private AnimKey[] GetActionAnimKeys { get; } = {AnimKey.Put};
        private AnimKey[] AddActionAnimKeys { get; } = {AnimKey.GiveHerbs};
        private AnimKey GetWalkAnimKey => AnimKey.Walk;
        private AnimKey AddWalkAnimKey => AnimKey.WalkHerbs;
        private Type GetTaskType => typeof(GetHerbsStockTask);
        private Type AddTaskType => typeof(AddHerbsStockTask);

        private readonly GetResourceSystem _getResourceSystem;
        private readonly AddResourceSystem _addResourceSystem;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly BuildingSceneObjectStorage _viewStorage;
        private readonly BuildingDtoStorage _dtoStorage;

        private readonly PathHelper _pathHelper;
        private readonly TaskStorage _taskStorage;
        private readonly IInstantiator _instantiator;

        private readonly PointsController _pointsController;
        private readonly List<ITask> _createdHerbsTasks;
        private readonly ResourceArgs _resourceArgs;
        
        private const float _timePointsUpdate = 60f;
        private const int _pointGenerateCount = 20;
        private const int _herbItemPoolCount = 20;
        private const string _resourceStatKey = "stat_maxResource";
        private const string _maxUnitsStatKey = "stat_maxUnits";
        private const string _productionPercentStatKey = "stat_productionPercent";

        private float _lasTimePointsUpdate = -1;
        private float _productionPercent;
        private BuildingDto _dto;
        private ResourceInfo _resourceInfo;
        private BuildingSceneObject _view;
        private IInventory _inventory;
        private IEnumerable<IPosSide> _taskPoints;
        private StatsCollection _statsCollection;
        private IDisposable _placeChangedEvent;
        private bool _finalized;

        public HerbsStock(string guid,
                          GetResourceSystem getResourceSystem,
                          AddResourceSystem addResourceSystem,
                          StatsCollectionStorage statsCollectionStorage,
                          BuildingSceneObjectStorage viewStorage,
                          BuildingDtoStorage dtoStorage,
                          IInstantiator instantiator,
                          PathHelper pathHelper,
                          TaskStorage taskStorage)
        {
            Id = guid;
            _getResourceSystem = getResourceSystem;
            _addResourceSystem = addResourceSystem;
            _statsCollectionStorage = statsCollectionStorage;
            _instantiator = instantiator;
            _viewStorage = viewStorage;
            _dtoStorage = dtoStorage;

            _pathHelper = pathHelper;
            _taskStorage = taskStorage;
            _pointsController = new PointsController();
            _createdHerbsTasks = new List<ITask>();
            _resourceArgs = ResourceArgs.Default();

        }

        public void Initialize()
        {
            _statsCollection = _statsCollectionStorage.Get(Id);
            var resourceStat = _statsCollection.Get<StatVital>(_resourceStatKey);
            _productionPercent = _statsCollection.GetValue(_productionPercentStatKey);
            var itemSlots = new[]
            {
                new ItemSlot(GetItemID, (int)resourceStat.CurrentValue, (int) resourceStat.Value),
                new ItemSlot(AddItemID, (int)resourceStat.CurrentValue, (int) resourceStat.Value)
            };
            var inventoryProtocol = new CreateInventoryProtocol(Id, res=>_inventory=res, itemSlots);
             _instantiator.Instantiate<CreateInventoryCommand>().Execute(inventoryProtocol);

            _view = _viewStorage.Get(Id);
            _dto = _dtoStorage.Get(Id);
            _taskPoints = _viewStorage.Get(Id).GetComponent<TasksPoints>().Points;
            _getResourceSystem.OnResourceChanged  += OnResourceChanged;
            _getResourceSystem.OnResourceDepleted += OnResourceChanged;
            _addResourceSystem.OnResourceChanged  += OnResourceAdd;

            _resourceInfo = _instantiator.Instantiate<ResourceInfo>(new object[] {Id});
            _resourceInfo.OnUpdate += OnUpdateResourceInfo;
            _pointsController.Initialize((int)_statsCollection.GetValue(_maxUnitsStatKey), GeneratePoints());

            _placeChangedEvent = MessageBroker.Default
                .Receive<PlaceChangedProtocol>()
                .Subscribe(OnPlaceChangedEventHandler);
            _resourceArgs.SetActionAnimKeys(GetActionAnimKeys);

            CreateHerbsTasks();
            UpdateStage();
            Production();
        }

        public void Dispose()
        {
            if (_finalized)  return;
            _finalized = true;
            _placeChangedEvent?.Dispose();
            _placeChangedEvent = null;
            StopTasks();
            _resourceInfo.OnUpdate -= OnUpdateResourceInfo;
            _resourceInfo.Dispose();
            _resourceInfo = null;
            _statsCollection = null;
            
            _getResourceSystem.OnResourceChanged -= OnResourceChanged;
            _getResourceSystem.OnResourceDepleted -= OnResourceChanged;
            _addResourceSystem.OnResourceChanged -= OnResourceAdd;
            
            _getResourceSystem.UnRegistration(GetItemID, Id);
            _addResourceSystem.UnRegistration(GetItemID, Id);
            _instantiator.Instantiate<DeleteInventoryCommand>().Execute(new DeleteInventoryProtocol(Id));

            _dto = null;
        }

    #region Herb production

        private IEnumerable<IPosSide> GeneratePoints()
        {
            if (_lasTimePointsUpdate > Time.time)
            {
                return default;
            }

            _lasTimePointsUpdate = _timePointsUpdate + Time.time;
            var pathHelperQuery = PathHelperQuery.Empty().UseGraphMask(GetType().Name);
            return _pathHelper
                .GetRandomNodes(pathHelperQuery, _pointGenerateCount)
                .Select(x => (IPosSide) new SPosSide {Position = x.Position, LookLeft = Tools.RandomBool()});
        }

        private void CreateHerbsTasks()
        {
            if (_finalized) return;

            while (_pointsController.HasPoint())
            {
                var args = new object[] {_pointsController.GetPoint().Position, AddItemID, _herbItemPoolCount, _resourceArgs};
                var getHerbsTask = _instantiator.Instantiate<GetCreateResourceBootstrapTask>(args);
                getHerbsTask.OnComplete  += OnGetHerbsTaskEnd;
                getHerbsTask.OnInterrupt += OnGetHerbsTaskEnd;

                _createdHerbsTasks.Add(getHerbsTask);
                _taskStorage.DeclareTask(Id, _dto.ModelID, getHerbsTask.GetName(), getHerbsTask, false);
            }
        }

        private void StopTasks()
        {
            var copyTasks = _createdHerbsTasks.ToArray();
            foreach (var task in copyTasks)
            {
                ClearTask(task);
            }
        }

        private void ClearTask(ITask task)
        {
            if (_createdHerbsTasks.Contains(task))
            {
                _createdHerbsTasks.Remove(task);
            }

            if (_taskStorage.HasTask(task.Guid))
            {
                _taskStorage.Remove(task.Guid);
            }

            if (!task.IsCompleted)
            {
                task.Interrupt();
            }
        }

        private void OnGetHerbsTaskEnd(ITask task)
        {
            if (_finalized) return;

            _pointsController.FreePoint();
            ClearTask(task);
            CreateHerbsTasks();
        }

        private void OnPlaceChangedEventHandler(PlaceChangedProtocol protocol)
        {
            var taskPoints = GeneratePoints();
            if (taskPoints == null)
            {
                return;
            }
            _pointsController.Replace(taskPoints);
        }

    #endregion

    #region Stock Production

        private void Production()
        {
            if (_finalized) return;
            
            var currPercent = (_inventory.GetItemSlot(GetItemID).Count / _statsCollection.GetValue(_resourceStatKey)) * 100f;
            // Get resource
            if (_inventory.HasItem(GetItemID) && currPercent >= _productionPercent &&
                !_getResourceSystem.Contains(GetItemID, Id))
            {
                var maxUnits = (int)_statsCollection.GetValue(_maxUnitsStatKey);
                var args = ResourceArgs.Default()
                           .SetActionAnimKeys(GetActionAnimKeys)
                           .SetWalkAnimKeys(GetWalkAnimKey);
                var protocol = new GetResourceProtocol(Id, _dto.ModelID, GetItemID, maxUnits, 
                                                       _taskPoints, GetTaskType, args);
                _getResourceSystem.Registration(protocol);
            }
            else if (!_inventory.HasItem(GetItemID) && _getResourceSystem.Contains(GetItemID, Id))
            {
                _getResourceSystem.UnRegistration(GetItemID, Id);
            }

            var itemSlot = _inventory.GetItemSlot(GetItemID);
            var needAdd = itemSlot == null || itemSlot.Count < itemSlot.Capacity;

            // Add resource
            if (needAdd && !_addResourceSystem.Contains(AddItemID, Id))
            {
                var maxUnits = (int)_statsCollection.GetValue(_maxUnitsStatKey);
                var args = ResourceArgs.Default()
                    .SetActionAnimKeys(AddActionAnimKeys)
                    .SetWalkAnimKeys(AddWalkAnimKey);
                var protocol = new AddResourceProtocol(Id, _dto.ModelID, AddItemID, maxUnits, 
                                                       _taskPoints, AddTaskType, args, false);
                _addResourceSystem.Registration(protocol);
            }
            else if (!needAdd && _addResourceSystem.Contains(AddItemID, Id))
            {
                _addResourceSystem.UnRegistration(AddItemID, Id);
                if (_inventory.HasItem(AddItemID))
                {
                    var deleteCount = _inventory.GetItemSlot(AddItemID).Count;
                    _inventory.Remove(AddItemID, deleteCount);
                }
            }
        }

        private void UpdateStage()
        {
            if (_finalized) return;

            var itemSlot = _inventory.GetItemSlot(GetItemID);
            var protocol = new SetStageBuildingProtocol(Id, itemSlot.Count, itemSlot.Capacity, OnSetStage);
            _instantiator.Instantiate<SetStageBuildingCommand>().Execute(protocol);
        }

        private void OnSetStage(StageActionProtocol protocol)
        {
            _view.SetInterractable(protocol.CurIndex != protocol.MaxIndex);
        }

        private void OnResourceAdd(string itemId, string guid)
        {
            if (guid != Id || itemId != AddItemID || _finalized)
            {
                return;
            }

            var addCount = _inventory.GetItemSlot(AddItemID).Count;
            var addItemProtocol = new AddItemsProtocol(GetItemID, addCount, Id);
            _instantiator.Instantiate<AddItemsCommand>().Execute(addItemProtocol);

            if (_inventory.HasItem(AddItemID))
            {
                _inventory.Remove(AddItemID, addCount);
            }

            OnResourceChanged(GetItemID, guid);
        }

        private void OnResourceChanged(string itemId, string guid)
        {
            if (guid != Id || itemId != GetItemID || _finalized)
            {
                return;
            }

            UpdateStage();
            Production();
        }
        
        private void OnUpdateResourceInfo()
        {
            if (_finalized) return;

            var slot = _inventory.GetItemSlot(GetItemID);
            _resourceInfo.SetInfo(Format.Resource(slot.Count, slot.Capacity));
        }

    #endregion
    }
}
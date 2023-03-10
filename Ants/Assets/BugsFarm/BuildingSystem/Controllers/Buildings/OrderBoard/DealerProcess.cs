using System;
using System.Linq;
using BugsFarm.AnimationsSystem;
using BugsFarm.InventorySystem;
using BugsFarm.Services.StateMachine;
using BugsFarm.UnitSystem;
using UniRx;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace BugsFarm.BuildingSystem
{
    public class DealerProcess : State
    {
        private readonly UnitDtoStorage _unitDtoStorage;
        private readonly OrderDtoStorage _orderDtoStorage;
        private readonly AddResourceSystem _addResourceSystem;
        private readonly GetResourceSystem _getResourceSystem;
        private readonly IActivitySystem _activitySystem;
        private readonly IInstantiator _instantiator;

        private readonly TasksPoints _buildingTaskPoints;
        private readonly IInventory _buildingInventory;

        private readonly BuildingDto _buildingDto;
        private readonly string _buildingId;
        private const string _orderItemId = "6";
        private const float _activityPingSeconds = 1f;
        private TasksPoints _unitFromPoints;
        private bool _initialized;
        private UnitDto _unitDto;
        private IDisposable _moveBuildingEvent;
        private IDisposable _activityPingEvent;

        public DealerProcess(string buildingId,
                             UnitDtoStorage unitDtoStorage,
                             OrderDtoStorage orderDtoStorage,
                             BuildingDtoStorage buildingDtoStorage,
                             BuildingSceneObjectStorage buildingSceneObjectStorage,
                             AddResourceSystem addResourceSystem,
                             GetResourceSystem getResourceSystem,
                             InventoryStorage inventoryStorage,
                             IActivitySystem activitySystem,
                             IInstantiator instantiator) : base("Process")
        {
            _buildingId = buildingId;
            _unitDtoStorage = unitDtoStorage;
            _orderDtoStorage = orderDtoStorage;
            _addResourceSystem = addResourceSystem;
            _getResourceSystem = getResourceSystem;
            _activitySystem = activitySystem;
            _instantiator = instantiator;
            _buildingTaskPoints = buildingSceneObjectStorage.Get(buildingId).GetComponent<TasksPoints>();
            _buildingInventory = inventoryStorage.Get(buildingId);
            _buildingDto = buildingDtoStorage.Get(buildingId);
        }

        public override void OnEnter(params object[] args)
        {
            if (!_initialized)
            {
                _unitFromPoints = (TasksPoints) args[0];
                var unitId = (string) args[1];
                _unitDto = _unitDtoStorage.Get(unitId);
                _addResourceSystem.OnResourceChanged += OnAddResourceChanged;
                _addResourceSystem.OnResourceFull += OnAddResourceChanged;
                _getResourceSystem.OnResourceChanged += OnGetResourceChanged;
                _getResourceSystem.OnResourceDepleted += OnGetResourceChanged;
                _moveBuildingEvent = MessageBroker.Default.Receive<PlaceBuildingProtocol>().Subscribe(OnSceneItemMoved);
                _initialized = true;
                
                foreach (var orderDto in _orderDtoStorage.Get())
                {
                    orderDto.OnStageChanged += OnOrderSwitchState;
                }

                GetProduction();
                AddProduction();
            }
        }

        public override void OnExit()
        {
            if (!_initialized) return;
            _addResourceSystem.OnResourceChanged -= OnAddResourceChanged;
            _addResourceSystem.OnResourceFull -= OnAddResourceChanged;
            _getResourceSystem.OnResourceChanged -= OnGetResourceChanged;
            _getResourceSystem.OnResourceDepleted -= OnGetResourceChanged;

            _addResourceSystem.UnRegistration(_orderItemId, _buildingId);
            _getResourceSystem.UnRegistration(_orderItemId, _buildingId);
            foreach (var orderDto in _orderDtoStorage.Get())
            {
                orderDto.OnStageChanged -= OnOrderSwitchState;
            }

            _initialized = false;
            _moveBuildingEvent?.Dispose();
            _moveBuildingEvent = null;
            _unitFromPoints = null;
            _unitDto = null;
        }

        private IPosSide GetFromDealerPoint()
        {
            var points = _unitFromPoints.Points.ToArray();
            var point1 = points[0];
            var point2 = points[1];
            var screenDistance = Vector2.Distance(point1.Position, point2.Position) / 2f; // 100% one screen
            var halfScreenDistance = screenDistance / 2f;                              // 50% half screen
            var buildingPoints = _buildingTaskPoints.Points.ToArray();
            var boardT = _buildingTaskPoints.Points
                .Sum(posSide => Vector2.Distance(posSide.Position, point1.Position)) / buildingPoints.Length;


            if (boardT <= screenDistance)
            {
                return boardT >= halfScreenDistance ? point1 : point2;
            }

            return boardT <= (screenDistance + halfScreenDistance) ? point2 : point1;
        }

        private void AddOrderItem()
        {
            if (!_initialized) return;

            if (!_buildingInventory.HasItem(_orderItemId))
            {
                var addItemsProtocol = new AddItemsProtocol(_orderItemId, 1, _buildingId);
                _instantiator.Instantiate<AddItemsCommand>().Execute(addItemsProtocol);
            }
        }

        private void AddProduction()
        {
            if (!_initialized) return;

            if (!_addResourceSystem.Contains(_orderItemId, _buildingId))
            {
                var args = ResourceArgs.Default()
                    .SetActionAnimKeys(AnimKey.Put)
                    .SetWalkAnimKeys(AnimKey.WalkGarbage);
                var taskPoints = new[] {GetFromDealerPoint()};
                var resourceProtocol = new AddResourceProtocol(_buildingId, _buildingDto.ModelID, _orderItemId, 1
                                                             , taskPoints, typeof(AddDealerFarmTask), args, true);
                _addResourceSystem.Registration(resourceProtocol);
            }
        }

        private void GetProduction()
        {
            if (!_initialized) return;

            var hasItem = _buildingInventory.HasItem(_orderItemId);
            var registred = _getResourceSystem.Contains(_orderItemId, _buildingId);
            if (hasItem && !registred)
            {
                var args = ResourceArgs.Default()
                    .SetActionAnimKeys(AnimKey.Put)
                    .SetWalkAnimKeys(AnimKey.Walk)
                    .SetRepeatCounts(Random.Range(1, 4));
                var taskPoints = _buildingTaskPoints.Points;
                var resourceProtocol = new GetResourceProtocol(_buildingId, _buildingDto.ModelID, _orderItemId, 1
                                                             , taskPoints, typeof(GetOrderBoardTask), args);
                _getResourceSystem.Registration(resourceProtocol);
                CreateDealerPingActivity();
            }
            else if (!hasItem && registred)
            {
                _activityPingEvent?.Dispose();
                _activityPingEvent = null;
                _getResourceSystem.UnRegistration(_orderItemId, _buildingId);
            }
        }

        private void CreateDealerPingActivity()
        {
            void Ping()
            {
                if (_unitDto == null ||
                    _activitySystem.IsActive(_unitDto.Guid) ||
                    !_getResourceSystem.Contains(_orderItemId, _buildingId))
                {
                    _activityPingEvent?.Dispose();
                    _activityPingEvent = null;
                    return;
                }

                _activitySystem.Activate(_unitDto.Guid, true, true);
                if (_activitySystem.IsActive(_unitDto.Guid))
                {
                    _activityPingEvent?.Dispose();
                    _activityPingEvent = null;
                }
            }
            _activityPingEvent?.Dispose();
            _activityPingEvent = Observable.Interval(TimeSpan.FromSeconds(_activityPingSeconds))
                .Subscribe(_ => Ping());
        }

        private void OnAddResourceChanged(string itemId, string ownerId)
        {
            if (!_initialized || _buildingId != ownerId || itemId != _orderItemId)
            {
                return;
            }

            if (_buildingInventory.HasItem(_orderItemId))
            {
                _buildingInventory.Remove(_orderItemId);
            }
        }

        private void OnGetResourceChanged(string itemId, string ownerId)
        {
            if (_orderItemId != itemId || _buildingId != ownerId) return;
            GetProduction();
        }

        private void OnOrderSwitchState(string orderId)
        {
            if (!_initialized || !_orderDtoStorage.HasEntity(orderId))
            {
                return;
            }

            var orderDto = _orderDtoStorage.Get(orderId);
            if (orderDto.Stage == OrderStage.Reward)
            {
                AddOrderItem();
                GetProduction();
                AddProduction();
            }
        }

        private void OnSceneItemMoved(PlaceBuildingProtocol protocol)
        {
            if (protocol.Guid != _buildingId || !_initialized || _unitDto == null)
            {
                return;
            }

            if (_addResourceSystem.Contains(_orderItemId, _buildingId))
            {
                var pointsContoller = _addResourceSystem.GetTaskPointController(_orderItemId, _buildingId);
                pointsContoller.Replace(new[] {GetFromDealerPoint()});
            }
            
            AddProduction();
        }
    }
}
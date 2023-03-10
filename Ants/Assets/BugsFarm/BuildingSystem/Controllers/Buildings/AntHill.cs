using System;
using System.Linq;
using BugsFarm.BuildingSystem.DeathSystem;
using BugsFarm.Services.CommandService;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.UIService;
using BugsFarm.TaskSystem;
using BugsFarm.UI;
using BugsFarm.UnitSystem;
using UniRx;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class AntHill : ISceneEntity, IInitializable, IPostInitializable
    {
        public string Id { get; }
        private readonly IInstantiator _instantiator;

        private readonly ResurrectBuildingDataStorage _resurrectBuildingDataStorage;
        private readonly AddResourceSystem _addResourceSystem;
        private readonly IReservedPlaceSystem _reservedPlaceSystem;
        private readonly UnitDtoStorage _unitDtoStorage;
        private readonly BuildingDtoStorage _buildingDtoStorage;
        private readonly IActivitySystem _activitySystem;
        private readonly PlaceIdStorage _placeIdStorage;
        private readonly CompositeDisposable _events;

        private const string _foodItemId = "0";
        private const string _grabageItemId = "3";
        private const string _grabageStockModelID = "45";
        private const string _foodStockModelID = "42";
        private const string _dumpsterModelID = "40";
        private const string _hospitalModelID = "48";
        private const string _queenModelID = "54";

        private ITask _initGrabageStockTask;
        private ITask _initFoodStockTask;
        private bool _finalized;
        private bool _initialized;


        public AntHill(string guid,
                       IInstantiator instantiator,
                       AddResourceSystem addResourceSystem,
                       IReservedPlaceSystem reservedPlaceSystem,
                       UnitDtoStorage unitDtoStorage,
                       BuildingDtoStorage buildingDtoStorage,
                       PlaceIdStorage placeIdStorage,
                       IActivitySystem activitySystem,
                       ResurrectBuildingDataStorage resurrectBuildingDataStorage)
        {
            Id = guid;
            _instantiator = instantiator;
            _addResourceSystem = addResourceSystem;
            _reservedPlaceSystem = reservedPlaceSystem;
            _unitDtoStorage = unitDtoStorage;
            _buildingDtoStorage = buildingDtoStorage;
            _placeIdStorage = placeIdStorage;
            _activitySystem = activitySystem;
            _resurrectBuildingDataStorage = resurrectBuildingDataStorage;
            _events = new CompositeDisposable();
        }

        public void Initialize()
        {
            if (_finalized || _initialized)
            {
                return;
            }
            
            _reservedPlaceSystem.OnPlaceFree  += OnPlaceFree;
            _addResourceSystem.OnSystemUpdate += OnAddSystemUpdate;
            Listen<DeathUnitProtocol>(OnUnitDeathEventHandler);
            Listen<PostDeathBuildingProtocol>(OnPostDeathBuildingEventHandler);
            _initialized = true;
            Production();
        }
        
        public void OnPostInitialize()
        {
            if (!HasLiveQueen())
            {
                OpenResurrectWindow();
            }
        }
        public void Dispose()
        {
            if (_finalized)
            {
                return;
            }

            _finalized = true;
            _events.Dispose();
            _events.Clear();
            _reservedPlaceSystem.OnPlaceFree -= OnPlaceFree;
            _addResourceSystem.OnSystemUpdate -= OnAddSystemUpdate;
        }

        private bool HasLiveQueen()
        {
            foreach (var resurrectBuildingData in _resurrectBuildingDataStorage.Get())
            {
                if (resurrectBuildingData.ModelID == _queenModelID)
                {
                    return false;
                }
            }
            return true;
        }

        private void Listen<T>(Action<T> onAction) where T : IProtocol
        {
            MessageBroker.Default.Receive<T>().Subscribe(onAction).AddTo(_events);
        }
        
        private void Production()
        {
            if (_finalized || !_initialized)
            {
                return;
            }

            var hasGrabageTasks = false;
            var defaultGrabage = 0;
            var hasDumpstersTasks = false;

            foreach (var buildingDto in _buildingDtoStorage.Get())
            {
                if (buildingDto.ModelID == _dumpsterModelID)
                {
                    hasDumpstersTasks = _addResourceSystem.HasTasks(_grabageItemId, buildingDto.Guid);
                }

                if (hasDumpstersTasks)
                {
                    break; // Нет смысла собирать информацию если есть мусорный бак с задачами
                }


                if (buildingDto.ModelID == _grabageStockModelID)
                {
                    hasGrabageTasks = _addResourceSystem.HasTasks(_grabageItemId, buildingDto.Guid);
                    if (hasGrabageTasks)
                    {
                        break;
                    }

                    if (buildingDto.PlaceNum == PlaceBuildingUtils.OffScreenPlaceNum)
                    {
                        defaultGrabage++;
                    }
                }
            }

            // Создаем сток мусора если :
            // нет мусорника или мусорники полные,
            // нет в запасе свободного стока мусора,
            // нет стока который принимает мусор,
            // есть место под сток мусора.
            if (!hasDumpstersTasks && !hasGrabageTasks && defaultGrabage == 0 && HasPlaceNum(_grabageStockModelID))
            {
                var buildingBuildProtocol = new CreateBuildingProtocol(_grabageStockModelID, PlaceBuildingUtils.OffScreenPlaceNum, true, true);
                _instantiator.Instantiate<CreateBuildingCommand>().Execute(buildingBuildProtocol);
            }

            var hasFoodStock = _buildingDtoStorage.Get().Any(x => x.ModelID == _foodStockModelID);
            // Создаем сток еды
            if (!hasFoodStock && HasPlaceNum(_foodStockModelID))
            {
                var buildingBuildProtocol = new CreateBuildingProtocol(_foodStockModelID, PlaceBuildingUtils.OffScreenPlaceNum, true, true);
                _instantiator.Instantiate<CreateBuildingCommand>().Execute(buildingBuildProtocol);
            }
        }
        
        private bool HasPlaceNum(string modelId)
        {
            return _placeIdStorage.Get().Any(x=> x.HasPlace(modelId) && !_reservedPlaceSystem.HasEntity(x.PlaceNumber));
        }
        
        private void OnPlaceFree(string placeNum)
        {
            if (_finalized || !_initialized)
            {
                return;
            }

            Production();
        }
        
        private void OnUnitDeathEventHandler(DeathUnitProtocol protocol)
        {
            if (protocol.DeathReason != DeathReason.Fighted)
            {
                return;
            }

            if (_buildingDtoStorage.Get().Any(x => x.ModelID == _hospitalModelID))
            {
                return;
            }
            
            DeleteUnit(protocol.UnitId);
        }
        private void OnPostDeathBuildingEventHandler(PostDeathBuildingProtocol protocol)
        {
            if(protocol.ModelID == _queenModelID)
                OpenResurrectWindow();
        }

        private void OpenResurrectWindow()
        {
            _instantiator.Instantiate<AllUnitsResurrectionCommand>().Execute();
        }
        private void DeleteUnit(string unitId)
        {
            if (_unitDtoStorage.HasEntity(unitId))
            {
                _instantiator.Instantiate<DeleteUnitCommand>()
                    .Execute(new DeleteUnitProtocol(unitId));
            }
        }

        private void OnAddSystemUpdate(string itemID, string guid)
        {
            if (itemID != _foodItemId && itemID != _grabageItemId || _finalized || !_initialized)
            {
                return;
            }

            Production();
        }

    }
}
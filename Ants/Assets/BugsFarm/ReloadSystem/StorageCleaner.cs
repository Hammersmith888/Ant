using System.Linq;
using BugsFarm.AnimationsSystem;
using BugsFarm.App;
using BugsFarm.AstarGraph;
using BugsFarm.AudioSystem;
using BugsFarm.BattleSystem;
using BugsFarm.BuildingSystem;
using BugsFarm.ChestSystem;
using BugsFarm.CurrencySystem;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.InventorySystem;
using BugsFarm.LeafHeapSystem;
using BugsFarm.RoomSystem;
using BugsFarm.Services.InteractorSystem;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StatsService;
using BugsFarm.Services.TypeRegistry;
using BugsFarm.SimulatingSystem;
using BugsFarm.SpeakerSystem;
using BugsFarm.UI;
using BugsFarm.UnitSystem;
using Zenject;

namespace BugsFarm.ReloadSystem
{
    public class StorageCleaner
    {
        private readonly IInstantiator _instantiator;
        private readonly InteractorStorage _interactorStorage;
        private readonly AnimatorStorage _animatorStorage;
        private readonly AnimationModelStorage _animationModelStorage;
        private readonly SceneEntityStorage _sceneEntityStorage;
        private readonly UnitTaskProcessorStorage _unitTaskProcessorStorage;
        private readonly AppState _appState;

        public StorageCleaner(IInstantiator instantiator,
            InteractorStorage interactorStorage,
            AnimatorStorage animatorStorage,
            AnimationModelStorage animationModelStorage,
            SceneEntityStorage sceneEntityStorage,
            UnitTaskProcessorStorage unitTaskProcessorStorage,
            AppState appState)
        {
            _appState = appState;
            _unitTaskProcessorStorage = unitTaskProcessorStorage;
            _sceneEntityStorage = sceneEntityStorage;
            _animatorStorage = animatorStorage;
            _animationModelStorage = animationModelStorage;
            _interactorStorage = interactorStorage;
            _instantiator = instantiator;
        }

        public void ClearStorages()
        {
            _appState.Dispose();

            _instantiator.Instantiate<UnloadStorageCommand<TypeItem>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<AudioModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<GraphMaskModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<BattlePassModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<DonatShopItemModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<MyBugItemModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<CurrencySettingModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<PhrasesModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<InventoryItemModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<BuildingsSimulationGroup>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<SimulatingFoodOrderModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<UnitsSimulationGroupModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<SimulatingRoomGroupModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<SimulatingTrainingModel>>().Do();

            _instantiator.Instantiate<UnloadStorageCommand<RoomModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<RoomNeighbourModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<RoomStatModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<LeafHeapModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<LeafHeapStatModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<ChestModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<ChestStatModel>>().Do();
            
            _instantiator.Instantiate<UnloadStorageCommand<RoomBaseSceneObject>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<BuildingSceneObject>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<ChestSceneObject>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<UnitSceneObject>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<UnitMoverDto>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<IUnitMover>>().Do();
            foreach (var unitTaskProcessor in _unitTaskProcessorStorage.Get().ToArray())
            {
                unitTaskProcessor.Dispose();
            }
            _unitTaskProcessorStorage.Clear();
            
            foreach (var sceneEntity in _sceneEntityStorage.Get())
            {
                sceneEntity.Dispose();
            }
            _sceneEntityStorage.Clear();
            _animatorStorage.Clear();
            _animationModelStorage.Clear();
            _instantiator.Instantiate<UnloadStorageCommand<LeafHeapSceneObject>>().Do();
            
            _instantiator.Instantiate<UnloadStorageCommand<IStateInfo>>().Do();


            //_instantiator.Instantiate<UnloadStorageCommand<IUnitTaskProcessor>>().Do();
            
            
            _instantiator.Instantiate<UnloadStorageCommand<BuildingOpenablesModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<BuildingModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<BuildingParamModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<BuildingShopItemModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<BuildingStageModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<BuildingStatModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<BuildingUpgradeModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<BuildingInfoParamsModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<BuildingRestockModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<BuildingSpeedupModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<OrderModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<HospitalSlotModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<PrisonSlotModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<AntHillTaskModel>>().Do();
            
            _instantiator.Instantiate<UnloadStorageCommand<UnitModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<UnitBirthModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<UnitShopItemModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<UnitStageModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<UnitStatModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<UnitTasksModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<UnitUpgradeModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<RoomNeighbourModel>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<RoomStatModel>>().Do();
            
            _instantiator.Instantiate<UnloadStorageCommand<StatsCollectionDto>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<RoomDto>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<BuildingDto>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<UnitDto>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<LeafHeapDto>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<ChestDto>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<InventoryDto>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<OrderDto>>().Do();
            // _instantiator.Instantiate<UnloadStorageCommand<UserDto>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<HospitalSlotDto>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<LarvaPointDto>>().Do();
            _instantiator.Instantiate<UnloadStorageCommand<UnitCivilRegistryDto>>().Do();
            

            foreach (var interactorService in _interactorStorage.Get())
            { 
                interactorService.Dispose();
            }
            _interactorStorage.Clear();
            //_instantiator.Instantiate<UnloadStorageCommand<IInteractorService>>().Do();
            
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.AnimationsSystem;
using BugsFarm.BuildingSystem.DeathSystem;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.InventorySystem;
using BugsFarm.RoomSystem;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.StateMachine;
using BugsFarm.Services.StatsService;
using BugsFarm.SimulationSystem;
using BugsFarm.SpeakerSystem;
using BugsFarm.TaskSystem;
using BugsFarm.UnitSystem;
using UniRx;
using UnityEngine;
using Zenject;
using TickableManager = Zenject.TickableManager;

namespace BugsFarm.BuildingSystem
{
    public class QueenOld : ISceneEntity, IInitializable
    {
        public string Id { get; }

        private readonly ISpeakerSystem _speakerSystem;
        private readonly IRoomsSystem _roomsSystem;
        private readonly IInstantiator _instantiator;
        private readonly ISimulationSystem _simulationSystem;
        private readonly GetResourceSystem _getResourceSystem;
        private readonly AddResourceSystem _addResourceSystem;
        private readonly TickableManager _tickableManager;
        private readonly UnitBirthModelStorage _unitBirthModelStorage;
        private readonly BuildingDtoStorage _buildingDtoStorage;
        private readonly UnitDtoStorage _unitDtoStorage;
        private readonly BuildingSceneObjectStorage _buildingSceneObjectStorage;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        
        private const AnimKey _rest = AnimKey.Idle;
        private const string _larvaTextKey = "BuildingsInner_54_Larva";
        private const string _birthTextKey = "BuildingsInner_54_Birth";

    #region Need keys
        
        private const string _itemWater = "4";
        private const string _itemFood = "0";
        private const string _itemSleep = "sleepItem";
        
        private const string _foodResourceStatKey  = "stat_consumeFood";
        private const string _waterResourceStatKey = "stat_consumeWater";
        private const string _sleepResourceStatKey = "stat_consumeSleep";

        private const string _noNeedEatTimeStatKey   = "stat_noNeedTimeFood";
        private const string _noNeedDrinkTimeStatKey = "stat_noNeedTimeWater";
        private const string _noNeedSleepTimeStatKey = "stat_noNeedTimeSleep";

        private const string _needEatTimeStatKey   = "stat_timeWithoutFood";
        private const string _needDrinkTimeStatKey = "stat_timeWithoutWater";
        private const string _needSleepTimeStatKey = "stat_timeWithoutSleep";

        private const string _eatPrefix   = "Eat_";
        private const string _drinkPrefix = "Drink_";
        private const string _sleepPrefix = "Sleep_";
        
    #endregion

    #region Stat keys
        
        private const string _betwenBornTimeStatKey = "stat_betwenBornTime";
        private const string _bornPerRoomStatKey = "stat_bornPerRoom";
        private const string _maxUnitsStatKey = "stat_maxUnits";
        private const string _bornStatKey = "stat_bornTime";
        
    #endregion

        private readonly Dictionary<string, NeedStatController> _needControllers;
        private readonly BuildingDeathSystem _buildingDeathSystem;
        private readonly IStateMachine _stateMachine;
        private BuildingSpineObject _view;
        private UnitBirthModel _birthModel;
        private BuildingDto _buildingDto;
        private StateInfo _stateInfo;
        private ResourceInfo _resourceInfo;
        private ISpineAnimator _animator;
        private IInventory _inventory;

        private IDisposable _openRoomEvent;
        private StatsCollection _statsCollection;
        private StatVital _betwenBornTimeStat;

        private bool _finalized;
        private ITask _birthTimerTask;
        private ITask _birthTask;
        private ITask _sleepTask;

        public QueenOld(string guid,
                     ISpeakerSystem speakerSystem,
                     IRoomsSystem roomsSystem,
                     IInstantiator instantiator,
                     ISimulationSystem simulationSystem,
                     GetResourceSystem getResourceSystem,
                     AddResourceSystem addResourceSystem,
                     UnitBirthModelStorage unitBirthModelStorage,
                     BuildingDtoStorage buildingDtoStorage,
                     UnitDtoStorage unitDtoStorage,
                     BuildingSceneObjectStorage buildingSceneObjectStorage,
                     StatsCollectionStorage statsCollectionStorage,
                     BuildingDeathSystem buildingDeathSystem,
                     IStateMachine stateMachine)
        {
            Id = guid;
            _speakerSystem = speakerSystem;
            _stateMachine = stateMachine;
            _roomsSystem = roomsSystem;
            _instantiator = instantiator;
            _simulationSystem = simulationSystem;
            _getResourceSystem = getResourceSystem;
            _addResourceSystem = addResourceSystem;
            _unitBirthModelStorage = unitBirthModelStorage;
            _buildingDtoStorage = buildingDtoStorage;
            _unitDtoStorage = unitDtoStorage;
            _buildingSceneObjectStorage = buildingSceneObjectStorage;
            _statsCollectionStorage = statsCollectionStorage;
            _buildingDeathSystem = buildingDeathSystem;
            _needControllers = new Dictionary<string, NeedStatController>();
        }

        public void Initialize()
        {
            _view = (BuildingSpineObject) _buildingSceneObjectStorage.Get(Id);
            _birthModel = _unitBirthModelStorage.Get(GetType().Name);
            _buildingDto = _buildingDtoStorage.Get(Id);
            
            _statsCollection = _statsCollectionStorage.Get(Id);
            _betwenBornTimeStat = _statsCollection.Get<StatVital>(_betwenBornTimeStatKey);

            _stateInfo    = _instantiator.Instantiate<StateInfo>(new object[] {Id});
            _resourceInfo = _instantiator.Instantiate<ResourceInfo>(new object[] {Id});
            _resourceInfo.OnUpdate += OnResourceInfoUpdate;
            _stateInfo.OnUpdate += OnUpdateStateInfo;

            var typeName = GetType().Name;
            var spine = _view.MainSkeleton;
            
            var slots = new[] { new ItemSlot(_itemFood, 0, (int)_statsCollection.GetValue(_foodResourceStatKey)), 
                                new ItemSlot(_itemWater, 0, (int)_statsCollection.GetValue(_waterResourceStatKey))};
            var inventoryProtocol = new CreateInventoryProtocol(Id, res => _inventory = res, slots);
            _instantiator.Instantiate<CreateInventoryCommand>().Execute(inventoryProtocol);

            var animProtocol = new CreateAnimatorProtocol(Id, typeName, res => _animator = res, spine);
            _instantiator.Instantiate<CreateAnimatorCommand<SpineAnimator>>().Execute(animProtocol);

            _speakerSystem.Registration(Id, typeName, _view.transform);
            _speakerSystem.OnBeforeSpeak += OnBeforeSpeak;
            _openRoomEvent = MessageBroker.Default.Receive<OpenRoomProtocol>().Subscribe(OnRoomOpened);

            CreateNeed(_itemFood,  _eatPrefix,   _foodResourceStatKey,  _noNeedEatTimeStatKey,   _needEatTimeStatKey);
            CreateNeed(_itemWater, _drinkPrefix, _waterResourceStatKey, _noNeedDrinkTimeStatKey, _needDrinkTimeStatKey);
            CreateNeed(_itemSleep, _sleepPrefix, _sleepResourceStatKey, _noNeedSleepTimeStatKey, _needSleepTimeStatKey);

            _animator.OnAnimationComplete += OnAnimationComplete;
            _getResourceSystem.OnSystemUpdate += OnGetSystemUpdate;
            _addResourceSystem.OnResourceChanged += OnResourceChanged;
            _addResourceSystem.OnResourceFull += OnResourceFull;
            
            _buildingDeathSystem.Register(Id, _needEatTimeStatKey, _noNeedSleepTimeStatKey);
            MessageBroker.Default.Receive<PostDeathBuildingProtocol>().Subscribe(OnBuildingDestroyed);
            
            InitBirthTimer();
            Update(Id);
        }

        public void Dispose()
        {
            if (_finalized) return;

            _finalized = true;
            _openRoomEvent?.Dispose();
            _openRoomEvent = null;
            _resourceInfo.OnUpdate -= OnResourceInfoUpdate;
            _stateInfo.OnUpdate -= OnUpdateStateInfo;
            _speakerSystem.OnBeforeSpeak -= OnBeforeSpeak;
            _resourceInfo.Dispose();
            _stateInfo.Dispose();
            foreach (var needController in _needControllers.Values)
            {
                needController?.Dispose();
            }

            _betwenBornTimeStat = null;
            _statsCollection = null;
            _animator.OnAnimationComplete -= OnAnimationComplete;
            _getResourceSystem.OnSystemUpdate -= OnGetSystemUpdate;
            _addResourceSystem.OnResourceChanged -= OnResourceChanged;
            _addResourceSystem.OnResourceFull -= OnResourceFull;
            _addResourceSystem.UnRegistration(_itemFood, Id);
            _addResourceSystem.UnRegistration(_itemWater, Id);

            _needControllers.Clear();
            _speakerSystem.UnRegistration(Id);
            
            _instantiator.Instantiate<RemoveAnimatorCommand>().Execute(new RemoveAnimatorProtocol(Id));
            _instantiator.Instantiate<DeleteInventoryCommand>().Execute(new DeleteInventoryProtocol(Id));
        }


        private void OnBuildingDestroyed(PostDeathBuildingProtocol postDeathBuildingProtocol)
        {
            if(postDeathBuildingProtocol.Guid != Id)
                return;

            ChangeState();
        }

        private void ChangeState()
        {
        }

        private void CreateNeed(string itemId, string prefix, string statKey1, string statKey2, string statKey3)
        {
            var args = new object[] {new NeedStatPtotocol(Id, prefix, statKey1, statKey2, statKey3, Update)};
            _needControllers.Add(itemId, _instantiator.Instantiate<NeedStatController>(args));
        }

        private void Update(string guid)
        {
            if (Id != guid || _finalized) return;
            var eatController = _needControllers[_itemFood];
            var alredyEat = _addResourceSystem.Contains(_itemFood, Id);
            var taskType = typeof(AddFeedQueenTask);
            
            if (eatController.IsNeed && !alredyEat)
            {
                _inventory.Remove(_itemFood);
                var resourceArgs = ResourceArgs.Default()
                    .SetActionAnimKeys(AnimKey.GiveFood)
                    .SetWalkAnimKeys(AnimKey.WalkFood);
                var taskPoints = _view.GetComponent<TasksPoints>().Points;
                var maxUnits = (int)_statsCollection.GetValue(_maxUnitsStatKey);
                var addProtocol = new AddResourceProtocol(Id, _buildingDto.ModelID, _itemFood, maxUnits, taskPoints,
                                                          taskType, resourceArgs, true, new object[]{eatController});
                _addResourceSystem.Registration(addProtocol);
                _animator.SetAnim(_rest);
            }
            else if (eatController.IsIdle && alredyEat)
            {
                _addResourceSystem.UnRegistration(_itemFood, Id);
                eatController.UseAvailable();
            }

            var drinkController = _needControllers[_itemWater];
            var alredyDrink = _addResourceSystem.Contains(_itemWater, Id);

            if (drinkController.IsNeed && !alredyDrink)
            {
                _inventory.Remove(_itemWater);
                var resourceArgs = ResourceArgs.Default()
                    .SetActionAnimKeys(AnimKey.GiveWater)
                    .SetWalkAnimKeys(AnimKey.WalkWater);
                var taskPoints = _view.GetComponent<TasksPoints>().Points;
                var maxUnits = (int)_statsCollection.GetValue(_maxUnitsStatKey);
                var addProtocol = new AddResourceProtocol(Id, _buildingDto.ModelID, _itemWater, maxUnits, taskPoints,
                                                          taskType, resourceArgs, true, new object[]{drinkController});
                _addResourceSystem.Registration(addProtocol);
                _animator.SetAnim(_rest);
            }
            else if (drinkController.IsIdle && alredyDrink)
            {
                _addResourceSystem.UnRegistration(_itemWater, Id);
                eatController.UseAvailable();
            }

            var sleepController = _needControllers[_itemSleep];
            var canSleep = eatController.IsIdle && drinkController.IsIdle;
            if (canSleep && sleepController.IsNeed && _sleepTask == null)
            {
                _sleepTask = _instantiator.Instantiate<SleepTask>(new object[] {_needControllers[_itemSleep]});
                _sleepTask.OnComplete += _ => Update(Id);
                _sleepTask.OnInterrupt += _ => Update(Id);
                _sleepTask.Execute(Id);
            }
            else if (!canSleep && _sleepTask != null)
            {
                _sleepTask.Interrupt();
                _sleepTask = null;
            }

            if (eatController.IsIdle && drinkController.IsIdle && sleepController.IsIdle)
            {
                if (!BirthProduction())
                {
                    if (!_animator.CurrentAnim.AnyOff(AnimKey.Birth, AnimKey.Eat))
                    {
                        _animator.SetAnim(_rest);
                    }
                }
            }
            else if (_birthTask != null)
            {
                _birthTask?.Interrupt();
                _birthTask = null;
            }
        }

        private void InitBirthTimer()
        {
            if (_birthTimerTask != null) return;

            _birthTimerTask = _instantiator.Instantiate<TimerFromStatKeyTask>(new object[]{TimeType.Minutes});
            _birthTimerTask.OnComplete  += OnBirthTimerComplete;
            _birthTimerTask.Execute(Id, _betwenBornTimeStatKey);
        }

        private bool BirthProduction()
        {
            // если уже родила или рожает
            if (_birthTimerTask != null || _birthTask != null || _finalized) return false;

            int LarvaToBirth()
            {
                var rooms = _roomsSystem.Opened().Count();
                var units = _unitDtoStorage.Get().ToArray();

                float count = units.Count(x => _birthModel.TrackingUnitsModelID.Contains(x.ModelID));
                return Mathf.RoundToInt(Mathf.Max(0, _statsCollection.GetValue(_bornPerRoomStatKey) - (count / rooms)));
            }

            var unitCount = LarvaToBirth();
            // некого рожать
            if (unitCount == 0) return false;

            var birthModel = _unitBirthModelStorage.Get(GetType().Name);
            var larvaCount = _unitDtoStorage.Get().Count(x => x.ModelID == birthModel.LarvaModelID);
            var taskPoints = _view.GetComponent<LarvaPoints>().Points.Skip(larvaCount).ToPositions();
            if (taskPoints.Length == 0)
            {
                return false;
            }

            // если места есть но, их меньше, рожает сколько есть мест.
            unitCount = Mathf.Min(taskPoints.Length, unitCount);

            _birthTask = _instantiator.Instantiate<LarvaBirthTask>();
            _birthTask.OnComplete += OnBirthComplete;
            _birthTask.OnInterrupt += OnBirthInterrupt;
            _birthTask.Execute(Id, birthModel, unitCount, taskPoints);
            return true;
        }

        private void OnAnimationComplete(AnimKey animKey)
        {
            if (animKey == AnimKey.Eat)
            {
                _animator.SetAnim(_rest);
                Update(Id);
            }
        }

        private void OnGetSystemUpdate(string itemId, string guid)
        {
            if (!itemId.AnyOff(_itemFood, _itemWater)) return;
            Update(Id);
        }

        private void OnResourceChanged(string itemId, string guid)
        {
            if (Id != guid || !itemId.AnyOff(_itemFood, _itemWater)) return;
            _animator.SetAnim(AnimKey.Eat);
        }

        private void OnResourceFull(string itemId, string guid)
        {
            if (Id != guid || !itemId.AnyOff(_itemFood, _itemWater)) return;
            OnResourceChanged(itemId, guid);
            _addResourceSystem.UnRegistration(itemId, guid);
            _needControllers[itemId].UseAvailable();
            _inventory.Remove(itemId);
        }

        private void OnRoomOpened(OpenRoomProtocol protocol)
        {
            Update(Id);
        }

        private void OnBirthTimerComplete(ITask task)
        {
            _birthTimerTask = null;
            Update(Id);
        }

        private void OnBirthComplete(ITask task)
        {
            _birthTask = null;
            _animator.SetAnim(_rest);
            _betwenBornTimeStat.SetMax();
            InitBirthTimer();
        }

        private void OnBirthInterrupt(ITask task)
        {
            _birthTask = null;
            _birthTimerTask?.Interrupt();
            _birthTimerTask = null;
            InitBirthTimer();
        }

        private void OnBeforeSpeak()
        {
            var needFoodInfo = _needControllers[_itemFood];
            var needWaterInfo = _needControllers[_itemWater];
            var needSleepInfo = _needControllers[_itemSleep];

            var noFood = needFoodInfo.IsNeed && !_getResourceSystem.HasItem(_itemFood);
            var noWater = needWaterInfo.IsNeed && !_getResourceSystem.HasItem(_itemWater);
            var isSleep = needSleepInfo.IsRestock;
            var state = noFood ? PhraseState.noFood :
                       noWater ? PhraseState.noWater :
                       isSleep ? PhraseState.none : PhraseState.idle;
            _speakerSystem.ChangeState(Id, state);
        }

        private void OnUpdateStateInfo()
        {
            string BuildInfo(string key, float time, bool noMinSec = false)
            {
                return LocalizationManager.Localize(key) + " : " + Format.Age(time, noMinSec);
            }

            var needFoodInfo = _needControllers[_itemFood];
            var needWaterInfo = _needControllers[_itemWater];
            var needSleepInfo = _needControllers[_itemSleep];
            var bornTime = _simulationSystem.GameAge -  _statsCollection.GetValue(_bornStatKey);
            var birthTime = Format.MinutesToSeconds(_betwenBornTimeStat.CurrentValue);

            var infoAge = BuildInfo(Texts.Age, (float) bornTime);
            var infoFood = BuildInfo(needFoodInfo.HeaderKey, needFoodInfo.Time, true);
            var infoWater = BuildInfo(needWaterInfo.HeaderKey, needWaterInfo.Time, true);
            var infoSleep = BuildInfo(needSleepInfo.HeaderKey, needSleepInfo.Time, true);
            var infoLarva = birthTime > 0 ? BuildInfo(_birthTextKey, birthTime) : "";

            _stateInfo.SetInfo(infoAge, infoFood, infoWater, infoSleep, infoLarva);
        }

        private void OnResourceInfoUpdate()
        {
            var units = _unitDtoStorage.Get().ToArray();
            var larvaCount = units.Count(x => _birthModel.LarvaModelID == x.ModelID);
            var larvaText = LocalizationManager.Localize(_larvaTextKey);

            _resourceInfo.SetInfo(larvaText + " : " + larvaCount);
        }
    }
}
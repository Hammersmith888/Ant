using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.AnimationsSystem;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.InventorySystem;
using BugsFarm.RoomSystem;
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

namespace BugsFarm.BuildingSystem.States
{
    public class QueenLifetimeState : State
    {
        public override string Id => "QueenLifetime";
        private string _queenGuid;


        private Dictionary<string, NeedStatController> _needControllers;

        private readonly BuildingSceneObjectStorage _buildingSceneObjectStorage;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly UnitBirthModelStorage _unitBirthModelStorage;
        private readonly LarvaPointDtoStorage _larvaPointDtoStorage;
        private readonly BuildingDtoStorage _buildingDtoStorage;
        private readonly AddResourceSystem _addResourceSystem;
        private readonly GetResourceSystem _getResourceSystem;
        private readonly ITickableManager _tickableManager;
        private readonly UnitDtoStorage _unitDtoStorage;
        private readonly IInstantiator _instantiator;
        private readonly IRoomsSystem _roomsSystem;
        private readonly ISimulationSystem _simulationSystem;
        private readonly ISpeakerSystem _speakerSystem;
        
        private LarvaPoints _larvaPoints;
        private IInventory _inventory;
        private StatVital _betwenBornTimeStat;
        private StatsCollection _statsCollection;
        private UnitBirthModel _unitBirthModel;
        private IDisposable _openRoomEvent;
        private BuildingSpineObject _view;
        private BuildingDto _buildingDto;
        private ISpineAnimator _animator;

        private StateInfo _stateInfo;
        private ResourceInfo _resourceInfo;

        private ITask _birthTimerTask;
        private ITask _sleepTask;
        private ITask _birthTask;
        private string _typeName;

        private const AnimKey _rest = AnimKey.Idle;

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
        
        private const string _bornPerRoomStatKey = "stat_bornPerRoom";
        private const string _betwenBornTimeStatKey = "stat_betwenBornTime";
        
        private const string _maxUnitsStatKey = "stat_maxUnits";
        private const string _larvaTextKey = "BuildingsInner_54_Larva";
        private const string _birthTextKey = "BuildingsInner_54_Birth";
        private const string _bornStatKey = "stat_bornTime";


        public QueenLifetimeState(AddResourceSystem addResourceSystem,
                                  IInstantiator instantiator,
                                  IRoomsSystem roomsSystem,
                                  UnitDtoStorage unitDtoStorage,
                                  UnitBirthModelStorage unitBirthModelStorage,
                                  GetResourceSystem getResourceSystem,
                                  ISimulationSystem simulationSystem,
                                  ISpeakerSystem speakerSystem,
                                  BuildingSceneObjectStorage buildingSceneObjectStorage,
                                  BuildingDtoStorage buildingDtoStorage,
                                  StatsCollectionStorage statsCollectionStorage,
                                  LarvaPointDtoStorage larvaPointDtoStorage)
        {
            _larvaPointDtoStorage = larvaPointDtoStorage;
            _buildingDtoStorage = buildingDtoStorage;
            _statsCollectionStorage = statsCollectionStorage;
            _buildingSceneObjectStorage = buildingSceneObjectStorage;
            _simulationSystem = simulationSystem;
            _getResourceSystem = getResourceSystem;
            _unitBirthModelStorage = unitBirthModelStorage;
            _unitDtoStorage = unitDtoStorage;
            _instantiator = instantiator;
            _addResourceSystem = addResourceSystem;
            _roomsSystem = roomsSystem;
            _speakerSystem = speakerSystem;
            _needControllers = new Dictionary<string, NeedStatController>();
        }


        [Inject]
        private void SetDependencies(string id, string typeName, ISpineAnimator animator)
        {
            _queenGuid = id;
            _typeName = typeName;
            _statsCollection = _statsCollectionStorage.Get(_queenGuid);
            var slots = new[] { new ItemSlot(_itemFood, 0, (int)_statsCollection.GetValue(_foodResourceStatKey)), 
                new ItemSlot(_itemWater, 0, (int)_statsCollection.GetValue(_waterResourceStatKey))};
            var inventoryProtocol = new CreateInventoryProtocol(_queenGuid, res => _inventory = res, slots);
            _instantiator.Instantiate<CreateInventoryCommand>().Execute(inventoryProtocol);
            _animator = animator;
            _view = (BuildingSpineObject) _buildingSceneObjectStorage.Get(_queenGuid);
            _larvaPoints = _view.GetComponent<LarvaPoints>();
            _unitBirthModel = _unitBirthModelStorage.Get(_typeName);
            _buildingDto = _buildingDtoStorage.Get(_queenGuid);
            _betwenBornTimeStat = _statsCollection.Get<StatVital>(_betwenBornTimeStatKey);
            _stateInfo = _instantiator.Instantiate<StateInfo>(new object[] {_queenGuid});
            _resourceInfo = _instantiator.Instantiate<ResourceInfo>(new object[] {_queenGuid});
        }
        public override void OnEnter(params object[] args)
        {
            _resourceInfo.OnUpdate += OnResourceInfoUpdate;
            _stateInfo.OnUpdate += OnUpdateStateInfo;
            _speakerSystem.Registration(_queenGuid, _typeName, _view.transform);
            _speakerSystem.OnBeforeSpeak += OnBeforeSpeak;
            _openRoomEvent = MessageBroker.Default.Receive<OpenRoomProtocol>().Subscribe(OnRoomOpened);

            CreateNeed(_itemFood,  _eatPrefix,   _foodResourceStatKey,  _noNeedEatTimeStatKey,   _needEatTimeStatKey);
            CreateNeed(_itemWater, _drinkPrefix, _waterResourceStatKey, _noNeedDrinkTimeStatKey, _needDrinkTimeStatKey);
            CreateNeed(_itemSleep, _sleepPrefix, _sleepResourceStatKey, _noNeedSleepTimeStatKey, _needSleepTimeStatKey);

            _animator.OnAnimationComplete += OnAnimationComplete;
            _getResourceSystem.OnSystemUpdate += OnGetSystemUpdate;
            _addResourceSystem.OnResourceChanged += OnResourceChanged;
            _addResourceSystem.OnResourceFull += OnResourceFull;
            
            InitBirthTimer();
            Update(_queenGuid);
        }
        public override void OnExit()
        {
            _openRoomEvent?.Dispose();
            _openRoomEvent = null;
            _resourceInfo.OnUpdate -= OnResourceInfoUpdate;
            _stateInfo.OnUpdate -= OnUpdateStateInfo;
            _speakerSystem.OnBeforeSpeak -= OnBeforeSpeak;

            foreach (var needController in _needControllers.Values)
            {
                needController?.Dispose();
            }

            _animator.OnAnimationComplete -= OnAnimationComplete;
            _getResourceSystem.OnSystemUpdate -= OnGetSystemUpdate;
            _addResourceSystem.OnResourceChanged -= OnResourceChanged;
            _addResourceSystem.OnResourceFull -= OnResourceFull;
            _addResourceSystem.UnRegistration(_itemFood, _queenGuid);
            _addResourceSystem.UnRegistration(_itemWater, _queenGuid);

            _needControllers.Clear();
            _speakerSystem.UnRegistration(_queenGuid);

        }
        
        private void CreateNeed(string itemId, string prefix, string statKey1, string statKey2, string statKey3)
        {
            var args = new object[] {new NeedStatPtotocol(_queenGuid, prefix, statKey1, statKey2, statKey3, Update)};
            _needControllers.Add(itemId, _instantiator.Instantiate<NeedStatController>(args));
        }
        
        private void Update(string id)
        {
            if (id != _queenGuid) return;
            
            var eatController = _needControllers[_itemFood];
            var alredyEat = _addResourceSystem.Contains(_itemFood, _queenGuid);
            var taskType = typeof(AddFeedQueenTask);
            
            if (eatController.IsNeed && !alredyEat)
            {
                _inventory.Remove(_itemFood);
                var resourceArgs = ResourceArgs.Default()
                    .SetActionAnimKeys(AnimKey.GiveFood)
                    .SetWalkAnimKeys(AnimKey.WalkFood);
                var taskPoints = _view.GetComponent<TasksPoints>().Points;
                var maxUnits = (int)_statsCollection.GetValue(_maxUnitsStatKey);
                var addProtocol = new AddResourceProtocol(_queenGuid, _buildingDto.ModelID, _itemFood, maxUnits, taskPoints,
                                                          taskType, resourceArgs, true, new object[]{eatController});
                _addResourceSystem.Registration(addProtocol);
                _animator.SetAnim(_rest);
            }
            else if (eatController.IsIdle && alredyEat)
            {
                _addResourceSystem.UnRegistration(_itemFood, _queenGuid);
                eatController.UseAvailable();
            }

            var drinkController = _needControllers[_itemWater];
            var alredyDrink = _addResourceSystem.Contains(_itemWater, _queenGuid);

            if (drinkController.IsNeed && !alredyDrink)
            {
                _inventory.Remove(_itemWater);
                var resourceArgs = ResourceArgs.Default()
                    .SetActionAnimKeys(AnimKey.GiveWater)
                    .SetWalkAnimKeys(AnimKey.WalkWater);
                var taskPoints = _view.GetComponent<TasksPoints>().Points;
                var maxUnits = (int)_statsCollection.GetValue(_maxUnitsStatKey);
                var addProtocol = new AddResourceProtocol(_queenGuid, _buildingDto.ModelID, _itemWater, maxUnits, taskPoints,
                                                          taskType, resourceArgs, true, new object[]{drinkController});
                _addResourceSystem.Registration(addProtocol);
                _animator.SetAnim(_rest);
            }
            else if (drinkController.IsIdle && alredyDrink)
            {
                _addResourceSystem.UnRegistration(_itemWater, _queenGuid);
                eatController.UseAvailable();
            }

            var sleepController = _needControllers[_itemSleep];
            var canSleep = eatController.IsIdle && drinkController.IsIdle;
            if (canSleep && sleepController.IsNeed && _sleepTask == null)
            {
                _sleepTask = _instantiator.Instantiate<SleepTask>(new object[] {_needControllers[_itemSleep]});
                _sleepTask.OnComplete += _ => Update(_queenGuid);
                _sleepTask.OnInterrupt += _ => Update(_queenGuid);
                _sleepTask.Execute(_queenGuid);
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

        private bool BirthProduction()
        {
            // если уже родила или рожает
            if (_birthTimerTask != null || _birthTask != null) return false;

            int LarvaToBirth()
            {
                var rooms = _roomsSystem.Opened().Count();
                var units = _unitDtoStorage.Get().ToArray();

                float count = units.Count(x => _unitBirthModel.TrackingUnitsModelID.Contains(x.ModelID));
                return Mathf.RoundToInt(Mathf.Max(0, _statsCollection.GetValue(_bornPerRoomStatKey) - (count / rooms)));
            }

            var unitCount = LarvaToBirth();
            // некого рожать
            if (unitCount == 0) return false;

            var birthModel = _unitBirthModelStorage.Get(_typeName);
            var larvaCount = _unitDtoStorage.Get().Count(x => x.ModelID == birthModel.LarvaModelID);
            var taskPoints = _larvaPoints.Points.Skip(larvaCount).ToPositions();
            if (taskPoints.Length == 0)
            {
                return false;
            }

            // если места есть но, их меньше, рожает сколько есть мест.
            unitCount = Mathf.Min(taskPoints.Length, unitCount);

            _birthTask = _instantiator.Instantiate<LarvaBirthTask>();
            _birthTask.OnComplete += OnBirthComplete;
            _birthTask.OnInterrupt += OnBirthInterrupt;
            _birthTask.Execute(_queenGuid, birthModel, unitCount, taskPoints);
            return true;
        }
        
        private void OnBirthTimerComplete(ITask task)
        {
            _birthTimerTask = null;
            Update(_queenGuid);
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
        private void InitBirthTimer()
        {
            if (_birthTimerTask != null) return;

            _birthTimerTask = _instantiator.Instantiate<TimerFromStatKeyTask>(new object[]{TimeType.Minutes});
            _birthTimerTask.OnComplete  += OnBirthTimerComplete;
            _birthTimerTask.Execute(_queenGuid, _betwenBornTimeStatKey);
        }

        private void OnRoomOpened(OpenRoomProtocol protocol)
        {
            Update(_queenGuid);
        }
        private void OnAnimationComplete(AnimKey animKey)
        {
            if (animKey == AnimKey.Eat)
            {
                _animator.SetAnim(_rest);
                Update(_queenGuid);
            }
        }

        private void OnGetSystemUpdate(string itemId, string guid)
        {
            if (!itemId.AnyOff(_itemFood, _itemWater)) return;
            Update(_queenGuid);
        }

     private void OnResourceChanged(string itemId, string guid)
        {
            if (_queenGuid != guid || !itemId.AnyOff(_itemFood, _itemWater)) return;
            _animator.SetAnim(AnimKey.Eat);
        }

        private void OnResourceFull(string itemId, string guid)
        {
            if (_queenGuid != guid || !itemId.AnyOff(_itemFood, _itemWater)) return;
            OnResourceChanged(itemId, guid);
            _addResourceSystem.UnRegistration(itemId, guid);
            _needControllers[itemId].UseAvailable();
            _inventory.Remove(itemId);
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
            _speakerSystem.ChangeState(_queenGuid, state);
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
            var larvaCount = units.Count(x => _unitBirthModel.LarvaModelID == x.ModelID);
            var larvaText = LocalizationManager.Localize(_larvaTextKey);

            _resourceInfo.SetInfo(larvaText + " : " + larvaCount);
        }


    }
}
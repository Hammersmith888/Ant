using BugsFarm.InfoCollectorSystem;
using BugsFarm.InventorySystem;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.StatsService;
using BugsFarm.SimulationSystem;
using BugsFarm.SpeakerSystem;
using BugsFarm.TaskSystem;
using UniRx;
using UnityEngine;
using Zenject;

namespace BugsFarm.UnitSystem
{
    public class UnitBase : ISceneEntity, IInitializable
    {
        public string Id { get; private set; }
        protected virtual string SpeakerId => GetType().Name;
        private UnitEatSystem   _unitEatSystem;
        private UnitDrinkSystem _unitDrinkSystem;
        private UnitDeathSystem _unitDeathSystem;
        private UnitSleepSystem _unitSleepSystem;
        
        protected IInstantiator Instantiator;
        protected UnitSceneObjectStorage UnitSceneObjectStorage;
        protected StatsCollectionStorage StatsCollectionStorage;
        private ISpeakerSystem _speakerSystem;
        private ISimulationSystem _simulationSystem;
        private IUnitFallSystem _unitFallSystem;
        private IActivitySystem _activitySystem;

        private const string _needTimeFoodStatKey  = "stat_timeWithoutFood";
        private const string _needTimeWaterStatKey = "stat_timeWithoutWater";
        private const string _bornTimeStatKey = "stat_bornTime";
        private const string _ammountCarryStatKey = "stat_ammountCarry";

        protected bool Finalized;
        protected StatsCollection StatsCollection;
        private UnitTaskProcessor _taskProcessor;
        private UnitSceneObject _view;
        private StateInfo _stateInfo;
        private CompositeDisposable _events;

        [Inject]
        private void Inject(string guid,
                            UnitEatSystem unitEatSystem,
                            UnitDrinkSystem unitDrinkSystem,
                            UnitDeathSystem unitDeathSystem,
                            UnitSleepSystem unitSleepSystem,
                            IInstantiator instantiator,
                            ISpeakerSystem speakerSystem,
                            IActivitySystem activitySystem,
                            ISimulationSystem simulationSystem,
                            IUnitFallSystem unitFallSystem,
                            UnitSceneObjectStorage unitSceneObjectStorage,
                            StatsCollectionStorage statsCollectionStorage)
        {
            Id = guid;
            _unitEatSystem = unitEatSystem;
            _unitDrinkSystem = unitDrinkSystem;
            _unitDeathSystem = unitDeathSystem;
            _unitSleepSystem = unitSleepSystem;
            Instantiator = instantiator;
            _speakerSystem = speakerSystem;
            _activitySystem = activitySystem;
            _simulationSystem = simulationSystem;
            _unitFallSystem = unitFallSystem;
            UnitSceneObjectStorage = unitSceneObjectStorage;
            StatsCollectionStorage = statsCollectionStorage;
            _events = new CompositeDisposable();
        }

    #region Zenject
        public virtual void Initialize()
        {
            if(Finalized) return;
            StatsCollection = StatsCollectionStorage.Get(Id);
            _view = UnitSceneObjectStorage.Get(Id);
            _stateInfo = Instantiator.Instantiate<StateInfo>(new object[] {Id});
            
            // инвентарь
            var ammountCarry = StatsCollection.GetValue(_ammountCarryStatKey);
            var invetoryProtocol = new CreateInventoryProtocol(Id, result => result.SetDefaultCapacity((int)ammountCarry));
            Instantiator.Instantiate<CreateInventoryCommand>().Execute(invetoryProtocol);

            _taskProcessor = Instantiator.Instantiate<UnitTaskProcessor>(new object[] {Id});
            _taskProcessor.Stop();
            _taskProcessor.Initialize();
            
            var activityProtocol = new ActivitySystemProtocol(Id, Play, Stop);
            _activitySystem.Registration(activityProtocol);
            MessageBroker.Default
                .Receive<EntityRepairedProtocol>()
                .Subscribe(OnRepairedUnitEventHandler)
                .AddTo(_events);
            if (_activitySystem.IsActive(Id))
            {
                _activitySystem.Activate(Id,true,true);
            }
            else
            {
                _view.SetActive(false);
            }
        }

        public virtual void Dispose()
        {
            if (Finalized)
            {
                return;
            }
            _activitySystem.UnRegistration(Id);
            Stop();
            _events?.Dispose();
            _events = null;
            _stateInfo.Dispose();
            _stateInfo = null;
            _taskProcessor.Dispose();
            StatsCollection = null;
            Instantiator.Instantiate<DeleteInventoryCommand>().Execute(new DeleteInventoryProtocol(Id));
            Finalized = true;
        }
    #endregion

    #region Activity System
        private void Play()
        {
            if(Finalized) return;
            _view.SetActive(true);
            _unitDeathSystem.OnUnitStartDie += OnUnitStartDie;
            _view.VisibleTrigger.OnVisible += OnSceneObjectVisible;
            _unitEatSystem.OnHungry    += OnUnitNeed;
            _unitDrinkSystem.OnHungry  += OnUnitNeed;
            _unitSleepSystem.OnSleepy  += OnUnitNeed;

            _speakerSystem.OnBeforeSpeak += OnBeforeSpeak;
            _stateInfo.OnUpdate += OnUpdateStateInfo;
            
            _unitFallSystem.Registration(Id);
            _unitDeathSystem.Registration(Id, _needTimeWaterStatKey, _needTimeFoodStatKey);
            _unitEatSystem.Registration(Id);
            _unitDrinkSystem.Registration(Id);
            _unitSleepSystem.Registration(Id);
            _speakerSystem.Registration(Id, SpeakerId, _view.transform);
            _speakerSystem.AllowSay(Id, _view.VisibleTrigger.Visible);
            
            _taskProcessor.OnFree += Update;

            if (_taskProcessor.IsFree)
            {
                if (!_taskProcessor.TryStartInterrupted())
                {
                    Update(null);
                }
            }
        }
        private void Stop()
        {
            if(Finalized) return;
            if (_view != null)
            {
                _view.SetActive(false);
                _view.VisibleTrigger.OnVisible -= OnSceneObjectVisible;
            }
            _unitDeathSystem.OnUnitStartDie -= OnUnitStartDie;
            _unitEatSystem.OnHungry    -= OnUnitNeed;
            _unitDrinkSystem.OnHungry  -= OnUnitNeed;
            _unitSleepSystem.OnSleepy  -= OnUnitNeed;
            
            _taskProcessor.OnFree -= Update;
            _speakerSystem.OnBeforeSpeak -= OnBeforeSpeak;
            _stateInfo.OnUpdate -= OnUpdateStateInfo;

            _unitFallSystem.UnRegistration(Id);
            _unitDeathSystem.UnRegistration(Id);
            _unitEatSystem.UnRegistration(Id);
            _unitDrinkSystem.UnRegistration(Id);
            _unitSleepSystem.UnRegistration(Id);
            _speakerSystem.UnRegistration(Id);
            
            _taskProcessor.Stop();
            _taskProcessor.Interrupt();
        }
    #endregion

    #region Need Tasks
        private void OnUnitStartDie(string guid)
        {
            if (Finalized || Id != guid)
            {
                return;
            }
            
            _speakerSystem.UnRegistration(Id);
            _unitEatSystem.UnRegistration(Id);
            _unitDrinkSystem.UnRegistration(Id);
            _unitSleepSystem.UnRegistration(Id);
            _taskProcessor.Stop();
        }
        private void OnUnitNeed(string guid)
        {
            if (Finalized || Id != guid)
            {
                return;
            }
            Update(null);
        }
        private void Update(ITask taskEnd)
        {
            if (Finalized)
            {
                return;
            }
            
            if (!_activitySystem.IsActive(Id) || 
                _unitFallSystem.IsFall(Id) || 
                _unitDeathSystem.IsRunned(Id))
            {
                _taskProcessor.Stop();
                return;
            }

            if (_unitDeathSystem.IsDeadly(Id))
            {
                _taskProcessor.Stop();
                _unitDeathSystem.Start(Id);
                return;
            }
            
            var isHungryFood = _unitEatSystem.IsHungry(Id);
            if (isHungryFood)
            {
                // Отключим активный поиск задач
                _taskProcessor.Stop();
                if (_unitEatSystem.IsConsume(Id))
                {
                    return;
                }
                
                if(_unitEatSystem.Start(Id))
                {
                    return;
                }
                // Если появился доступный ресурс надо обновить состояние.
                isHungryFood = _unitEatSystem.IsHungry(Id);
            }
            
            var isHungryWater = _unitDrinkSystem.IsHungry(Id);
            if (isHungryWater)
            {
                // Отключим активный поиск задач
                _taskProcessor.Stop();
                
                if (_unitDrinkSystem.IsConsume(Id))
                {
                    return;
                }

                if(_unitDrinkSystem.Start(Id))
                {
                    return;
                }
                // Если появился доступный ресурс надо обновить состояние.
                isHungryWater = _unitDrinkSystem.IsHungry(Id);
            }

            if (!isHungryFood && !isHungryWater)
            {
                if (_unitSleepSystem.IsSleep(Id) || 
                    _unitSleepSystem.Start(Id))
                {
                    _taskProcessor.Stop();
                    return;
                }
            }
            
            // Если юнит голоден.
            if(isHungryFood || isHungryWater)
            {
                // Отключим получение задач
                _taskProcessor.Stop();
                var restTask = Instantiator.Instantiate<HungryRest>();
                restTask.SetAction(() => Update(null));
                if (_taskProcessor.CanInterrupt(restTask))
                {
                    _taskProcessor.SetTask(restTask);
                    return;
                }
            }
            
            // Если текущей задачи нет, выполним обновления в отдыхе
            if (_taskProcessor.IsFree)
            {
                // Включаем получение сообщений о новых задачах
                _taskProcessor.Play();
                var restTask = Instantiator.Instantiate<RestTask>();
                restTask.SetAction(_taskProcessor.Update);
                _taskProcessor.SetTask(restTask);
            }
        }
    #endregion
        
    #region Other
        private void OnRepairedUnitEventHandler(EntityRepairedProtocol protocol)
        {
            if (Finalized || protocol.EntityId != Id)
            {
                return;
            }
            _unitEatSystem.SetMax(Id);
            _unitDrinkSystem.SetMax(Id);
            _unitSleepSystem.SetMax(Id);
        }
        private void OnSceneObjectVisible(bool visible)
        {
            if (Finalized || !_activitySystem.IsActive(Id) || !_speakerSystem.HasEntity(Id))
            {
                return;
            }
            _speakerSystem.AllowSay(Id, visible);
        }
        private void OnBeforeSpeak()
        {
            if (Finalized || !_activitySystem.IsActive(Id)) return;
            
            var isHungryFood  = _unitEatSystem.IsHungry(Id) && !_unitEatSystem.IsConsume(Id);
            var isHungryWater = _unitDrinkSystem.IsHungry(Id) && !_unitDrinkSystem.IsConsume(Id);
            var isSleep       = _unitSleepSystem.IsSleep(Id);
            var isDeadly      = _unitDeathSystem.IsDeadly(Id);
            
            var phraseState = isDeadly      ? PhraseState.none :
                              isHungryFood  ? PhraseState.noFood :
                              isHungryWater ? PhraseState.noWater : 
                              isSleep       ? PhraseState.none : PhraseState.idle;
            _speakerSystem.ChangeState(Id, phraseState);
        }
        private void OnUpdateStateInfo()
        {
            if (Finalized || !_activitySystem.IsActive(Id)) return;
            
            var food  = _unitEatSystem.GetInfo(Id);
            var water = _unitDrinkSystem.GetInfo(Id);
            var sleep = _unitSleepSystem.GetInfo(Id);
            var bornTime = _simulationSystem.GameAge - StatsCollection.GetValue(_bornTimeStatKey);

            var strTask  = LocalizationManager.Localize(_taskProcessor.TaskName);
            
            var strAge   = $"{LocalizationManager.Localize(Texts.Age)} : { Format.Age(bornTime)}";
            var strEat   = $"{LocalizationManager.Localize(food.HeaderKey)} : {Format.Age(food.Time, true)}";
            var strWater = $"{LocalizationManager.Localize(water.HeaderKey)} : {Format.Age(water.Time, true)}";
            var strSleep = $"{LocalizationManager.Localize(sleep.HeaderKey)} : {Format.Age(sleep.Time, true)}";
            _stateInfo.SetInfo(strTask,strAge,strEat,strWater,strSleep);
        }
    #endregion
    }
}
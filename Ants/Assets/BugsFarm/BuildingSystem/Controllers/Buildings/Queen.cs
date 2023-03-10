using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.AnimationsSystem;
using BugsFarm.BuildingSystem.DeathSystem;
using BugsFarm.BuildingSystem.States;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.InventorySystem;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StateMachine;
using BugsFarm.Services.StatsService;
using BugsFarm.UnitSystem;
using UniRx;
using Zenject;
using TickableManager = Zenject.TickableManager;

namespace BugsFarm.BuildingSystem
{
    public class Queen : ISceneEntity, IInitializable
    {
        public string Id { get; }

        private readonly IInstantiator _instantiator;
        private readonly TickableManager _tickableManager;

        private const string _lifetimeStateKey = "QueenLifetime";
        private const string _deadStateKey = "DeadBuildingState";

    #region Need keys
        
        private const string _noNeedSleepTimeStatKey = "stat_noNeedTimeSleep";
        private const string _needEatTimeStatKey   = "stat_timeWithoutFood";
        
    #endregion

        private readonly ResurrectBuildingDataStorage _resurrectBuildingDataStorage;
        private readonly BuildingDeathSystem _buildingDeathSystem;
        private readonly IStateMachine _stateMachine;
        private readonly BuildingSceneObjectStorage _buildingSceneObjectStorage;
        private CompositeDisposable _compositeDisposable;

        private StatsCollection _statsCollection;
        private StatVital _betwenBornTimeStat;
        private ResourceInfo _resourceInfo;
        private StateInfo _stateInfo;
        
        private bool _finalized;


        public Queen(string guid,
                    IInstantiator instantiator,
                     BuildingDeathSystem buildingDeathSystem,
                     IStateMachine stateMachine,
                     ResurrectBuildingDataStorage resurrectBuildingDataStorage,
                    BuildingSceneObjectStorage buildingSceneObjectStorage)
        {
            Id = guid;
            _buildingSceneObjectStorage = buildingSceneObjectStorage;
            _stateMachine = stateMachine;
            _instantiator = instantiator;
            _resurrectBuildingDataStorage = resurrectBuildingDataStorage;
            _buildingDeathSystem = buildingDeathSystem;
            _compositeDisposable = new CompositeDisposable();
        }

        public void Initialize()
        {
            var view = (BuildingSpineObject) _buildingSceneObjectStorage.Get(Id);

            var typeName = GetType().Name;
            var spine = view.MainSkeleton;

            ISpineAnimator animator = default;
            var animProtocol = new CreateAnimatorProtocol(Id, typeName, res => animator = res, spine);
            _instantiator.Instantiate<CreateAnimatorCommand<SpineAnimator>>().Execute(animProtocol);
            
            _buildingDeathSystem.Register(Id, _needEatTimeStatKey, _noNeedSleepTimeStatKey);
            MessageBroker.Default.Receive<PostDeathBuildingProtocol>().Subscribe(OnBuildingDestroyed).AddTo(_compositeDisposable);
            MessageBroker.Default.Receive<ResurrectBuildingData>().Subscribe(OnBuildingResurrected).AddTo(_compositeDisposable);
            
            QueenLifetimeState lifetimeState = _instantiator.Instantiate<QueenLifetimeState>(new object[]{Id, typeName, animator});
            _stateMachine.Add(lifetimeState);
            QueenDestroyedState deathState = _instantiator.Instantiate<QueenDestroyedState>(new object[] {animator});
            _stateMachine.Add(deathState);
            _stateMachine.Switch(_resurrectBuildingDataStorage.HasEntity(Id) ? _deadStateKey : _lifetimeStateKey);
        }

        public void Dispose()
        {
            if (_finalized) return;

            _finalized = true;
            _resourceInfo?.Dispose();
            _stateInfo?.Dispose();
            _betwenBornTimeStat = null;
            _statsCollection = null;
            _instantiator.Instantiate<RemoveAnimatorCommand>().Execute(new RemoveAnimatorProtocol(Id));
            _instantiator.Instantiate<DeleteInventoryCommand>().Execute(new DeleteInventoryProtocol(Id));
            _stateMachine.Current?.OnExit();
            _stateMachine.Clear();
            _compositeDisposable.Dispose();
        }

        private void OnBuildingDestroyed(PostDeathBuildingProtocol postDeathBuildingProtocol)
        {
            if(postDeathBuildingProtocol.Guid != Id)
                return;
            ChangeState(_deadStateKey);
        }

        private void OnBuildingResurrected(ResurrectBuildingData resurrectBuildingData)
        {
            if (resurrectBuildingData.Guid != Id)
                return;
            ChangeState(_lifetimeStateKey);
        }
        
        private void ChangeState(string state)
        {
            _stateMachine.Switch(state);
        }

   
    }
}
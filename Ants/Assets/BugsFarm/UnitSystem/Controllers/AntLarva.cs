using BugsFarm.AnimationsSystem;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.StatsService;
using BugsFarm.SimulationSystem;
using UnityEngine;
using Zenject;

namespace BugsFarm.UnitSystem
{
    public class AntLarva : ISceneEntity, IInitializable
    {
        public string Id { get; }
        private int Stage => (int) _statsCollection.GetValue(_larvaStageStatKey);

        private readonly IInstantiator _instantiator;
        private readonly IActivitySystem _activitySystem;
        private readonly ISimulationSystem _simulationSystem;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly UnitSceneObjectStorage _unitSceneObjectStorage;

        private const string _leftTimeTextKey = "BugsInner_9_LeftTime";
        private const string _growthTimeStatKey = "stat_growthTime";
        private const string _bornTimeStatKey = "stat_bornTime";
        private const string _larvaStageStatKey = "stat_stage";

        private bool _finalized;
        private StateInfo _stateInfo;
        private UnitTaskProcessor _taskProcessor;
        private CustomAnimationPlayer _customAnimationPlayer;
        private UnitSceneObject _view;
        private StatsCollection _statsCollection;
        private StatVital _growthTimeStat;

        public AntLarva(string guid,
                        IInstantiator instantiator,
                        IActivitySystem activitySystem,
                        ISimulationSystem simulationSystem,
                        StatsCollectionStorage statsCollectionStorage,
                        UnitSceneObjectStorage unitSceneObjectStorage)
        {
            _instantiator = instantiator;
            _activitySystem = activitySystem;
            _simulationSystem = simulationSystem;
            _statsCollectionStorage = statsCollectionStorage;
            _unitSceneObjectStorage = unitSceneObjectStorage;
            Id = guid;
        }

        public void Initialize()
        {
            _statsCollection = _statsCollectionStorage.Get(Id);
            _view = _unitSceneObjectStorage.Get(Id);

            _growthTimeStat = _statsCollection.Get<StatVital>(_growthTimeStatKey);

            var animModelId = GetType().Name + "_" + (Stage - 1);
            var animatorProtocol = new CreateAnimatorProtocol(Id, animModelId, _view.MainSkeleton);
            _instantiator.Instantiate<CreateAnimatorCommand<SpineAnimator>>().Execute(animatorProtocol);

            var moverProtocol = new CreateMoverProtocol(Id);
            _instantiator.Instantiate<CreateMoverCommand<UnitLarvaMover>>().Execute(moverProtocol);

            _taskProcessor = _instantiator.Instantiate<UnitTaskProcessor>(new object[] {Id});
            _taskProcessor.Initialize();
            _taskProcessor.Stop();

            _customAnimationPlayer = _instantiator.Instantiate<CustomAnimationPlayer>(new object[] {Id});
            _customAnimationPlayer.SetAnimationClips(true,
                                                     new CustomAnimationClip(AnimKey.Idle, 3),
                                                     new CustomAnimationClip(AnimKey.Idle1, 1),
                                                     new CustomAnimationClip(AnimKey.Idle2, 1),
                                                     new CustomAnimationClip(AnimKey.Idle, 2),
                                                     new CustomAnimationClip(AnimKey.Idle2, 1));

            _stateInfo = _instantiator.Instantiate<StateInfo>(new object[] {Id});

            var activityProtocol = new ActivitySystemProtocol(Id, Play, Stop);
            _activitySystem.Registration(activityProtocol);
            if (_activitySystem.IsActive(Id))
            {
                _activitySystem.Activate(Id,true,true);
            }
            else
            {
                _view.SetActive(false);
            }
            _view.SetActive(_activitySystem.IsActive(Id));
        }

        public void Dispose()
        {
            if (_finalized) return;
            _finalized = true;
            _taskProcessor.Interrupt();
            _taskProcessor.Dispose();
            _taskProcessor = null;
            _stateInfo.OnUpdate -= OnUpdateStateInfo;
            _stateInfo.Dispose();
            _stateInfo = null;
            _growthTimeStat = null;
            _statsCollection = null;

            _instantiator.Instantiate<DeleteMoverCommand>().Execute(new DeleteMoverProtocol(Id));
            _instantiator.Instantiate<RemoveAnimatorCommand>().Execute(new RemoveAnimatorProtocol(Id));
        }

        private void Play()
        {
            if (_finalized) return;
            _view.SetActive(true);
            _view.SetInteraction(true);
            _stateInfo.OnUpdate += OnUpdateStateInfo;
            InitGrowthTask();
        }

        private void Stop()
        {
            if (_finalized) return;
            _view.SetInteraction(false);
            _view.SetActive(false);
            _taskProcessor.Interrupt();
            _stateInfo.OnUpdate -= OnUpdateStateInfo;
        }

        private void InitGrowthTask()
        {
            var args = new object[] {_customAnimationPlayer};
            var task = _instantiator.Instantiate<AntLarvaGrowthTask>(args);
            _taskProcessor.SetTask(task);
        }
        
        private void OnUpdateStateInfo()
        {
            if (_finalized) return;
            var currMaxLeftTime = _growthTimeStat.Value * (Stage - 1);
            var bornTime = _simulationSystem.GameAge - _statsCollection.GetValue(_bornTimeStatKey);
            var currentStage = _statsCollection.Get<StatVital>(_larvaStageStatKey).CurrentValue;
            var leftTime = Format.MinutesToSeconds(_growthTimeStat.CurrentValue + (_growthTimeStat.Value * (Stage - currentStage + 1)));

            var strAge = LocalizationManager.Localize(Texts.Age) + " : " + Format.Age(bornTime);
            var strLeft = LocalizationManager.Localize(_leftTimeTextKey) + " : " + Format.Age(leftTime);
            _stateInfo.SetInfo(strAge, strLeft);
        }
    }
}
using System.Collections.Generic;
using System.Globalization;
using BugsFarm.AnimationsSystem;
using BugsFarm.AssetLoaderSystem;
using BugsFarm.CurrencyCollectorSystem;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.StatsService;
using BugsFarm.SimulationSystem;
using BugsFarm.TaskSystem;
using BugsFarm.UI;
using BugsFarm.UnitSystem;
using BugsFarm.UserSystem;
using UniRx;
using UnityEngine;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class Prison : ISceneEntity, IInitializable
    {
        public string Id { get; }
        private readonly IUser _user;
        private readonly SpineLoader _spineLoader;
        private readonly TaskStorage _taskStorage;
        private readonly IInstantiator _instantiator;
        private readonly UnitModelStorage _unitModelStorage;
        private readonly ISimulationSystem _simulationSystem;
        private readonly BuildingDtoStorage _buildingDtoStorage;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly ICurrencyCollectorSystem _currencyCollectorSystem;
        private readonly BuildingSceneObjectStorage _buildingSceneObjectStorage;
        private readonly PrisonSlotModelStorage _prisonSlotModelStorage;
        private readonly BuildingBuildSystem _buildingBuildSystem;
        private readonly CompositeDisposable _events;
        
        private const string _resourceStatKey = "stat_resource";
        private const string _rewardIdStatKey = "stat_rewardId";
        private const string _takeCountStatKey = "stat_takeCount";
        private const string _rewardCountStatKey = "stat_rewardCount";
        private const string _prisonerIdStatKey = "stat_prisonerId";
        private const string _prisonerTimeStatKey = "stat_prisonerTime";
        private const string _patrolPauseTimeStatKey = "stat_patrolPauseTime";
        private const string _prisonerTextKey = "BuildingInner_53_Prisoner";
        private const string _emptyPrisonTextKey = "BuildingInner_53_Empty";

        private bool _finalized;
        private BuildingDto _selfDto;
        private StatsCollection _statsCollection;
        private PrisonSceneObject _buildingSceneObject;
        private BoubbleInteractor _popupInteractor;
        private IEnumerable<IPosSide> _tasksPoints;
        private StatModifiable _prisonerIdStat;
        private StatVital _prisonerTimeStat;
        private StatVital _takeCountStat;
        private StatVital _resourceStat;
        private StateInfo _stateInfo;
        
        private ITask _partolTask;
        private ITask _prisonerTimeTask;
        private ITask _partolPauseTask;
        private ISpineAnimator _spineAnimator;
        
        public Prison(string guid, 
                      IUser user,
                      SpineLoader spineLoader, 
                      TaskStorage taskStorage, 
                      IInstantiator instantiator,
                      UnitModelStorage unitModelStorage, 
                      ISimulationSystem simulationSystem,
                      BuildingDtoStorage buildingDtoStorage, 
                      StatsCollectionStorage statsCollectionStorage,
                      ICurrencyCollectorSystem currencyCollectorSystem, 
                      BuildingSceneObjectStorage buildingSceneObjectStorage,
                      PrisonSlotModelStorage prisonSlotModelStorage, 
                      BuildingBuildSystem buildingBuildSystem)
        {
            Id = guid;
            _user = user;
            _spineLoader = spineLoader;
            _taskStorage = taskStorage;
            _instantiator = instantiator;
            _unitModelStorage = unitModelStorage;
            _simulationSystem = simulationSystem;
            _buildingDtoStorage = buildingDtoStorage;
            _statsCollectionStorage = statsCollectionStorage;
            _currencyCollectorSystem = currencyCollectorSystem;
            _buildingSceneObjectStorage = buildingSceneObjectStorage;
            _prisonSlotModelStorage = prisonSlotModelStorage;
            _buildingBuildSystem = buildingBuildSystem;
            _events = new CompositeDisposable();
        }

        public void Initialize()
        {
            if (_finalized) return;
            var view = _buildingSceneObjectStorage.Get(Id);
            _stateInfo = _instantiator.Instantiate<StateInfo>(new[] {Id});
            _stateInfo.OnUpdate += OnStateInfoUpdate;
            _buildingSceneObject = (PrisonSceneObject) _buildingSceneObjectStorage.Get(Id);
            _statsCollection = _statsCollectionStorage.Get(Id);
            _resourceStat = _statsCollection.Get<StatVital>(_resourceStatKey);
            _prisonerTimeStat = _statsCollection.Get<StatVital>(_prisonerTimeStatKey);
            _takeCountStat = _statsCollection.Get<StatVital>(_takeCountStatKey);
            _prisonerIdStat = _statsCollection.Get<StatModifiable>(_prisonerIdStatKey);
            _tasksPoints = view.GetComponent<TasksPoints>().Points;
            _selfDto = _buildingDtoStorage.Get(Id);
            _buildingBuildSystem.OnStarted += OnBuildStarted;
            _buildingBuildSystem.OnCompleted += OnBuildCompleted;
            _buildingBuildSystem.Registration(Id);
            MessageBroker.Default.Receive<DeathUnitProtocol>()
                .Subscribe(OnDeathUnitEventHandler)
                .AddTo(_events);

            MessageBroker.Default.Receive<PlaceBuildingProtocol>()
                .Subscribe(OnMoveBuildingEventHandler)
                .AddTo(_events);
            
            _simulationSystem.OnSimulationEnd += OnSimulationEndEventHandler;

            if (_buildingBuildSystem.CanBuild(Id))
            {
                _buildingBuildSystem.Start(Id);
            }
            else
            {
                InitPatrolPauseTimer();
                if (HasPrisoner())
                {
                    PrisonerProduction();
                }
                else
                {
                    SetupPlaceHolder(null);
                    CreatePopup();
                }
            }
        }

        public void Dispose()
        {
            if (_finalized)
            {
                return;
            }

            SetupPlaceHolder(null);
            _finalized = true;
            _stateInfo?.Dispose();
            _events?.Dispose();
            _events?.Clear();
            _simulationSystem.OnSimulationEnd -= OnSimulationEndEventHandler;
            _buildingBuildSystem.OnStarted -= OnBuildStarted;
            _buildingBuildSystem.OnCompleted -= OnBuildCompleted;
            _buildingBuildSystem.UnRegistration(Id);

            if (_partolTask != null && _taskStorage.HasTask(_partolTask.Guid))
            {
                _taskStorage.Remove(_partolTask.Guid);
            }
            _popupInteractor?.Dispose();
            _partolPauseTask?.Interrupt();
            _prisonerTimeTask?.Interrupt();
            _partolTask?.Interrupt();
            
            _selfDto = null;
            _stateInfo = null;
            _partolTask = null;
            _tasksPoints = null;
            _resourceStat = null;
            _partolPauseTask = null;
            _statsCollection = null;
            _popupInteractor = null;
            _prisonerTimeTask = null;
            _prisonerTimeStat = null;
            _buildingSceneObject = null;
        }

        private void PatrolProduction()
        {
            if (_finalized ||
                _partolTask != null ||
                _buildingBuildSystem.IsBuilding(Id))
            {
                return;
            }

            var args = new object[] {Id, _tasksPoints};
            _partolTask = _instantiator.Instantiate<PatrolBootstrapTask>(args);
            _partolTask.OnComplete += OnPatrolTaskEnd;
            _partolTask.OnInterrupt += OnPatrolTaskEnd;
            _taskStorage.DeclareTask(Id, _selfDto.ModelID, _partolTask.GetName(), _partolTask, false);
        }

        private void InitPatrolPauseTimer()
        {
            if (_finalized || _buildingBuildSystem.IsBuilding(Id))
            {
                return;
            }

            _partolTask?.Interrupt();
            _partolTask = null;
            _partolPauseTask?.Interrupt();
            _partolPauseTask = _instantiator.Instantiate<TimerFromStatKeyTask>(new object[]{TimeType.Minutes});
            _partolPauseTask.OnComplete += _ =>
            {
                _statsCollection.Get<StatVital>(_patrolPauseTimeStatKey).SetMax();
                _partolPauseTask = null;
                PatrolProduction();
            };
            _partolPauseTask.Execute(Id, _patrolPauseTimeStatKey);
        }

        private void PrisonerProduction()
        {
            if (_finalized ||
                !HasPrisoner() ||
                _buildingBuildSystem.IsBuilding(Id) ||
                _takeCountStat.CurrentValue == 0)
            {
                SetupPlaceHolder(null);
                return;
            }
            

            _prisonerTimeTask?.Interrupt();
            _prisonerTimeTask = _instantiator.Instantiate<TimerFromStatKeyTask>(new object[]{TimeType.Minutes});
            _prisonerTimeTask.OnComplete += _ =>
            {
                _prisonerTimeTask = null;
                _takeCountStat.CurrentValue -= 1;
                _resourceStat.CurrentValue += _statsCollection.GetValue(_rewardCountStatKey) / 
                                              _takeCountStat.Value;
                CreatePopup();
                _prisonerTimeStat.SetMax();
                if (_takeCountStat.CurrentValue == 0)
                {
                    SetupPlaceHolder(null);
                    _prisonerIdStat.ClearModifiers();
                    _takeCountStat.SetMax();
                }
                else
                {
                    PrisonerProduction();
                }
            };
            _prisonerTimeTask.Execute(Id, _prisonerTimeStatKey);
            SetupPlaceHolder(((int)_prisonerIdStat.Value).ToString());
        }

        private bool HasPrisoner()
        {
            return !_finalized && _prisonerIdStat.Value != 0;
        }

        private void SetupPlaceHolder(string modelId)
        {
            if (_finalized || _simulationSystem.Simulation)
            {
                return;
            }
            
            if (_spineAnimator != null && _spineAnimator.Id == modelId)
            {
                return;
            }
            
            if (string.IsNullOrEmpty(modelId))
            {
                _instantiator.Instantiate<RemoveAnimatorCommand>()
                    .Execute(new RemoveAnimatorProtocol(_spineAnimator?.Id));
                _spineAnimator = null;
                _buildingSceneObject.SetAnimationActive(false);
                return;
            }

            if (!_unitModelStorage.HasEntity(modelId) || 
                !_prisonSlotModelStorage.HasEntity(modelId))
            {
                _buildingSceneObject.SetAnimationActive(false);
                return;
            }
            
            var unitModel  = _unitModelStorage.Get(modelId);
            var prisonSlot = _prisonSlotModelStorage.Get(modelId);
            var dataAsset  = _spineLoader.Load(prisonSlot.AssetPath, unitModel.TypeName);
            _buildingSceneObject.SkeletonAnimation.ClearState();
            _buildingSceneObject.SetAnimationActive(dataAsset);
            _buildingSceneObject.SetSkeletonDataAsset(dataAsset);
            if (dataAsset)
            {
                _instantiator.Instantiate<CreateAnimatorCommand<SpineAnimator>>()
                    .Execute(new CreateAnimatorProtocol(modelId, unitModel.TypeName,
                                                        res => _spineAnimator = res,
                                                        _buildingSceneObject.SkeletonAnimation));
                _spineAnimator.SetAnim(AnimKey.Idle);
            }
        }

        private void CreatePopup()
        {
            if (_finalized || _buildingBuildSystem.IsBuilding(Id))
            {
                return;
            }
            
            if (_popupInteractor == null && _resourceStat.CurrentValue > 0)
            {
                var args = new [] {_buildingSceneObject.PopupPoint};
                _popupInteractor = _instantiator.Instantiate<BoubbleInteractor>(args);
                _popupInteractor.Init();
                _popupInteractor.SetCurrency("0");
                _popupInteractor.SetActionTap(OnPopupTapped);
                _popupInteractor.Update();
            }
        }

        private void OnPopupTapped()
        {
            if (_finalized)
            {
                return;
            }
            var currencyId = _statsCollection.GetValue(_rewardIdStatKey).ToString(CultureInfo.InvariantCulture);
            var currencyCount = (int)_resourceStat.CurrentValue;
            if (currencyCount > 0)
            {
                _currencyCollectorSystem.Collect(_buildingSceneObject.PopupPoint.position,
                                                 currencyId,
                                                 currencyCount,
                                                 true,
                                                 left =>
                                                 {
                                                     var collected = (currencyCount - left);
                                                     currencyCount -= collected;
                                                     _resourceStat.CurrentValue -= collected;
                                                     _user.AddCurrency(currencyId, collected);
                                                 });
            }

            _popupInteractor?.Dispose();
            _popupInteractor = null;
        }

        private void OnBuildCompleted(string buildingId)
        {
            if (_finalized || Id != buildingId)
            {
                return;
            }

            PatrolProduction();
            PrisonerProduction();
            CreatePopup();
        }

        private void OnBuildStarted(string buildingId)
        {
            if (_finalized || Id != buildingId)
            {
                return;
            }

            _partolTask?.Interrupt();
            _partolTask = null;
            _prisonerTimeTask?.Interrupt();
            _prisonerTimeTask = null;
            _popupInteractor?.Dispose();
            _popupInteractor = null;
            SetupPlaceHolder(null);
        }

        private void OnDeathUnitEventHandler(DeathUnitProtocol protocol)
        {
            if (_finalized ||
                _buildingBuildSystem.IsBuilding(Id) ||
                protocol.DeathReason != UnitSystem.DeathReason.Fighted ||
                HasPrisoner())
            {
                return;
            }

            if (_prisonSlotModelStorage.HasEntity(protocol.UnitId) &&
                int.TryParse(protocol.UnitId, out var modelId))
            {
                _prisonerTimeStat.SetMax();
                _takeCountStat.SetMax();
                _prisonerIdStat.ClearModifiers();
                _prisonerIdStat.AddModifier(new StatModBaseAdd(modelId));
                PrisonerProduction();
            }
        }

        private void OnMoveBuildingEventHandler(PlaceBuildingProtocol protocol)
        {
            if (protocol.Guid != Id)
            {
                return;
            }

            _popupInteractor?.Update();
        }
        
        private void OnSimulationEndEventHandler()
        {
            if (_finalized)
            {
                return;
            }
            
            var prisonerId = HasPrisoner() ? ((int) _prisonerIdStat.Value).ToString() : null;
            SetupPlaceHolder(prisonerId);
        }

        private void OnPatrolTaskEnd(ITask obj)
        {
            if (_finalized)
            {
                return;
            }

            if (_partolTask != null && _taskStorage.HasTask(_partolTask.Guid))
            {
                _taskStorage.Remove(_partolTask.Guid);
            }

            _partolTask?.Interrupt();
            _partolTask = null;
            if (!_buildingBuildSystem.IsBuilding(Id))
            {
                InitPatrolPauseTimer();
            }
        }
        
        private void OnStateInfoUpdate()
        {
            if (_finalized)
            {
                return;
            }

            if (_buildingBuildSystem.IsBuilding(Id))
            {
                _stateInfo.SetInfo("");
                return;
            }
            string prisoner;
            if (HasPrisoner())
            {
                var currentTime = (_prisonerTimeStat.Value * Mathf.Max(0,_takeCountStat.CurrentValue -1)) + 
                                   _prisonerTimeStat.CurrentValue;
                var prisonerTime = Format.Age(Format.MinutesToSeconds(currentTime));
                prisoner = string.Format(LocalizationManager.Localize(_prisonerTextKey), prisonerTime);
            }
            else
            {
                prisoner = LocalizationManager.Localize(_emptyPrisonTextKey);
            }

            _stateInfo.SetInfo(prisoner);
        }
    }
}
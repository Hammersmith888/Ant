using System;
using System.Globalization;
using BugsFarm.AnimationsSystem;
using BugsFarm.CurrencyCollectorSystem;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.Quest;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StatsService;
using BugsFarm.TaskSystem;
using BugsFarm.UI;
using BugsFarm.UserSystem;
using UniRx;
using UnityEngine;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class Goldmine : ISceneEntity, IInitializable
    {
        public string Id { get; }

        private readonly IUser _user;
        private readonly TaskStorage _taskStorage;
        private readonly IInstantiator _instantiator;
        private readonly IBuildingBuildSystem _buildingBuildSystem;
        private readonly ICurrencyCollectorSystem _currencyCollection;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly BuildingDtoStorage _dtoStorage;
        private readonly BuildingSceneObjectStorage _viewStorage;

        private const string _resourceStatKey = "stat_capacity";
        private const string _currencyIdStatKey = "stat_currencyID";

        private readonly CompositeDisposable _events;
        private ResourceInfo _resourceInfo;
        private ResourceBarInfo _resourceBarInfo;
        private GoldmineSceneObject _view;
        private BoubbleInteractor _popupInteractor;
        private StatsCollection _statCollection;
        private StatVital _resourceStat;
        private Transform _popupPoint;
        private ITask _miningTask;
        private bool _finalized;
        private bool _isCollecting;

        public Goldmine(string guid,
                        IUser user,
                        TaskStorage taskStorage,
                        IInstantiator instantiator,
                        IBuildingBuildSystem buildingBuildSystem,
                        ICurrencyCollectorSystem currencyCollection,
                        StatsCollectionStorage statsCollectionStorage,
                        BuildingDtoStorage dtoStorage,
                        BuildingSceneObjectStorage viewStorage)
        {
            Id = guid;
            _user = user;
            _taskStorage = taskStorage;
            _instantiator = instantiator;
            _buildingBuildSystem = buildingBuildSystem;
            _currencyCollection = currencyCollection;
            _statsCollectionStorage = statsCollectionStorage;
            _dtoStorage = dtoStorage;
            _viewStorage = viewStorage;
            _events = new CompositeDisposable();
        }

        public void Initialize()
        {
            _view = (GoldmineSceneObject) _viewStorage.Get(Id);
            _popupPoint = _view.PopupPoint;
            _statCollection = _statsCollectionStorage.Get(Id);
            _resourceStat = _statCollection.Get<StatVital>(_resourceStatKey);
            _resourceStat.OnCurrentValueChanged += OnResourceCurrentValueChanged;
            
            var typeKey = GetType().Name;
            var animatorProtocol = new CreateAnimatorProtocol(Id, typeKey, _view.MainSkeleton);
            _instantiator.Instantiate<CreateAnimatorCommand<SpineAnimator>>().Execute(animatorProtocol);

            _buildingBuildSystem.OnCompleted += OnBuildingCompleted;
            _buildingBuildSystem.OnStarted += OnBuildingStarted;
            _buildingBuildSystem.Registration(Id);

            _resourceInfo = _instantiator.Instantiate<ResourceInfo>(new object[] {Id});
            _resourceBarInfo = _instantiator.Instantiate<ResourceBarInfo>(new object[] {Id});
            _resourceInfo.OnUpdate += OnResourceInfoUpdate;
            _resourceBarInfo.OnUpdate += OnResourceBarInfoUpdate;

            MessageBroker.Default.Receive<PlaceBuildingProtocol>().Subscribe(protocol =>
            {
                if (protocol.Guid != Id)
                {
                    return;
                }
                _popupInteractor?.Update();
            }).AddTo(_events);

            if (_buildingBuildSystem.CanBuild(Id))
            {
                _buildingBuildSystem.Start(Id);
            }
            else
            {
                CreatePopup();
                Production();
            }
        }

        public void Dispose()
        {
            if(_finalized) return;
            _events?.Dispose();
            _events?.Clear();
            _popupInteractor?.Dispose();
            _miningTask?.Interrupt();
            _popupInteractor = null;
            _miningTask = null;

            var animatorProtocol = new RemoveAnimatorProtocol(Id);
            _instantiator.Instantiate<RemoveAnimatorCommand>().Execute(animatorProtocol);

            _resourceInfo.OnUpdate -= OnResourceInfoUpdate;
            _resourceBarInfo.OnUpdate -= OnResourceBarInfoUpdate;
            _resourceInfo.Dispose();
            _resourceBarInfo.Dispose();
            _resourceInfo = null;
            _resourceBarInfo = null;
            
            _resourceStat.OnCurrentValueChanged -= OnResourceCurrentValueChanged;
            _resourceStat = null;
            _statCollection = null;
            
            _buildingBuildSystem.OnCompleted -= OnBuildingCompleted;
            _buildingBuildSystem.OnStarted -= OnBuildingStarted;

            _buildingBuildSystem.UnRegistration(Id);
            _finalized = true;
        }

        private void Production()
        {
            if (_finalized) return;
            if(_miningTask != null) return; 
            if (!CanMining()) return;
            if(_buildingBuildSystem.IsBuilding(Id)) return;
            
            if (_view == null)
                return;
            var points = _view.GetComponent<TasksPoints>();
            var taskPoints = points.Points;
            var dto = _dtoStorage.Get(Id);
            _miningTask = _instantiator.Instantiate<GoldmineBootstrapTask>(new object[] {Id, taskPoints});
            _miningTask.OnInterrupt += OnMiningEnd;
            _miningTask.OnComplete  += OnMiningEnd;
            _taskStorage.DeclareTask(Id, dto.ModelID, _miningTask.GetName(), _miningTask);
        }

        private bool CanMining()
        {
            return _resourceStat.CurrentValue < _resourceStat.Value;
        }

        private void CreatePopup()
        {
            if(_finalized) return;
            if (_popupInteractor == null && _resourceStat.CurrentValue >= 1)
            {
                _popupInteractor = _instantiator.Instantiate<BoubbleInteractor>(new object[] {_popupPoint});
                _popupInteractor.Init();
                _popupInteractor.SetActionTap(OnPopupTapped);
                _popupInteractor.SetCurrency(_statCollection.GetValue(_currencyIdStatKey).ToString(CultureInfo.InvariantCulture));
                _popupInteractor.SetCoins(_resourceStat);
                _popupInteractor.Update();
            }
        }

        private void OnMiningEnd(ITask task)
        {
            if(_finalized || _buildingBuildSystem.IsBuilding(Id)) return;
            _miningTask = null;
            Production();
        }

        private void OnResourceCurrentValueChanged(object sender, EventArgs e)
        {
            if(_finalized || _isCollecting) return;
            if (_resourceStat.CurrentValue <= 20 && _resourceStat.CurrentValue > 0)
            {
                return;
            }
            if (_resourceStat.CurrentValue <= 0 && _popupInteractor != null)
            {
                _popupInteractor.Dispose();
                _popupInteractor = null;
                return;
            }
            CreatePopup();
        }

        private void OnPopupTapped()
        {
            if(_finalized) return;
            _isCollecting = true;
            var currencyId = _statCollection.GetValue(_currencyIdStatKey).ToString(CultureInfo.InvariantCulture);
            var count = (int) _resourceStat.CurrentValue;
            _currencyCollection.Collect(_popupPoint.position,
                                        currencyId,
                                        count,
                                        true,
                                        left =>
                                        {
                                            var collected = _resourceStat.CurrentValue - left;
                                            _resourceStat.CurrentValue = left;
                                            _user.AddCurrency(currencyId, (int)collected);
                                        }, 
                                        ClearResources);
            MessageBroker.Default.Publish(new QuestUpdateProtocol()
            {
                QuestType = QuestType.CollectResource,
                ReferenceID = _dtoStorage.Get(Id).ModelID,
                Value = count
            });
            _popupInteractor?.Dispose();
            _popupInteractor = null;
        }

        private void ClearResources()
        {
            _resourceStat.CurrentValue = 0;
            _isCollecting = false;
            Production();
        }
        private void OnBuildingStarted(string guid)
        {
            if (guid != Id || _finalized)  return;

            if (_miningTask != null && _taskStorage.HasTask(_miningTask.Guid))
            {
                _taskStorage.Remove(_miningTask.Guid);
                _miningTask.Interrupt();
                _miningTask = null;
            }
            else if (_miningTask != null)
            {
                _miningTask.Interrupt();
                _miningTask = null;
            }
            _popupInteractor?.Dispose();
            _popupInteractor = null;
        }

        private void OnBuildingCompleted(string guid)
        {
            if (guid != Id || _finalized)  return;
            CreatePopup();
            Production();
        }

        private void OnResourceBarInfoUpdate()
        {
            if(_finalized) return;
            _resourceBarInfo.SetInfo(_resourceStat.CurrentValue / _resourceStat.Value,
                                     _statCollection.GetValue(_currencyIdStatKey).ToString(CultureInfo.InvariantCulture));
        }

        private void OnResourceInfoUpdate()
        {
            if(_finalized) return;
            _resourceInfo.SetInfo(((int) _resourceStat.CurrentValue).ToString());
        }
    }
}
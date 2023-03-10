using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BugsFarm.AssetLoaderSystem;
using BugsFarm.CurrencySystem;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.Quest;
using BugsFarm.Services.InputService;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.StatsService;
using BugsFarm.Services.UIService;
using BugsFarm.UI;
using BugsFarm.UnitSystem;
using UniRx;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace BugsFarm.BuildingSystem
{
    public class OrderBoardInteractionCommand : InteractionBaseCommand
    {
        private readonly IInstantiator _instantiator;
        private readonly IInputController<MainLayer> _inputController;
        private readonly IActivitySystem _activitySystem;
        private readonly IconLoader _iconLoader;
        private readonly BuildingModelStorage _buildingModelsStorage;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly GetResourceSystem _getResourceSystem;
        private readonly CurrencySettingStorage _currencySettingStorage;
        private readonly UnitModelStorage _unitModelStorage;
        private readonly UnitDtoStorage _unitDtoStorage;
        private readonly OrderDtoStorage _orderDtoStorage;
        private readonly OrderModelsStorage _orderModelsStorage;
        private readonly IUIService _uiService;
        
        private readonly Dictionary<string, Dictionary<string, OrderItem>> _orders;
        private readonly Dictionary<string, OrderRewardItem> _rewards;
        private readonly Dictionary<string, OrderTimerItem> _orderTimers;
        private readonly Dictionary<string, OrderNextItem> _nextTimers;
        private const string _orderNextTimerTextKey = "UIOrderBoard_NextTimer";
        private const string _dialogTextKey = "UIOrderBoard_Dealer";
        private const string _dialogIdStatKey = "stat_dialogId";
        
        private IDisposable _upgradeUnitEvent;
        private StatsCollection _statsCollection;
        private string _buildingId;
        private bool _initialized;
        
        public OrderBoardInteractionCommand(IUIService uiService,
                                            IInstantiator instantiator,
                                            IInputController<MainLayer> inputController,
                                            IActivitySystem activitySystem,
                                            CurrencySettingStorage currencySettingStorage,
                                            BuildingModelStorage buildingModelsStorage,
                                            StatsCollectionStorage statsCollectionStorage,
                                            GetResourceSystem getResourceSystem,
                                            UnitModelStorage unitModelStorage,
                                            UnitDtoStorage unitDtoStorage,
                                            OrderDtoStorage orderDtoStorage,
                                            OrderModelsStorage orderModelsStorage,
                                            IconLoader iconLoader)
        {
            _instantiator = instantiator;
            _inputController = inputController;
            _activitySystem = activitySystem;
            _iconLoader = iconLoader;
            _buildingModelsStorage = buildingModelsStorage;
            _statsCollectionStorage = statsCollectionStorage;
            _getResourceSystem = getResourceSystem;
            _currencySettingStorage = currencySettingStorage;
            _unitModelStorage = unitModelStorage;
            _unitDtoStorage = unitDtoStorage;
            _orderDtoStorage = orderDtoStorage;
            _orderModelsStorage = orderModelsStorage;
            _orders = new Dictionary<string, Dictionary<string, OrderItem>>();
            _rewards = new Dictionary<string, OrderRewardItem>();
            _orderTimers = new Dictionary<string, OrderTimerItem>();
            _nextTimers = new Dictionary<string, OrderNextItem>();
            _uiService = uiService;
        }
        
        public override Task Execute(InteractionProtocol protocol)
        {
            if (_initialized) return Task.CompletedTask;
            
            _buildingId = protocol.Guid;
            _initialized = true;
            _inputController.Lock();

            var window = _uiService.Show<UIOrderBoard>();
            window.OnInfoClicked += OnWindowInfoClicked;
            window.OnCloseClicked += (sender, args) => Dispose();
            _statsCollection = _statsCollectionStorage.Get(_buildingId);
            
            _getResourceSystem.OnSystemUpdate += OnResourceSystemUpdate;
            _getResourceSystem.OnResourceChanged += OnResourceSystemUpdate;
            _getResourceSystem.OnResourceDepleted += OnResourceSystemUpdate;
            _unitDtoStorage.OnStorageChanged += OnUnitsUpdate;
            _activitySystem.OnStateChanged += OnUnitsUpdate;
            _upgradeUnitEvent = MessageBroker.Default.Receive<UpgradeUnitProtocol>()
                .Subscribe(upgradeProtocol => OnUnitsUpdate(upgradeProtocol.UnitId));
            foreach (var orderDto in _orderDtoStorage.Get())
            {
                orderDto.OnStageChanged += UpdateOrder;
                UpdateOrder(orderDto.OrderID);
            }
            
            return Task.CompletedTask;
        }
        
        private void Dispose()
        {
            if (!_initialized) return;
            _inputController.UnLock();
            _getResourceSystem.OnSystemUpdate -= OnResourceSystemUpdate;
            _getResourceSystem.OnResourceChanged -= OnResourceSystemUpdate;
            _getResourceSystem.OnResourceDepleted -= OnResourceSystemUpdate;
            _unitDtoStorage.OnStorageChanged -= OnUnitsUpdate;
            _activitySystem.OnStateChanged -= OnUnitsUpdate;
            _upgradeUnitEvent?.Dispose();
            var window = _uiService.Get<UIOrderBoard>();
            window.HidedEvent += CloseWinEvent;
            _uiService.Hide<UIOrderBoard>();
            
        }

        private void CloseWinEvent()
        {
            var tempoOrders = _orderDtoStorage.Get().ToArray();
            foreach (var orderDto in tempoOrders)
            {
                RemoveOrderItems(orderDto.OrderID);
                RemoveOrderTimer(orderDto.OrderID);
                RemoveReward(orderDto.OrderID);
                RemoveNextOrderTimer(orderDto.OrderID);
                orderDto.OnStageChanged -= UpdateOrder;
            }

            _statsCollection = null;
            _upgradeUnitEvent?.Dispose();
            _upgradeUnitEvent = null;
            _initialized = false;
            _orders.Clear();
            _rewards.Clear();
            _orderTimers.Clear();
            _nextTimers.Clear();
        
    }

        private void UpdateOrder(string orderId)
        {
            if (!_initialized)
            {
                return;
            }

            if (!_orderDtoStorage.HasEntity(orderId))
            {
                RemoveOrderItems(orderId);
                RemoveOrderTimer(orderId);
                RemoveReward(orderId);
                RemoveNextOrderTimer(orderId);
                return;
            }

            var orderDto = _orderDtoStorage.Get(orderId);
            switch (orderDto.Stage)
            {
                case OrderStage.Process:
                    RemoveNextOrderTimer(orderId);
                    CreateOrderTimer(orderId);
                    CreateOrderItems(orderId);
                    UpdateOrderTimer(orderId);
                    break;
            
                case OrderStage.Reward:
                    RemoveOrderItems(orderId);
                    RemoveOrderTimer(orderId);
                    RemoveNextOrderTimer(orderId);
                    CreateReward(orderId);
                    break;
            
                case OrderStage.Rewarding:
                    if (_rewards.ContainsKey(orderId))
                    {
                        _rewards[orderId].SetInteractableGetButton(false);
                    }
            
                    break;
            
                case OrderStage.WaitNextOrder:
                    RemoveOrderItems(orderId);
                    RemoveOrderTimer(orderId);
                    RemoveReward(orderId);
                    CreateNextOrderTimer(orderId);
                    break;
                
                case OrderStage.Finalized:
                    RemoveOrderItems(orderId);
                    RemoveOrderTimer(orderId);
                    RemoveReward(orderId);
                    RemoveNextOrderTimer(orderId);
                    break;
            }

            UpdateDealerDialog(orderId);
        }
        
        private void UpdateDealerDialog(string orderId)
        {
            var window = _uiService.Get<UIOrderBoard>();

            if ((_orders.Count == 0 || _statsCollection.GetValue(_dialogIdStatKey) == 0) && !_orderModelsStorage.Get(orderId).IsSpecial)
            {
                _statsCollection.RemoveAllModifiers(_dialogIdStatKey);
               // _statsCollection.AddModifier(_dialogIdStatKey, new StatModBaseAdd(Random.Range(1, 5)));// localization range
                _statsCollection.AddModifier(_dialogIdStatKey, new StatModBaseAdd(int.Parse(orderId) + 1));// localization range
            }
            var key = _dialogTextKey + (_orders.Count > 0 ? (int)_statsCollection.GetValue(_dialogIdStatKey) : 0);
            var text = LocalizationManager.Localize(key);
            window.SetDealerDialogText(text);
        }

        private void UpdateOrderItem(string itemId, string orderId)
        {
            if (!_orderDtoStorage.HasEntity(orderId) || 
                !_orders.ContainsKey(orderId) ||
                !_orders[orderId].ContainsKey(itemId) ||
                !_orderTimers.ContainsKey(orderId))
            {
                return;
            }
            
            var orderItemView = _orders[orderId][itemId];
            var orderDto = _orderDtoStorage.Get(orderId);
            var itemDto = orderDto.Items[itemId];
            var orderTimer = _orderTimers[orderId];
            const string resItemId = "0";
            switch (itemDto.Stage)
            {
                case OrderItemStage.Idle:
                    orderItemView.ActiveCompleteText(false);
                    orderItemView.ActiveResourceInfo(true);
                    orderItemView.ActiveCollectButton(true);
                    int currCount;
                    var maxCount = itemDto.Params["count"].BaseValue;
                    if (itemDto.IsUnit)
                    {
                        currCount = _unitDtoStorage.Get()
                            .Where(x => x.ModelID == itemDto.ModelID && _activitySystem.IsActive(x.Guid))
                            .Count(dto => itemDto.Params.Values.All(param => ValidateStatByParam(param, dto.Guid)));
                    }
                    else
                    {
                        currCount = _getResourceSystem.Count(resItemId, itemDto.ModelID);
                    }

                    var canCollect = currCount >= maxCount;
                    var color = canCollect ? "green" : "red"; 
                    orderItemView.SetInteractableCollect(canCollect);
                    orderItemView.SetResourceInfo(Format.ResourceColored(currCount, color, maxCount, "", true));
                    orderTimer.ActivePhantomButton(true);
                    break;
                case OrderItemStage.Process:
                    orderItemView.ActiveCompleteText(false);
                    orderItemView.ActiveResourceInfo(false);
                    orderItemView.ActiveCollectButton(false);
                    orderTimer.ActivePhantomButton(false);
                    break;
                
                case OrderItemStage.Compelte:
                    orderItemView.ActiveCompleteText(true);
                    orderItemView.ActiveResourceInfo(false);
                    orderItemView.ActiveCollectButton(false);
                    orderTimer.ActivePhantomButton(false);
                    break;
            }
        }
        
        private void UpdateOrderTimer(string orderId)
        {
            if (!_orderTimers.ContainsKey(orderId) || !_orderDtoStorage.HasEntity(orderId))
            {
                return;
            }
            
            var orderDto = _orderDtoStorage.Get(orderId);
            var orderTimer = _orderTimers[orderId];
            var orderModel = _orderModelsStorage.Get(orderId);
            if (orderModel.IsSpecial)
            {
                orderTimer.ActivePhantomButton(orderDto.Items.Values.First().Stage == OrderItemStage.Idle);
            }
            else
            {
                orderTimer.ActivePhantomButton(false);
            }
        }

        private Sprite GetOrderItemIcon(string modelId, bool isUnit)
        {
            if (isUnit)
            {
                var unitModel = _unitModelStorage.Get(modelId);
                return _iconLoader.Load(unitModel.TypeName);
            }

            var buildingModel = _buildingModelsStorage.Get(modelId);
            return _iconLoader.LoadOrDefault(buildingModel.TypeName, buildingModel.TypeID);
        }
        
        private bool ValidateStatByParam(OrderItemParam param, string entityId)
        {
            var statsCollection = _statsCollectionStorage.Get(entityId);
            if (!statsCollection.HasEntity("stat_" + param.Key))
            {
                Debug.LogError($"ValidateStatByParam : stat_{param.Key} not found ");
                return true;
            }
            var value = statsCollection.GetValue("stat_" + param.Key);
            return value >= param.CurrentValue && value <= param.BaseValue;
        }

    #region Create

        private void CreateOrderItems(string orderId)
        {
            var window = _uiService.Get<UIOrderBoard>();
            
            if (_orders.ContainsKey(orderId))
            {
                return;
            }
            
            _orders.Add(orderId, new Dictionary<string, OrderItem>());
            var orderItems = _orders[orderId];
            var orderModel = _orderModelsStorage.Get(orderId);
            var orderDto = _orderDtoStorage.Get(orderId);
            var itemsContainer = orderModel.IsSpecial ? window.SpecialOrderContainer : window.BaseOrderItemsContainer;
            var itemPrefab = orderModel.IsSpecial ? window.SpecialOrderItemPrefab : window.BaseOrderItemPrefab;
            foreach (var itemDto in orderDto.Items.Values)
            {
                var itemView = _instantiator.InstantiatePrefabForComponent<OrderItem>(itemPrefab, itemsContainer);
                var itemID = itemDto.ItemID;
                
                itemView.Initilize(itemID); // TODO : listen action on collect clicked
                itemView.ActiveDescription(orderModel.IsSpecial);
                itemView.ActiveLevelImage(itemDto.IsUnit);
                itemView.SetIcon(GetOrderItemIcon(itemDto.ModelID, itemDto.IsUnit));
                itemView.SetDescription(LocalizationManager.HasKey(itemDto.LocalizationID) ? LocalizationManager.Localize(itemDto.LocalizationID) : "");
                itemView.SetLevelText(itemDto.IsUnit? ((int)itemDto.Params["level"].BaseValue).ToString() : "");
                itemView.OnCollectClick += _ => OnItemCollect(itemID, orderId);
                    
                orderItems.Add(itemID, itemView);
                UpdateOrderItem(itemID, orderId);
                itemDto.OnStageChanged += _ => UpdateOrderItem(itemID, orderId);
            }
        }

        private void CreateOrderTimer(string orderId)
        {
            var window = _uiService.Get<UIOrderBoard>();

            if (_orderTimers.ContainsKey(orderId))
            {
                return;
            }

            var orderModel = _orderModelsStorage.Get(orderId);
            var orderDto = _orderDtoStorage.Get(orderId);
            var orderTimerPrefab = orderModel.IsSpecial ? window.SpecialOrderTimerPrefab : window.BaseOrderTimerPrefab;
            var viewContainer = orderModel.IsSpecial ? window.SpecialOrderContainer : window.BaseOrderContainer;
            var orderTimerView = _instantiator.InstantiatePrefabForComponent<OrderTimerItem>(orderTimerPrefab, viewContainer);
            orderTimerView.ActiveTimer(true);
            _orderTimers.Add(orderId, orderTimerView);
            orderDto.LifeTime.OnCurrentValueChanged += OnOrderLifeTimeUpdate;
        }

        private void CreateReward(string orderId)
        {
            var window = _uiService.Get<UIOrderBoard>();

            if (_rewards.ContainsKey(orderId))
            {
                return;
            }

            var orderModel = _orderModelsStorage.Get(orderId);
            var orderDto = _orderDtoStorage.Get(orderId);
            var viewContainer = orderModel.IsSpecial ? window.SpecialOrderContainer : window.BaseOrderContainer;
            var rewardPrefab = window.RewardItemPrefab;
            var rewardView = _instantiator.InstantiatePrefabForComponent<OrderRewardItem>(rewardPrefab, viewContainer);
            var currencyPrefab = rewardView.CurrencyItemPrefab;
            
            foreach (var param in orderDto.Rewards)
            {
                var settingModel = _currencySettingStorage.Get(param.Key);
                var currencyItem = _instantiator.InstantiatePrefabForComponent<OrderCurrencyItem>(currencyPrefab, rewardView.CurrencyContainer);
                currencyItem.OrderID = orderId;
                currencyItem.SetID(param.Key);
                currencyItem.SetSprite(_iconLoader.LoadCurrency(param.Key));
                currencyItem.SetActiveImage(true);
                currencyItem.SetText(((int)param.CurrentValue).ToString());
                currencyItem.SetTextColor(settingModel.ConvertedColor());
            }

            rewardView.SetInteractableGetButton(orderDto.Stage == OrderStage.Reward);
            rewardView.Initialize(orderId);
            rewardView.OnGetRewardClick += OnGetRerwardClick;
            _rewards.Add(orderId, rewardView);
        }

        private void CreateNextOrderTimer(string orderId)
        {
            var window = _uiService.Get<UIOrderBoard>();

            if (_nextTimers.ContainsKey(orderId))
            {
                return;
            }

            var orderModel = _orderModelsStorage.Get(orderId);
            var orderDto = _orderDtoStorage.Get(orderId);
            var timerPrefab = window.OrderNextItemPrefab;
            var viewContainer = orderModel.IsSpecial ? window.SpecialOrderContainer : window.BaseOrderContainer;
            var orderTimerView = _instantiator.InstantiatePrefabForComponent<OrderNextItem>(timerPrefab, viewContainer);
            orderTimerView.ActiveTimer(true);
            orderTimerView.ActiveDescriptionText(true);
            orderTimerView.SetDescriptionText(LocalizationManager.Localize(_orderNextTimerTextKey));
            orderDto.NextOrderTime.OnCurrentValueChanged += OnNextOrderTimeUpdate;
            _nextTimers.Add(orderId, orderTimerView);
        }

    #endregion

    #region Remove

        private void RemoveOrderItems(string orderId)
        {
            if (!_orders.ContainsKey(orderId))
            {
                return;
            }

            if (!_orders[orderId].Any())
            {
                _orders.Remove(orderId);
                return;
            }

            var tempoItems = _orders[orderId].Values.ToArray();
            _orders.Remove(orderId);
            foreach (var orderItem in tempoItems)
            {
                Object.Destroy(orderItem.gameObject);
            }
        }

        private void RemoveOrderTimer(string orderId)
        {
            if (!_orderTimers.ContainsKey(orderId))
            {
                return;
            }

            if (_orderDtoStorage.HasEntity(orderId))
            {
                var orderDto = _orderDtoStorage.Get(orderId);
                orderDto.LifeTime.OnCurrentValueChanged -= OnOrderLifeTimeUpdate;
            }

            var orderTimer = _orderTimers[orderId];
            _orderTimers.Remove(orderId);
            orderTimer.ActivePhantomButton(false);
            orderTimer.ActiveTimer(false);
            Object.Destroy(orderTimer.gameObject);
        }

        private void RemoveReward(string orderId)
        {
            if (!_rewards.ContainsKey(orderId))
            {
                return;
            }

            var rewadView = _rewards[orderId];
            _rewards.Remove(orderId);
            rewadView.OnGetRewardClick -= OnGetRerwardClick;
            rewadView.Dispose();
            Object.Destroy(rewadView.gameObject);
        }

        private void RemoveNextOrderTimer(string orderId)
        {
            if (!_nextTimers.ContainsKey(orderId))
            {
                return;
            }

            var orderTimer = _nextTimers[orderId];
            _nextTimers.Remove(orderId);
            if (_orderDtoStorage.HasEntity(orderId))
            {
                var orderDto = _orderDtoStorage.Get(orderId);
                orderDto.NextOrderTime.OnCurrentValueChanged -= OnNextOrderTimeUpdate;
            }

            Object.Destroy(orderTimer.gameObject);
        }

    #endregion

    #region Handlers
        
        private void OnUnitsUpdate(string entityId)
        {
            if(!_initialized || !_orders.Any()) return;
            foreach (var orderDto in _orderDtoStorage.Get())
            {
                foreach (var itemDto in orderDto.Items.Values.Where(itemDto => itemDto.IsUnit))
                {
                    UpdateOrderItem(itemDto.ItemID, orderDto.OrderID);
                }
            }
        }
        
        private void OnResourceSystemUpdate(string itemId, string ownerId)
        {
            if(!_initialized || itemId != "0") return;
            
            foreach (var orderDto in _orderDtoStorage.Get())
            {
                foreach (var itemDto in orderDto.Items.Values.Where(itemDto => !itemDto.IsUnit))
                {
                    UpdateOrderItem(itemDto.ItemID, orderDto.OrderID);
                }
            }
        }
        
        private void OnItemCollect(string itemId, string orderId)
        {
            if (!_orderDtoStorage.HasEntity(orderId))
            {
                return;
            }
            MessageBroker.Default.Publish(new AntHillTaskActionCompletedProtocol()
            {
                ModelID = orderId,
                TaskType = AntHillTaskType.Order,
                EntityType = AntHillTaskReferenceGroup.Building
            });
            MessageBroker.Default.Publish(new QuestUpdateProtocol()
            {
                QuestType = QuestType.CompleteOrder,
                ReferenceID = QuestType.Any,
                Value = 1
            });
            var orderDto = _orderDtoStorage.Get(orderId);
            orderDto.Items[itemId].Stage = OrderItemStage.Process;
        }

        private void OnOrderLifeTimeUpdate(OrderItemParam param)
        {
            if (!_orderTimers.ContainsKey(param.Id))
            {
                return;
            }
            
            var orderTimer = _orderTimers[param.Id];
            orderTimer.SetTimerText(Format.Time(TimeSpan.FromMinutes(param.CurrentValue)));
        }

        private void OnNextOrderTimeUpdate(OrderItemParam param)
        {
            if (!_nextTimers.ContainsKey(param.Id))
            {
                return;
            }

            var timer = _orderDtoStorage.Get(param.Id).NextOrderTime;
            var format = Format.Age(Format.MinutesToSeconds(timer.CurrentValue));
            _nextTimers[param.Id].SetTimerText(format);
        }

        private void OnGetRerwardClick(string orderId)
        {
            if (!_orderDtoStorage.HasEntity(orderId))
            {
                return;
            }

            var orderdto = _orderDtoStorage.Get(orderId);
            orderdto.Stage = OrderStage.Rewarding;
        }

        private void OnWindowInfoClicked(object sender, EventArgs e)
        {
            if (!_initialized) return;
            Dispose();
            _instantiator.Instantiate<InteractionCommand>()
                .Execute(new InteractionProtocol(_buildingId, SceneObjectType.Building));
        }

    #endregion
    }
}
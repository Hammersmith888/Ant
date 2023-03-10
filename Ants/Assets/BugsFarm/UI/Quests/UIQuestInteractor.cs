using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BugsFarm.AnimationsSystem;
using BugsFarm.AssetLoaderSystem;
using BugsFarm.CurrencyCollectorSystem;
using BugsFarm.FarmCameraSystem;
using BugsFarm.Quest;
using BugsFarm.ReloadSystem;
using BugsFarm.Services.InputService;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.UIService;
using BugsFarm.SimulationSystem;
using BugsFarm.UserSystem;
using Microsoft.SqlServer.Server;
using UniRx;
using UnityEngine;
using Zenject;
using Format = BugsFarm.InfoCollectorSystem.Format;

namespace BugsFarm.UI
{
    public class UIQuestInteractor : ITickable
    {
        private readonly IUser _user;
        private readonly IUIService _uiService;
        private readonly IconLoader _iconLoader;
        private readonly IInstantiator _instantiator;
        private readonly QuestSystemController _questSystemController;
        private readonly IInputController<MainLayer> _inputController;
        private readonly QuestElementDtoStorage _questElementDtoStorage;
        private readonly ICurrencyCollectorSystem _currencyCollectorSystem;
        private readonly QuestElementModelStorage _questElementModelStorage;
        private readonly FarmCameraSceneObject _farmCameraSceneObject;
        private readonly QuestGroupDtoStorage _questGroupDtoStorage;
        private readonly ITickableManager _tickableManager;

        private List<UIQuestItem> _itemSlots;
        private List<QuestGiftPointInteractor> _giftInteractors;
        private IDisposable _reloadEvent;
        private int _completedQuests;
        private int _totalQuestCounts;
        private UIQuestsWindow _window;
        private QuestGroupDto _questGroupDto;

        private const string _goldCurrencyID = "0";
        private const string _questReadyTextKey = "UIOrderBoard_OrderItemComplete";
        
        public UIQuestInteractor(IUIService uiService,
                                 QuestSystemController questSystemController,
                                 QuestElementDtoStorage questElementDtoStorage,
                                 QuestElementModelStorage questElementModelStorage,
                                 QuestGroupDtoStorage questGroupDtoStorage,
                                 IInstantiator instantiator,
                                 IInputController<MainLayer> inputController,
                                 ICurrencyCollectorSystem currencyCollectorSystem,
                                 FarmCameraSceneObject farmCameraSceneObject,
                                 ITickableManager tickableManager,
                                 IUser user,
                                 IconLoader iconLoader)
        {
            _inputController = inputController;
            _iconLoader = iconLoader;
            _questGroupDtoStorage = questGroupDtoStorage;
            _farmCameraSceneObject = farmCameraSceneObject;
            _user = user;
            _instantiator = instantiator;
            _currencyCollectorSystem = currencyCollectorSystem;
            _uiService = uiService;
            _questElementDtoStorage = questElementDtoStorage;
            _questElementModelStorage = questElementModelStorage;
            _tickableManager = tickableManager;
            _questSystemController = questSystemController;
            _itemSlots = new List<UIQuestItem>();
            _giftInteractors = new List<QuestGiftPointInteractor>();
        }

        public void Initialize()
        {
            _questSystemController.Initialize();
            _uiService.Show<UIQuestsButton>().ClickEvent += OpenQuestWindow;
            _reloadEvent = MessageBroker.Default.Receive<GameReloadingReport>().Subscribe(OnGameReloading);


            var currentDay = _questSystemController.CurrentDay.ToString();
            if (!_questGroupDtoStorage.HasEntity(currentDay))
                return;
            
            QuestGroupDto groupDto = _questGroupDtoStorage.Get(currentDay);
            var questWindow = _uiService.Get<UIQuestsWindow>();

            for (int i = 0; i < groupDto.VirtualChests.Count; i++)
            {
                var giftInteractor = _instantiator.Instantiate<QuestGiftPointInteractor>(new object[]{questWindow.GiftPoints[i]});
                giftInteractor.Initialize();
                giftInteractor.OnCollectRewardButtonClicked += OnCollectRewardClicked;
                _giftInteractors.Add(giftInteractor);
            }

        }

        private void OnCollectRewardClicked(VirtualChestDto virtualChestDto)
        {
            Debug.Log("On collect reward clicked");
            _instantiator.Instantiate<VirtualChestInteractor>().Execute(virtualChestDto);
            OpenButtonWindow(null, null);
            
            if (CheckAllCompleted())
            {
                
            }
        }

        private bool CheckAllCompleted()
        {
            Debug.Log("All is completed");
            return true;
        }

        private void OnGameReloading(GameReloadingReport reloadingReport)
        {
            foreach (var giftInteractor in _giftInteractors)
            {
                giftInteractor.OnCollectRewardButtonClicked -= OnCollectRewardClicked;
                giftInteractor.Dispose();
            }
            
            _questSystemController.Dispose();
            _reloadEvent.Dispose();   
        }
        private void OpenButtonWindow(object sender, EventArgs e)
        {
            _window.HidedEvent += OnWindowClosed;
            _uiService.Hide<UIQuestsWindow>();
            _uiService.Show<UIQuestsButton>().ClickEvent += OpenQuestWindow;
            _inputController.UnLock();
            _questSystemController.OnQuestsRefreshed -= RefreshQuests;
            _questSystemController.QuestExpiredEvent -= OnQuestExpired;
            _tickableManager.Remove(this);
        }

        private void OnWindowClosed()
        {
            RemoveSlots();
        }

        private void OpenQuestWindow(object sender, EventArgs e)
        {
            _uiService.Hide<UIQuestsButton>();
            _window = _uiService.Show<UIQuestsWindow>();
            _window.CloseEvent += OpenButtonWindow;
            _inputController.Lock();
            _questSystemController.OnQuestsRefreshed += RefreshQuests;
            _questSystemController.QuestExpiredEvent += OnQuestExpired;
            CreateSlots();
            UpdateGiftPoints();
            _tickableManager.Add(this);
        }

        private void OnQuestExpired(string questGuid)
        {
            var slot = _itemSlots.Find(x => x.Guid == questGuid);
            var guid = slot.Guid;
            var dto = _questElementDtoStorage.Get(guid);
            dto.IsStashed = true;
            slot.CompleteQuest(() =>
            {
                DestroySlot(slot);
            });
            Debug.Log($"Quest is expired: {questGuid}");
        }


        private void UpdateGiftPoints()
        {
            float progress = (float) _completedQuests / _totalQuestCounts;
            var currentDay = _questSystemController.CurrentDay.ToString();
            
            if(!_questGroupDtoStorage.HasEntity(currentDay))
                return;
            
            _questGroupDto = _questGroupDtoStorage.Get(currentDay);
            
            for (int i = 0; i < _giftInteractors.Count; i++)
            {
                _giftInteractors[i].SetVirtualChestDto(_questGroupDto.VirtualChests[i], progress);
            }
        }

        private void RefreshQuests()
        {
            RemoveSlots();
            CreateSlots();
        }

        private void CreateSlots()
        {
            _totalQuestCounts = 0;
            _completedQuests = 0;
            
            foreach (var questsGroup in _questSystemController.DailyQuests.Values)
            {
                for (int i = 0; i < questsGroup.Count; i++)
                {
                    _totalQuestCounts++;
                    var guid = questsGroup[i].Guid;
                    var dto = _questElementDtoStorage.Get(guid);
                    var model = _questElementModelStorage.Get(dto.ModelID);
                    
                    if (dto.IsCompleted)
                    {
                        _completedQuests++;
                    }
                    if (dto.IsStashed)
                    {
                        continue;
                    }

                    var slot = _instantiator.InstantiatePrefabForComponent<UIQuestItem>(_window.QuestItemPrefab, _window.Content);
                    _itemSlots.Add(slot);
                    slot.Initialize(questsGroup[i].Guid);
                    slot.OnButtonClicked += TryToGetReward;
                    slot.SetQuestIcon(_iconLoader.Load(model.QuestIcon));
                    slot.SetProgressFill((float)dto.CurrentValue/dto.GoalValue);
                    slot.SetProgressData(questsGroup[i].IsCompleted() ? LocalizationManager.Localize(_questReadyTextKey) : $"{dto.CurrentValue} / {dto.GoalValue}");
                    slot.SetProgressColor(questsGroup[i].IsCompleted() ? Color.white : Color.black);
                    slot.SetRewardValue(model.GoldReward.ToString());
                    slot.SetActiveTimer(dto.TimeLeftForDiscarding > 0.0f && !questsGroup[i].IsCompleted());
                    slot.SetDescriptionText(questsGroup[i].GetTitleText());
                    slot.ActivateRewardButton(questsGroup[i].IsCompleted());

                }
            }
            UpdateWindowData();
        }
        public void Tick()
        {
            foreach (var itemSlot in _itemSlots)
            {
                if(itemSlot == null)
                    continue;
                
                var leftTimeInMinutes = _questElementDtoStorage.Get(itemSlot.Guid).TimeLeftForDiscarding;
                if(leftTimeInMinutes <= 0.0f)
                    continue;
                itemSlot.UpdateTimeLeft(Format.Time(TimeSpan.FromMinutes(leftTimeInMinutes)));
            }
        }
        private void UpdateWindowData()
        {
            _window.SetDay(_questSystemController.CurrentDay);
            var progress01 = _totalQuestCounts == 0 ? 0 : (float) _completedQuests / _totalQuestCounts;
            _window.ChangeProgressFill(progress01);
        }
        
        
        
        private void TryToGetReward(UIQuestItem uiQuestItem)
        {
            var guid = uiQuestItem.Guid;
            var dto = _questElementDtoStorage.Get(guid);
            var model = _questElementModelStorage.Get(dto.ModelID);
            dto.IsStashed = true;
            dto.IsCompleted = true;
            _completedQuests++;
            _itemSlots.Remove(uiQuestItem);
            UpdateWindowData();
            int hasGold = model.GoldReward;
            _currencyCollectorSystem.Collect(
                    GetSlotPosition(uiQuestItem), 
                    _goldCurrencyID,
                    model.GoldReward,
                    true,
                    left =>
                    {
                        var collected = hasGold - left;
                        hasGold = left;
                        _user.AddCurrency(_goldCurrencyID, collected);
                    },
                    () => _user.AddCurrency(_goldCurrencyID, hasGold)
                );
            uiQuestItem.CompleteQuest(() =>
            {
                DestroySlot(uiQuestItem);
            });
            UpdateGiftPoints();
        }

        private Vector2 GetSlotPosition(UIQuestItem uiQuestItem)
        {
            return _farmCameraSceneObject.Camera.ScreenToWorldPoint(uiQuestItem.ButtonPosition);
        }
        private void DestroySlot(UIQuestItem uiQuestItem)
        {
            GameObject.Destroy(uiQuestItem.gameObject);
        }
        private void RemoveSlots()
        {
            for (int i = 0; i < _itemSlots.Count; i++)
            {
                if(_itemSlots[i] == null)
                    continue;
                _itemSlots[i].OnButtonClicked -= TryToGetReward;
                DestroySlot(_itemSlots[i]);
            }
            _itemSlots.Clear();
        }


    }
}
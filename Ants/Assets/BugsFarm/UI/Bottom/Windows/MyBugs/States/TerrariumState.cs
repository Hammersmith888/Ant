using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.AssetLoaderSystem;
using BugsFarm.BuildingSystem;
using BugsFarm.CurrencySystem;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.StateMachine;
using BugsFarm.Services.StatsService;
using BugsFarm.Services.UIService;
using BugsFarm.UnitSystem;
using BugsFarm.UserSystem;
using BugsFarm.Utility;
using UniRx;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace BugsFarm.UI
{
    public class TerrariumState : State
    {
        private readonly IStateMachine _myBugsStateMachine;
        private readonly IInstantiator _instantiator;
        private readonly IUIService _uiService;
        private readonly IUser _user;
        private readonly IconLoader _iconLoader;
        private readonly UnitDtoStorage _unitDtoStorage;
        private readonly UnitModelStorage _unitModelStorage;
        private readonly MyBugsItemModelStorage _myBugsItemModelStorage;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly UnitUpgradeModelStorage _unitUpgradeModelStorage;
        private readonly Dictionary<string, MyBugItem> _myBugItems;
        private readonly Dictionary<string, ParamItem> _paramItems;
        private const string _infoDefaultHeaderTextKey = "UIMyBugs_DefaultInfoHeader";
        private const string _maxDisplayValueStatKey = "stat_maxDisplayValue";
        private const string _xpStatKey = "stat_xp";
        private const string _levelStatKey = "stat_level";
        private string _selected;
        
        public TerrariumState(IStateMachine myBugsStateMachine,
                              IInstantiator instantiator,
                              IUIService uiService,
                              IUser user,
                              IconLoader iconLoader,
                              UnitDtoStorage unitDtoStorage,
                              UnitModelStorage unitModelStorage,
                              MyBugsItemModelStorage myBugsItemModelStorage,
                              StatsCollectionStorage statsCollectionStorage,
                              UnitUpgradeModelStorage unitUpgradeModelStorage) : base("Terrarium")
        {
            _myBugsStateMachine = myBugsStateMachine;
            _instantiator = instantiator;
            _uiService = uiService;
            _user = user;
            _iconLoader = iconLoader;
            _unitDtoStorage = unitDtoStorage;
            _unitModelStorage = unitModelStorage;
            _myBugsItemModelStorage = myBugsItemModelStorage;
            _statsCollectionStorage = statsCollectionStorage;
            _unitUpgradeModelStorage = unitUpgradeModelStorage;
            _myBugItems = new Dictionary<string, MyBugItem>();
            _paramItems = new Dictionary<string, ParamItem>();
        }

        public override void OnEnter(params object[] args)
        {
            base.OnEnter(args);
            _uiService.Get<UIMyBugsWindow>().TerrariumView.Show();
            _unitDtoStorage.OnStorageChanged += OnUnitsStorageChanged;
            FillMyBugs();
            FillInfo(_selected);
        }

        public override void OnExit()
        {
            base.OnExit();
            _uiService.Get<UIMyBugsWindow>().TerrariumView.Hide();
            _unitDtoStorage.OnStorageChanged -= OnUnitsStorageChanged;
            ClearInfo();
            ClearMyBugs();
        }
        
        private void FillInfo(string id)
        {
            ClearInfo();
            if (string.IsNullOrEmpty(id))
            {
                if (_myBugItems.Count == 0)
                {
                    Select(null);
                    return;
                }
                id = _myBugItems.Values.First().Id;
            }

            if (!_unitDtoStorage.TryGet(id, out var dto) ||
                !_myBugsItemModelStorage.TryGet(dto.ModelID, out var myBugModel))
            {
                Select(null);
                return;
            }

            Select(id);
            var model = _unitModelStorage.Get(dto.ModelID);
            var canUpgrade = GetUpgradePrice(_selected, out var price);
            var view = _uiService.Get<UIMyBugsWindow>().TerrariumView;
            
            view.SetInfoHeaderText(LocalizationHelper.GetBugName(dto.NameID, model.IsFemale));
            view.SetAvatarIcon(_iconLoader.Load(model.TypeName));
            view.SetUpgradeButtonActive(canUpgrade);
            view.SetUpgradeCurrencyIcon(canUpgrade ? _iconLoader.LoadCurrency(price.ModelID) : null);
            view.SetUpgradePriceText(price.Count.ToString());
            view.SetUpgradeInteractable(canUpgrade && _user.HasCurrency(price));
            view.SetInfoButtonActive(true);
            view.UpgradeEvent += OnUpgradeEventHandler;
            view.InfoEvent += (sender, args) =>
            {
                _myBugsStateMachine.Switch("Wiki", model.ModelID);
            };
            
            if (!_statsCollectionStorage.HasEntity(_selected))
            {
                return;
            }

            var curerntStatsCollections = _statsCollectionStorage.Get(_selected);
            if (curerntStatsCollections.HasEntity(_xpStatKey))
            {
                var stat = curerntStatsCollections.Get<StatVital>(_xpStatKey);
                view.SetExpirienceProgress(stat.CurrentValue / stat.Value);
                view.SetExpirienceProgressActive(true);
            }
            
            var userStatsColelction = _statsCollectionStorage.Get(_user.Id);
            var maxDisplayStatValue = userStatsColelction.GetValue(_maxDisplayValueStatKey);
            foreach (var statId in myBugModel.Stats)
            {
                if (!curerntStatsCollections.HasEntity(statId))
                {
                    continue;
                }

                var statValue = curerntStatsCollections.GetValue(statId);
                var paramItem = _instantiator
                    .InstantiatePrefabForComponent<ParamItem>(view.ParamPrefabItem, view.ParamsContainer);
                paramItem.SetProgress(Mathf.Clamp01(statValue / maxDisplayStatValue));
                paramItem.SetProgressText(""); // TODO : fill if value overflow of max display value
                paramItem.SetIcon(_iconLoader.Load(statId.Replace("stat_","")));
                _paramItems.Add(statId, paramItem);
            }
        }
        
        private void FillMyBugs()
        {
            ClearMyBugs();
            var view = _uiService.Get<UIMyBugsWindow>().TerrariumView;
            foreach (var unitDto in _unitDtoStorage.Get().ToArray())
            {
                if (!_myBugsItemModelStorage.HasEntity(unitDto.ModelID))
                {
                    continue;
                }
                
                var model = _unitModelStorage.Get(unitDto.ModelID);
                var item = _instantiator.InstantiatePrefabForComponent<MyBugItem>(view.MyBugPrefabItem, view.MyBugsContainer);
                item.Id = unitDto.Guid;
                item.SetIcon(_iconLoader.Load(model.TypeName));
                item.ClickEvent += OnMyBugClickEventHandler;
                item.SetLevelText(GetLevel(unitDto.Guid));
                _myBugItems.Add(unitDto.Guid, item);
            }
        }

        private void ClearInfo()
        {
            foreach (var paramItem in _paramItems.Values.ToArray())
            {
                if (paramItem)
                {
                    Object.Destroy(paramItem.gameObject);     
                }
            }
            _paramItems.Clear();
            
            var view = _uiService.Get<UIMyBugsWindow>().TerrariumView;
            view.ResetEvents();
            view.SetInfoButtonActive(false);
            view.SetExpirienceProgressActive(false);
            view.SetUpgradeButtonActive(false);
            view.SetAvatarIcon(null);
            view.SetInfoHeaderText(LocalizationManager.Localize(_infoDefaultHeaderTextKey));
        }
        
        private void ClearMyBugs()
        {
            foreach (var item in _myBugItems.Values.ToArray())
            {
                if (item != null)
                {
                    Object.Destroy(item.gameObject);
                }
            }
            _myBugItems.Clear();
        }

        private void Select(string id)
        {
            _selected = id;
            foreach (var item in _myBugItems.Values)
            {
                item.SetSelected(_selected == item.Id);
            }
        }

        private bool GetUpgradePrice(string unitId, out CurrencyModel price)
        {
            price = default;
            if (string.IsNullOrEmpty(unitId) || 
                !_unitDtoStorage.HasEntity(unitId))
            {
                return false;
            }
            var dto = _unitDtoStorage.Get(unitId);
            if (!_unitUpgradeModelStorage.HasEntity(dto.ModelID))
            {
                return false;
            }
            


            
            var nextLevel = 1;
            var upgradeModel = _unitUpgradeModelStorage.Get(dto.ModelID);
            var getEntityLevelCommand = _instantiator.Instantiate<GetEntityLevelCommand>();
            getEntityLevelCommand.Execute(new GetEntitytLevelProtocol(dto.Guid, res => nextLevel += res));

            if (!upgradeModel.Levels.ContainsKey(nextLevel))
            {
                return false;
            }
            
            var upgradeLevelModel = upgradeModel.Levels[nextLevel];
            price = upgradeLevelModel.Price;
            if (_statsCollectionStorage.TryGet(unitId, out var statsCollection) &&
                statsCollection.TryGet<StatVital>(_xpStatKey, out var xpStat))
            {
                price.Count = (int) (price.Count - (price.Count * (xpStat.CurrentValue / xpStat.Value)));
            }
            return true;
        }
        
        private void OnUpgradeEventHandler(object sender, EventArgs e)
        {
            if (GetUpgradePrice(_selected, out var price) && _user.HasCurrency(price))
            {
                MessageBroker.Default.Publish(new AntHillTaskActionCompletedProtocol()
                {
                    Guid = _selected,
                    ModelID = _unitDtoStorage.Get(_selected).ModelID,
                    EntityType = AntHillTaskReferenceGroup.Unit,
                    TaskType = AntHillTaskType.Upgrade
                });
                _user.AddCurrency(price.ModelID, -price.Count);
                _instantiator.Instantiate<UpgradeUnitCommand>()
                    .Execute(new UpgradeUnitProtocol(_selected));
                
                if (_myBugItems.ContainsKey(_selected))
                {
                    _myBugItems[_selected].SetLevelText(GetLevel(_selected));
                }
            }
            FillInfo(_selected);
        }

        private void OnMyBugClickEventHandler(object sender, string id)
        {
            FillInfo(id);
        }
        
        private void OnUnitsStorageChanged(string unitId)
        {
            FillMyBugs();
            FillInfo(_selected);
        }

        private string GetLevel(string unitId)
        {
            return _statsCollectionStorage.TryGet(unitId, out var statsCollection) ? 
                ((int) statsCollection.GetValue(_levelStatKey)).ToString() : "1";
        }
    }
}
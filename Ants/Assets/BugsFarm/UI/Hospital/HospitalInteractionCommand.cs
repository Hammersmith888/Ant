using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BugsFarm.AssetLoaderSystem;
using BugsFarm.BuildingSystem;
using BugsFarm.CurrencySystem;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.Services.InputService;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.StatsService;
using BugsFarm.Services.UIService;
using BugsFarm.UnitSystem;
using BugsFarm.UserSystem;
using UniRx;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace BugsFarm.UI
{
    public class HospitalInteractionCommand : InteractionBaseCommand
    {
        private readonly IUser _user;
        private readonly IUIService _uiService;
        private readonly IInstantiator _instantiator;
        private readonly IInputController<MainLayer> _inputController;
        private readonly IBuildingBuildSystem _buildingBuildSystem;
        private readonly IconLoader _iconLoader;
        private readonly UnitDtoStorage _unitDtoStorage;
        private readonly UnitModelStorage _unitModelStorage;
        private readonly UnitUpgradeModelStorage _unitUpgradeModelStorage;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly HospitalSlotDtoStorage _hospitalSlotDtoStorage;
        private readonly HospitalSlotModelStorage _hospitalSlotModelStorage;
        private readonly BuildingUpgradeModelStorage _buildingUpgradeModelStorage;
        private readonly BuildingDtoStorage _buildingDtoStorage;

        /// <summary>
        /// folderId, slots
        /// </summary>
        private readonly Dictionary<string, Dictionary<string,HospitalItemSlot>> _slots;
        /// <summary>
        /// folderId, empty slots
        /// </summary>
        private readonly Dictionary<string, List<HospitalEmptySlot>> _slotsEmpty;

        private const string _reserveFolder = "reserve";
        private const string _repairFolder = "repair";
        private const string _repairCountStatKey = "stat_repairCount";
        private const string _requiredLevelTextKey = "UIHospital_RequiredLevel";
        private const string _upgradeMultiplierStatKey = "stat_upgradeMultiplier";
        private const string _maxLevelStatKey = "stat_maxLevel";
        private const string _ladyBugModelId = "3";
        private readonly CompositeDisposable _events;
        private BuildingUpgradeModel _upgradeModel;
        private BuildingDto _buildingDto;
        private StatsCollection _statsCollection;
        
        public HospitalInteractionCommand(IUser user,
                                          IUIService uiService, 
                                          IInstantiator instantiator,
                                          IInputController<MainLayer> inputController,
                                          IBuildingBuildSystem buildingBuildSystem,
                                          IconLoader iconLoader,
                                          UnitDtoStorage unitDtoStorage,
                                          UnitModelStorage unitModelStorage,
                                          UnitUpgradeModelStorage unitUpgradeModelStorage,
                                          StatsCollectionStorage statsCollectionStorage,
                                          HospitalSlotDtoStorage hospitalSlotDtoStorage,
                                          HospitalSlotModelStorage hospitalSlotModelStorage,
                                          BuildingUpgradeModelStorage buildingUpgradeModelStorage,
                                          BuildingDtoStorage buildingDtoStorage)
        {
            _user = user;
            _uiService = uiService;
            _instantiator = instantiator;
            _inputController = inputController;
            _buildingBuildSystem = buildingBuildSystem;
            _iconLoader = iconLoader;
            _unitDtoStorage = unitDtoStorage;
            _unitModelStorage = unitModelStorage;
            _unitUpgradeModelStorage = unitUpgradeModelStorage;
            _statsCollectionStorage = statsCollectionStorage;
            _hospitalSlotDtoStorage = hospitalSlotDtoStorage;
            _hospitalSlotModelStorage = hospitalSlotModelStorage;
            _buildingUpgradeModelStorage = buildingUpgradeModelStorage;
            _buildingDtoStorage = buildingDtoStorage;
            _slots = new Dictionary<string, Dictionary<string,HospitalItemSlot>>
            {
                {_reserveFolder,new Dictionary<string, HospitalItemSlot>()},
                {_repairFolder,new Dictionary<string, HospitalItemSlot>()},
            };
            _slotsEmpty = new Dictionary<string, List<HospitalEmptySlot>>();
            _events = new CompositeDisposable();
        }

        public override Task Execute(InteractionProtocol protocol)
        {
            if (_buildingBuildSystem.IsBuilding(protocol.Guid))
            {
                SwitchToInfoMenu(protocol);
                return base.Execute(protocol);
            }
            _inputController.Lock();
            _hospitalSlotDtoStorage.OnStorageChanged += OnHospitalSlotsChanged;
            _buildingDto = _buildingDtoStorage.Get(protocol.Guid);
            _upgradeModel = _buildingUpgradeModelStorage.Get(_buildingDto.ModelID);
            _statsCollection = _statsCollectionStorage.Get(_buildingDto.Guid);
            _buildingBuildSystem.OnStarted   += OnBuildingProcesses;
            _buildingBuildSystem.OnCompleted += OnBuildingProcesses;
            var window = _uiService.Show<UIHospitalWindow>();
            window.CloseEvent += (sender, args) =>
            {
                _buildingDto = null;
                _events.Dispose();
                _events.Clear();
                _hospitalSlotDtoStorage.OnStorageChanged -= OnHospitalSlotsChanged;
                _buildingBuildSystem.OnStarted   -= OnBuildingProcesses;
                _buildingBuildSystem.OnCompleted -= OnBuildingProcesses;
                _uiService.Hide<UIHospitalWindow>();
                var slots = _slots.Values
                                  .SelectMany(x => x.Values)
                                  .OfType<MonoBehaviour>()
                                  .Union(_slotsEmpty.Values.SelectMany(x => x))
                                  .ToArray();
                _slots.Clear();
                _slotsEmpty.Clear();
                DestroySlots(slots);
                _inputController.UnLock();
            };
            window.InfoEvent += (sender, args) =>
            {
                SwitchToInfoMenu(protocol);
            };
            
            window.CreateRandomEvent += (sender, args) =>
            {
                var unitModels = _hospitalSlotModelStorage.Get().ToArray();
                var randomModel = Tools.RandomItem(unitModels);
                var createUnitProtocol = new CreateUnitProtocol(randomModel.ModelId,true);
                _instantiator.Instantiate<CreateUnitCommand>().Execute(createUnitProtocol);
                _instantiator.Instantiate<UnitSpawnCommand<SpawnUnitTask>>().Execute(new UnitSpawnProtocol(createUnitProtocol.Guid));
                _statsCollectionStorage.Get(createUnitProtocol.Guid).AddModifier("stat_level", new StatModBaseAdd(Random.Range(0,3)));
                MessageBroker.Default.Publish(new DeathUnitProtocol{DeathReason = UnitSystem.DeathReason.Fighted, UnitId = createUnitProtocol.Guid});
            };

            MessageBroker.Default
                         .Receive<UserLevelChangedProtocol>()
                         .Subscribe(_ => Update())
                         .AddTo(_events);
            
            MessageBroker.Default
                         .Receive<UserCurrencyChangedProtocol>()
                         .Subscribe(_ => Update())
                         .AddTo(_events);
            Update();
            return base.Execute(protocol);
        }

        private void SwitchToInfoMenu(InteractionProtocol protocol)
        {
            _uiService.Get<UIHospitalWindow>().Close();
            protocol.ObjectType = SceneObjectType.Building;
            _instantiator.Instantiate<InteractionCommand>().Execute(protocol);
        }

        private void Update()
        {
            var slots = _hospitalSlotDtoStorage.Get().ToArray();
            var reserve = new List<HospitalSlotDto>();
            var repairing = new List<HospitalSlotDto>();
            foreach (var slotDto in slots)
            {
                if (slotDto.Repairing)
                {
                    repairing.Add(slotDto);
                }
                else
                {
                    reserve.Add(slotDto);
                }
            }
            FillReserve(reserve);
            FillRepair(repairing);
            UpdateEmptySlots();
        }

        private void UpdateEmptySlots()
        {
            if (_slots.ContainsKey(_reserveFolder))
            {
                var maxFreeSlots = 6;
                var rowCount = 3;
                var slotCount = _slots[_reserveFolder].Count;
                var endCount = (slotCount % rowCount);
                slotCount = endCount > 0 ? (maxFreeSlots - endCount) : maxFreeSlots / 2;
                FillEmptySlots(_reserveFolder, new object[slotCount]);  
            }
            
            if (_slots.ContainsKey(_repairFolder))
            {
                var buildingLevel = GetLevel(_buildingDto.Guid);
                var freeSlots = FreeRepairSlotsCount();
                var args = new List<object>(new object[freeSlots]);
                foreach (var upgradeLevelModel in _upgradeModel.Levels.Values.OrderBy(x=>x.Level))
                {
                    if (upgradeLevelModel.Level <= buildingLevel)
                    {
                        continue;
                    }
                    var count = upgradeLevelModel.Stats
                        .Sum(statModel => statModel.StatID == _repairCountStatKey? statModel.Value : 0);
                    
                    if (count == 0)
                    {
                        continue;
                    }

                    for (var i = 0; i < count; i++)
                    {
                        args.Add(upgradeLevelModel.Level); 
                    }
                }
                FillEmptySlots(_repairFolder, args.ToArray());
            }
        }

        private void UpdateSlot(string folderId, string slotId, int freeSlots = -1)
        {
            if (!_slots.ContainsKey(folderId) || 
                !_slots[folderId].ContainsKey(slotId))
            {
                return;
            }

            if (!_hospitalSlotDtoStorage.HasEntity(slotId))
            {
                RemoveSlot(folderId, slotId);
                Debug.LogError($"Slot missed but updated, id : {slotId}");
                return;
            }

            var slotDto = _hospitalSlotDtoStorage.Get(slotId);
            var slot = _slots[folderId][slotId];
            var isReserve = folderId == _reserveFolder;
            var slotLevel = GetLevel(slotId);
            var byLevel = slotLevel <= _statsCollection.GetValue(_maxLevelStatKey);
            slot.SetBuyButtonActive(isReserve);
            slot.SetLifeTimeActive(isReserve);
            slot.SetRemoveButtonActive(isReserve);
            slot.SetRepairProgressActive(!isReserve);
            slot.SetIcon(GetIcon(slotDto.ModelId));
            slot.SetLevelText(slotLevel.ToString());
            slot.SetLevelColor(byLevel ? slot.InitLevelColor : Color.red);
            GetPrice(slotId, out var price);
            if (isReserve)
            {
                freeSlots = freeSlots < 0 ? FreeRepairSlotsCount() : freeSlots;
                slot.SetBuyButtonInteractable(byLevel && _user.HasCurrency(price) && CanProduction() && freeSlots > 0);
                slot.SetPriceText(price.Count.ToString());
            }
            else
            {
                OnRepairTimeChanged(slotDto.Id);
            }
        }

        private void RemoveSlot(string folderId, string slotId)
        {
            if (!_slots.ContainsKey(folderId) || 
                !_slots[folderId].ContainsKey(slotId))
            {
                return;
            }
            var slot = _slots[folderId][slotId];
            _slots[folderId].Remove(slotId);
            DestroySlots(slot);
            UpdateEmptySlots();
        }

        private void MoveSlotToRepair(string slotId)
        {
            if (!_slots.ContainsKey(_reserveFolder) ||
                !_slots[_reserveFolder].ContainsKey(slotId))
            {
                return;
            }

            var slot = _slots[_reserveFolder][slotId];
            var window = _uiService.Get<UIHospitalWindow>();
            var slotDto = _hospitalSlotDtoStorage.Get(slotId);
            slotDto.LifeTime.OnCurrentValueChanged -= OnReserveTimeChanged;
            slotDto.RepairTime.OnCurrentValueChanged += OnRepairTimeChanged;
            slot.transform.SetParent(window.RepairContent);
            _slots[_reserveFolder].Remove(slotId);
            _slots[_repairFolder].Add(slotId, slot);
            Update();
        }
        
        private void FillReserve(IEnumerable<HospitalSlotDto> slots)
        {
            var window = _uiService.Get<UIHospitalWindow>();
            if (!_slots.ContainsKey(_reserveFolder))
            {
                _slots.Add(_reserveFolder, new Dictionary<string, HospitalItemSlot>());
            }
            
            var slotItems = _slots[_reserveFolder];
            var freeRepairSlots = FreeRepairSlotsCount();
            foreach (var slotDto in slots)
            {
                if (slotItems.ContainsKey(slotDto.Id))
                {
                    UpdateSlot(_reserveFolder, slotDto.Id, freeRepairSlots);
                    continue;
                }
                var slot = _instantiator.InstantiatePrefabForComponent<HospitalItemSlot>(window.ItemSlotPrefab, 
                                                                                         window.ReserveContent);
                slot.Init(slotDto.Id);
                slot.RemoveEvent += OnReserveRemoveEventHandler;
                slot.BuyEvent += OnSlotBuyEventHandler;
                slotDto.LifeTime.OnCurrentValueChanged += OnReserveTimeChanged;
                slotItems.Add(slotDto.Id, slot);
                UpdateSlot(_reserveFolder, slotDto.Id, freeRepairSlots);
            }
        }

        private void FillRepair(IEnumerable<HospitalSlotDto> slots)
        {
            var window = _uiService.Get<UIHospitalWindow>();
            if (!_slots.ContainsKey(_repairFolder))
            {
                _slots.Add(_repairFolder, new Dictionary<string, HospitalItemSlot>());
            }
            
            var slotItems = _slots[_repairFolder];
            foreach (var slotDto in slots)
            {
                if (slotItems.ContainsKey(slotDto.Id))
                {
                    UpdateSlot(_repairFolder, slotDto.Id);
                    continue;
                }
                var slot = _instantiator.InstantiatePrefabForComponent<HospitalItemSlot>(window.ItemSlotPrefab,
                                                                                         window.RepairContent);
                slot.Init(slotDto.Id);
                slotDto.RepairTime.OnCurrentValueChanged += OnRepairTimeChanged;
                slotItems.Add(slotDto.Id, slot);
                UpdateSlot(_repairFolder, slotDto.Id);
            }
        }

        private void FillEmptySlots(string folderId, params object[] slotArgs)
        {
            if (slotArgs.Length == 0)
            {
                return;
            }
            
            if (!folderId.AnyOff(_repairFolder, _reserveFolder))
            {
                Debug.LogError($"folder id not correct : {folderId}");
                return;
            }
            if (!_slotsEmpty.ContainsKey(folderId))
            {
                _slotsEmpty.Add(folderId, new List<HospitalEmptySlot>());
            }
            else
            {
                var slots = _slotsEmpty[folderId].ToArray();
                _slotsEmpty[folderId].Clear();
                DestroySlots(slots);
            }
            
            var slotsEmpty = _slotsEmpty[folderId];
            var window = _uiService.Get<UIHospitalWindow>();
            var content = folderId == _reserveFolder ? window.ReserveContent : window.RepairContent;
            foreach (var arg in slotArgs)
            {
                var slot = _instantiator
                    .InstantiatePrefabForComponent<HospitalEmptySlot>(window.EmptySlotPrefab, content);
                if (arg == null)
                {
                    slot.SetEmptyActive(true);
                    slot.SetRequiredActive(false);
                }
                else
                {
                    var text = LocalizationManager.Localize(_requiredLevelTextKey);
                    slot.SetRequiredText(string.Format(text, arg));
                    slot.SetEmptyActive(false);
                    slot.SetRequiredActive(true);
                }
                slotsEmpty.Add(slot);
            }
        }

        private void DestroySlots(params MonoBehaviour[] slots)
        {
            foreach (var view in slots)
            {
                if (view)
                {
                    Object.Destroy(view.gameObject);  
                }
            }
        }

        private int FreeRepairSlotsCount()
        {
            var maxCount = (int)_statsCollection.GetValue(_repairCountStatKey);
            var occupied = _hospitalSlotDtoStorage.Get().Count(x => x.Repairing);
            var free = maxCount - occupied;
            return free > 0 ? free : 0;
        }

        private bool CanProduction()
        {
            return !_buildingBuildSystem.IsBuilding(_buildingDto.Guid) &&
                   _unitDtoStorage.Get().ToArray().Any(unitDto => unitDto.ModelID == _ladyBugModelId && 
                                                                  !_hospitalSlotDtoStorage.HasEntity(unitDto.Guid));
        }

        private Sprite GetIcon(string modelId)
        {
            return !_unitModelStorage.HasEntity(modelId) ? default : 
                _iconLoader.Load(_unitModelStorage.Get(modelId).TypeName);
        }

        private int GetLevel(string entityId)
        {
            if (!_statsCollectionStorage.HasEntity(entityId))
            {
                return 0;
            }
            var statsCollection = _statsCollectionStorage.Get(entityId);
            var statKey = "stat_level";
            
            return statsCollection.HasEntity(statKey) ? 
                (int) statsCollection.GetValue(statKey): 0;
        }

        private bool GetPrice(string slotId, out CurrencyModel price)
        {
            price = default;
            if (!_hospitalSlotDtoStorage.HasEntity(slotId))
            {
                Debug.LogError($"Slot does not exist");
                return false;
            }

            var dto   = _hospitalSlotDtoStorage.Get(slotId);
            var model = _hospitalSlotModelStorage.Get(dto.ModelId);
            var upgradePriceCount = 0;
            var entityLevel = GetLevel(dto.Id);
            if (entityLevel > 1 && _unitUpgradeModelStorage.HasEntity(dto.ModelId))
            {
                var upgradeModel = _unitUpgradeModelStorage.Get(dto.ModelId);
                upgradePriceCount = upgradeModel.Levels
                                    .TakeWhile(x => x.Key <= entityLevel)
                                    .Sum(x => x.Value.Price.Count);
            }

            price = model.Price;
            price.Count += (int) (upgradePriceCount / _statsCollection.GetValue(_upgradeMultiplierStatKey));
            return true;
        }
        
        private void OnSlotBuyEventHandler(object sender, string slotId)
        {
            if (!_hospitalSlotDtoStorage.HasEntity(slotId))
            {
                return;
            }

            if (!GetPrice(slotId, out var price))
            {
                return;
            }

            if (!_user.HasCurrency(price))
            {
                UpdateSlot(_reserveFolder, slotId);
                return;
            }

            _user.AddCurrency(price.ModelID, -price.Count);
            MessageBroker.Default.Publish(new HospitalSlotRepairProtocol{SlotId = slotId});
            MoveSlotToRepair(slotId);
        }
        
        private void OnReserveTimeChanged(string slotId)
        {
            if (!_slots.ContainsKey(_reserveFolder) || 
                !_slots[_reserveFolder].ContainsKey(slotId))
            {
                return;
            }
            var slot = _slots[_reserveFolder][slotId];
            var slotDto = _hospitalSlotDtoStorage.Get(slotId);
            var progress = slotDto.LifeTime.CurrentValue / slotDto.LifeTime.MaxValue;
            slot.SetLifeTimeColor(Color.Lerp(Color.red, Color.green, progress));
            slot.SetLifeTimeProgress(progress);
        }

        private void OnRepairTimeChanged(string slotId)
        {
            if (!_slots.ContainsKey(_repairFolder) || 
                !_slots[_repairFolder].ContainsKey(slotId) ||
                !_hospitalSlotDtoStorage.HasEntity(slotId))
            {
                return;
            }
            var slot = _slots[_repairFolder][slotId];
            var slotDto = _hospitalSlotDtoStorage.Get(slotId);
            var progress = 1f - (slotDto.RepairTime.CurrentValue / slotDto.RepairTime.MaxValue);
            slot.SetRepairProgressText(Format.Age(Format.MinutesToSeconds(slotDto.RepairTime.CurrentValue), true));
            slot.SetRepairProgress(progress);
        }
        
        private void OnHospitalSlotsChanged(string slotId)
        {
            if (!_hospitalSlotDtoStorage.HasEntity(slotId))
            {
                RemoveSlot(_repairFolder, slotId);
                RemoveSlot(_reserveFolder, slotId);
            }
            Update();
        }

        private void OnReserveRemoveEventHandler(object sender, string slotId)
        {
            RemoveSlot(_reserveFolder, slotId);
            MessageBroker.Default.Publish(new HospitalRemoveSlotProtocol{SlotId = slotId});
        }
        
        private void OnBuildingProcesses(string buildingId)
        {
            if (buildingId != _buildingDto.Guid)
            {
                return;
            }

            Update();
        }
    }
}
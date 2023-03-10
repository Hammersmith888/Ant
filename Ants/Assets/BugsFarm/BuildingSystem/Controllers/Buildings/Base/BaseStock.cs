using System;
using System.Collections.Generic;
using BugsFarm.AnimationsSystem;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.InventorySystem;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StatsService;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public abstract class BaseStock : ISceneEntity, IInitializable
    {
        public string Id { get; private set; }

    #region Abstract base

        protected abstract string ItemID { get; }
        protected virtual int GetActionCycles => 1;
        protected virtual int AddActionCycles => 1;
        protected abstract AnimKey[] GetActionAnimKeys { get; }
        protected virtual AnimKey GetWalkAnimKey => AnimKey.Walk;
        protected abstract AnimKey[] AddActionAnimKeys { get; }
        protected virtual AnimKey AddWalkAnimKey => AnimKey.Walk;
        protected abstract Type GetTaskType { get; }
        protected abstract Type AddTaskType { get; }

    #endregion

        protected GetResourceSystem GetResourceSystem;
        protected AddResourceSystem AddResourceSystem;
        private StatsCollectionStorage _statsCollectionStorage;
        private BuildingSceneObjectStorage _viewStorage;

        protected BuildingDtoStorage DtoStorage;
        protected IInstantiator Instantiator;

        protected const string _resourceStatKey = "stat_maxResource";
        protected const string _maxUnitsStatKey = "stat_maxUnits";

        private ResourceInfo _resourceInfo;
        protected IEnumerable<IPosSide> TaskPoints;
        protected BuildingDto Dto;
        protected IInventory Inventory;

        private StatVital _resourceStat;
        protected StatsCollection StatsCollection;
        protected BuildingSceneObject View;
        protected bool Finalized;

        [Inject]
        private void Inject(string guid,
                            GetResourceSystem getResourceSystem,
                            AddResourceSystem addResourceSystem,
                            StatsCollectionStorage statsCollectionStorage,
                            BuildingSceneObjectStorage viewStorage,
                            BuildingDtoStorage dtoStorage,
                            IInstantiator instantiator)
        {
            Id = guid;
            GetResourceSystem = getResourceSystem;
            AddResourceSystem = addResourceSystem;
            _statsCollectionStorage = statsCollectionStorage;
            Instantiator = instantiator;
            _viewStorage = viewStorage;
            DtoStorage = dtoStorage;
        }

        public virtual void Initialize()
        {
            Dto = DtoStorage.Get(Id);
            View = _viewStorage.Get(Id);
            
            StatsCollection = _statsCollectionStorage.Get(Id);
            _resourceStat = StatsCollection.Get<StatVital>(_resourceStatKey);
            _resourceStat.OnValueChanged += OnResourceValueChanged;
            
            var slot = new ItemSlot(ItemID, 0, (int) _resourceStat.Value);
            var inventoryProtocol = new CreateInventoryProtocol(Id, res => Inventory = res, slot);
            Instantiator.Instantiate<CreateInventoryCommand>().Execute(inventoryProtocol);

            TaskPoints = View.GetComponent<TasksPoints>().Points;
            GetResourceSystem.OnResourceChanged  += OnResourceChaned;
            GetResourceSystem.OnResourceDepleted += OnResourceDepleted;
            AddResourceSystem.OnResourceFull     += OnResourceFull;
            AddResourceSystem.OnResourceChanged  += OnResourceChaned;

            _resourceInfo = Instantiator.Instantiate<ResourceInfo>(new object[] {Id});
            _resourceInfo.OnUpdate += OnUpdateResourceInfo;

            UpdateStage();
            Production();
        }

        public virtual void Dispose()
        {
            if (Finalized) return;
            Finalized = true;
            _resourceInfo.OnUpdate -= OnUpdateResourceInfo;
            _resourceInfo.Dispose();
            _resourceInfo = null;
            
            _resourceStat.OnValueChanged -= OnResourceValueChanged;
            _resourceStat = null;
            StatsCollection = null;
            
            GetResourceSystem.OnResourceChanged -= OnResourceChaned;
            GetResourceSystem.OnResourceDepleted -= OnResourceDepleted;
            AddResourceSystem.OnResourceFull -= OnResourceFull;
            AddResourceSystem.OnResourceChanged -= OnResourceChaned;

            GetResourceSystem.UnRegistration(ItemID, Id);
            AddResourceSystem.UnRegistration(ItemID, Id);
            
            Instantiator.Instantiate<DeleteInventoryCommand>().Execute(new DeleteInventoryProtocol(Id));
        }

        protected virtual void Production()
        {
            if (Finalized) return;
            // Get resource
            if (Inventory.HasItem(ItemID) && !GetResourceSystem.Contains(ItemID, Id))
            {
                var maxUnits = (int)StatsCollection.GetValue(_maxUnitsStatKey);
                var args = ResourceArgs.Default()
                    .SetActionAnimKeys(GetActionAnimKeys)
                    .SetWalkAnimKeys(GetWalkAnimKey)
                    .SetRepeatCounts(GetActionCycles);
                var protocol = new GetResourceProtocol(Id, Dto.ModelID, ItemID, maxUnits, 
                                                       TaskPoints, GetTaskType, args);
                GetResourceSystem.Registration(protocol);
            }
            else if (!Inventory.HasItem(ItemID) && GetResourceSystem.Contains(ItemID, Id))
            {
                GetResourceSystem.UnRegistration(ItemID, Id);
            }

            var itemSlot = Inventory.GetItemSlot(ItemID);
            var needAdd = itemSlot.Count < itemSlot.Capacity;

            // Add resource
            if (needAdd && !AddResourceSystem.Contains(ItemID, Id))
            {
                var maxUnits = (int)StatsCollection.GetValue(_maxUnitsStatKey);
                var args = ResourceArgs.Default()
                    .SetActionAnimKeys(AddActionAnimKeys)
                    .SetWalkAnimKeys(AddWalkAnimKey)
                    .SetRepeatCounts(AddActionCycles);
                var protocol = new AddResourceProtocol(Id, Dto.ModelID, ItemID, maxUnits, 
                                                       TaskPoints, AddTaskType, args, false);
                AddResourceSystem.Registration(protocol);
            }
            else if (!needAdd && AddResourceSystem.Contains(ItemID, Id))
            {
                AddResourceSystem.UnRegistration(ItemID, Id);
            }
        }

        protected virtual void UpdateStage()
        {
            if (Finalized) return;
            var itemSlot = Inventory.GetItemSlot(ItemID);
            var protocol = new SetStageBuildingProtocol(Id, itemSlot.Count, itemSlot.Capacity, OnStageChanged);
            Instantiator.Instantiate<SetStageBuildingCommand>().Execute(protocol);
        }

        protected virtual void OnResourceChaned(string itemId, string guid)
        {
            if (guid != Id || itemId != ItemID) return;
            UpdateStage();
            Production();
        }

        protected virtual void OnResourceFull(string itemId, string guid)
        {
            if (guid != Id || itemId != ItemID) return;
            UpdateStage();
            Production();
        }

        protected virtual void OnResourceDepleted(string itemId, string guid)
        {
            if (guid != Id || itemId != ItemID) return;
            UpdateStage();
            Production();
        }
        
        private void OnUpdateResourceInfo()
        {
            var slot = Inventory.GetItemSlot(ItemID);
            _resourceInfo.SetInfo(Format.Resource(slot.Count, slot.Capacity));
        }
        
        protected virtual void OnStageChanged(StageActionProtocol protocol){}

        protected virtual void SelfDestroy()
        {
            if (Finalized) return;
            var deleteProcotol = new DeleteBuildingProtocol(Id);
            var deleteCommand = Instantiator.Instantiate<DeleteBuildingCommand>();
            deleteCommand.Execute(deleteProcotol);
        }
        
        private void OnResourceValueChanged(object sender, EventArgs e)
        {
            if (Inventory == null || _resourceStat == null)
            {
                return;
            }
            Inventory.SetSlotCapacity(ItemID, (int)_resourceStat.Value);
        }
    }
}
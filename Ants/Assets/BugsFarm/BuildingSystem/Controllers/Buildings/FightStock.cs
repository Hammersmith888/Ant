using System;
using BugsFarm.AnimationsSystem;
using UniRx;

namespace BugsFarm.BuildingSystem
{
    public class FightStock : BaseStock
    {
        protected override string ItemID => "0"; // food Item
        protected override AnimKey[] GetActionAnimKeys { get; } = {AnimKey.TakeFoodLow};
        protected override AnimKey[] AddActionAnimKeys { get; } = {AnimKey.GiveFood};
        protected override AnimKey GetWalkAnimKey => AnimKey.Walk;
        protected override AnimKey AddWalkAnimKey => AnimKey.WalkFood;
        protected override Type GetTaskType => typeof(GetFightStockTask);
        protected override Type AddTaskType => typeof(AddFightStockTask);
        
        public override void Initialize()
        {
            if(Finalized) return;
            base.Initialize();
            Inventory.UpdateInventoryAction += OnUpdateInventory;
        }
        
        public override void Dispose()
        {
            if(Finalized) return;
            Inventory.UpdateInventoryAction -= OnUpdateInventory;
            base.Dispose();
        }
        
        protected override void UpdateStage() { }
        
        protected override void Production()
        {
            if (Finalized) return;
            // Get resource
            var anyItemIdInGame = GetResourceSystem.HasItemExcludeModelIDs(ItemID, Dto.ModelID);
            if (Inventory.HasItem(ItemID) && !anyItemIdInGame && !GetResourceSystem.Contains(ItemID, Id))
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
            else if ((!Inventory.HasItem(ItemID) || anyItemIdInGame) && GetResourceSystem.Contains(ItemID, Id))
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
        
        private void OnUpdateInventory(string itemId)
        {
            if(Finalized) return;
            MessageBroker.Default.Publish(new StockChangedProtocol{Guid = Id,ItemId = itemId, ModelId = Dto?.ModelID});
        }
    }
}
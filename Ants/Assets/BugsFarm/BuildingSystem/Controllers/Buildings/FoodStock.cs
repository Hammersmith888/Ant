using System;
using BugsFarm.AnimationsSystem;

namespace BugsFarm.BuildingSystem
{
    public class FoodStock : BaseStock
    {        
        protected override string ItemID => "0"; // предмет еды
        protected override AnimKey[] GetActionAnimKeys {get;} = {AnimKey.TakeFoodLow};
        protected override AnimKey[] AddActionAnimKeys {get;} = {AnimKey.GiveFood};
        protected override AnimKey GetWalkAnimKey => AnimKey.Walk;
        protected override AnimKey AddWalkAnimKey => AnimKey.WalkFood;
        protected override Type GetTaskType => typeof(GetFoodStockTask);
        protected override Type AddTaskType => typeof(AddFoodStockTask);

        private const string _fightStockModelId = "41";
        protected override void OnStageChanged(StageActionProtocol protocol)
        {
            View.SetInterractable(protocol.CurIndex != protocol.MaxIndex);
        }

        protected override void OnResourceDepleted(string itemId, string guid)
        {
            if(guid != Id || itemId != ItemID || Finalized)  return;
            UpdateStage();
            SelfDestroy();
        }
        
        protected override void Production()
        {
            if (Finalized) return;
            // Get resource
            var anyItemIdInGame = GetResourceSystem.HasItemExcludeModelIDs(ItemID, Dto.ModelID, _fightStockModelId);
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
    }
}
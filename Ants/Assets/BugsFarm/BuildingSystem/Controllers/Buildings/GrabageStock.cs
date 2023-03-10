using System;
using System.Linq;
using BugsFarm.AnimationsSystem;

namespace BugsFarm.BuildingSystem
{
    public class GrabageStock : BaseStock
    {
        protected override string ItemID => "3"; // предмет мусора
        protected override AnimKey[] GetActionAnimKeys { get; } = {AnimKey.TakeGarbage};
        protected override AnimKey[] AddActionAnimKeys { get; } = {AnimKey.GarbageDropPile};
        protected override AnimKey GetWalkAnimKey => AnimKey.Walk;
        protected override AnimKey AddWalkAnimKey => AnimKey.WalkGarbage;
        protected override Type GetTaskType => typeof(GetGrabageStockTask);
        protected override Type AddTaskType => typeof(AddGrabageStockTask);

        private const string _dumpsterModelId = "40";
        protected override void OnStageChanged(StageActionProtocol protocol)
        {
            View.SetInterractable(protocol.CurIndex != protocol.MaxIndex);
        }

        protected override void OnResourceDepleted(string itemId, string guid)
        {
            if (guid != Id || itemId != ItemID || Finalized) return;
            UpdateStage();
            SelfDestroy();
        }
        protected override void Production()
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
            var hasDumpster = DtoStorage.Get()
                .Any(x => x.ModelID == _dumpsterModelId && AddResourceSystem.HasTasks(ItemID, x.Guid));
            // Add resource
            if (needAdd && !hasDumpster && !AddResourceSystem.Contains(ItemID, Id))
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
            else if ((!needAdd || hasDumpster) && AddResourceSystem.Contains(ItemID, Id))
            {
                AddResourceSystem.UnRegistration(ItemID, Id);
            }
        }
    }
}
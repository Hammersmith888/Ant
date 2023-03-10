using System;
using System.Collections.Generic;
using BugsFarm.InventorySystem;

namespace BugsFarm.BuildingSystem
{
    public class GetResourceController 
    {
        public event Action<GetResourceController> OnDepleted;
        public event Action<GetResourceController> OnChanged;
        public string ItemID { get; }
        public int ItemCount => _inventory.GetItemSlot(ItemID).Count;
        /// <summary>
        /// Resource owner guid
        /// </summary>
        public string OwnerGuid   { get; }
        public string ModelID     { get; }
        /// <summary>
        /// Controller guid
        /// </summary>
        public string Guid     { get; }
        public bool IsDisabled { get; private set; } 
        public Type TaskType   { get; }
        public ResourceArgs Args  { get; }
        public PointsController PointsController { get; } 
        
        private readonly IInventory _inventory;
        public GetResourceController(InventoryStorage inventoryStorage, 
                                     GetResourceProtocol protocol)
        {
            ItemID            = protocol.ItemID;
            OwnerGuid         = protocol.Guid;
            TaskType          = protocol.TaskType;
            Guid              = System.Guid.NewGuid().ToString();
            PointsController  = new PointsController();
            _inventory        = inventoryStorage.Get(OwnerGuid);
            Args              = protocol.Args;
            ModelID           = protocol.ModelID;
            PointsController.Initialize(protocol.MaxUnits, protocol.Points);
        }
        
        public IEnumerable<IItem> GetItems(ref int count)
        {
            if (IsDisabled || !_inventory.HasItem(ItemID))
            {
                count = 0;
                return default;
            }
            
            var items = _inventory.GetItems(ItemID, ref count);
            _inventory.Remove(ItemID, count);
            return items;
        }
        
        public IEnumerable<IItem> GetImmediateItems(ref int count)
        {
            if (IsDisabled || !_inventory.HasItem(ItemID))
            {
                count = 0;
                return default;
            }
            
            var items = GetItems(ref count);
            if (!_inventory.HasItem(ItemID))
            {
                OnDepleted?.Invoke(this);
            }
            else
            {
                OnChanged?.Invoke(this);
            }
            return items;
        }
        
        public bool CanCreateTask()
        {
            return !IsDisabled && PointsController.HasPoint() && _inventory.HasItem(ItemID);
        }
        
        public void Release()
        {
            if (IsDisabled)
            {
                return;
            }
            
            PointsController.FreePoint();
            if (!_inventory.HasItem(ItemID))
            {
                OnDepleted?.Invoke(this);
            }
            else
            {
                OnChanged?.Invoke(this);
            }
        }
        
        public void Dispose()
        {
            IsDisabled = true;
        }
    }
}
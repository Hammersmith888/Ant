using System;
using BugsFarm.InventorySystem;
using UniRx;
using UnityEngine;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class AddResourceController 
    {
        public event Action<AddResourceController> OnFull;
        public event Action<AddResourceController> OnChanged;
        public string ItemID { get; }
        public int NeedItemCount => _slot.Capacity - _slot.Count;

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
        public bool TaskNotify { get; }
        public Type TaskType   { get; }
        public object[] TaskExtraArgs   { get; }
        public ResourceArgs Args  { get; }
        public PointsController PointsController { get; } 
        
        private readonly IInstantiator _instantiator;
        private readonly IInventorySlot _slot;
        
        public AddResourceController(InventoryStorage inventoryStorage, 
                                     IInstantiator instantiator, 
                                     AddResourceProtocol protocol)
        {
            ItemID            = protocol.ItemID;
            OwnerGuid         = protocol.Guid;
            ModelID           = protocol.ModelID;
            TaskType          = protocol.TaskType;
            TaskExtraArgs     = protocol.TaskExtraArgs;
            Guid              = System.Guid.NewGuid().ToString();
            PointsController  = new PointsController();
            _instantiator     = instantiator;
            Args              = protocol.Args;
            PointsController.Initialize(protocol.MaxUnits, protocol.Points);
            TaskNotify = protocol.TaskNotify;
            _slot = inventoryStorage.Get(OwnerGuid).GetItemSlot(ItemID); 
        }

        public void AddItem(ref int count)
        {
            if (IsDisabled || count == 0)
            {
                count = 0;
                return;
            }
            
            if (_slot.Capacity > 0)
            {
                if ((_slot.Count + count) > _slot.Capacity)
                {
                    count = _slot.Capacity - _slot.Count;
                }
            }
            var addProtocol = new AddItemsProtocol(ItemID, count, OwnerGuid);
            _instantiator.Instantiate<AddItemsCommand>().Execute(addProtocol);
        }
        public bool CanCreateTask()
        {
            return !IsDisabled && 
                   PointsController.HasPoint() &&
                   IsNeed();
        }
        public void Release()
        {
            if(IsDisabled) return;
            PointsController.FreePoint();
            if (!IsNeed())
            {
                OnFull?.Invoke(this);
                return;
            }
            OnChanged?.Invoke(this);
        }
        public void Dispose()
        {
            IsDisabled = true;
        }

        private bool IsNeed()
        {
            // если нет предметов или меньше максимума то нужно добавлять
            return _slot.Count < _slot.Capacity;
        }
    }
}
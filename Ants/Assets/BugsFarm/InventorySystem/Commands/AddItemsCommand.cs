using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BugsFarm.Services.CommandService;
using Zenject;

namespace BugsFarm.InventorySystem
{
    public class AddItemsCommand : ICommand<AddItemsProtocol>
    {
        private readonly IInstantiator _instantiator;
        private readonly InventoryItemModelStorage _itemModelStorage;
        private readonly InventoryStorage _inventoryStorage;

        public AddItemsCommand(IInstantiator instantiator,
                               InventoryItemModelStorage itemModelStorage,
                               InventoryStorage inventoryStorage)
        {
            _instantiator = instantiator;
            _itemModelStorage = itemModelStorage;
            _inventoryStorage = inventoryStorage;
        }

        public Task Execute(AddItemsProtocol protocol)
        {
            if (!_itemModelStorage.HasEntity(protocol.ItemID))
            {
                throw new InvalidOperationException();
            }

            if (!_inventoryStorage.HasEntity(protocol.Guid))
            {
                throw new InvalidOperationException();
            }

            if (protocol.Count <= 0)
            {
                return Task.CompletedTask;
            }

            var outListItems = new List<IItem>();
            var createItemProtocol = new CreateItemProtocol(protocol.ItemID, protocol.Count, outListItems);
            _instantiator.Instantiate<CreateItemCommand>().Execute(createItemProtocol);
            _inventoryStorage.Get(protocol.Guid).AddItems(outListItems);
            return Task.CompletedTask;
        }
    }
}
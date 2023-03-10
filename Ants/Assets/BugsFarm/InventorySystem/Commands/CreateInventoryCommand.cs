using System.Collections.Generic;
using System.Threading.Tasks;
using BugsFarm.Services.CommandService;
using Zenject;

namespace BugsFarm.InventorySystem
{
    public class CreateInventoryCommand : ICommand<CreateInventoryProtocol>
    {
        private readonly IInstantiator _instantiator;
        private readonly InventoryDtoStorage _invetoryDtoStorage;
        private readonly InventoryStorage _inventoryStorage;

        public CreateInventoryCommand(IInstantiator instantiator,
                                      InventoryDtoStorage invetoryDtoStorage,
                                      InventoryStorage inventoryStorage)
        {
            _instantiator = instantiator;
            _invetoryDtoStorage = invetoryDtoStorage;
            _inventoryStorage = inventoryStorage;
        }

        public Task Execute(CreateInventoryProtocol protocol)
        {
            if (!_invetoryDtoStorage.HasEntity(protocol.Guid))
            {
                var slots = new List<ItemSlot>(protocol.Items);
                var dto = new InventoryDto(protocol.Guid, slots);
                _invetoryDtoStorage.Add(dto);
            }

            if (!_inventoryStorage.HasEntity(protocol.Guid))
            {
                _inventoryStorage.Add(_instantiator.Instantiate<Inventory>(new object[] {_invetoryDtoStorage.Get(protocol.Guid)}));
            }
            
            protocol.CallbackResult?.Invoke(_inventoryStorage.Get(protocol.Guid));

            return Task.CompletedTask;
        }
    }
}
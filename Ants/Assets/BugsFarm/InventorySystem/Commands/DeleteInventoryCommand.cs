using System.Threading.Tasks;
using BugsFarm.Services.CommandService;

namespace BugsFarm.InventorySystem
{
    public class DeleteInventoryCommand : ICommand<DeleteInventoryProtocol>
    {
        private readonly InventoryDtoStorage _invetoryDtoStorage;
        private readonly InventoryStorage _inventoryStorage;

        public DeleteInventoryCommand(InventoryDtoStorage invetoryDtoStorage,
                                      InventoryStorage inventoryStorage)
        {
            _invetoryDtoStorage = invetoryDtoStorage;
            _inventoryStorage = inventoryStorage;
        }

        public Task Execute(DeleteInventoryProtocol protocol)
        {
            if (_inventoryStorage.HasEntity(protocol.Guid))
            {
                var inventory = _inventoryStorage.Get(protocol.Guid);
                _inventoryStorage.Remove(protocol.Guid);
                inventory.Dispose();
            }
            
            if (_invetoryDtoStorage.HasEntity(protocol.Guid))
            {
                _invetoryDtoStorage.Remove(protocol.Guid);
            }
            
            return Task.CompletedTask;
        }
    }
}
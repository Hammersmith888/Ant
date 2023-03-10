using System;
using System.Threading.Tasks;
using BugsFarm.Services.CommandService;
using Zenject;

namespace BugsFarm.InventorySystem
{
    public class CreateItemCommand : ICommand<CreateItemProtocol>
    {
        private readonly IInstantiator _instantiator;
        private readonly InventoryItemModelStorage _itemModelStorage;

        public CreateItemCommand(IInstantiator instantiator,
                                 InventoryItemModelStorage itemModelStorage)
        {
            _instantiator = instantiator;
            _itemModelStorage = itemModelStorage;
        }

        public Task Execute(CreateItemProtocol protocol)
        {
            if (!_itemModelStorage.HasEntity(protocol.ItemID))
            {
                throw new InvalidOperationException();
            }

            var args = new object[] {protocol.ItemID, protocol.Count};
            var item = _instantiator.Instantiate<InventoryItem>(args);
            protocol.OutItemList.Add(item);

            return Task.CompletedTask;
        }
    }
}
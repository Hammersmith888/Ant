using System;
using System.Threading.Tasks;
using BugsFarm.InventorySystem;
using BugsFarm.Services.CommandService;
using UniRx;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class RestockBuildingCommand : ICommand<RestockBuildingProtocol>
    {
        private readonly IInstantiator _instantiator;
        private readonly BuildingRestockModelStorage _buildingRestockModelStorage;
        private readonly BuildingDtoStorage _buildingDtoStorage;
        private readonly InventoryStorage _inventoryStorage;

        public RestockBuildingCommand(IInstantiator instantiator,
                                      BuildingRestockModelStorage buildingRestockModelStorage,
                                      BuildingDtoStorage buildingDtoStorage,
                                      InventoryStorage inventoryStorage)
        {
            _instantiator = instantiator;
            _buildingRestockModelStorage = buildingRestockModelStorage;
            _buildingDtoStorage = buildingDtoStorage;
            _inventoryStorage = inventoryStorage;
        }

        public Task Execute(RestockBuildingProtocol protocol)
        {
            if (!_buildingDtoStorage.HasEntity(protocol.Guid))
            {
                throw new InvalidOperationException($"Building with Id : {protocol.Guid}" +
                                                    $", does not exist");
            }

            var buildingDto = _buildingDtoStorage.Get(protocol.Guid);
            if (!_buildingRestockModelStorage.HasEntity(buildingDto.ModelID))
            {
                throw new InvalidOperationException($"Building with ModelID : {buildingDto.ModelID}" +
                                                    $", does not have {nameof(BuildingRestockModel)}!");
            }
            if (!_inventoryStorage.HasEntity(buildingDto.Guid))
            {
                throw new InvalidOperationException($"Building with ModelID : {buildingDto.ModelID}" +
                                                    $", does not have {nameof(IInventory)}!");
            }
            
            var restockModel = _buildingRestockModelStorage.Get(buildingDto.ModelID);
            _instantiator.Instantiate<AddItemsCommand>().Execute(new AddItemsProtocol(restockModel.ItemId, 
                                                                  protocol.Count, buildingDto.Guid));
            var inventory = _inventoryStorage.Get(buildingDto.Guid);
            var itemSlot = inventory.GetItemSlot(restockModel.ItemId);
            var stageProtocol = new SetStageBuildingProtocol(buildingDto.Guid,itemSlot.Count,itemSlot.Capacity);
            _instantiator.Instantiate<SetStageBuildingCommand>().Execute(stageProtocol);
            MessageBroker.Default.Publish(protocol);
            return Task.CompletedTask;
        }
    }
}
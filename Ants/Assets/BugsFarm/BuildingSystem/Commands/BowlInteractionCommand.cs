using System.Threading.Tasks;
using BugsFarm.InventorySystem;
using BugsFarm.Quest;
using BugsFarm.Services.SceneEntity;
using UniRx;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class BowlInteractionCommand : InteractionBaseCommand
    {
        private readonly IInstantiator _instantiator;
        private readonly InventoryStorage _inventoryStorage;
        private readonly BuildingSceneObjectStorage _buildingSceneObjectStorage;
        private const string _itemId = "4"; // water item
        private const int _tapRestockPrecent = 25;

        public BowlInteractionCommand(IInstantiator instantiator,
                                      InventoryStorage inventoryStorage,
                                      BuildingSceneObjectStorage buildingSceneObjectStorage)
        {
            _instantiator = instantiator;
            _inventoryStorage = inventoryStorage;
            _buildingSceneObjectStorage = buildingSceneObjectStorage;
        }
        public override Task Execute(InteractionProtocol protocol)
        {
            if (_inventoryStorage.HasEntity(protocol.Guid))
            {
                var inventory = _inventoryStorage.Get(protocol.Guid);
                var itemSlot = inventory.GetItemSlot(_itemId);
                if (itemSlot.Count < itemSlot.Capacity)
                {
                    MessageBroker.Default.Publish(new QuestUpdateProtocol()
                    {
                        QuestType = QuestType.UpdateBuilding,
                        ReferenceID = "39",
                        Value = 1
                    });
                    
                    var addCount = (_tapRestockPrecent / 100f) * itemSlot.Capacity;
                    var addItemsProtocol = new AddItemsProtocol(_itemId, (int)addCount, protocol.Guid);
                    _instantiator.Instantiate<AddItemsCommand>().Execute(addItemsProtocol);
                    UpdateWaterSize(protocol.Guid);
                    return Task.CompletedTask;
                } 
            }

            // если ресурс полный, значит можно выполнять интерактивные операции.
            protocol.ObjectType = SceneObjectType.Building;
            return _instantiator.Instantiate<InteractionCommand>().Execute(protocol);
        }
        
        private void UpdateWaterSize(string buildingId)
        {
            if (!_inventoryStorage.HasEntity(buildingId) || 
                !_buildingSceneObjectStorage.HasEntity(buildingId))
            {
                return;
            }

            var view = (BowlSceneObject)_buildingSceneObjectStorage.Get(buildingId);
            var inventory = _inventoryStorage.Get(buildingId);
            var itemSlot = inventory.GetItemSlot(_itemId);
            var currProgerss = (float) itemSlot.Count / itemSlot.Capacity;
            var currWaterSize = currProgerss * view.InitWaterSize;
            var size = view.WaterRenderer.size.SetY(currWaterSize);
            view.WaterRenderer.size = size;
        }
    }
}
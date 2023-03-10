using System.Threading.Tasks;
using BugsFarm.Services.CommandService;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StatsService;
using Zenject;

namespace BugsFarm.ChestSystem
{
    public class CreateChestCommand : ICommand<CreateChestProtocol>
    {
        private readonly IInstantiator _instantiator;
        private readonly SceneEntityStorage _sceneEntityController;
        private readonly ChestDtoStorage _dtoStorage;
        private readonly ChestStatModelStorage _statModelStorage;
        
        public CreateChestCommand(IInstantiator instantiator,
                                 SceneEntityStorage sceneEntityController,
                                 ChestDtoStorage dtoStorage,
                                 ChestStatModelStorage statModelStorage)
        {
            _instantiator = instantiator;
            _sceneEntityController = sceneEntityController;
            _dtoStorage = dtoStorage;
            _statModelStorage = statModelStorage;
        }
        
        public Task Execute(CreateChestProtocol protocol)
        {
            ChestDto chestDto;
            if (!string.IsNullOrEmpty(protocol.ModelID))
            {
                chestDto = new ChestDto(protocol.ModelID, protocol.Guid);
                _dtoStorage.Add(chestDto);
            }
            else
            {
                chestDto = _dtoStorage.Get(protocol.Guid);
            }
            
            var stats = new StatModel[0];
            if (_statModelStorage.HasEntity(chestDto.ModelID))
            {
                stats = _statModelStorage.Get(chestDto.ModelID).Stats;
            }
            var collectionProtocol = new CreateStatsCollectionProtocol(chestDto.Guid, stats);
            _instantiator.Instantiate<CreateStatsCollectionCommand<StatsCollection>>().Execute(collectionProtocol);
            
            var entity = _instantiator.Instantiate<Chest>(new []{protocol.Guid});
            _sceneEntityController.Add(entity);
            entity.Initialize();

            return Task.CompletedTask;
        }
    }
}
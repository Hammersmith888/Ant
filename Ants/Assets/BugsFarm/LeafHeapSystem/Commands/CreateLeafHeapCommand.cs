using System.Threading.Tasks;
using BugsFarm.LeafHeapSystem.Controllers;
using BugsFarm.Services.CommandService;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StatsService;
using Zenject;

namespace BugsFarm.LeafHeapSystem
{
    public class CreateLeafHeapCommand : ICommand<CreateLeafHeapProtocol>
    {
        private readonly IInstantiator _instantiator;
        private readonly SceneEntityStorage _sceneEntityController;
        private readonly LeafHeapDtoStorage _dtoStorage;
        private readonly LeafHeapStatModelStorage _statModelStorage;

        public CreateLeafHeapCommand(IInstantiator instantiator,
                                    SceneEntityStorage sceneEntityController,
                                    LeafHeapDtoStorage dtoStorage,
                                    LeafHeapStatModelStorage statModelStorage)
        {
            _instantiator = instantiator;
            _sceneEntityController = sceneEntityController;
            _dtoStorage = dtoStorage;
            _statModelStorage = statModelStorage;
        }

        public Task Execute(CreateLeafHeapProtocol protocol)
        {
            LeafHeapDto leafHeapDto;
            if (!string.IsNullOrEmpty(protocol.ModelID))
            {
                leafHeapDto = new LeafHeapDto(protocol.ModelID, protocol.Guid);
                _dtoStorage.Add(leafHeapDto);
            }
            else
            {
                leafHeapDto = _dtoStorage.Get(protocol.Guid);
            }

            var stats = new StatModel[0];
            if (_statModelStorage.HasEntity(leafHeapDto.ModelID))
            {
                stats = _statModelStorage.Get(leafHeapDto.ModelID).Stats;
            }
            var collectionProtocol = new CreateStatsCollectionProtocol(leafHeapDto.Guid, stats);
            _instantiator.Instantiate<CreateStatsCollectionCommand<StatsCollection>>().Execute(collectionProtocol);
            
            var entity = _instantiator.Instantiate<LeafHeap>(new[] {protocol.Guid});
            _sceneEntityController.Add(entity);

            var buildLeafHeapSceneObjectProtocol = new CreateLeafHeapSceneObjectProtocol(protocol.Guid);
            _instantiator.Instantiate<CreateLeafHeapSceneObjectCommand>().Execute(buildLeafHeapSceneObjectProtocol);
            entity.Initialize();

            return Task.CompletedTask;
        }
    }
}
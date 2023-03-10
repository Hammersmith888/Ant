using System.Threading.Tasks;
using BugsFarm.Services.CommandService;
using BugsFarm.Services.StorageService;

namespace BugsFarm.Services.StatsService
{
    public class DeleteStatsCollectionCommand : ICommand<DeleteStatsCollectionProtocol>
    {
        private readonly IStorage<StatsCollection> _statsCollection;
        private readonly IStorage<StatsCollectionDto> _statsCollectionDto;

        public DeleteStatsCollectionCommand(IStorage<StatsCollection> statsCollection, IStorage<StatsCollectionDto> statsCollectionDto)
        {
            _statsCollection = statsCollection;
            _statsCollectionDto = statsCollectionDto;
        }
        
        public Task Execute(DeleteStatsCollectionProtocol protocol)
        {
            if (_statsCollectionDto.HasEntity(protocol.Guid))
            {
                _statsCollectionDto.Remove(protocol.Guid);
            }
            if (_statsCollection.HasEntity(protocol.Guid))
            {
                _statsCollection.Remove(protocol.Guid);
            }
            return Task.CompletedTask;
        }
    }
}
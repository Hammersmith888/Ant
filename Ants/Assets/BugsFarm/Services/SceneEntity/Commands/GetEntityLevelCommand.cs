using System.Threading.Tasks;
using BugsFarm.Services.CommandService;
using BugsFarm.Services.StatsService;

namespace BugsFarm.Services.SceneEntity
{
    public class GetEntityLevelCommand : ICommand<GetEntitytLevelProtocol>
    {
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private const string _levelStatKey = "stat_level";
        public GetEntityLevelCommand(StatsCollectionStorage statsCollectionStorage)
        {
            _statsCollectionStorage = statsCollectionStorage;
        }

        public Task Execute(GetEntitytLevelProtocol protocol)
        {
            var level = 0;
            if (_statsCollectionStorage.HasEntity(protocol.EntityId))
            {
                var statCollection = _statsCollectionStorage.Get(protocol.EntityId);
                if (statCollection.HasEntity(_levelStatKey))
                {
                    level = (int) statCollection.GetValue(_levelStatKey);
                }
            }
            protocol.Result?.Invoke(level);
            return Task.CompletedTask;
        }
    }
}
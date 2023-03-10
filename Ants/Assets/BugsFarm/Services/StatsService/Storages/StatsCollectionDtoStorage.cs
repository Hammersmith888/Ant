using BugsFarm.ReloadSystem;
using BugsFarm.Services.SaveManagerService;
using BugsFarm.Services.StorageService;
using UniRx;

namespace BugsFarm.Services.StatsService
{
    public class StatsCollectionDtoStorage : Storage<StatsCollectionDto>
    {

        public StatsCollectionDtoStorage(ISavableStorage savableStorage)
        {
            savableStorage.Register(this);
            MessageBroker.Default.Receive<GameReloadingReport>().Subscribe(x => savableStorage.Register(this));
        }
    }
}
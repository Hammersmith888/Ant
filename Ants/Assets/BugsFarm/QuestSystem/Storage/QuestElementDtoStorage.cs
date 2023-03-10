using BugsFarm.Services.SaveManagerService;
using BugsFarm.Services.StorageService;

namespace BugsFarm.Quest
{
    public class QuestElementDtoStorage : Storage<QuestElementDto>
    {
        public QuestElementDtoStorage(ISavableStorage savableStorage)
        {
            savableStorage.Register(this);
        }
    }
}
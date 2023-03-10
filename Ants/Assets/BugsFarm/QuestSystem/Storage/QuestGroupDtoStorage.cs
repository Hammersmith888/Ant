using BugsFarm.Services.SaveManagerService;
using BugsFarm.Services.StorageService;

namespace BugsFarm.Quest
{
    public class QuestGroupDtoStorage : Storage<QuestGroupDto>
    {
        public QuestGroupDtoStorage(ISavableStorage savableStorage)
        {
            savableStorage.Register(this);
        }
    }
}
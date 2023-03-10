using BugsFarm.Services.StorageService;

namespace BugsFarm.InfoCollectorSystem
{
    public interface IBuildingInfo : IStorageItem
    {
        float Progress { get; }
        string Info { get; }
        string Description { get; }
    }
}
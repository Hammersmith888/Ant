namespace BugsFarm.Services.SaveManagerService
{
    public interface ISaveManagerLocal
    {
        bool HasSaves(string saveDataKey);
        string Load(string saveDataKey);
        void Save(string saveDataKey, string data);
        void Remove(string saveDataKey);
    }
}

namespace BugsFarm.Services.SaveManagerService
{
    public interface ISaveManager
    {
        bool HasSaves(string saveDataKey);
        string Load(string saveDataKey);
        void Save(string data, string saveDataKey);
        
        void LoadAll(string saveDataKey);
        void SaveAll(string saveDataKey);
        void Remove(string saveDataKey);
    }
}
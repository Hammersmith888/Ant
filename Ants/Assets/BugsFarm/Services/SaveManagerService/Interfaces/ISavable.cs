namespace BugsFarm.Services.SaveManagerService
{
    public interface ISavable
    {
        string GetTypeKey();
        string Save();
        void Load(string jsonData);
    }
}
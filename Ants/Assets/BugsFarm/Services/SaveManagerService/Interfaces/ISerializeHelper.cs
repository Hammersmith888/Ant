namespace BugsFarm.Services.SaveManagerService
{
    public interface ISerializeHelper
    {
        bool HasSerializedData(string key);
        string GetSerializedData(string key);
        void SaveSerializedData(string data, string key);
        void DeleteSerializedData(string key);
    }
}
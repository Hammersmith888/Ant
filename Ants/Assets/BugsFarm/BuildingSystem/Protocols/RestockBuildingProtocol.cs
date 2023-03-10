using BugsFarm.Services.CommandService;

namespace BugsFarm.BuildingSystem
{
    public readonly struct RestockBuildingProtocol : IProtocol
    {
        public readonly string Guid;
        public readonly string ItemId;
        public readonly int Count;
        public RestockBuildingProtocol(string guid, 
                                       string itemId,
                                       int count)
        {
            Guid = guid;
            ItemId = itemId;
            Count = count;
        }
    }
}
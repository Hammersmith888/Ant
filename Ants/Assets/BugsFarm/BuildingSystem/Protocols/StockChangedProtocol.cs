using BugsFarm.Services.CommandService;

namespace BugsFarm.BuildingSystem
{
    public struct StockChangedProtocol : IProtocol
    {
        public string Guid;
        public string ModelId;
        public string ItemId;
    }
}
using BugsFarm.Services.CommandService;

namespace BugsFarm.Services.StatsService
{
    public struct DeleteStatsCollectionProtocol : IProtocol
    {
        public string Guid;
    }
}
using BugsFarm.Services.CommandService;

namespace BugsFarm.Services.StatsService
{
    public readonly struct CreateStatsCollectionProtocol : IProtocol
    {
        public readonly string Guid;
        public readonly StatModel[] StatModels;

        public CreateStatsCollectionProtocol(string guid, params StatModel[] statModels)
        {
            Guid = guid;
            StatModels = statModels;
        }
    }
}
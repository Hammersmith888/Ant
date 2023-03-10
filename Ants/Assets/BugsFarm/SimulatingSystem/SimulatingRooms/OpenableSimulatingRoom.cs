using BugsFarm.Services.StatsService;

namespace BugsFarm.SimulatingSystem
{
    public class OpenableSimulatingRoom : ISimulatingRoom
    {
        public string Guid => _guid;
        public string ModelID => _modelID;
        
        public string Group => SimulatingRoomGroups.Openable;
        public int Capacity => 0;

        private string _guid;
        private readonly string _modelID;
        private readonly StatsCollection _statsCollection;
        private const string _progressStatKey = "stat_progress";

        public OpenableSimulatingRoom(string guid, string modelID, StatsCollection statsCollection)
        {
            _guid = guid;
            _modelID = modelID;
            _statsCollection = statsCollection;
        }

        public bool IsOpened()
        {
            return _statsCollection.GetValue(_progressStatKey) > 0;
        }

        public int UpProgress(float percent)
        {
            SetOpened();
            return 0;
        }
        
        public void SetOpened()
        {
            _statsCollection.Get<StatModifiable>(_progressStatKey).AddModifier(new StatModBaseAdd(1.0f));
        }
    }
}
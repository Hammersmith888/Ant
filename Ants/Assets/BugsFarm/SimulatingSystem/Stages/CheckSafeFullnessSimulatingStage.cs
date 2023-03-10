using System.Collections.Generic;
using System.Linq;
using BugsFarm.Services.StatsService;

namespace BugsFarm.SimulatingSystem
{
    public class CheckSafeFullnessSimulatingStage
    {
        private readonly StatsCollectionStorage _statsCollectionStorage;

        private Dictionary<string, Timer> _filledSafes;

        private const string _safeResourceStatKey = "stat_resource";

        private const float _safeDayWaitValue = 1440.0f;
        
        public CheckSafeFullnessSimulatingStage(StatsCollectionStorage statsCollectionStorage)
        {
            _statsCollectionStorage = statsCollectionStorage;
            _filledSafes = new Dictionary<string, Timer>();
        }
        
        public void CheckSafeFullness(float simulationTime, Dictionary<string, List<SimulatingEntityDto>> simulationData)
        {
            if (!simulationData.ContainsKey(SimulatingEntityType.Safe))
                return;

            foreach (var timer in _filledSafes.ToArray())
            {
                timer.Value.SubtractTime(simulationTime);
                if (timer.Value.IsExpired)
                {
                    SetSafeEmpty(timer.Key);
                    _filledSafes.Remove(timer.Key);
                }
                
            }

            foreach (var safe in simulationData[SimulatingEntityType.Safe])
            {
                StatsCollection statsCollection = _statsCollectionStorage.Get(safe.Guid);
                var resourceStat = statsCollection.Get<StatVital>(_safeResourceStatKey);
                if (resourceStat.CurrentValue >= resourceStat.Value && !_filledSafes.ContainsKey(safe.Guid))
                {
                    _filledSafes.Add(safe.Guid, new Timer(_safeDayWaitValue));
                }
            }
        }

        private void SetSafeEmpty(string guid)
        {
            _statsCollectionStorage.Get(guid).Get<StatVital>(_safeResourceStatKey).CurrentValue = 0;
        }

        public void Dispose()
        {
            _filledSafes.Clear();
        }
    }
}
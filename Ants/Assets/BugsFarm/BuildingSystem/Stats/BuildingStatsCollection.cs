using System;
using BugsFarm.Services.StatsService;
using BugsFarm.SimulationSystem;
using StatsCollection = BugsFarm.Services.StatsService.StatsCollection;

namespace BugsFarm.BuildingSystem
{
    [Serializable]
    public class BuildingStatsCollection : StatsCollection
    {
        private readonly ISimulationSystem _simulationSystem;
        private const string _bornTimeStatKey = "stat_bornTime";

        public BuildingStatsCollection(ISimulationSystem simulationSystem)
        {
            _simulationSystem = simulationSystem;
        }

        protected override void OnInstalled()
        {
            base.OnInstalled();
            if (HasEntity(_bornTimeStatKey) && GetValue(_bornTimeStatKey) <= 0)
            {
                AddModifier(_bornTimeStatKey, new StatModBaseAdd((float)_simulationSystem.GameAge));
            }
        }
    }
}
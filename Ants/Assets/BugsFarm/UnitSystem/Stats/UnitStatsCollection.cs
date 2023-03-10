using System;
using BugsFarm.Services.StatsService;
using BugsFarm.SimulationSystem;

namespace BugsFarm.UnitSystem
{
    [Serializable]
    public class UnitStatsCollection : StatsCollection
    {
        private readonly ISimulationSystem _simulationSystem;
        private const string _bornTimeStatKey = "stat_bornTime";
        public UnitStatsCollection(ISimulationSystem simulationSystem)
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
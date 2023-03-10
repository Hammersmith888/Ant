using System;

namespace BugsFarm.SimulationSystem
{
    public interface ISimulationSystem
    {
        event Action OnSimulationStart; // TODO move to EventBus
        event Action OnSimulationEnd;   // TODO move to EventBus
        /// <summary>
        /// In seconds
        /// </summary>
        double GameAge { get; set; }
        double LastExitTime { get; } // TODO move to user stats
        /// <summary>
        /// In seconds
        /// </summary>
        float DeltaTime { get; }
        float OrigDeltaTime { get; } // TODO maybe not need
        bool Simulation { get; }
        void Simulate(double time);
        void Reset(); 
    }
}
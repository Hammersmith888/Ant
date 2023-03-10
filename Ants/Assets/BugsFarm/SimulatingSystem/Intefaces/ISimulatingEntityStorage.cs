using System.Collections.Generic;

namespace BugsFarm.SimulatingSystem
{
    public interface ISimulatingEntityStorage
    {
        Dictionary<string, List<SimulatingEntityDto>> CreateTemporaryDatabase();
        void Remove(string guid);
        void Add(SimulatingEntityDto entityDto);
    }
}
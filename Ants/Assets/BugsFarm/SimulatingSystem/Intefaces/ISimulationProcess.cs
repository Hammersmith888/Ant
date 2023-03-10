namespace BugsFarm.SimulatingSystem
{
    public interface ISimulationProcess
    {
        void Simulate(float minutesInCycle, float dayModifier, float cycleNum);
        void PostSimulate(double simulatingTime, double currentSimulatingTime);
        void SimulateOneTime(double simulatingTime, double currentSimulatingTime);
        void Dispose();
    }
}
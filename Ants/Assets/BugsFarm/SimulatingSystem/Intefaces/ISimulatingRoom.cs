namespace BugsFarm.SimulatingSystem
{
    public interface ISimulatingRoom
    {
        string Guid { get; }
        string ModelID { get; }
        string Group { get; }
        
        int Capacity { get; }
        bool IsOpened();
        void SetOpened();
        int UpProgress(float percent);
    }
}
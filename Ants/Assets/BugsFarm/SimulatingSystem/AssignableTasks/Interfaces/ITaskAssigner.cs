namespace BugsFarm.SimulatingSystem.AssignableTasks
{
    public interface ITaskAssigner
    {
        bool CanAssign(string guid);
        void Assign(string guid);
    }
}
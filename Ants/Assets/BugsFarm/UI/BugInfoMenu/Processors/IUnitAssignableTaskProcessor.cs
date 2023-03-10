namespace BugsFarm.UI
{
    public interface IUnitAssignableTaskProcessor
    {
        bool CanExecute(string guid);
        void Execute(string guid);
    }
}
using System;

namespace BugsFarm.Services.BootstrapService
{
    public interface IProcessingCommands
    {
        event EventHandler AllCommandsDone;

        bool IsExecuting { get; }

        void AddCommand(ICommand cmd);
        void StartExecute();
        void Clear();

        bool Any();
    }
}
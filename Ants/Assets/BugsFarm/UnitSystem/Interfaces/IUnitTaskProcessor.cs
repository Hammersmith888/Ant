using System;
using BugsFarm.Services.StorageService;
using BugsFarm.TaskSystem;

namespace BugsFarm.UnitSystem
{
    public interface IUnitTaskProcessor : IStorageItem
    {
        bool IsFree { get; }
        event Action<ITask> OnFree;
        void SetTask(ITask task, params object[] args);
        ITask GetCurrentTask();
        bool CanExecute(ITask task);
        bool CanInterrupt(ITask other = null);
        void Update();
        void Interrupt();
        void Dispose();
        void Stop();
    }
}
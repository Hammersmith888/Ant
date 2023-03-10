using System;

namespace BugsFarm.Services.InputService
{

    public interface IInputController<T> where T : IInputLayer
    {
        event EventHandler LockChangedEvent;
        bool Locked { get; }
        void Lock();
        void UnLock();
    }
}
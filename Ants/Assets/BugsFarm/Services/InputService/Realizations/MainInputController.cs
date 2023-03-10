using System;

namespace BugsFarm.Services.InputService
{
    public class MainInputController  : IInputController<MainLayer>
    {
        public event EventHandler LockChangedEvent;
        public virtual bool Locked { get; protected set; }
        
        public virtual void Lock()
        {
            if (Locked)
            {
                return;
            }
            Locked = true;
            LockChangedEvent?.Invoke(this, EventArgs.Empty);
        }

        public virtual void UnLock()
        {
            if (!Locked)
            {
                return;
            }
            
            Locked = false;
            LockChangedEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}
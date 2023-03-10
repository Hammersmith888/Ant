using System;

namespace BugsFarm.Services.InputService
{
    public class InputController<T>  : IInputController<T> where T : IInputLayer
    {
        public virtual bool Locked => _mainlayer.Locked || _locked;
        public event EventHandler LockChangedEvent;
        
        private readonly IInputController<MainLayer> _mainlayer;
        private bool _locked;
        public InputController(IInputController<MainLayer> mainlayer)
        {
            _mainlayer = mainlayer;
            _mainlayer.LockChangedEvent += LockChangedEvent;
        }
        
        public void Lock()
        {
            if (_locked)
            {
                return;
            }
            
            _locked = true;
            LockChangedEvent?.Invoke(this,EventArgs.Empty);
        }

        public void UnLock()
        {
            if (!_locked)
            {
                return;
            }
            _locked = false;
            LockChangedEvent?.Invoke(this,EventArgs.Empty); 
        }
    }
}
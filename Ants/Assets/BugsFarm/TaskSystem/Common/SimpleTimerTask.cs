using UnityEngine;
using Zenject;

namespace BugsFarm.TaskSystem
{
    public class SimpleTimerTask : BaseTimerTask
    {
        private TickableManager _tickableManager;
       
        [Inject]
        private void Inject(TickableManager tickableManager)
        {
            _tickableManager = tickableManager;
        }

        public override void Execute(params object[] args)
        {
            if (IsRunned) return;
            base.Execute(args);
            _tickableManager.Add(this);
        }

        public override void Tick()
        {
            ApplyTime(Time.deltaTime);
        }

        protected override void OnDisposed()
        {
            if (IsExecuted)
            {
                _tickableManager.Remove(this);
            }
            base.OnDisposed();
        }
    }
}
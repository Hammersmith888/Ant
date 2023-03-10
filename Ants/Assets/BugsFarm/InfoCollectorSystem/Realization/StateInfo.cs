using System;
using System.Collections.Generic;

namespace BugsFarm.InfoCollectorSystem
{
    public class StateInfo : IStateInfo, IDisposable
    {
        public IEnumerable<string> Info { get; private set; }
        public string Id { get; }
        private readonly StateInfoStorage _stateInfoStorage;
        private bool _finalized;
        public StateInfo(string guid, StateInfoStorage stateInfoStorage)
        {
            Id = guid;
            Info = new string[0];
            _stateInfoStorage = stateInfoStorage;
            _stateInfoStorage.Add(this);
        }

        public void Update()
        {
            if(_finalized) return;
            OnUpdate?.Invoke();
        }

        internal event Action OnUpdate;

        internal void SetInfo(params string[] args)
        {
            if(_finalized) return;
            Info = args ?? new string[0];
        }

        public void Dispose()
        {
            if(_finalized) return;
            _finalized = true;
            OnUpdate = null;
            _stateInfoStorage.Remove(this);
        }
    }
}
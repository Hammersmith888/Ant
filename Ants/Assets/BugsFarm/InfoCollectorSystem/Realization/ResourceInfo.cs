using System;

namespace BugsFarm.InfoCollectorSystem
{
    public class ResourceInfo : IResourceInfo, IDisposable
    {
        public string Id { get; }
        public string Info { get; private set; }
        private readonly ResourceInfoStorage _resourceInfoStorage;
        private bool _finalized;

        public ResourceInfo(string id, ResourceInfoStorage resourceInfoStorage)
        {
            Id = id;
            Info = "";
            _resourceInfoStorage = resourceInfoStorage;
            resourceInfoStorage.Add(this);
        }

        public void Update()
        {
            if (_finalized) return;
            OnUpdate?.Invoke();
        }

        public void Dispose()
        {
            if (_finalized) return;
            _finalized = true;
            OnUpdate = null;
            _resourceInfoStorage.Remove(this);
        }

        internal event Action OnUpdate;

        internal void SetInfo(string arg)
        {
            if (_finalized) return;
            Info = arg;
        }
    }
}
using System;

namespace BugsFarm.InfoCollectorSystem
{
    public class ResourceBarInfo : IResourceBarInfo, IDisposable
    {
        public string Id { get; }
        public float Progress { get; private set; }
        public string CurrencyId { get; private set; }
        
        private readonly ResourceBarInfoStorage _resourceBarInfoStorage;
        private bool _finalized;

        public ResourceBarInfo(string id, ResourceBarInfoStorage resourceBarInfoStorage)
        {
            Id = id;
            _resourceBarInfoStorage = resourceBarInfoStorage;
            resourceBarInfoStorage.Add(this);
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
            _resourceBarInfoStorage.Remove(this);
        }

        internal event Action OnUpdate;

        internal void SetInfo(float progress, string currencyId = null)
        {
            if (_finalized) return;
            CurrencyId = currencyId;
            Progress = progress;
        }
    }
}
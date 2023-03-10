using BugsFarm.Services.StatsService;
using Zenject;

namespace BugsFarm.TaskSystem
{
    public class TimerFromStatKeyTask : TimerFromStatTask
    {
        private StatsCollectionStorage _statscollectionStorage;
        
        [Inject]
        private void Inject (StatsCollectionStorage statscollectionStorage)
        {
            _statscollectionStorage = statscollectionStorage;
        }

        public override void Execute(params object[] args)
        {
            if (IsRunned) return;
            var entityGuid = (string) args[0];
            var statKey = (string) args[1];
            var statCollection = _statscollectionStorage.Get(entityGuid);
            base.Execute(statCollection.Get<StatVital>(statKey));
        }

        protected override void OnDisposed()
        {
            base.OnDisposed();
            _statscollectionStorage = null;
        }
    }
}
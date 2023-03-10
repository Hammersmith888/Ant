using System;
using System.Collections.Generic;
using BugsFarm.Services.StatsService;
using UniRx;

namespace BugsFarm.UnitSystem
{
    public class DeathController
    {
        private readonly string _guid;
        private readonly Dictionary<string, StatVital> _stats;

        public DeathController(string guid,
                               IEnumerable<string> statKeys,
                               StatsCollectionStorage statCollectionStorage)
        {
            _guid = guid;
            var statCollection = statCollectionStorage.Get(_guid);
            _stats = new Dictionary<string, StatVital>();
            foreach (var key in statKeys)
            {
                var stat = statCollection.Get<StatVital>(key);
                stat.OnValueChanged += OnCurrentValueChanged;
                _stats.Add(key, stat);
            }
        }

        public void Dispose()
        {
            foreach (var stat in _stats.Values)
            {
                stat.OnCurrentValueChanged -= OnCurrentValueChanged;
            }

            _stats.Clear();
        }

        private void OnCurrentValueChanged(object sender, EventArgs e)
        {
            if (!(sender is StatVital stat)) return;
            if (!_stats.ContainsKey(stat.Id)) return;
            if (stat.CurrentValue >= stat.Value) // уф, в этом случае время или ресурс должен
                                                 // преодолеть максимум, тогда это и будет причиной смерти.
            {
                MessageBroker.Default.Publish(new DeathUnitProtocol
                {
                    UnitId = _guid, 
                    DeathReason = stat.Id.Replace("stat_", "")
                });
            }
        }
    }
}
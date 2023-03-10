using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BugsFarm.Services.SheetLoader;
using BugsFarm.Services.StatsService;
using BugsFarm.Utility;
using UnityEngine;

namespace BugsFarm.LeafHeapSystem.ConfigImporter
{
    public class LeafHeapConfigImporter : SheetLoader
    {
        [SerializeField] private bool _prettyPrint = false;
        
        [ExposeMethodInEditor]
        private void LeafHeapModelsConfigs()
        {
            Load("LeafHeapModels", data =>
            {
                var config = new List<LeafHeapModel>();
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                lines.RemoveAt(0); // headers
                foreach (var line in lines)
                {
                    var entries = line.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
                    if (entries.Length == 0) continue;
                    var modelId = entries[0];
                    var roomModel = new LeafHeapModel {ModelID = modelId};
                    config.Add(roomModel);
                }

                ConfigHelper.Save(config.ToArray(), "LeafHeapModels");
            });
        }

        [ExposeMethodInEditor]
        private void LeafHeapStatsConfigs()
        {
            Load("LeafHeapStatModels", data =>
            {
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                var config = new Dictionary<string, LeafHeapStatModel>(); 
                var statDecode = new Dictionary<string, string>
                {
                    {"Static", nameof(Stat)},
                    {"Modifiable", nameof(StatModifiable)},
                    {"Attribute", nameof(StatAttribute)},
                    {"Vitality", nameof(StatVital)},
                };
                lines.RemoveAt(0); // headers
                foreach (var line in lines)
                {
                    var items = line.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries).ToList();
                    var modelId = items[1];
                    if (!config.ContainsKey(modelId))
                    {
                        config.Add(modelId, new LeafHeapStatModel{ModelID = modelId, Stats = new StatModel[0]});
                    }

                    var model = config[modelId];
                    var statType = statDecode[items[4]];
                    var param = items[3].Split(new[] {"/"}, StringSplitOptions.RemoveEmptyEntries);
                    var baseValue = float.Parse(param.Length > 1 ? param[1] : param[0], CultureInfo.InvariantCulture);
                    var initValue = float.Parse(param.Length > 1 ? param[0] : "0", CultureInfo.InvariantCulture);
                    model.Stats = model.Stats.Append(new StatModel
                    {
                        StatID = items[2],
                        StatType = statType,
                        BaseValue = baseValue,
                        InitValue = initValue,
                    }).ToArray();
                    config[modelId] = model;
                }
                
                ConfigHelper.Save(config.Values.ToArray(), "LeafHeapStatModels", _prettyPrint);
            });
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BugsFarm.CurrencySystem;
using BugsFarm.Services.SheetLoader;
using BugsFarm.Services.StatsService;
using BugsFarm.Utility;
using UnityEngine;

namespace BugsFarm.ChestSystem.ConfigImporter
{
    public class ChestConfigImporter : SheetLoader
    {
        [SerializeField] private bool _prettyPrint = false;

        [ExposeMethodInEditor]
        private void ChestModelsConfigs()
        {
            Load("ChestModels", data =>
            {
                var config = new List<ChestModel>();
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                lines.RemoveAt(0); // headers
                foreach (var line in lines)
                {
                    var entries = line.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
                    if (entries.Length == 0) continue;
                    var typeID = int.Parse(entries[1]);
                    var chestModel = new ChestModel {ModelID = entries[0], TypeID = typeID};
                    var items = new List<CurrencyModel>();

                    for (var i = 2; i < entries.Length;)
                    {
                        items.Add(new CurrencyModel {ModelID = entries[i], Count = int.Parse(entries[i + 1])});
                        i += 2;
                    }

                    chestModel.Items = items.ToArray();
                    config.Add(chestModel);
                }

                ConfigHelper.Save(config.ToArray(), "ChestModels", _prettyPrint);
            });
        }

        [ExposeMethodInEditor]
        private void ChestStatsConfigs()
        {
            Load("ChestStatModels", data =>
            {
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                var config = new Dictionary<string, ChestStatModel>(); 
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
                        config.Add(modelId, new ChestStatModel{ModelID = modelId, Stats = new StatModel[0]});
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

                ConfigHelper.Save(config.Values.ToArray(), "ChestStatModels", _prettyPrint);
            });
        }
    }
}
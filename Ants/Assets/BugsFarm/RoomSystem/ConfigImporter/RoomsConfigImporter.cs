using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BugsFarm.CurrencySystem;
using BugsFarm.Services.SheetLoader;
using BugsFarm.Services.StatsService;
using BugsFarm.Utility;
using UnityEngine;

namespace BugsFarm.RoomSystem.ConfigImporter
{
    public class RoomsConfigImporter : SheetLoader
    {
        [SerializeField] private bool _prettyPrint = false;

        [ExposeMethodInEditor]
        private void RoomNeighboursConfigs()
        {
            Load("RoomNeighbourModels", data =>
            {
                var config = new List<RoomNeighbourModel>();
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                lines.RemoveAt(0); // headers
                foreach (var line in lines)
                {
                    var entries = line.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
                    var modelId = entries[0];
                    var roomModel = new RoomNeighbourModel {ModelID = modelId};
                    var items = new List<string>(entries);
                    items.RemoveAt(0);
                    roomModel.Neighbours = items.ToArray();
                    config.Add(roomModel);
                }

                ConfigHelper.Save(config.ToArray(), "RoomNeighbourModels", _prettyPrint);
            });
        }

        [ExposeMethodInEditor]
        private void RoomModelsConfigs()
        {
            Load("RoomModels", data =>
            {
                var config = new List<RoomModel>();
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                lines.RemoveAt(0); // headers
                foreach (var line in lines)
                {
                    var entries = line.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
                    if (entries.Length == 0) continue;
                    var modelId = entries[0];
                    var typeName = entries[1];
                    var roomModel = new RoomModel {ModelID = modelId, TypeName = typeName};
                    var items = new List<CurrencyModel>();

                    for (var i = 2; i < entries.Length;)
                    {
                        items.Add(new CurrencyModel {ModelID = entries[i], Count = int.Parse(entries[i + 1])});
                        i += 2;
                    }

                    roomModel.Price = items.ToArray();
                    config.Add(roomModel);
                }

                ConfigHelper.Save(config.ToArray(), "RoomModels", _prettyPrint);
            });
        }

        [ExposeMethodInEditor]
        private void RoomStatsConfigs()
        {
            Load("RoomStatModels", data =>
            {
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                var config = new Dictionary<string, RoomStatModel>(); 
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
                        config.Add(modelId, new RoomStatModel{ModelID = modelId, Stats = new StatModel[0]});
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
                
                ConfigHelper.Save(config.Values.ToArray(), "RoomStatModels", _prettyPrint);
            });
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.Services.SheetLoader;
using BugsFarm.Services.StatsService;
using BugsFarm.Utility;
using UnityEngine;

namespace BugsFarm.BattleSystem.ConfigImporter
{
    public class BattleConfigImporter : SheetLoader
    {
        [SerializeField] private bool _prettyPrint;
        [ExposeMethodInEditor]
        private void BattlePassConfigToJson()
        {
            Load("BattlePassModels", data =>
            {
                var config = new List<BattlePassModel>();
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                var headers = lines[0].Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
                lines.RemoveAt(0); // remove headers
                lines.RemoveRange(9,2);
                foreach (var line in lines)
                {
                    var items = line.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries).ToList();
                    var modelID = items[1];
                    var itemParams = new List<BattlePassParam>();
                    for (var i = 2; i < 5; i++)    
                    {
                        var values = items[i].Split(new[] {"/"}, StringSplitOptions.RemoveEmptyEntries);
                        if (values.Contains("-")) continue;

                        var key = char.ToLower(headers[i][0]) + headers[i].Substring(1); // ToLower -> toLower
                        var hasMin = values.Length > 1;
                        var current = hasMin ? float.Parse(values[0]) : 0;
                        var maximum = hasMin ? float.Parse(values[1]) : float.Parse(values[0]);
                        
                        itemParams.Add(new BattlePassParam
                        {
                            Id = key,
                            MinValue = current,
                            MaxValue = maximum
                        });
                    }

                    config.Add(new BattlePassModel
                    {
                        ModelID = modelID,
                        Params = itemParams.ToDictionary(x => x.Id)
                    });
                }

                ConfigHelper.Save(config.ToArray(), "BattlePassModels", _prettyPrint);
                Debug.Log($"Complete : <color=green>BattlePassModels</color>");
            });
        }
    }
}
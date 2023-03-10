using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.Services.SheetLoader;
using BugsFarm.Utility;
using UnityEngine;

namespace BugsFarm.CurrencySystem.ConfigImporter
{
    public class CurrencyConfigImporter : SheetLoader
    {

        [ExposeMethodInEditor]
        private void CurrencySettingConfigs()
        {
            Load("CurrencySettingModels", data =>
            {
                var config = new List<CurrencySettingModel>();
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
                lines.RemoveAt(0); // headers
                foreach (var line in lines)
                {
                    var entries = line.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
                    if (entries.Length == 0) continue;
                    var currencyID = entries[0];
                    var hexColor = entries[1];
                    config.Add(new CurrencySettingModel
                    {
                        ModelID = currencyID, 
                        HexColor = hexColor
                    });
                }

                ConfigHelper.Save(config.ToArray(), "CurrencySettingModels"); 
            });

        }
    }
}
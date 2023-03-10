using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.Services.SheetLoader;
using BugsFarm.Utility;
using UnityEngine;

namespace BugsFarm.UI
{
    public class BottomHudConfigImporter : SheetLoader
    {
        [SerializeField] private bool _prettyPrint = false;

        [ExposeMethodInEditor]
        private void DonateShopItemModelConfigToJson()
        {
            Load("DonatShopItemModels", data =>
            {
                var config = new List<DonatShopItemModel>();
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                lines.RemoveAt(0); //Headers

                foreach (var line in lines)
                {
                    var items = line.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
                    
                    config.Add(new DonatShopItemModel
                    {
                        Count = int.Parse(items[3]),
                        IconName = items[4],
                        ModelId = items[0],
                        Price = int.Parse(items[2]),
                        TypeId = items[1]
                    });
                }
                
                ConfigHelper.Save(config.ToArray(), "DonatShopItemModels", _prettyPrint);
            });
        }
        
        [ExposeMethodInEditor]
        private void MyBugModelConfigToJson()
        {
            Load("MyBugItemModels", data =>
            {
                var config = new List<MyBugItemModel>();
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                lines.RemoveAt(0); //Headers

                foreach (var line in lines)
                {
                    var items = line.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
                    var stats = new List<string>();
                    for (var i = 2; i < items.Length; i++)
                    {
                        stats.Add("stat_" + items[i]);
                    }
                    
                    config.Add(new MyBugItemModel
                    {
                        ModelId = items[1],
                        Stats = stats
                    });
                }
                
                ConfigHelper.Save(config.ToArray(), "MyBugItemModels", _prettyPrint);
            });
        }
        
    }
}

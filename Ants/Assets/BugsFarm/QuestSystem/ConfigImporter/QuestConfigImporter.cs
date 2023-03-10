using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BugsFarm.Services.SheetLoader;
using BugsFarm.Utility;
using UnityEngine;

namespace BugsFarm.Quest
{
    public class QuestConfigImporter : SheetLoader
    {
        [SerializeField] private bool _prettyPrint;
        
        [ExposeMethodInEditor]
        private void LoadQuestElementsData()
        {
            Load("QuestItems", data =>
            {
                var config = new List<QuestElementModel>();
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                lines.RemoveAt(0); // headers
                foreach (var line in lines)
                {
                    var entries = line.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
                    
                    var roomModel = new QuestElementModel()
                    {
                        ModelID = entries[1],
                        LocalizationKey = entries[2],
                        QuestIcon = entries[3],
                        Level = int.Parse(entries[4]),
                        GoldReward = int.Parse(entries[5]),
                        QuestType = entries[6],
                        MinGoalValue = int.Parse(entries[7]),
                        MaxGoalValue = int.Parse(entries[8]),
                        ReferenceID = entries[9],
                        QuestDurationInMinutes = float.Parse(entries[10], CultureInfo.InvariantCulture)
                    };
                    config.Add(roomModel);
                }

                ConfigHelper.Save(config.ToArray(), "QuestElementModels", _prettyPrint);
            });
        }
        
        [ExposeMethodInEditor]
        private void LoadQuestGroupsData()
        {
            Load("QuestGroups", data =>
            {
                var config = new List<QuestGroupModel>();
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                lines.RemoveAt(0); // headers
                foreach (var line in lines)
                {
                    var entries = line.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);

                    List<VirtualChestModel> chestModels = new List<VirtualChestModel>();
                    
                    for (int i = 2; i < entries.Length; i += 4)
                    {
                        chestModels.Add(new VirtualChestModel()
                        {
                            ModelID = entries[i],
                            Reward = int.Parse(entries[i + 1]),
                            CurrencyID = entries[i + 2],
                            Treshold = float.Parse(entries[i + 3], CultureInfo.InvariantCulture)
                        });
                    }
                    
                    var roomModel = new QuestGroupModel()
                    {
                        ModelID = entries[0],
                        Duration = double.Parse(entries[1]),
                        VirtualChestModels = chestModels
                    };
                    config.Add(roomModel);
                }

                ConfigHelper.Save(config.ToArray(), "QuestGroupModels", _prettyPrint);
            });
        }

    }

}

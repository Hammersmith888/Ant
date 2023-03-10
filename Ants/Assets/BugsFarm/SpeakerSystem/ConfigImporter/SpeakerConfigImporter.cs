using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.Services.SheetLoader;
using BugsFarm.Utility;
using UnityEngine;

namespace BugsFarm.SpeakerSystem.ConfigImporter
{
    public class SpeakerConfigImporter : SheetLoader
    {
        [SerializeField] private bool _prettyPrint;
        [ExposeMethodInEditor]
        private void PhrasesConfigToJson()
        {
            Load("PhrasesModels", data =>
            {
                // (ModelId, (GroupeId, Models))
                var models = new Dictionary<string, List<PhraseGroupModel>>();
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                lines.RemoveAt(0); // remove headers
                foreach (var line in lines)
                {
                    var items = line.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries).ToList();
                    var groupeId = items[0];
                    var modelId = items[1];
                    items.RemoveRange(0,2);
                    if (!models.ContainsKey(modelId))
                    {
                        models.Add(modelId, new List<PhraseGroupModel>());
                    }
                    
                    models[modelId].Add(new PhraseGroupModel
                    {
                        PahraseGroupID = groupeId, 
                        Phrases = items.Select(int.Parse).ToArray()
                    });
                }

                var config = models.Select(modelKpv => new PhrasesModel
                    {
                        Groupe = modelKpv.Value.ToArray(), 
                        ModelID = modelKpv.Key
                    })
                    .ToArray();

                ConfigHelper.Save(config, "PhrasesModels", _prettyPrint);
                Debug.Log($"Complete : <color=green>PhrasesModels</color>");
            });
        }
    }
}
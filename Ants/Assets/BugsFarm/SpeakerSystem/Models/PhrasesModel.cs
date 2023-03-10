using System;
using System.Linq;
using BugsFarm.Services.StorageService;

namespace BugsFarm.SpeakerSystem
{
    [Serializable]
    public struct PhrasesModel : IStorageItem
    {
        public string ModelID;
        public PhraseGroupModel[] Groupe;

        string IStorageItem.Id => ModelID;

        public PhraseGroupModel Get(string key)
        {
            return Groupe.FirstOrDefault(x => x.PahraseGroupID == key);
        }
        
        public bool Contains(string key)
        {
            return Groupe.Any(x => x.PahraseGroupID == key);
        }
    }
}
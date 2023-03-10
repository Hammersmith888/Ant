using System;
using Random = UnityEngine.Random;

namespace BugsFarm.SpeakerSystem
{
    [Serializable]
    public struct PhraseGroupModel
    {
        public string PahraseGroupID;
        public int[] Phrases;

        public string GetRandomPhrase()
        {
            return PahraseGroupID + "_" + Phrases[Random.Range(0, Phrases.Length)] + "_phrase";
        }
    }
}
using System;
using UnityEngine;

namespace BugsFarm.SpeakerSystem
{
    public interface ISpeakerSystem
    {
        event Action OnBeforeSpeak;
        void Registration(string guid, string phrasesModelId, Transform view);
        void UnRegistration(string guid);
        void ChangeState(string guid, PhraseState state);
        void AllowSay(string guid, bool allow);
        void Say(string guid, PhraseState state);
        bool HasEntity(string guid);
        void Welcome();
    }
}
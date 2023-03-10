using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.FarmCameraSystem;
using BugsFarm.Services.MonoPoolService;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.UIService;
using BugsFarm.SimulationSystem;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BugsFarm.SpeakerSystem
{
    public class SpeakerSystem : ISpeakerSystem
    {
        private class Speaker
        {
            public Transform Target;
            public PhrasesModel Model;
            public PhraseState State = PhraseState.none;
            public bool Allow = true;
        }

        public event Action OnBeforeSpeak;
        private readonly Vector2 _delayRange;
        private readonly Dictionary<string, Speaker> _speakers;
        private readonly List<string> _speakersSay;
        private readonly FarmCameraController _farmCameraController;
        private readonly UIWorldRoot _parent;
        private readonly ISimulationSystem _simulationSystem;
        private readonly PhrasesStorage _phraseModelStorage;
        private readonly IMonoPool _monoPool;
        private const float _welcomeDelay = 1f;
        private const string _speakerTargetName = "SpeakerPoint";
        private bool _scheduledNotify;

        public SpeakerSystem(FarmCameraController farmCameraController,
                             UIWorldRoot speakerRoot,
                             ISimulationSystem simulationSystem,
                             PhrasesStorage phraseModelStorage,
                             IMonoPool monoPool)
        {
            _delayRange = new Vector2(30, 60);
            _speakers = new Dictionary<string, Speaker>();
            _speakersSay = new List<string>();
            _farmCameraController = farmCameraController;
            _parent = speakerRoot;
            _simulationSystem = simulationSystem;
            _phraseModelStorage = phraseModelStorage;
            _monoPool = monoPool;
        }

        public void Registration(string guid, string phrasesModelId, Transform view)
        {
            if (HasEntity(guid)) return;
            if (!_phraseModelStorage.HasEntity(phrasesModelId))
            {
                Debug.LogError($"{this} : {nameof(PhrasesModel)} does not exist for ModelID : {phrasesModelId}");
                return;
            }

            var phrasesModel = _phraseModelStorage.Get(phrasesModelId);
            var target = view.Find(_speakerTargetName);
            var speaker = new Speaker {Model = phrasesModel, Target = target};
            _speakers.Add(guid, speaker);
        }

        public void UnRegistration(string guid)
        {
            if (!HasEntity(guid)) return;
            _speakers.Remove(guid);
            _speakersSay.Remove(guid);
        }

        public void ChangeState(string guid, PhraseState state)
        {
            if (!HasEntity(guid)) return;
            _speakers[guid].State = state;
        }

        public void AllowSay(string guid, bool allow)
        {
            if(!HasEntity(guid)) return;
            _speakers[guid].Allow = allow;
        }

        public void Say(string guid, PhraseState state)
        {
            if (_simulationSystem.Simulation) return;
            if (!HasEntity(guid)) return;

            if (state == PhraseState.none || _speakersSay.Contains(guid)) return;

            var speakerParams = SpeakerParams.Normal;
            var speaker = _speakers[guid];
            if(!speaker.Allow) return;
            
            var nSpeakers = Tools.RandomRange(speakerParams.CountSpeakersRange);
            var count = nSpeakers - _speakersSay.Count;
            if (count <= 0) return;

            ChangeState(guid, state);
            var phraseGroupe = speaker.State.ToString();
            if (!speaker.Model.Contains(phraseGroupe)) return;

            var phraseGroupModel = speaker.Model.Get(phraseGroupe);
            var phrase = phraseGroupModel.GetRandomPhrase();
            var lifeTime = speakerParams.LifeTime;
            _speakersSay.Add(guid);

            Say(speaker, 0f, lifeTime, LocalizationManager.Localize(phrase));
            DOVirtual.DelayedCall(lifeTime, () => _speakersSay.Remove(guid));
        }

        public bool HasEntity(string guid)
        {
            return _speakers.ContainsKey(guid);
        }

        public void Welcome()
        {
            foreach (var speaker in _speakers)
            {
                speaker.Value.State = PhraseState.greetings;
            }

            DOVirtual.DelayedCall(_welcomeDelay, () => Notify(SpeakerParams.Greeting));
            NextNotify();
        }

        private void NextNotify(float timerInSeconds = 0)
        {
            if (_scheduledNotify) return;

            _scheduledNotify = true;
            var timer = timerInSeconds + Tools.RandomRange(_delayRange);
            DOVirtual.DelayedCall(timer, () => { _scheduledNotify = false; Notify(SpeakerParams.Normal); });
        }

        private void Notify(SpeakerParams speakerParams)
        {
            var maxShowTime = speakerParams.LifeTime;
            if (_simulationSystem.Simulation)
            {
                NextNotify(maxShowTime);
                return;
            }

            if (!speakerParams.IsGreeting)
            {
                OnBeforeSpeak?.Invoke();
            }

            var nSpeakers = Tools.RandomRange(speakerParams.CountSpeakersRange);
            var listeners = nSpeakers - _speakersSay.Count;
            if (listeners <= 0)
            {
                NextNotify(maxShowTime);
                return;
            }

            var speakerIds = _speakers.Keys.ToArray();
            var speakersFree = speakerIds.Where(x => _speakers[x].State != PhraseState.none && _speakers[x].Allow)
                                         .Except(_speakersSay)
                                         .ToList();
            listeners = Mathf.Min(speakersFree.Count, listeners);
            if (listeners == 0)
            {
                NextNotify(maxShowTime);
                return;
            }
            
            while (listeners > 0)
            {
                var speakerIndex = Random.Range(0, speakersFree.Count);
                var speakerId = speakersFree[speakerIndex];
                var speaker = _speakers[speakerId];
                speakersFree.RemoveAt(speakerIndex);

                var delayTotal = Tools.RandomRange(speakerParams.DelayRange) * listeners;
                var phraseGroupe = speaker.State.ToString();

                if (!speaker.Model.Contains(phraseGroupe))
                {
                    listeners--;
                    continue;
                }

                var phraseModel = speaker.Model.Get(phraseGroupe);
                var phrase = phraseModel.GetRandomPhrase();
                var removeTime = speakerParams.LifeTime + delayTotal;
                _speakersSay.Add(speakerId);

                Say(speaker, delayTotal, speakerParams.LifeTime, LocalizationManager.Localize(phrase));
                maxShowTime = Mathf.Max(maxShowTime, removeTime);
                DOVirtual.DelayedCall(removeTime, () => _speakersSay.Remove(speakerId));
                listeners--;
            }

            NextNotify(maxShowTime);
        }

        private void Say(Speaker speaker, float delay, float lifeTime, string phrase)
        {
            DOVirtual.DelayedCall(delay, () =>
            {
                if (speaker != null && speaker.Allow)
                {
                    var uiSpeaker = _monoPool.Spawn<UISpeaker>(_parent.Transform);
                    uiSpeaker.Init(_farmCameraController.Camera, speaker.Target, lifeTime, phrase); 
                }
            });
        }
    }
}
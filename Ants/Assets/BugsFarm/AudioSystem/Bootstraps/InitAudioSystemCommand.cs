using BugsFarm.Services.BootstrapService;
using UnityEngine;

namespace BugsFarm.AudioSystem
{
    public class InitAudioSystemCommand : Command
    {
        private readonly AudioModelStorage _audioModelStorage;
        private const string _modelsResourcePath = "Audio/";
        public InitAudioSystemCommand(AudioModelStorage audioModelStorage)
        {
            _audioModelStorage = audioModelStorage;
        }

        public override void Do()
        {
            var models = Resources.LoadAll<AudioModel>(_modelsResourcePath);
            foreach (var audioModel in models)
            {
                _audioModelStorage.Add(audioModel);
            }
            OnDone();
        }
    }
}
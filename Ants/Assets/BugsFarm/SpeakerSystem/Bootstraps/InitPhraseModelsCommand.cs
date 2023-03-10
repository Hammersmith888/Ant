using BugsFarm.Services.BootstrapService;
using BugsFarm.Utility;

namespace BugsFarm.SpeakerSystem
{
    public class InitPhraseModelsCommand : Command
    {
        private readonly PhrasesStorage _phraseModelsStorage;

        public InitPhraseModelsCommand(PhrasesStorage phraseModelsStorage)
        {
            _phraseModelsStorage = phraseModelsStorage;
        }

        public override void Do()
        {
            var config = ConfigHelper.Load<PhrasesModel>("PhrasesModels");
            foreach (var model in config)
            {
                _phraseModelsStorage.Add(model);
            }

            OnDone();
        }
    }
}
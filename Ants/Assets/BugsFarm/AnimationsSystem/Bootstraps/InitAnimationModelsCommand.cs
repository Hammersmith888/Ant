using BugsFarm.Services.BootstrapService;
using UnityEngine;

namespace BugsFarm.AnimationsSystem
{
    public class InitAnimationModelsCommand : Command
    {
        private readonly AnimationModelStorage _animationModelsStorage;
        private const string _basePath = "Animations/";
        private readonly string[] _directories = {"Buildings", "Misc", "Units"};

        public InitAnimationModelsCommand(AnimationModelStorage animationModelsStorage)
        {
            _animationModelsStorage = animationModelsStorage;
        }

        public override void Do()
        {
            foreach (var directory in _directories)
            {
                var targetPath = _basePath + directory;
                var loadedModels = Resources.LoadAll<AnimationModel>(targetPath);
                foreach (var model in loadedModels)
                {
                    _animationModelsStorage.Add(model);
                }
            }
            
            OnDone();
        }
    }
}
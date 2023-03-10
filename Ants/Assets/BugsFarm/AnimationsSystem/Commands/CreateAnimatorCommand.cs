using System;
using System.Linq;
using System.Threading.Tasks;
using BugsFarm.Services.CommandService;
using BugsFarm.UnitSystem;
using Zenject;

namespace BugsFarm.AnimationsSystem
{
    public class CreateAnimatorCommand<T> : ICommand<CreateAnimatorProtocol> where T : ISpineAnimator
    {
        private readonly IInstantiator _instantiator;
        private readonly AnimatorStorage _animatorStorage;
        private readonly AnimationModelStorage _animationModelStorage;

        public CreateAnimatorCommand(IInstantiator instantiator,
                                     AnimatorStorage animatorStorage,
                                     AnimationModelStorage animationModelStorage)
        {
            _instantiator = instantiator;
            _animatorStorage = animatorStorage;
            _animationModelStorage = animationModelStorage;
        }
        
        public Task Execute(CreateAnimatorProtocol protocol)
        {
            if (_animatorStorage.HasEntity(protocol.Guid))
            {
                return Task.CompletedTask;
            }

            if(!_animationModelStorage.HasEntity(protocol.AnimModelId))
            {
                throw new ArgumentException();
            }
            
            var animModel = _animationModelStorage.Get(protocol.AnimModelId);
            var args = protocol.Args.Append(animModel).Append(protocol.Guid);
            var spineAnimator = _instantiator.Instantiate<T>(args);
            spineAnimator.Initialize();
            _animatorStorage.Add(spineAnimator);
            protocol.Result?.Invoke(spineAnimator);
            return Task.CompletedTask;
        }
    }
}
using System.Threading.Tasks;
using BugsFarm.Services.CommandService;

namespace BugsFarm.AnimationsSystem
{
    public class RemoveAnimatorCommand : ICommand<RemoveAnimatorProtocol>
    {
        private readonly AnimatorStorage _animatorStorage;

        public RemoveAnimatorCommand(AnimatorStorage animatorStorage)
        {
            _animatorStorage = animatorStorage;
        }
        
        public Task Execute(RemoveAnimatorProtocol protocol)
        {
            if (!_animatorStorage.HasEntity(protocol.Guid))
            {
                return Task.CompletedTask;
            }
            
            var animator = _animatorStorage.Get(protocol.Guid);
            _animatorStorage.Remove(protocol.Guid);
            animator?.Dispose();
            return Task.CompletedTask;
        }
    }
}
using System.Threading.Tasks;
using BugsFarm.Services.CommandService;

namespace BugsFarm.Services.SceneEntity
{
    public class InteractionBaseCommand : ICommand<InteractionProtocol>
    {
        public virtual Task Execute(InteractionProtocol protocol)
        {
            return Task.CompletedTask;
        }
    }
}
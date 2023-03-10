using System.Threading.Tasks;
using BugsFarm.Services.CommandService;
using BugsFarm.Services.SimpleLocalization;
using Zenject;

namespace BugsFarm.Services.SceneEntity
{
    public class GetEntityNameCommand : ICommand<GetEntityNameProtocol>
    {
        private readonly IInstantiator _instantiator;
        public GetEntityNameCommand(IInstantiator instantiator)
        {
            _instantiator = instantiator;
        }

        public Task Execute(GetEntityNameProtocol protocol)
        {
            var entityName = LocalizationManager.Localize($"{protocol.Prefix}{protocol.ModelId}");
            if (!string.IsNullOrEmpty(protocol.EntityId))
            {
                _instantiator.Instantiate<GetEntityLevelCommand>()
                    .Execute(new GetEntitytLevelProtocol(protocol.EntityId,
                level =>
                {
                    if (level > 0)
                    {
                        entityName = $"{entityName} {LocalizationManager.Localize("LVL")}{level}";
                    }
                }));
            }

            protocol.Result?.Invoke(entityName);
            return Task.CompletedTask;
        }
    }
}
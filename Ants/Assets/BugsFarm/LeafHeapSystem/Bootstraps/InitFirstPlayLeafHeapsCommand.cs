using BugsFarm.Services.BootstrapService;
using Zenject;

namespace BugsFarm.LeafHeapSystem
{
    public class InitFirstPlayLeafHeapsCommand : Command
    {
        private readonly IInstantiator _instantiator;
        private readonly LeafHeapModelStorage _leafHeapModelsStorage;

        public InitFirstPlayLeafHeapsCommand(IInstantiator instantiator,
                                             LeafHeapModelStorage leafHeapModelsStorage)
        {
            _instantiator = instantiator;
            _leafHeapModelsStorage = leafHeapModelsStorage;
        }
        
        public override void Do()
        {
            var buildLeafHeapCommand = _instantiator.Instantiate<CreateLeafHeapCommand>();
            foreach (var model in _leafHeapModelsStorage.Get())
            {
                var buildLeafHeapProcotol = new CreateLeafHeapProtocol(model.ModelID, true);
                buildLeafHeapCommand.Execute(buildLeafHeapProcotol);
            }
            
            OnDone();
        }
    }
}
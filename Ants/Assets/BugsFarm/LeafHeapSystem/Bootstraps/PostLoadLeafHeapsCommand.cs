using BugsFarm.Services.BootstrapService;
using UnityEngine;
using Zenject;

namespace BugsFarm.LeafHeapSystem
{
    public class PostLoadLeafHeapsCommand : Command
    {
        private readonly IInstantiator _instantiator;
        private readonly LeafHeapDtoStorage _leafHeapDtoStorage;

        public PostLoadLeafHeapsCommand(IInstantiator instantiator,
                                        LeafHeapDtoStorage leafHeapDtoStorage)
        {
            _instantiator = instantiator;
            _leafHeapDtoStorage = leafHeapDtoStorage;
        }

        public override void Do()
        {
            var buildingCommand = _instantiator.Instantiate<CreateLeafHeapCommand>();
            foreach (var leafHeapDto in _leafHeapDtoStorage.Get())
            {
                buildingCommand.Execute(new CreateLeafHeapProtocol(leafHeapDto.Guid, false));
            }

            OnDone();
        }
    }
}
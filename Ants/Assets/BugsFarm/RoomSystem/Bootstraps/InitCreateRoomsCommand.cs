using BugsFarm.Services.BootstrapService;
using UnityEngine;
using Zenject;

namespace BugsFarm.RoomSystem
{
    public class InitCreateRoomsCommand : Command
    {
        private readonly IRoomsSystem _roomsSystem;
        private readonly IInstantiator _instantiator;
        private readonly RoomModelStorage _roomModelStorage;

        public InitCreateRoomsCommand(IRoomsSystem roomsSystem,
                                      IInstantiator instantiator,
                                      RoomModelStorage roomModelStorage)
        {
            _roomsSystem = roomsSystem;
            _instantiator = instantiator;
            _roomModelStorage = roomModelStorage;
        }

        public override void Do()
        {
            var buildRoomCommand = _instantiator.Instantiate<CreateRoomCommand>();
            foreach (var roomModel in _roomModelStorage.Get())
            {
                var buildRoomProtocol = new CreateRoomProtocol(roomModel.ModelID, true);
                buildRoomCommand.Execute(buildRoomProtocol);
            }

            _roomsSystem.Initialize();
            OnDone();
        }
    }
}
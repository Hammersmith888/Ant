using System;
using System.Linq;
using BugsFarm.RoomSystem;
using BugsFarm.Services.StatsService;
using UniRx;
using Zenject;

namespace BugsFarm.ChestSystem
{
    public class ChestOpenableSystem : IDisposable, IInitializable
    {
        private readonly ChestDtoStorage _chestDtoStorage;
        private readonly RoomDtoStorage _roomDtoStorage;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private const string _contentShowedStatKey = "stat_contentShowed";
        private IDisposable _openRoomEvent;
        public ChestOpenableSystem(ChestDtoStorage chestDtoStorage,
                                   RoomDtoStorage roomDtoStorage,
                                   StatsCollectionStorage statsCollectionStorage)
        {
            _chestDtoStorage = chestDtoStorage;
            _roomDtoStorage = roomDtoStorage;
            _statsCollectionStorage = statsCollectionStorage;
        }

        public void Initialize()
        {
            _openRoomEvent = MessageBroker.Default.Receive<OpenRoomProtocol>().Subscribe(OnRoomOpened);
        }

        public void Dispose()
        {
            _openRoomEvent?.Dispose();
            _openRoomEvent = null;
        }

        private void OnRoomOpened(OpenRoomProtocol protocol)
        {
            if (!_roomDtoStorage.HasEntity(protocol.Guid))
            {
                return;
            }

            var roomDto = _roomDtoStorage.Get(protocol.Guid);
            var chestDto = _chestDtoStorage.Get().FirstOrDefault(x => x.ModelID == roomDto.ModelID);

            if (chestDto == null)
            {
                return;
            }

            var statCollection = _statsCollectionStorage.Get(chestDto.Guid);
            statCollection.AddModifier(_contentShowedStatKey, new StatModBaseAdd(1));
        }
    }
}
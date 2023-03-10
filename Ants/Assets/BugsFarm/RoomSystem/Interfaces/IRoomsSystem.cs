using System.Collections.Generic;

namespace BugsFarm.RoomSystem
{
    public interface IRoomsSystem
    {
        void Initialize();
        void Registration(RoomSystemProtocol protocol);
        void UnRegistration(string guid);
        void OpenRoom(string guid);
        bool HasEntity(string guid);
        IEnumerable<string> Opened();
    }
}
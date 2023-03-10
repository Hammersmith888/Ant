using System;
using BugsFarm.Services.CommandService;

namespace BugsFarm.RoomSystem
{
    public class RoomSystemProtocol : IProtocol
    {
        public readonly string Guid;
        public readonly bool HasTasks;
        public readonly bool HasDependency;
        public readonly Func<bool> IsOpened;

        public RoomSystemProtocol(string guid, Func<bool> isOpened, bool hasTasks, bool hasDependency)
        {
            Guid = guid;
            IsOpened = isOpened;
            HasTasks = hasTasks;
            HasDependency = hasDependency;
        }
    }
}
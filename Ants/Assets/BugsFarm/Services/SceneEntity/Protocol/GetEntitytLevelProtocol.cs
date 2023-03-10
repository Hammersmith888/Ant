using System;
using BugsFarm.Services.CommandService;

namespace BugsFarm.Services.SceneEntity
{
    public readonly struct GetEntitytLevelProtocol : IProtocol
    {
        public readonly string EntityId;
        public readonly Action<int> Result;

        public GetEntitytLevelProtocol(string entityId, Action<int> result = null)
        {
            EntityId = entityId;
            Result = result;
        }
    }
}
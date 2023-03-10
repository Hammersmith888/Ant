using System;
using BugsFarm.Services.CommandService;

namespace BugsFarm.Services.SceneEntity
{
    public readonly struct GetEntityNameProtocol : IProtocol
    {
        public readonly string ModelId;
        public readonly string Prefix;
        public readonly string EntityId;
        public readonly Action<string> Result;

        public GetEntityNameProtocol(string modelId, 
                                     string prefix = "",
                                     Action<string> result = null)
        {
            ModelId = modelId;
            Prefix = prefix;
            EntityId = null;
            Result = result;
        }
        public GetEntityNameProtocol(string modelId, 
                                     string entityId,
                                     string prefix = "",
                                     Action<string> result = null)
        {
            Prefix = prefix;
            ModelId = modelId;
            EntityId = entityId;
            Result = result;
        }
    }
}
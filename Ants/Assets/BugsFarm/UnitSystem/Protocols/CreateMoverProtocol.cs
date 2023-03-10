using System;
using BugsFarm.Services.CommandService;

namespace BugsFarm.UnitSystem
{
    public readonly struct CreateMoverProtocol : IProtocol
    {
        public readonly string Guid;
        public readonly object[] Args;
        public readonly Action<IUnitMover> Result;
        public CreateMoverProtocol(string guid, params object[] args)
        {
            Guid = guid;
            Args = args ?? new object[0];
            Result = default;
        }
        public CreateMoverProtocol(string guid, Action<IUnitMover> result, params object[] args)
        {
            Guid = guid;
            Args = args ?? new object[0];
            Result = result;
        }
    }
}
using System;
using BugsFarm.Services.CommandService;

namespace BugsFarm.UnitSystem
{
    public readonly struct ActivitySystemProtocol : IProtocol
    {
        public readonly Action StopProcess;
        public readonly Action PlayProcess;
        public readonly string Guid;

        public ActivitySystemProtocol(string guid, Action playProcess,  Action stopProcess)
        {
            StopProcess = stopProcess;
            PlayProcess = playProcess;
            Guid = guid;
        }
    }
}
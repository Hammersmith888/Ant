using System;

namespace BugsFarm.UnitSystem.Obsolete
{
    public interface IAiControlable : IPostLoadRestorable, IDisposable
    {
        bool TryStart();
        bool IsInterruptible { get; }
        StateAnt AiType { get; }
        int Priority { get; }
        ATask Task { get; }
        void Setup(SM_AntRoot root);
    }
}
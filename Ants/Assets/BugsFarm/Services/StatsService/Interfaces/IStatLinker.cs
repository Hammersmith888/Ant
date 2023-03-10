using System;

namespace BugsFarm.Services.StatsService
{
    public interface IStatLinker : IDisposable, IStatValueChanged
    {
        Stat Stat { get; }
        string LinkerId { get; }
        float Value { get; }
    }
}
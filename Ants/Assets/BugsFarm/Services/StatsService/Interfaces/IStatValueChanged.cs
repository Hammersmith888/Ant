using System;

namespace BugsFarm.Services.StatsService
{
    public interface IStatValueChanged
    {
        event EventHandler OnValueChanged;
    }
}